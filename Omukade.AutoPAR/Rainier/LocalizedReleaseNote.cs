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

using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Omukade.AutoPAR.Rainier
{
    public class LocalizedReleaseNote
    {
        public class NoteEntry
        {
            public string Header;
            public List<string> LineItems;
        }

        public string Version;

        [JsonProperty("Date")]
        public string DateRaw;

        public List<NoteEntry> Notes;

        public string SupportInfo;

        /// <summary>
        /// Parses the release note's datestring (<see cref="DateRaw"/>) to a <see cref="DateTime"/>.
        /// </summary>
        public DateTime DateParsed { get => DateTime.ParseExact(DateRaw, "M-d-yyyy", CultureInfo.InvariantCulture); }

        /// <summary>
        /// Generates a version string based on the release note's <see cref="Version"/> and <see cref="DateRaw"/>.
        /// </summary>
        /// <remarks>The version string resembles, but will not match the game's build date. All generated version strings will have the "-est" suffix to indicate this.</remarks>
        public string ComputedBuildVersion
        {
            get
            {
                DateTime releaseDate = this.DateParsed;
                Match versionMatch = Regex.Match(this.Version, """Version (\d+).(\d+).(\d+) \((\d+)\)""");

                return $"{versionMatch.Groups[1].Value}.{versionMatch.Groups[2].Value}.{versionMatch.Groups[3].Value}.{versionMatch.Groups[4].Value}.{releaseDate.Year:D4}{releaseDate.Month:D2}{releaseDate.Day:D2}-est";
            }
        }
    }
}
