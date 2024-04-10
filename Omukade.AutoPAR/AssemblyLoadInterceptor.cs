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

using System.Runtime.Loader;

namespace Omukade.AutoPAR
{
    public static class AssemblyLoadInterceptor
    {
        private static string searchFolder;
        public static ParCore ParCore { get; } = new ParCore();

        /// <summary>
        /// Initializes AutoPAR on assemblies found in <see cref="searchFolder"/> via a "file-not-found" hook on the default <see cref="AssemblyLoadContext"/>.
        /// </summary>
        /// <param name="searchFolder">The directory to search for assemblies to use for AutoPAR.</param>
        /// <exception cref="ArgumentNullException"><paramref name="searchFolder"/> was null.</exception>
        /// <exception cref="ArgumentException"><paramref name="searchFolder"/> doesn't exist or is not a directory.</exception>
        /// <remarks>Assemblies can't be in <see cref="Environment.CurrentDirectory"/> or the host assembly's location as they will be picked up as-is by the normal assembly loading process and not fall through to <see cref="AssemblyLoadContext.Resolving"/>.</remarks>
        public static void Initialize(string searchFolder)
        {
            if(searchFolder == null)
            {
                throw new ArgumentNullException(nameof(searchFolder));
            }

            if(!Directory.Exists(searchFolder))
            {
                throw new ArgumentException($"Search folder ({searchFolder}) does not exist, or is not a directory.", nameof(searchFolder));
            }

            AssemblyLoadInterceptor.searchFolder = searchFolder;
            AssemblyLoadContext.Default.Resolving += Default_Resolving;
        }

        private static System.Reflection.Assembly? Default_Resolving(AssemblyLoadContext loadContext, System.Reflection.AssemblyName assemblyName)
        {
            string sourceFileLocation = Path.Combine(searchFolder, assemblyName.Name + ".dll");
            if (!File.Exists(sourceFileLocation))
            {
                return null;
            }

            using MemoryStream assemblyStream = ParCore.ProcessAssembly(sourceFileLocation);
            return loadContext.LoadFromStream(assemblyStream);
        }
    }
}