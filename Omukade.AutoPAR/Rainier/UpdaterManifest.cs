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
