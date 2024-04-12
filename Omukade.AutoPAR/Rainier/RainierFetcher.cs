/*************************************************************************
* Omukade Manual Procedual Assembly Rewriter ("ManualPAR")
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.AutoPAR.Rainier
{
    /// <summary>
    /// Helper class to fetch and manage PTCGL "Rainier" game files.
    /// </summary>
    public static class RainierFetcher
    {
        /// <summary>
        /// The filename that game client data is saved to. If this filename is changed in an existing install, update files will need to be redownloaded.
        /// </summary>
        public static string UpdateZipFilename = GetDefaultUpdateZipFilename();

        [Obsolete("Use " + nameof(UpdateDirectory))]
        public static string ComputedUpdateDirectory => UpdateDirectory;

        /// <summary>
        /// The directory updates will be downloaded to, and where game binaries can be read from for AutoPAR. Defaults to RainierSharedDataHelper.GetSharedDataDirectory() / rainier-client
        /// </summary>
        public static string UpdateDirectory = GetDefaultUpdateDirectory();

        /// <summary>
        /// The initial computed value of <see cref="UpdateDirectory"/>. Typically not used unless the field is changed for some reason and it needs to be changed back to the initial default.
        /// </summary>
        public static string GetDefaultUpdateDirectory() => Path.Combine(RainierSharedDataHelper.GetSharedDataDirectory(), "rainier-client");

        /// <summary>
        /// The initial computed value of <see cref="UpdateZipFilename"/>. Typically not used unless the field is changed for some reason and it needs to be changed back to the initial default.
        /// </summary>
        public static string GetDefaultUpdateZipFilename() => Path.Combine(RainierSharedDataHelper.GetSharedDataDirectory(), "rainier-client.zip");

        /// <summary>
        /// Fetches update manifest data from the PTCGL CDN, containing eg the update URL + hash.
        /// </summary>
        /// <returns></returns>
        public async static Task<UpdaterManifest> GetUpdateManifestAsync()
        {
            return await DownloadJsonAsync<UpdaterManifest>("https://cdn.studio-prod.pokemon.com/rainier/updater/StandaloneWindows64/Manifest.json");
        }

        public async static Task<LocalizedReleaseNote> GetLocalizedReleaseNoteAsync(UpdaterManifest manifest, string locale = UpdaterManifest.RELEASE_NOTE_LANGUAGE_EN)
        {
            if(!manifest.ReleaseNotes.TryGetValue(locale, out var releaseNoteUrl))
            {
                throw new ArgumentOutOfRangeException(nameof(locale), locale, "Release notes not available for this locale. Use one of the UpdaterManifest.RELEASE_NOTE_LANGUAGE_xxx constants.");
            }

            return await DownloadJsonAsync<LocalizedReleaseNote>(releaseNoteUrl);
        }

        private async static Task<T> DownloadJsonAsync<T>(string url) where T : new()
        {
            using HttpClient client = new HttpClient();
            string dataRaw = await client.GetStringAsync(url);
            T dataParsed = JsonConvert.DeserializeObject<T>(dataRaw);
            return dataParsed;
        }

        /// <summary>
        /// Determines if based on the current manifest vs a previously downloaded file at <see cref="UpdateZipFilename"/>, a new update file needs to be downloaded.
        /// </summary>
        /// <param name="manifest">The <see cref="UpdaterManifest"/> retreived from CDN via <see cref="GetUpdateManifestAsync"/></param>
        /// <returns>True if the update file has been changed since previously downloaded, or no previous cached copy exists. False if no download is needed.</returns>
        /// <remarks>If an MD5 file exists alongside the ZIP file, it is checked for the hash rather than hashing the entire update file.</remarks>
        public static bool DoesNeedUpdate(UpdaterManifest manifest)
        {
            string md5Filename = UpdateZipFilename + ".md5";

            if(!File.Exists(UpdateZipFilename))
            {
                // If the ZIP file has been deleted, but the MD5 hasn't, assume this is user error and delete the MD5 file as well so it doesn't cause problems later.
                if(File.Exists(md5Filename))
                {
                    File.Delete(md5Filename);
                }

                return true;
            }

            string existingUpdateFileHash;

            if(File.Exists(md5Filename))
            {
                existingUpdateFileHash = File.ReadAllText(md5Filename);
            }
            else
            {
                existingUpdateFileHash = ComputeHashForUpdateFile();

                // No MD5 file exists with this file yet; create one so it can be referred to later.
                File.WriteAllText(md5Filename, existingUpdateFileHash);
            }

            return existingUpdateFileHash != manifest.CompressedArtifact.MD5;
        }

        /// <summary>
        /// Downloads the current version of the PTCGL "Rainier" client.
        /// </summary>
        /// <param name="manifest">The <see cref="UpdaterManifest"/> retreived from CDN via <see cref="GetUpdateManifestAsync"/></param>
        /// <exception cref="InvalidDataException">The hash of the downloaded file does not match the hash of the manifest.</exception>
        public static async Task DownloadUpdateFile(UpdaterManifest manifest)
        {
            using HttpClient client = new HttpClient();
            using Stream responseStream = await client.GetStreamAsync(manifest.CompressedArtifact.Url);

            await DownloadUpdateFileMockable(manifest, responseStream).ConfigureAwait(false);
        }

        /// <summary>
        /// Internal Use Only; please use <see cref="DownloadUpdateFile(UpdaterManifest)"/>. Downloads the update from an abritary stream (whether an HTTP response payload or raw file), saves it, and validates its hash.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="responseStream"></param>
        /// <exception cref="InvalidDataException"></exception>
        internal static async Task DownloadUpdateFileMockable(UpdaterManifest manifest, Stream responseStream)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UpdateZipFilename));

            string hashFilename = UpdateZipFilename + ".md5";
            if(File.Exists(hashFilename))
            {
                File.Delete(hashFilename);
            }

            using FileStream updateFileStream = new FileStream(UpdateZipFilename, FileMode.Create, FileAccess.Write, FileShare.None);
            await responseStream.CopyToAsync(updateFileStream).ConfigureAwait(false);
            updateFileStream.Close();

            // Verify the downloaded file
            string downloadedFileHash = ComputeHashForUpdateFile();
            if (downloadedFileHash != manifest.CompressedArtifact.MD5)
            {
                throw new InvalidDataException($"Downloaded update is corrupted (expected hash {manifest.CompressedArtifact.MD5}; actual {downloadedFileHash})");
            }

            File.WriteAllText(UpdateZipFilename + ".md5", downloadedFileHash);
        }

        /// <summary>
        /// Extracts the contents of the update file into the current directory.
        /// </summary>
        /// <param name="deleteExistingUpdateFolder">If an update folder already exists, optionally delete it and all of its contents.</param>
        /// <exception cref="FileNotFoundException">The update ZIP was not found.</exception>
        public static void ExtractUpdateFile(bool deleteExistingUpdateFolder = true)
        {
            const string MANAGED_FOLDER = "Pokemon TCG Live_Data/Managed/";

            if (!File.Exists(UpdateZipFilename)) throw new FileNotFoundException($"The update file was not found - {UpdateZipFilename}");

            if(deleteExistingUpdateFolder && Directory.Exists(RainierFetcher.UpdateDirectory))
            {
                Directory.Delete(RainierFetcher.UpdateDirectory, recursive: true);
            }

            using ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(UpdateZipFilename);
            foreach(ZipEntry entry in zipFile)
            {
                if (!entry.IsFile) continue;
                if (!entry.Name.StartsWith(MANAGED_FOLDER)) continue;

                string normalizedPath = Path.PathSeparator == '/' ? entry.Name : entry.Name.Replace('/', Path.PathSeparator);
                string directoryPath = Path.Combine(RainierFetcher.UpdateDirectory, Path.GetDirectoryName(normalizedPath.AsSpan().Slice(MANAGED_FOLDER.Length)).ToString());
                Directory.CreateDirectory(directoryPath);

                using Stream fileContentsStream = zipFile.GetInputStream(entry);
                using FileStream destFileStream = new FileStream(Path.Combine(directoryPath, Path.GetFileName(entry.Name)), FileMode.Create, FileAccess.Write, FileShare.None);
                fileContentsStream.CopyTo(destFileStream);
            }
        }

        static string ComputeHashForUpdateFile()
        {
            byte[] finalHash;
            using (FileStream existingUpdateFile = new FileStream(UpdateZipFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using MD5 hashFunction = MD5.Create();
                finalHash = hashFunction.ComputeHash(existingUpdateFile);
            }

            Span<char> finalHashChars = stackalloc char[32];
            for (int i = 0; i < 16; i++)
            {
                WriteByteToSpan(finalHashChars.Slice(i * 2, 2), finalHash[i]);
            }

            return new string(finalHashChars);
        }

        static char[] hexChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        static void WriteByteToSpan(Span<char> writeBuffer, byte byteToWrite)
        {
            int high = ((byteToWrite & 0xf0) >> 4);
            int low = (byteToWrite & 0x0f);

            writeBuffer[0] = hexChars[high];
            writeBuffer[1] = hexChars[low];
        }
    }
}
