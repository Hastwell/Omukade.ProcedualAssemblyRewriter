using Omukade.AutoPAR.Rainier;

namespace Omukade.AutoPAR.Tests
{
    public class TestRainierFetcher : IDisposable
    {
        readonly string UPDATE_FILE_FNAME = Path.Join("Content", "sample-update.zip");
        const string UPDATE_FILE_MD5 = "0c2dd059463023ab001a059105a1fa85";

        public TestRainierFetcher()
        {
            RainierFetcher.UpdateZipFilename = Path.Combine(Environment.CurrentDirectory, "rainier-client.zip");
            RainierFetcher.UpdateDirectory = Path.Combine(Environment.CurrentDirectory, "rainier-client");
        }

        public void Dispose()
        {
            RainierFetcher.UpdateZipFilename = RainierFetcher.GetDefaultUpdateZipFilename();
            RainierFetcher.UpdateDirectory = RainierFetcher.GetDefaultUpdateDirectory();
        }


        [Fact]
        public async void CanFetchManifestsAtAll()
        {
            UpdaterManifest retrievedManifest = await RainierFetcher.GetUpdateManifestAsync();

            Assert.NotNull(retrievedManifest?.CompressedArtifact.Url);
            Assert.NotNull(retrievedManifest?.CompressedArtifact.MD5);
        }

        [Fact]
        public async void DownloadAndVerify_Success()
        {
            using FileStream sampleUpdateFile = new FileStream(UPDATE_FILE_FNAME, FileMode.Open, FileAccess.Read);

            UpdaterManifest manifest = new UpdaterManifest
            { CompressedArtifact = new UpdaterManifest.ArtifactDetails
                {
                    MD5 = UPDATE_FILE_MD5,
                    Url = null
                }
            };

            await RainierFetcher.DownloadUpdateFileMockable(manifest, sampleUpdateFile);
        }

        [Fact]
        public async void DownloadAndVerify_CorruptedDownload()
        {
            using FileStream sampleUpdateFile = new FileStream(UPDATE_FILE_FNAME, FileMode.Open, FileAccess.Read);

            UpdaterManifest manifest = new UpdaterManifest
            {
                CompressedArtifact = new UpdaterManifest.ArtifactDetails
                {
                    MD5 = "567f47d6f7b09ef0174c45a48faa0967", // the string "whatever it was you were looking for, this wasn't it"
                    Url = null
                }
            };

            await Assert.ThrowsAsync<InvalidDataException>(async () => await RainierFetcher.DownloadUpdateFileMockable(manifest, sampleUpdateFile));
        }

        [Fact]
        public void ExtractUpdateFreshSlate()
        {
            File.Copy(UPDATE_FILE_FNAME, RainierFetcher.UpdateZipFilename, overwrite: true);

            if(Directory.Exists(RainierFetcher.UpdateDirectory))
            {
                Directory.Delete(RainierFetcher.UpdateDirectory, recursive: true);
            }

            RainierFetcher.ExtractUpdateFile();
            Assert.True(File.Exists(Path.Combine(RainierFetcher.UpdateDirectory, "example.file.dll")), "Expected file didn't get created.");
        }

        [Fact]
        public void ExtractUpdateExistingUpdateDirectory()
        {
            File.Copy(UPDATE_FILE_FNAME, RainierFetcher.UpdateZipFilename, overwrite: true);

            string fileToDelete = Path.Combine(RainierFetcher.UpdateDirectory, "test-file.txt");
            Directory.CreateDirectory(RainierFetcher.UpdateDirectory);
            File.WriteAllText(fileToDelete, "test data that should be deleted");

            RainierFetcher.ExtractUpdateFile();
            Assert.True(File.Exists(Path.Combine(RainierFetcher.UpdateDirectory, "example.file.dll")), "Expected file didn't get created.");
            Assert.False(File.Exists(fileToDelete), "File expected to be deleted during update extraction is still present.");
        }

        [Fact]
        public async void GetReleaseNote()
        {
            UpdaterManifest retrievedManifest = await RainierFetcher.GetUpdateManifestAsync();
            LocalizedReleaseNote releaseNote = await RainierFetcher.GetLocalizedReleaseNoteAsync(retrievedManifest);
        }

        [Theory]
        [InlineData("1-2-2011", 2011, 01, 02)]
        [InlineData("01-02-2011", 2011, 01, 02)]
        [InlineData("11-22-2011", 2011, 11, 22)]
        [InlineData("2-28-2023", 2023, 02, 28)]
        public void ReleaseNoteParseDate(string dateStringRaw, int expectedYear, int expectedMonth, int expectedDay)
        {
            DateTime expectedDate = new DateTime(expectedYear, expectedMonth, expectedDay);
            LocalizedReleaseNote releaseNote = new LocalizedReleaseNote
            {
                DateRaw = dateStringRaw
            };

            Assert.Equal(expectedDate, releaseNote.DateParsed);
        }

        [Theory]
        [InlineData("Version 1.3.30 (165830)", "01-02-2023", "1.3.30.165830.20230102-est")]
        public void TransformReleaseVersionString(string versionStringRaw, string dateStringRaw, string expectedVersionString)
        {
            LocalizedReleaseNote releaseNote = new LocalizedReleaseNote
            {
                DateRaw = dateStringRaw,
                Version = versionStringRaw
            };

            Assert.Equal(expectedVersionString, releaseNote.ComputedBuildVersion);
        }
    }
}