using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.AutoPAR.Rainier
{
    public class UpdaterManifest
    {
        public record class ArtifactDetails
        {
            public string MD5;
            public string Url;
        }

        public ArtifactDetails CompressedArtifact;

        public List<ArtifactDetails> BackgroundImages;

        /// <summary>
        /// A list of release notes by language.
        /// </summary>
        public Dictionary<string, string> ReleaseNotes;

        public const string RELEASE_NOTE_LANGUAGE_EN = "en";
        public const string RELEASE_NOTE_LANGUAGE_FR = "fr";
        public const string RELEASE_NOTE_LANGUAGE_IT = "it";
        public const string RELEASE_NOTE_LANGUAGE_DE = "de";
        public const string RELEASE_NOTE_LANGUAGE_ES = "es";
        public const string RELEASE_NOTE_LANGUAGE_ES_LA = "esla";
        public const string RELEASE_NOTE_LANGUAGE_PT_BR = "ptbr";
    }
}
