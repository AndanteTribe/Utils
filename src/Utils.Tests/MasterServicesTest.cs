using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Linq;
using AndanteTribe.Utils.GameServices;
using AndanteTribe.Utils.MasterSample;
using AndanteTribe.Utils.MasterSample.Enums;
using AndanteTribe.Utils.MasterSample.Units;
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
                LanguageIndex = (uint)Language.Japanese,
            },
            new MasterSettings(
                GetMasterDirectoryPath(),
                MemoryDatabase.GetMetaDatabase(),
                static () => new DatabaseBuilder()
            )
            {
                MaxLanguageCount = MaxLanguageCount,
                LanguageIndex = (uint)Language.English,
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

        [Test]
        [TestCaseSource(nameof(s_testCases))]
        public void Build_WritesPlainFile_EqualsLoadBytes(MasterSettings settings)
        {
            // Arrange
            var expected = MasterConverter.Load(settings);
            var tempPath = Path.Combine(Path.GetTempPath(), $"master_{(Language)settings.LanguageIndex}.bin");

            try
            {
                // Act
                MasterConverter.Build(settings, tempPath);

                // Assert
                Assert.That(File.Exists(tempPath), Is.True, "出力ファイルが生成されていません。");
                var actual = File.ReadAllBytes(tempPath);
                Assert.That(actual, Is.EqualTo(expected), "Build による平文出力が Load() のバイナリと一致しません。");
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        [TestCaseSource(nameof(s_testCases))]
        public void Build_WithAes_WritesEncryptedFile_DecryptsToLoadBytes(MasterSettings settings)
        {
            // Arrange
            var expected = MasterConverter.Load(settings);
            var tempPath = Path.Combine(Path.GetTempPath(), $"master_enc_{(Language)settings.LanguageIndex}.bin");

            using var aes = Aes.Create();
            aes.Key = new byte[16]
            {
                0x00, 0x01, 0x02, 0x03,
                0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B,
                0x0C, 0x0D, 0x0E, 0x0F,
            };
            aes.IV = new byte[16]
            {
                0x0F, 0x0E, 0x0D, 0x0C,
                0x0B, 0x0A, 0x09, 0x08,
                0x07, 0x06, 0x05, 0x04,
                0x03, 0x02, 0x01, 0x00,
            };
            aes.Mode = CipherMode.CBC;
            try
            {
                // Act
                MasterConverter.Build(settings, tempPath, aes);

                // Assert file exists
                Assert.That(File.Exists(tempPath), Is.True, "AES での出力ファイルが生成されていません。");

                // Read encrypted bytes and decrypt using the same AES instance
                var encrypted = File.ReadAllBytes(tempPath);
                byte[] decrypted;
                using (var ms = new MemoryStream(encrypted))
                using (var crypto = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var outMs = new MemoryStream())
                {
                    crypto.CopyTo(outMs);
                    decrypted = outMs.ToArray();
                }

                Assert.That(decrypted, Is.EqualTo(expected), "AES 出力を復号したバイナリが Load() のバイナリと一致しません。");
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        [TestCaseSource(nameof(s_testCases))]
        public void MasterSample_CsvFiles_NotEmptyAndContainData(MasterSettings settings)
        {
            var expected = MasterConverter.Load(settings);
            var table = new MemoryDatabase(expected, maxDegreeOfParallelism: Environment.ProcessorCount);

            // enemy
            var enemyTable = table.EnemyMasterEntityTable;

            var e1 = enemyTable.FindById((BattleField.Air, 1));
            Assert.That(e1.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ゲイザー" : "Gazer"));
            Assert.That(e1.Property, Is.EqualTo(Nature.Chaos | Nature.Neutral));
            Assert.That(e1.Status, Is.EqualTo(new BasicStatus(1, 1, 1, 100, 1, 1, 0)));
            Assert.That(e1.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Normal, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal)));

            var e2 = enemyTable.FindById((BattleField.Ground, 1));
            Assert.That(e2.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "人間" : "Human"));
            Assert.That(e2.Property, Is.EqualTo(Nature.Order | Nature.Neutral));
            Assert.That(e2.Status, Is.EqualTo(new BasicStatus(1, 1, 1, 100, 1, 1, 0)));
            Assert.That(e2.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Normal, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal)));

            var e10 = enemyTable.FindById((BattleField.Ground, 10));
            Assert.That(e10.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル(全体)" : "Hecatoncheires(All)"));
            Assert.That(e10.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e10.Status, Is.EqualTo(new BasicStatus(5000, 100, 20, 50, 1, 0, 0)));
            Assert.That(e10.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Normal, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal)));

            var e11 = enemyTable.FindById((BattleField.Ground, 11));
            Assert.That(e11.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル（頭）" : "Hecatoncheires (Head)"));
            Assert.That(e11.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e11.Status, Is.EqualTo(new BasicStatus(1000, 0, 0, 50, 0, 0, 0)));
            Assert.That(e11.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Resist, Compatibility.Weak, Compatibility.Normal, Compatibility.Normal, Compatibility.Normal)));

            var e12 = enemyTable.FindById((BattleField.Ground, 12));
            Assert.That(e12.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル（胴体）" : "Hecatoncheires (Body)"));
            Assert.That(e12.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e12.Status, Is.EqualTo(new BasicStatus(1000, 0, 0, 50, 0, 0, 0)));
            Assert.That(e12.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Resist, Compatibility.Resist, Compatibility.Normal, Compatibility.Normal, Compatibility.Nullify)));

            var e13 = enemyTable.FindById((BattleField.Ground, 13));
            Assert.That(e13.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル（右腕）" : "Hecatoncheires (Right Arm)"));
            Assert.That(e13.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e13.Status, Is.EqualTo(new BasicStatus(1000, 0, 0, 50, 0, 0, 0)));
            Assert.That(e13.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Normal, Compatibility.Weak, Compatibility.Normal, Compatibility.Resist, Compatibility.Nullify)));

            var e14 = enemyTable.FindById((BattleField.Ground, 14));
            Assert.That(e14.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル（左腕）" : "Hecatoncheires (Left Arm)"));
            Assert.That(e14.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e14.Status, Is.EqualTo(new BasicStatus(1000, 0, 0, 50, 0, 0, 0)));
            Assert.That(e14.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Normal, Compatibility.Weak, Compatibility.Normal, Compatibility.Resist, Compatibility.Nullify)));

            var e15 = enemyTable.FindById((BattleField.Ground, 15));
            Assert.That(e15.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル（右脚）" : "Hecatoncheires (Right Foot)"));
            Assert.That(e15.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e15.Status, Is.EqualTo(new BasicStatus(1000, 0, 0, 50, 0, 0, 0)));
            Assert.That(e15.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Weak, Compatibility.Normal, Compatibility.Resist, Compatibility.Normal, Compatibility.Normal)));

            var e16 = enemyTable.FindById((BattleField.Ground, 16));
            Assert.That(e16.Species, Is.EqualTo(settings.LanguageIndex == (uint)Language.Japanese ? "ヘカトンケイル（左脚）" : "Hecatoncheires (Left Foot)"));
            Assert.That(e16.Property, Is.EqualTo(Nature.Chaos | Nature.Good));
            Assert.That(e16.Status, Is.EqualTo(new BasicStatus(1000, 0, 0, 50, 0, 0, 0)));
            Assert.That(e16.Compatibilities, Is.EqualTo(new CompatibilityGroup(Compatibility.Weak, Compatibility.Normal, Compatibility.Resist, Compatibility.Normal, Compatibility.Normal)));

            var enemyGroundTable = table.GroundEnemyMasterEntityTable;
            var eg1 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 1));
            Assert.That(eg1.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 1)));
            Assert.That(eg1.IdleChaseDistance, Is.EqualTo(50));
            Assert.That(eg1.BattleChaseDistance, Is.EqualTo(100));

            var eg10 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 10));
            Assert.That(eg10.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 10)));
            Assert.That(eg10.IdleChaseDistance, Is.EqualTo(50));
            Assert.That(eg10.BattleChaseDistance, Is.EqualTo(100));

            var eg11 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 11));
            Assert.That(eg11.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 11)));
            Assert.That(eg11.IdleChaseDistance, Is.EqualTo(0));
            Assert.That(eg11.BattleChaseDistance, Is.EqualTo(0));

            var eg12 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 12));
            Assert.That(eg12.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 12)));
            Assert.That(eg12.IdleChaseDistance, Is.EqualTo(0));
            Assert.That(eg12.BattleChaseDistance, Is.EqualTo(0));

            var eg13 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 13));
            Assert.That(eg13.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 13)));
            Assert.That(eg13.IdleChaseDistance, Is.EqualTo(0));
            Assert.That(eg13.BattleChaseDistance, Is.EqualTo(0));

            var eg14 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 14));
            Assert.That(eg14.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 14)));
            Assert.That(eg14.IdleChaseDistance, Is.EqualTo(0));
            Assert.That(eg14.BattleChaseDistance, Is.EqualTo(0));

            var eg15 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 15));
            Assert.That(eg15.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 15)));
            Assert.That(eg15.IdleChaseDistance, Is.EqualTo(0));
            Assert.That(eg15.BattleChaseDistance, Is.EqualTo(0));

            var eg16 = enemyGroundTable.FindById((GroundEnemyCategory.Common, 16));
            Assert.That(eg16.EnemyId, Is.EqualTo(new MasterId<BattleField>(BattleField.Ground, 16)));
            Assert.That(eg16.IdleChaseDistance, Is.EqualTo(0));
            Assert.That(eg16.BattleChaseDistance, Is.EqualTo(0));

            // text テーブルの検証
            var textTable = table.TextMasterEntityTable;
            switch ((Language)settings.LanguageIndex)
            {
                case Language.Japanese:
                    var tj1 = textTable.FindById((TextCategory.Toast, 9997));
                    Assert.That(tj1.Format.ToString(), Is.EqualTo("こんにちは！"));
                    var tj2 = textTable.FindById((TextCategory.Toast, 9998));
                    Assert.That(tj2.Format.ToString(), Is.EqualTo("{0}が{1}にアイテム{2}を{3}個渡しました。"));
                    var tj3 = textTable.FindById((TextCategory.Toast, 9999));
                    Assert.That(tj3.Format.ToString(), Is.EqualTo("魔法石 {0:N0}個"));
                    break;
                case Language.English:
                    var te1 = textTable.FindById((TextCategory.Toast, 9997));
                    Assert.That(te1.Format.ToString(), Is.EqualTo("Hello!"));
                    var te2 = textTable.FindById((TextCategory.Toast, 9998));
                    Assert.That(te2.Format.ToString(), Is.EqualTo("{0} gave {3} {2} to {1}."));
                    var te3 = textTable.FindById((TextCategory.Toast, 9999));
                    Assert.That(te3.Format.ToString(), Is.EqualTo("Magic Store {0:N0}"));
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

