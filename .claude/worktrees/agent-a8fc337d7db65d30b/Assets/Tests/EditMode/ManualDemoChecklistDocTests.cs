using System.IO;
using NUnit.Framework;

namespace LuoLuoTrip.Tests.EditMode
{
    public class ManualDemoChecklistDocTests
    {
        private const string ChecklistPath = "Assets/Docs/MANUAL_DEMO_VALIDATION_CHECKLIST.md";

        [Test]
        public void ManualDemoChecklist_Exists()
        {
            Assert.That(File.Exists(ChecklistPath), Is.True, "Manual demo validation checklist must be checked in.");
        }

        [Test]
        public void ManualDemoChecklist_CoversRequiredManualPassItems()
        {
            var text = File.ReadAllText(ChecklistPath);

            Assert.That(text, Does.Contain("F7"));
            Assert.That(text, Does.Contain("F8"));
            Assert.That(text, Does.Contain("F5"));
            Assert.That(text, Does.Contain("F9"));
            Assert.That(text, Does.Contain("F10"));
            Assert.That(text, Does.Contain("E with no target"));
            Assert.That(text, Does.Contain("EnergyNode priority"));
            Assert.That(text, Does.Contain("Expected pass criteria"));
        }
    }
}
