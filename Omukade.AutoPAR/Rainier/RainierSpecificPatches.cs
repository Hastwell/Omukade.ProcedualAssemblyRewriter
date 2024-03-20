using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.AutoPAR.Rainier
{
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
