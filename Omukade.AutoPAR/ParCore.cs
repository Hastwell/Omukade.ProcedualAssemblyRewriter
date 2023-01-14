/*************************************************************************
* Omukade Auto Procedual Assembly Rewriter ("AutoPAR")
* (c) 2023 Hastwell/Electrosheep Networks 
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

using Mono.Cecil;

namespace Omukade.AutoPAR
{
    public class ParCore
    {
        private const int FILESIZE_PADDING = 4096;

        /// <summary>
        /// Processes an assembly at the given location by making all members public.
        /// </summary>
        /// <param name="sourceFile">The location of the assembly to process.</param>
        /// <returns>A <see cref="MemoryStream"/> containing the processed assembly, savable to disk or loadable using eg <see cref="System.Reflection.Assembly.Load(byte[])"/> or <see cref="System.Runtime.Loader.AssemblyLoadContext.LoadFromStream(Stream)"/>. The stream must be disposed after the assembly is loaded.</returns>
        public MemoryStream ProcessAssembly(string sourceFile)
        {
            ModuleDefinition currentAssembly = ModuleDefinition.ReadModule(sourceFile, new ReaderParameters { AssemblyResolver = CecilHelpers.GetResolverSeachingInDirectories(Path.GetDirectoryName(sourceFile)) });

            foreach (TypeDefinition type in currentAssembly.Types)
            {
                CecilHelpers.PublicifyType(type);
            }

            // Allocate a MemoryStream that can reasonably hold the updated assembly.
            int originalFilesize = (int)new FileInfo(sourceFile).Length;
            MemoryStream outputBuffer = new MemoryStream(originalFilesize + FILESIZE_PADDING);

            currentAssembly.Write(outputBuffer);

            outputBuffer.Position = 0;

            return outputBuffer;
        }

        /// <summary>
        /// Processes an assembly at the given location by making all members public, and saves the processed assembly to disk.
        /// </summary>
        /// <param name="sourceFile">The location of the assembly to process.</param>
        /// <param name="destFile">The location to save the updated assembly.</param>
        public void ProcessAssemblyAndSaveToFile(string sourceFile, string destFile)
        {
            using MemoryStream ms = ProcessAssembly(sourceFile);
            using FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.SetLength(ms.Length);
            ms.CopyTo(fs);
        }
    }
}
