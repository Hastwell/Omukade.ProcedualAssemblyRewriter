using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
