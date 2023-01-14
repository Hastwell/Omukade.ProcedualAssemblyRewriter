using System.Runtime.Loader;

namespace Omukade.AutoPAR
{
    public static class AssemblyLoadInterceptor
    {
        private static string searchFolder;
        private static ParCore parCore = new ParCore();

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

            using MemoryStream assemblyStream = parCore.ProcessAssembly(sourceFileLocation);
            return loadContext.LoadFromStream(assemblyStream);
        }
    }
}