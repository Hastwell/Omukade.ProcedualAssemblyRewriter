using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.AutoPAR
{
    /// <summary>
    /// Helper to locate the installation path of certain well-known programs/games.
    /// </summary>
    [Obsolete("Use RainierFetcher to auto-update Rainier without depending on a user-managed local install")]
    public static class InstallationFinder
    {
        /// <summary>
        /// Attempts to find the aseemblies for the current user's Pokemon TCG Live installation. Returns null if not found, or current OS is not Windows.
        /// </summary>
        public static string? FindPtcglInstallAssemblyDirectory()
        {
            if(!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            using(RegistryKey? userInstallKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\The Pokémon Company International\Pokémon Trading Card Game Live"))
            {
                if(userInstallKey != null)
                {
                    object? rawKey = userInstallKey.GetValue("Path");
                    if(rawKey is string)
                    {
                        return Path.Combine( (string)rawKey, "Pokémon Trading Card Game Live", "Pokemon TCG Live_Data", "Managed");
                    }
                }
            }

            return null;
        }
    }
}
