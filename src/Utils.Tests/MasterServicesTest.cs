using System;
using System.IO;
using AndanteTribe.Utils.MasterSample;
using AndanteTribe.Utils.MasterServices;
using NUnit.Framework;

namespace AndanteTribe.Utils.Tests
{
    public class MasterServicesTest
    {
        private const uint MaxLanguageCount = 2;
        private static readonly MasterSettings[] s_testCases =
        {
            new MasterSettings(
                GetMasterDirectoryPath(),
                MemoryDatabase.GetMetaDatabase(),
                static () => new DatabaseBuilder()
            )
            {
                MaxLanguageCount = MaxLanguageCount,
                LanguageIndex = 0,
            },
            new MasterSettings(
                GetMasterDirectoryPath(),
                MemoryDatabase.GetMetaDatabase(),
                static () => new DatabaseBuilder()
            )
            {
                MaxLanguageCount = MaxLanguageCount,
                LanguageIndex = 1,
            },
        };

        [Test]
        [TestCaseSource(nameof(s_testCases))]
        public void AllValidationTest(MasterSettings settings)
        {
            TestContext.Out.WriteLine(settings.InputDirectoryPath);
            var bin = MasterConverter.Load(settings);
            var table = new MemoryDatabase(bin, maxDegreeOfParallelism: Environment.ProcessorCount);

            var validateResult = table.Validate();
            Assert.That(validateResult.IsValidationFailed, Is.False, "マスターバリデーションに失敗しました。\n" + validateResult.FormatFailedResults());
        }

        private static string GetMasterDirectoryPath()
        {
#if UNITY_EDITOR
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../Utils.Tests/.master"));
#else
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".master");
#endif
            return Path.GetFullPath(path);
        }
    }
}