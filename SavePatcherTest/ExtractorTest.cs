using Ionic.Zip;
using SavePatcher.Extractor;
using SavePatcher.Models;
using SevenZip;
using SevenZipExtractor = SavePatcher.Extractor.SevenZipExtractor;

namespace SavePatcherTest
{
    [TestClass]
    public class ExtractorTest
    {
        [TestMethod]
        public void TestZipExtractorWithoutPassword()
        {
            // create zip
            string[] testFiles = ["test1.txt", "test2.txt", "test\\test1.txt", "test\\test2.txt"];
            foreach (string file in testFiles)
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(file))!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Create(file).Close();
            }

            var zip = new ZipFile();
            zip.AddFiles(testFiles);
            zip.Save("test.zip");
            IExtractor extractor = new ZipExtractor();
            Task<Result<string>> testTask = extractor.ExtractAsync("test.zip", new ExtractOption());
            testTask.Wait();
            Result<string> resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            string[] fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            Assert.AreEqual(testFiles.Length, fileInfos.Length);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Array.Sort(fileInfos);
            Array.Sort(testFiles);
            Assert.IsTrue(fileInfos.SequenceEqual(testFiles));
            // test extract specific files
            string[] originFile = [testFiles[0], testFiles[2]];
            testTask = extractor.ExtractAsync("test.zip",
                new ExtractOption { SpecificFiles = originFile });
            testTask.Wait();
            resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Assert.AreEqual(originFile.Length, fileInfos.Length);
            Array.Sort(fileInfos);
            Array.Sort(originFile);
            Assert.IsTrue(fileInfos.SequenceEqual(originFile));
        }

        [TestMethod]
        public void TestZipExtractorWithPassword()
        {
            // create zip
            string[] testFiles =
                ["test1_password.txt", "test2_password.txt", "test\\test1_password.txt", "test\\test2_password.txt"];
            foreach (string file in testFiles)
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(file))!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Create(file).Close();
            }

            var zip = new ZipFile { Password = "test_password" };
            zip.AddFiles(testFiles);
            zip.Save("test_password.zip");
            IExtractor extractor = new ZipExtractor();
            Task<Result<string>> testTask =
                extractor.ExtractAsync("test_password.zip", new ExtractOption { Password = "test_password" });
            testTask.Wait();
            Result<string> resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            string[] fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            Assert.AreEqual(testFiles.Length, fileInfos.Length);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Array.Sort(fileInfos);
            Array.Sort(testFiles);
            Assert.IsTrue(fileInfos.SequenceEqual(testFiles));
            // test extract specific files
            string[] originFile = [testFiles[0], testFiles[2]];
            testTask = extractor.ExtractAsync("test_password.zip",
                new ExtractOption { SpecificFiles = originFile, Password = "test_password" });
            testTask.Wait();
            resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Assert.AreEqual(originFile.Length, fileInfos.Length);
            Array.Sort(fileInfos);
            Array.Sort(originFile);
            Assert.IsTrue(fileInfos.SequenceEqual(originFile));
        }

        [TestMethod]
        public void Test7ZExtractorWithoutPassword()
        {
            string[] testFiles =
                ["test1.txt", "test2.txt", "test\\test1.txt", "test\\test2.txt"];
            foreach (string file in testFiles)
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(file))!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Create(file).Close();
            }

            SevenZipBase.SetLibraryPath("7z.dll");
            var compressor = new SevenZipCompressor
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMode = CompressionMode.Create,
                CompressionLevel = CompressionLevel.Normal
            };
            compressor.CompressFiles("test.7z", testFiles);
            IExtractor extractor = new SevenZipExtractor();
            Task<Result<string>> testTask = extractor.ExtractAsync("test.7z", new ExtractOption());
            testTask.Wait();
            Result<string> resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            string[] fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            Assert.AreEqual(testFiles.Length, fileInfos.Length);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Array.Sort(fileInfos);
            Array.Sort(testFiles);
            Assert.IsTrue(fileInfos.SequenceEqual(testFiles));
            // test extract specific files
            string[] originFile = [testFiles[0], testFiles[2]];
            testTask = extractor.ExtractAsync("test.7z",
                new ExtractOption { SpecificFiles = originFile, Password = "test" });
            testTask.Wait();
            resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Assert.AreEqual(originFile.Length, fileInfos.Length);
            Array.Sort(fileInfos);
            Array.Sort(originFile);
            Assert.IsTrue(fileInfos.SequenceEqual(originFile));
        }

        [TestMethod]
        public void Test7ZExtractorWithPassword()
        {
            string[] testFiles =
                ["test1_password.txt", "test2_password.txt", "test\\test1_password.txt", "test\\test2_password.txt"];
            foreach (string file in testFiles)
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(file))!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Create(file).Close();
            }

            SevenZipBase.SetLibraryPath("7z.dll");
            var compressor = new SevenZipCompressor
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMode = CompressionMode.Create,
                CompressionLevel = CompressionLevel.Normal,
                EncryptHeaders = true
            };
            compressor.CompressFilesEncrypted("test_password.7z", "test_password", testFiles);
            IExtractor extractor = new SevenZipExtractor();
            Task<Result<string>> testTask =
                extractor.ExtractAsync("test_password.7z", new ExtractOption { Password = "test_password" });
            testTask.Wait();
            Result<string> resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            string[] fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            Assert.AreEqual(testFiles.Length, fileInfos.Length);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Array.Sort(fileInfos);
            Array.Sort(testFiles);
            Assert.IsTrue(fileInfos.SequenceEqual(testFiles));
            // test extract specific files
            string[] originFile = [testFiles[0], testFiles[2]];
            testTask = extractor.ExtractAsync("test_password.7z",
                new ExtractOption { SpecificFiles = originFile, Password = "test_password" });
            testTask.Wait();
            resultPath = testTask.Result;
            Assert.IsTrue(resultPath.Success);
            Assert.IsNotNull(resultPath.Value);
            fileInfos = Directory.GetFiles(resultPath.Value, "*.*", SearchOption.AllDirectories);
            fileInfos = Array.ConvertAll(fileInfos, input => Path.GetRelativePath(resultPath.Value, input));
            Assert.AreEqual(originFile.Length, fileInfos.Length);
            Array.Sort(fileInfos);
            Array.Sort(originFile);
            Assert.IsTrue(fileInfos.SequenceEqual(originFile));
        }
    }
}