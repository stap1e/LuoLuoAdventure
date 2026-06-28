#!/usr/bin/env python3
"""
parse_unity_test_results.py
===========================
Parse Unity NUnit XML test result files and print a summary.
Also supports JSON summary files produced by the CI fallback runner.

If the XML/JSON file does not exist, reads the corresponding Editor.log
and outputs suspected root-cause lines.

Usage:
    python parse_unity_test_results.py <xml_or_json_path> [log_path]

Exit codes:
    0 = all tests passed (or XML parsed with 0 failures)
    1 = XML/JSON missing AND/OR tests failed
    2 = XML exists but could not be parsed (malformed)
"""

import sys
import os
import json
import xml.etree.ElementTree as ET


def _int_attr(element, name, default=0):
    """Safely get an integer attribute, handling both NUnit 2.x and 3.x naming."""
    val = element.get(name)
    if val is None:
        return default
    try:
        return int(val)
    except (ValueError, TypeError):
        return default


def parse_xml(xml_path):
    """Parse Unity NUnit XML and return (total, passed, failed, errors, skipped, failed_names)."""
    tree = ET.parse(xml_path)
    root = tree.getroot()

    total = 0
    passed = 0
    failed = 0
    errors = 0
    skipped = 0
    failed_names = []

    # NUnit 3.x: <test-run total="N" passed="N" failed="N" inconclusive="N" skipped="N">
    # NUnit 2.x: <test-results total="N" failures="N" errors="N" skipped="N">
    # We handle both by checking multiple attribute names.

    if root.tag in ("test-run", "test-results"):
        total = _int_attr(root, "total", 0)
        # NUnit 3.x uses "failed", NUnit 2.x uses "failures"
        failed = _int_attr(root, "failed", _int_attr(root, "failures", 0))
        # NUnit 3.x has no "errors" at top level; NUnit 2.x does
        errors = _int_attr(root, "errors", 0)
        skipped = _int_attr(root, "skipped", _int_attr(root, "inconclusive", 0))
        passed = _int_attr(root, "passed", total - failed - errors - skipped)

        # Collect failed test cases
        for case in root.iter("test-case"):
            result_attr = case.get("result", "")
            if result_attr in ("Failed", "Failure", "Error"):
                name = case.get("fullname", case.get("name", "unknown"))
                failed_names.append(name)
    else:
        # Try to find any test-suite or test-results child
        for child in root.iter():
            if child.tag in ("test-results", "test-run"):
                return parse_xml_from_element(child)
        raise ValueError(f"Unexpected root tag: {root.tag}")

    return total, passed, failed, errors, skipped, failed_names


def parse_xml_from_element(element):
    """Parse from a specific XML element (recursive fallback)."""
    total = _int_attr(element, "total", 0)
    failed = _int_attr(element, "failed", _int_attr(element, "failures", 0))
    errors = _int_attr(element, "errors", 0)
    skipped = _int_attr(element, "skipped", _int_attr(element, "inconclusive", 0))
    passed = _int_attr(element, "passed", total - failed - errors - skipped)
    failed_names = []
    for case in element.iter("test-case"):
        result_attr = case.get("result", "")
        if result_attr in ("Failed", "Failure", "Error"):
            name = case.get("fullname", case.get("name", "unknown"))
            failed_names.append(name)
    return total, passed, failed, errors, skipped, failed_names


def parse_json(json_path):
    """Parse a JSON summary file produced by the CI fallback runner.

    Expected schema:
    {
        "total": int,
        "passed": int,
        "failed": int,
        "errors": int,
        "skipped": int,
        "failed_names": [str, ...]
    }
    """
    with open(json_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    total = int(data.get("total", 0))
    passed = int(data.get("passed", 0))
    failed = int(data.get("failed", 0))
    errors = int(data.get("errors", 0))
    skipped = int(data.get("skipped", 0))
    failed_names = data.get("failed_names", [])
    if not isinstance(failed_names, list):
        failed_names = []

    return total, passed, failed, errors, skipped, failed_names


def scan_log(log_path):
    """Scan Editor.log for suspected root-cause lines."""
    if not log_path or not os.path.exists(log_path):
        return []

    patterns = [
        "batchmode quit successfully invoked",
        "error cs",
        "scripts have compiler errors",
        "testrunner",
        "teststarter",
        "nunit",
        "exception",
        "another unity instance",
        "multiple unity instances",
        "project open",
        "license",
        "return code",
    ]

    matches = []
    with open(log_path, "r", encoding="utf-8", errors="replace") as f:
        for line in f:
            line_lower = line.lower()
            for pat in patterns:
                if pat in line_lower:
                    matches.append(line.rstrip())
                    break

    return matches


def main():
    if len(sys.argv) < 2:
        print("Usage: python parse_unity_test_results.py <xml_or_json_path> [log_path]")
        return 2

    result_path = sys.argv[1]
    log_path = sys.argv[2] if len(sys.argv) > 2 else None

    # If no explicit log path, try to infer from result path
    if not log_path:
        result_dir = os.path.dirname(result_path)
        result_name = os.path.basename(result_path)
        if "editmode" in result_name.lower():
            log_path = os.path.join(result_dir, "editmode-editor.log")
        elif "playmode" in result_name.lower():
            log_path = os.path.join(result_dir, "playmode-editor.log")

    if not os.path.exists(result_path):
        print("=" * 60)
        print("TEST RESULT FILE NOT FOUND")
        print("=" * 60)
        print(f"  Path: {result_path}")
        print()

        log_matches = scan_log(log_path)
        if log_matches:
            print("Suspected root-cause lines from Editor.log:")
            print(f"  Log path: {log_path}")
            print("-" * 60)
            for line in log_matches[:50]:
                print(f"  {line}")
            print("-" * 60)
        else:
            print("No Editor.log found or no matching lines.")
            if log_path:
                print(f"  Log path: {log_path}")

        print()
        print("CONCLUSION: Test infrastructure blocked.")
        print("Unity batchmode -runTests did not generate result file.")
        print("See: Assets/Docs/TEST_RUNNER_RELIABILITY.md")
        return 1

    # Determine file type and parse accordingly
    is_json = result_path.lower().endswith(".json")

    print("=" * 60)
    print("TEST RESULTS SUMMARY")
    print("=" * 60)
    print(f"  File: {result_path}")
    print(f"  Type: {'JSON' if is_json else 'NUnit XML'}")
    print()

    try:
        if is_json:
            total, passed, failed, errors, skipped, failed_names = parse_json(result_path)
        else:
            total, passed, failed, errors, skipped, failed_names = parse_xml(result_path)
    except ET.ParseError as e:
        print(f"  ERROR: Failed to parse XML: {e}")
        return 2
    except (json.JSONDecodeError, KeyError) as e:
        print(f"  ERROR: Failed to parse JSON: {e}")
        return 2
    except Exception as e:
        print(f"  ERROR: Unexpected error parsing: {e}")
        return 2

    print(f"  Total:   {total}")
    print(f"  Passed:  {passed}")
    print(f"  Failed:  {failed}")
    print(f"  Errors:  {errors}")
    print(f"  Skipped: {skipped}")
    print()

    if failed_names:
        print("Failed test names:")
        for name in failed_names:
            print(f"  - {name}")
        print()

    if failed > 0 or errors > 0:
        print("RESULT: FAIL")
        return 1
    else:
        print("RESULT: PASS")
        return 0


if __name__ == "__main__":
    sys.exit(main())
