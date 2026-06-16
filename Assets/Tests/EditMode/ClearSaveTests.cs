using LuoLuoTrip.Save;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class ClearSaveTests
    {
        private const string TestSaveFile = "test_clear_save";

        [Test]
        public void Delete_RemovesSaveFile()
        {
            var save = new GameSaveData();
            SaveService.Write(save, TestSaveFile);
            Assert.That(SaveService.SaveExists(TestSaveFile), Is.True);

            SaveService.Delete(TestSaveFile);
            Assert.That(SaveService.SaveExists(TestSaveFile), Is.False);
        }

        [Test]
        public void Delete_DoesNotThrow_WhenNoFileExists()
        {
            Assert.DoesNotThrow(() => SaveService.Delete("nonexistent_save_file"));
        }

        [Test]
        public void SaveExists_ReturnsFalse_WhenNoFile()
        {
            Assert.That(SaveService.SaveExists("definitely_nonexistent_file_xyz"), Is.False);
        }

        [Test]
        public void TryRead_ReturnsFalse_WhenNoFile()
        {
            var result = SaveService.TryRead("definitely_nonexistent_file_xyz", out var save);
            Assert.That(result, Is.False);
            Assert.That(save, Is.Null);
        }

        [Test]
        public void WriteAndRead_RoundTrip()
        {
            var original = new GameSaveData();
            original.commander.commanderLevel = 5;
            original.commander.experience = 500;

            SaveService.Write(original, TestSaveFile);

            try
            {
                var loaded = SaveService.TryRead(TestSaveFile, out var save);
                Assert.That(loaded, Is.True);
                Assert.That(save.commander.commanderLevel, Is.EqualTo(5));
                Assert.That(save.commander.experience, Is.EqualTo(500));
            }
            finally
            {
                SaveService.Delete(TestSaveFile);
            }
        }
    }
}
