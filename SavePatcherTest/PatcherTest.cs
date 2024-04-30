using SavePatcher.Extractor;
using SavePatcher.Patcher;
using SevenZip;

namespace SavePatcherTest
{
    [TestClass]
    public class PatcherTest
    {
        [TestMethod]
        public void TestSavePatcher()
        {
            string[] testFiles =
            [
                "test1.txt", "test2.txt", "test\\test1.txt",
                "test\\test2.txt"
            ];
            foreach (string file in testFiles)
            {
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), "test_patcher");
                string dir = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(basePath, file)))!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Create(Path.Combine(basePath, file)).Close();
            }

            SevenZipBase.SetLibraryPath("7z.dll");
            var compressor = new SevenZipCompressor
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMode = CompressionMode.Create,
                CompressionLevel = CompressionLevel.Normal
            };
            compressor.CompressFiles("test_patcher\\test.7z", testFiles);
            if (!Directory.Exists("test_patcher\\test_dest"))
            {
                Directory.CreateDirectory("test_patcher\\test_dest");
            }

            if (!Directory.Exists("test_patcher\\test_dest2"))
            {
                Directory.CreateDirectory("test_patcher\\test_dest2");
            }

            if (!Directory.Exists("test_patcher\\test_dest_web"))
            {
                Directory.CreateDirectory("test_patcher\\test_dest_web");
            }

            // test patch all files
            IPatcher patcher = new SavePatcher.Patcher.SavePatcher(new ExtractorFactory())
            {
                FilePath = "test_patcher\\test.7z", DestinationPath = "test_patcher\\test_dest"
            };
            var patchResult = patcher.Patch();
            Assert.IsTrue(patchResult.Success);
            string[] input = Array.ConvertAll(testFiles,
                s => Path.Combine(Directory.GetCurrentDirectory(), "test_patcher", s));
            input = Array.ConvertAll(input, Path.GetFullPath);
            input = Array.ConvertAll(input,
                s => Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "test_patcher"), s));
            string[] destFiles = Directory.GetFiles("test_patcher\\test_dest", "*.*", SearchOption.AllDirectories);
            destFiles = Array.ConvertAll(destFiles, Path.GetFullPath);
            destFiles = Array.ConvertAll(destFiles,
                s => Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "test_patcher\\test_dest"), s));
            Array.Sort(input);
            Array.Sort(destFiles);
            Assert.IsTrue(input.SequenceEqual(destFiles));

            // test patch specified files
            patcher = new SavePatcher.Patcher.SavePatcher(new ExtractorFactory())
            {
                FilePath = "test_patcher\\test.7z",
                DestinationPath = "test_patcher\\test_dest2",
                PatchFiles = [testFiles[0], testFiles[2]]
            };
            patchResult = patcher.Patch();
            Assert.IsTrue(patchResult.Success);
            input = Array.ConvertAll([testFiles[0], testFiles[2]],
                s => Path.Combine(Directory.GetCurrentDirectory(), "test_patcher", s));
            input = Array.ConvertAll(input, Path.GetFullPath);
            input = Array.ConvertAll(input,
                s => Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "test_patcher"), s));
            destFiles = Directory.GetFiles("test_patcher\\test_dest2", "*.*", SearchOption.AllDirectories);
            destFiles = Array.ConvertAll(destFiles, Path.GetFullPath);
            destFiles = Array.ConvertAll(destFiles,
                s => Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "test_patcher\\test_dest2"),
                    s));
            Array.Sort(input);
            Array.Sort(destFiles);
            Assert.IsTrue(input.SequenceEqual(destFiles));

            // test url files
            patcher = new SavePatcher.Patcher.SavePatcher(new ExtractorFactory())
            {
                FilePath = "https://file-examples.com/storage/fe121d443b662e6a8a224ff/2017/02/zip_2MB.zip",
                DestinationPath = "test_patcher\\test_dest_web"
            };
            patchResult = patcher.Patch();
            Assert.IsTrue(patchResult.Success);
        }
    }
}