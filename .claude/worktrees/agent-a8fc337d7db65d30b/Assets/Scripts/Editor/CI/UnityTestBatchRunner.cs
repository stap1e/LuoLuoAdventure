using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace LuoLuoTrip.Editor.CI
{
    /// <summary>
    /// Editor-only CI test runner that uses TestRunnerApi to launch tests
    /// and exits the Editor when complete. Intended as a fallback when
    /// batchmode -runTests does not produce XML (e.g. when -quit interferes).
    ///
    /// Usage from command line:
    ///   Unity.exe -batchmode -projectPath ... -executeMethod LuoLuoTrip.Editor.CI.UnityTestBatchRunner.RunEditModeTests -logFile ... -quit
    ///
    /// Optional env var: LUOLUO_TEST_FILTER = semicolon-separated test full names.
    ///
    /// The runner writes:
    ///   - JSON summary to TestResults/ci-editmode-summary.json (or ci-playmode-summary.json)
    ///
    /// All output paths are relative to the project root.
    /// </summary>
    public static class UnityTestBatchRunner
    {
        private const string EditModeJson = "TestResults/ci-editmode-summary.json";
        private const string PlayModeJson = "TestResults/ci-playmode-summary.json";

        public static void RunEditModeTests()
        {
            RunTests(TestMode.EditMode, EditModeJson);
        }

        public static void RunPlayModeTests()
        {
            RunTests(TestMode.PlayMode, PlayModeJson);
        }

        public static void RunAllTests()
        {
            RunTests(TestMode.EditMode | TestMode.PlayMode, EditModeJson);
        }

        private static void RunTests(TestMode mode, string jsonPath)
        {
            var absJsonPath = GetAbsolutePath(jsonPath);
            var filter = Environment.GetEnvironmentVariable("LUOLUO_TEST_FILTER") ?? "";

            EnsureDir(absJsonPath);

            Debug.Log($"[CI Runner] Starting tests mode={mode} filter='{filter}' json={absJsonPath}");

            var api = ScriptableObject.CreateInstance<TestRunnerApi>();

            var executionFilter = new Filter
            {
                testMode = mode,
                targetPlatform = BuildTarget.StandaloneWindows64
            };
            if (!string.IsNullOrEmpty(filter))
            {
                executionFilter.testNames = filter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var reporter = new ResultReporter(absJsonPath, null);
            reporter.SetCompletionAction(() =>
            {
                var summary = reporter.Summary;
                Debug.Log($"[CI Runner] Done total={summary.Total} passed={summary.Passed} failed={summary.Failed} errors={summary.Errors}");
                EditorApplication.Exit(summary.Failed > 0 || summary.Errors > 0 ? 2 : 0);
            });

            api.RegisterCallbacks(reporter);
            api.Execute(new ExecutionSettings(executionFilter));
        }

        private static string GetAbsolutePath(string relativePath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }

        private static void EnsureDir(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// ICallbacks implementation that collects results and writes JSON.
        /// </summary>
        private class ResultReporter : ICallbacks
        {
            private readonly string _jsonPath;
            private Action _onComplete;
            public TestSummary Summary { get; private set; }

            public ResultReporter(string jsonPath, Action onComplete)
            {
                _jsonPath = jsonPath;
                _onComplete = onComplete;
                Summary = new TestSummary();
            }

            public void SetCompletionAction(Action action)
            {
                _onComplete = action;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log($"[CI Runner] Run started");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Summary.Total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
                Summary.Passed = result.PassCount;
                Summary.Failed = result.FailCount;
                Summary.Errors = 0; // NUnit 3.x folds errors into failures
                Summary.Skipped = result.SkipCount + result.InconclusiveCount;

                CollectFailedNames(result, Summary.FailedNames);

                WriteJson();

                Debug.Log($"[CI Runner] Run finished: total={Summary.Total} passed={Summary.Passed} failed={Summary.Failed} errors={Summary.Errors} skipped={Summary.Skipped}");

                _onComplete?.Invoke();
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            private void CollectFailedNames(ITestResultAdaptor result, List<string> names)
            {
                if (result.Test != null && result.Test.IsSuite == false)
                {
                    if (result.ResultState == "Failed" || result.ResultState == "Error" ||
                        result.ResultState == "Cancelled")
                    {
                        names.Add(result.Test.FullName);
                    }
                }
                if (result.HasChildren)
                {
                    foreach (var child in result.Children)
                        CollectFailedNames(child, names);
                }
            }

            private void WriteJson()
            {
                var wrapper = new JsonWrapper(Summary);
                var json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(_jsonPath, json, Encoding.UTF8);
                Debug.Log($"[CI Runner] JSON written: {_jsonPath}");
            }
        }

        [Serializable]
        private class JsonWrapper
        {
            public int total;
            public int passed;
            public int failed;
            public int errors;
            public int skipped;
            public List<string> failed_names;

            public JsonWrapper(TestSummary s)
            {
                total = s.Total;
                passed = s.Passed;
                failed = s.Failed;
                errors = s.Errors;
                skipped = s.Skipped;
                failed_names = s.FailedNames;
            }
        }

        private class TestSummary
        {
            public int Total;
            public int Passed;
            public int Failed;
            public int Errors;
            public int Skipped;
            public readonly List<string> FailedNames = new List<string>();
        }
    }
}
