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

using Microsoft.Win32;
using System.Runtime.InteropServices;

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
