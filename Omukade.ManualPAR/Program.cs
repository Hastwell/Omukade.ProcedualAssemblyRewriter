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

using Omukade.AutoPAR;

internal class Program
{
    static bool enableDryRun = false;
    static ParCore parCore = new ParCore();

    private static void Main(string[] args)
    {
        HashSet<string> filesToSkip = new HashSet<string>(new string[] { "Newtonsoft.Json.dll", "Accessibility.dll", "mscorlib.dll", "System.Web.dll", "System.Xml.dll", "System.dll", "System.Data.dll", "System.Core.dll" });

        string? sourceDirectory = args.FirstOrDefault(arg => !arg.StartsWith("-"));
        enableDryRun = args.Contains("--dry-run");

        if(sourceDirectory == null)
        {
            Console.Error.WriteLine("Target directory not specified; cannot perform processing.");
            return;
        }

        string targetDirectory = sourceDirectory.TrimEnd(Path.DirectorySeparatorChar) + "_PAR";
        Directory.CreateDirectory(targetDirectory);

        IEnumerable<string> targetFiles = Directory.GetFiles(sourceDirectory, "*.dll")
            .Where(file => !filesToSkip.Contains(Path.GetFileName(file)));

        string getDestFilename(string sourceFilename) => Path.Combine(targetDirectory, Path.GetFileName(sourceFilename));

        DateTime startTime = DateTime.UtcNow;
        if (args.Contains("--parallel"))
        {
            targetFiles.AsParallel().ForAll(targetFile => ProcessWorkOnFile(targetFile, getDestFilename(targetFile)));
        }
        else
        {
            foreach (string targetFile in targetFiles)
            {
                ProcessWorkOnFile(targetFile, getDestFilename(targetFile));
            }
        }
        TimeSpan elapsedTime = DateTime.UtcNow - startTime;
        Console.WriteLine($"Processed {targetFiles.Count()} assemblies in {elapsedTime.TotalSeconds:F} sec");
    }

    static void ProcessWorkOnFile(string sourceFile, string destinationFile)
    {
        try
        {
            if(enableDryRun)
            {
                // Intentionally process the assembly, then dispose of the results as per dry-run
                parCore.ProcessAssembly(sourceFile).Dispose();
            }
            else
            {
                parCore.ProcessAssemblyAndSaveToFile(sourceFile, destinationFile);
            }
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Error processing {Path.GetFileName(sourceFile)} - {ex.GetType().FullName} : {ex.Message}");
            return;
        }
    }
}