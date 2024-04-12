using Omukade.AutoPAR;
using Omukade.AutoPAR.Rainier;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            UpdaterManifest updateManifest = RainierFetcher.GetUpdateManifestAsync().Result;
            string targetDirectory = RainierFetcher.UpdateDirectory.TrimEnd(Path.DirectorySeparatorChar) + "_PAR";

            if (RainierFetcher.DoesNeedUpdate(updateManifest))
            {
                LocalizedReleaseNote releaseNote = RainierFetcher.GetLocalizedReleaseNoteAsync(updateManifest).Result;
                Console.WriteLine($"Downloading Rainier update for {releaseNote.Version}...");
                RainierFetcher.DownloadUpdateFile(updateManifest).Wait();
                Console.WriteLine($"Extracting update...");
                RainierFetcher.ExtractUpdateFile();

                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory, true);
                }
                CreateParAssemblies(targetDirectory);
            }
            else
            {
                Console.WriteLine("Current Rainier version is up-to-date");
                if(!Directory.Exists(targetDirectory))
                {
                    CreateParAssemblies(targetDirectory);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("AutoPAR exception while checking/fetching Rainier. Breaking the build...");
            Console.Error.WriteLine($"{ex.GetType().FullName} - {ex.Message}\n{ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    static void CreateParAssemblies(string targetDirectory)
    {
        Console.WriteLine($"Creating PAR assemblies...");

        HashSet<string> filesToSkip = new HashSet<string>(new string[] { "Newtonsoft.Json.dll", "Accessibility.dll", "mscorlib.dll", "System.Web.dll", "System.Xml.dll", "System.dll", "System.Data.dll", "System.Core.dll" });

        ParCore parCore = new ParCore();
        parCore.CecilProcessors.Add(RainierSpecificPatches.MakeGameStateCloneVirtual);
        parCore.CecilProcessors.Add(RainierSpecificPatches.AddJsonIgnoreAttribute_SetKnockoutAtFullHealthByDamageMetaData);

        Directory.CreateDirectory(targetDirectory);

        IEnumerable<string> targetFiles = Directory.GetFiles(RainierFetcher.UpdateDirectory, "*.dll")
            .Where(file => !filesToSkip.Contains(Path.GetFileName(file)));

        foreach (string sourceFile in targetFiles)
        {
            ProcessFile(parCore, sourceFile, targetDirectory);
        }
    }
    static void ProcessFile(ParCore parCore, string sourceFile, string targetDirectory)
    {
        try
        {
            string destinationFile = Path.Combine(targetDirectory, Path.GetFileName(sourceFile));
            parCore.ProcessAssemblyAndSaveToFile(sourceFile, destinationFile);
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Error while processing " + Path.GetFileName(sourceFile));
            throw;
        }
    }
}