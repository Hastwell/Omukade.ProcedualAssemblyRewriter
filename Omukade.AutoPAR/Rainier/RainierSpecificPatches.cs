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
using Newtonsoft.Json;
using System.Reflection;

namespace Omukade.AutoPAR.Rainier
{
    /// <summary>
    /// Additional preprocessor steps specific to Rainier/PTCGL.
    /// </summary>
    public class RainierSpecificPatches
    {
        public static void MakeGameStateCloneVirtual(TypeDefinition type)
        {
            if (type?.Namespace != "SharedSDKUtils" || type?.Name != "GameState") return;

            MethodDefinition? copyStateMethod = type.Methods.FirstOrDefault(m => m.Name == "CopyState");
            if(copyStateMethod == null)
            {
                throw new InvalidOperationException("Found GameState, but no CopyState was found.");
            }

            copyStateMethod.IsVirtual = true;
        }

        public static void AddJsonIgnoreAttribute_SetKnockoutAtFullHealthByDamageMetaData(TypeDefinition type)
        {
            if (type?.Namespace != "MatchLogic" || type?.Name != "ResolveAttack") return;

            Type jsonIgnoreType = typeof(JsonIgnoreAttribute);
            ConstructorInfo jsonIgnoreConstructor = jsonIgnoreType.GetConstructor(Type.EmptyTypes)!;
            MethodReference constructorRef = type.Module.ImportReference(jsonIgnoreConstructor);

            type.Fields.First(f => f.Name == "_setKnockoutAtFullHealthByDamageMetaData").CustomAttributes.Add(new CustomAttribute(constructorRef));
        }
    }
}
