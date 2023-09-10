#pragma warning disable CS8321 // Local function is declared but never used
#pragma warning disable CS0436 // Type conflicts with imported type

using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using System.Linq;
using OpCodes = Mono.Cecil.Cil.OpCodes;

/// <summary>
/// @doc Creates Hooks.Main.Create. Allows plugins to extend and return a custom Terraria.Main instance.
/// </summary>
[Modification(ModType.PostPatch, "Hook SpawnNPC", ModPriority.Last)]
[MonoMod.MonoModIgnore]
void HookSpawnNPC(MonoModder modder) {


    var spawnNPCMethod = modder.GetMethodDefinition(() => Terraria.NPC.SpawnNPC());

    var processor = spawnNPCMethod.Body.GetILProcessor();

    var var_tempDefSpawnRate = new VariableDefinition(modder.Module.TypeSystem.Int32);
    var var_tempDefMaxSpawns = new VariableDefinition(modder.Module.TypeSystem.Int32);

    var var_plyIndex = spawnNPCMethod.Body.Variables[13];

    spawnNPCMethod.Body.Variables.Add(var_tempDefSpawnRate);
    spawnNPCMethod.Body.Variables.Add(var_tempDefMaxSpawns);

    var start = spawnNPCMethod.Body.Instructions.First(i => i.OpCode == OpCodes.Stloc_S && i.Operand == spawnNPCMethod.Body.Variables[25]);

    processor.InsertAfter(start,
        new { OpCodes.Ldloc_S, Operand = var_plyIndex },
        new { OpCodes.Call, Operand = modder.Module.ImportReference(modder.GetMethodDefinition(() => OTAPI.Hooks.NPC.InvokeGetDefaultSpawnRate(0))) },
        new { OpCodes.Stloc_S, Operand = var_tempDefSpawnRate },
        new { OpCodes.Ldloc_S, Operand = var_plyIndex },
        new { OpCodes.Call, Operand = modder.Module.ImportReference(modder.GetMethodDefinition(() => OTAPI.Hooks.NPC.InvokeGetDefaultMaxSpawns(0))) },
        new { OpCodes.Stloc_S, Operand = var_tempDefMaxSpawns });

    var list = spawnNPCMethod.Body.Instructions.Where(
        i => i.OpCode == OpCodes.Ldsfld &&
        i.Operand is FieldReference fieldDefinition &&
        (fieldDefinition.Name == nameof(Terraria.NPC.defaultMaxSpawns) || fieldDefinition.Name == nameof(Terraria.NPC.defaultSpawnRate))).ToList();

    list.RemoveAt(0);

    foreach (var call in list) {
        VariableDefinition tempVar;
        if(((FieldReference)call.Operand).Name == nameof(Terraria.NPC.defaultMaxSpawns)) {
            tempVar = var_tempDefMaxSpawns;
        }
        else {
            tempVar = var_tempDefSpawnRate;
        }
        call.OpCode = OpCodes.Ldloc_S;
        call.Operand = tempVar;
    }
}

namespace OTAPI {
	public static partial class Hooks {
        public static partial class NPC {
            public static int InvokeGetDefaultMaxSpawns(int playerId) {
                if (GetDefaultMaxSpawns != null) {
                    return GetDefaultMaxSpawns(playerId);
                }
                return global::Terraria.NPC.defaultMaxSpawns;
            }
            public delegate int GetDefaultMaxSpawnsDele(int playerId);
            public static GetDefaultMaxSpawnsDele GetDefaultMaxSpawns;
            public static int InvokeGetDefaultSpawnRate(int playerId) {
                if (GetDefaultSpawnRate != null) {
                    return GetDefaultSpawnRate(playerId);
                }
                return global::Terraria.NPC.defaultSpawnRate;
            }
            public delegate int GetDefaultSpawnRateDele(int playerId);
            public static GetDefaultSpawnRateDele GetDefaultSpawnRate;
        }
    }
}
