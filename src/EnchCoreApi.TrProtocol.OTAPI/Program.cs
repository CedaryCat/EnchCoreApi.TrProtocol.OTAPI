using Mono.Cecil;
using MonoMod.Utils;

namespace EnchCoreApi.TrProtocol.OTAPI
{
    [MonoMod.MonoModIgnore]
    internal class Program {
        static void Main(string[] args)
        {
            new Patcher().Patch();
        }
    }
}