using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

#pragma warning disable CS8321 // Local function is declared but never used
[Modification(ModType.PostMerge, "Hooking WorldGenerator...")]
[MonoMod.MonoModIgnore]
void HookWorldGenerator(ModFramework.ModFwModder modder)
{
    MakeAllVirtual(modder.GetDefinition<Terraria.WorldBuilding.WorldGenerator>());
    modder.OnRewritingMethodBody += (modder, body, instr, instri) =>
    {
        if (instr.OpCode == OpCodes.Newobj)
        {
            if (body.Method.DeclaringType.FullName.Contains(nameof(OTAPI))) return;
            var operandMethod = instr.Operand as MethodReference;
            if (operandMethod.DeclaringType == modder.GetDefinition<Terraria.WorldBuilding.WorldGenerator>())
            {
                instr.OpCode = OpCodes.Call;
                instr.Operand = modder.Module.ImportReference(modder.GetMethodDefinition(() => OTAPI.Hooks.WorldGen.CreateWorldGeneratorInstance(0, null)));
            }
        }
    };
    static void MakeAllVirtual(TypeDefinition type, params MethodDefinition[] ignores)
    {
        var methods = type.Methods.Where(m => !m.IsConstructor && !m.IsStatic && m.Name != "cctor" && m.Name != "ctor").ToList();
        methods.AddRange(type.Properties.Select(p => p.SetMethod).Where(m => m != null && !m.IsStatic));
        methods.AddRange(type.Properties.Select(p => p.GetMethod).Where(m => m != null && !m.IsStatic));
        foreach (var method in methods)
        {
            if (ignores.Contains(method)) continue;

            method.IsVirtual = true;
            method.IsNewSlot = true;
        }

        foreach (var t in type.Module.Types)
        {
            foreach (var method in t.Methods)
            {
                if (method.HasBody)
                {
                    foreach (var instruction in method.Body.Instructions)
                    {
                        if (methods.Any(x => x == instruction.Operand))
                        {
                            if (instruction.OpCode != OpCodes.Callvirt)
                            {
                                instruction.OpCode = OpCodes.Callvirt;
                            }
                        }
                    }
                }
            }
        }
    }
}



namespace OTAPI
{
    public static partial class Hooks
    {
        public static partial class WorldGen
        {
            public delegate global::Terraria.WorldBuilding.WorldGenerator CreateWorldGeneratorHandler(int seed, global::Terraria.WorldBuilding.WorldGenConfiguration configuration);

            public static CreateWorldGeneratorHandler CreateWorldGenerator;
            internal static global::Terraria.WorldBuilding.WorldGenerator CreateWorldGeneratorInstance(int seed, global::Terraria.WorldBuilding.WorldGenConfiguration configuration)
            {
                return Hooks.WorldGen.CreateWorldGenerator?.Invoke(seed, configuration) ?? new global::Terraria.WorldBuilding.WorldGenerator(seed, configuration);
            }
        }
    }
}