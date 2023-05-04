using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
