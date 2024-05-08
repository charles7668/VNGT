using SavePatcher.Configs;
using SavePatcher.Models;
using YamlDotNet.Serialization;

namespace SavePatcherTest
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void TestYamlConfigReaderWithSavePatcherConfig()
        {
            var testConfig1 = new SavePatcherConfig
            {
                ConfigName = "test1",
                DestinationPath = "destinationPath",
                FilePath = "filePath",
                PatchFiles = ["file1", "file2"],
                ZipPassword = "password"
            };
            ISerializer serializer = new SerializerBuilder()
                .Build();
            string serialized = serializer.Serialize(testConfig1);
            IConfigReader<SavePatcherConfig> reader = new YamlConfigReader<SavePatcherConfig>();
            Result<SavePatcherConfig> readResult = reader.Read(serialized);
            Assert.IsTrue(readResult.Success);
            SavePatcherConfig? readConfig1 = readResult.Value;
            Assert.IsFalse(readConfig1 == null);
            Assert.AreEqual(testConfig1.ConfigName, readConfig1.ConfigName);
            Assert.AreEqual(testConfig1.DestinationPath, readConfig1.DestinationPath);
            Assert.AreEqual(testConfig1.FilePath, readConfig1.FilePath);
            Assert.AreEqual(testConfig1.ZipPassword, readConfig1.ZipPassword);
            Assert.IsTrue(testConfig1.PatchFiles.SequenceEqual(readConfig1.PatchFiles));
        }
    }
}