using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.Localization;

#pragma warning disable CS8321 // Local function is declared but never used
[Modification(ModType.PostMerge, "Hooking dedServ")]
[MonoMod.MonoModIgnore]
void HookDedServ(ModFramework.ModFwModder modder)
{
    foreach (Tuple<string, MethodDefinition> replacement in new Tuple<string, MethodDefinition>[]
    {
        new Tuple<string, MethodDefinition>("CLI.ChooseWorld",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.ChooseWorldOperate())),
        new Tuple<string, MethodDefinition>("CLI.DeleteConfirmation",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.GetDeleteConfirmationCL())),
        new Tuple<string, MethodDefinition>("CLI.ChooseSize",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetWorldSizeCL())),
        new Tuple<string, MethodDefinition>("CLI.ChooseDifficulty",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetWorldDifficultyCL())),
        new Tuple<string, MethodDefinition>("CLI.ChooseEvil",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetWorldEvilCL())),
        new Tuple<string, MethodDefinition>("CLI.EnterWorldName",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetWorldNameCL())),
        new Tuple<string, MethodDefinition>("CLI.EnterSeed",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetWorldSeedCL())),
        new Tuple<string, MethodDefinition>("CLI.SetInitialMaxPlayers",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetMaxPlayerCL())),
        new Tuple<string, MethodDefinition>("CLI.SetInitialPort",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetGamePortCL())),
        new Tuple<string, MethodDefinition>("CLI.AutomaticPortForward",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetAutoForwardCL())),
        new Tuple<string, MethodDefinition>("CLI.EnterServerPassword",modder.GetMethodDefinition(() => OTAPI.Hooks.Server.SetPassworldCL())),
    })
    {
        bool findedIdentifyStr = false;
        foreach (var instruction in modder.GetMethodDefinition(() => new Terraria.Main().DedServ()).Body.Instructions)
        {
            if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand as string == replacement.Item1)
            {
                findedIdentifyStr = true;
            }
            if (findedIdentifyStr && instruction.OpCode == OpCodes.Call && (instruction.Operand as MethodReference).Name == nameof(Terraria.Main.ReadLineInput))
            {
                instruction.Operand = modder.Module.Import(replacement.Item2);
                break;
            }
        }
    }
}

namespace OTAPI
{
    public enum WorldSize
    {
        Small = 1,
        Medium,
        Large
    }
    public enum Difficulty
    {
        Classic = 1,
        Expert,
        Master,
        Journey
    }
    public enum WorldEvil
    {
        Random = 1,
        Corrupt,
        Crimson
    }
    public static partial class Hooks
    {
        public static partial class Server
        {
            public class DedServWorldOperateEventArgs : HandledEventArgs
            {
                public List<Terraria.IO.WorldFileData> WorldList { get; set; }
                public void DeleteWorld(Terraria.IO.WorldFileData delete)
                {
                    AutoConsole = new DeleteWorld(delete);
                }
                public void NewWorld(WorldSize size, Difficulty difficulty, WorldEvil evil, string name, string seed)
                {
                    AutoConsole = new CreateWorld(size, difficulty, evil, name, seed);
                }
                public void Nop()
                {
                    AutoConsole = new NopOperate();
                }
                public void SwitchWorldAndStartServer(Terraria.IO.WorldFileData select, int maxPlayer, int port, bool autoForward, string password)
                {
                    AutoConsole = new SwitchWorldAndStartServer(select, maxPlayer, port, autoForward, password);
                }
            }
            public delegate void WorldOperateDele(DedServWorldOperateEventArgs args);
            public static WorldOperateDele DedServWorldOperate;

            internal static DedServAutoConsole AutoConsole = new NopOperate();
            private interface IDeleteWorldCLM
            {
                string DoDeleteWorldCL();
                string GetDeleteConfirmationCL();
            }
            private interface INewWorldCLM
            {
                string DoNewWorldCL();
                string SetWorldSizeCL();
                string SetWorldDifficultyCL();
                string SetWorldEvilCL();
                string SetWorldSeedCL();
                string SetWorldNameCL();
            }
            private interface ISwitchServerAndStartCLM
            {
                string DoSwitchServerCL();
                string SetMaxPlayerCL();
                string SetGamePortCL();
                string SetAutoForwardCL();
                string SetPassworldCL();
            }
            internal abstract class DedServAutoConsole : IDeleteWorldCLM, INewWorldCLM, ISwitchServerAndStartCLM
            {
                public virtual string DoDeleteWorldCL() => Console.ReadLine();
                public virtual string GetDeleteConfirmationCL() => Console.ReadLine();
                public virtual string DoNewWorldCL() => Console.ReadLine();
                public virtual string SetWorldSizeCL() => Console.ReadLine();
                public virtual string SetWorldDifficultyCL() => Console.ReadLine();
                public virtual string SetWorldEvilCL() => Console.ReadLine();
                public virtual string SetWorldSeedCL() => Console.ReadLine();
                public virtual string SetWorldNameCL() => Console.ReadLine();
                public virtual string DoSwitchServerCL() => Console.ReadLine();
                public virtual string SetMaxPlayerCL() => Console.ReadLine();
                public virtual string SetGamePortCL() => Console.ReadLine();
                public virtual string SetPassworldCL() => Console.ReadLine();
                public virtual string SetAutoForwardCL() => Console.ReadLine();
            }
            internal class NopOperate : DedServAutoConsole
            {
            }
            internal class DeleteWorld : DedServAutoConsole
            {
                global::Terraria.IO.WorldFileData WorldFile { get; set; }
                public DeleteWorld(global::Terraria.IO.WorldFileData delete)
                {
                    WorldFile = delete;
                }
                public override string DoDeleteWorldCL()
                {
                    var id = global::Terraria.Main.WorldList.IndexOf(WorldFile);
                    return Language.GetTextValue("CLI.DeleteWorld_Command") + " " + (++id).ToString();
                }
                public override string GetDeleteConfirmationCL() => Language.GetTextValue("CLI.ShortYes");
            }
            internal class CreateWorld : DedServAutoConsole
            {
                public WorldSize WorldSize { get; set; }
                public Difficulty Difficulty { get; set; }
                public WorldEvil WorldEvil { get; set; }
                public string WorldName { get; set; }
                public string WorldSeed { get; set; }
                public CreateWorld(WorldSize size, Difficulty difficulty, WorldEvil evil, string name, string seed)
                {
                    WorldSize = size;
                    Difficulty = difficulty;
                    WorldEvil = evil;
                    WorldName = name;
                    WorldSeed = seed;
                }
                public override string DoNewWorldCL() => "n";
                public override string SetWorldSizeCL() => ((int)WorldSize).ToString();
                public override string SetWorldDifficultyCL() => ((int)Difficulty).ToString();
                public override string SetWorldEvilCL() => ((int)WorldEvil).ToString();
                public override string SetWorldSeedCL() => WorldSeed;
                public override string SetWorldNameCL() => WorldName;
            }
            internal class SwitchWorldAndStartServer : DedServAutoConsole
            {
                public global::Terraria.IO.WorldFileData WorldFile { get; set; }
                public int MaxPlayer { get; set; }
                public int GamePort { get; set; }
                public bool AutoForward { get; set; }
                public string Password { get; set; }
                public SwitchWorldAndStartServer(global::Terraria.IO.WorldFileData select, int maxPlayer, int port, bool autoForward, string password)
                {
                    WorldFile = select;
                    MaxPlayer = maxPlayer;
                    GamePort = port;
                    AutoForward = autoForward;
                    Password = password;
                }
                public override string DoSwitchServerCL()
                {
                    var id = global::Terraria.Main.WorldList.IndexOf(WorldFile);
                    if (id < 0)
                    {
                        id = global::Terraria.Main.WorldList.Count;
                        global::Terraria.Main.WorldList.Add(WorldFile);
                    }
                    return (id + 1).ToString();
                }
                public override string SetMaxPlayerCL() => MaxPlayer.ToString();
                public override string SetGamePortCL() => GamePort.ToString();
                public override string SetAutoForwardCL() => AutoForward ? Language.GetTextValue("CLI.ShortYes") : Language.GetTextValue("CLI.ShortNo");
                public override string SetPassworldCL() => Password;
            }
            internal static string ChooseWorldOperate()
            {
                AutoConsole = new NopOperate();
                var args = new OTAPI.Hooks.Server.DedServWorldOperateEventArgs()
                {
                    WorldList = global::Terraria.Main.WorldList,
                };
                Hooks.Server.DedServWorldOperate?.Invoke(args);
                if (AutoConsole is DeleteWorld)
                {
                    return AutoConsole.DoDeleteWorldCL();
                }
                else if (AutoConsole is CreateWorld)
                {
                    return AutoConsole.DoNewWorldCL();
                }
                else if (AutoConsole is SwitchWorldAndStartServer)
                {
                    return AutoConsole.DoSwitchServerCL();
                }
                return Terraria.Main.ReadLineInput();
            }
            private static string CheckTimes(ref bool called, Func<string> direct)
            {
                if (!called)
                {
                    called = true;
                    return direct();
                }
                return Console.ReadLine();
            }
            internal static string GetDeleteConfirmationCL() => CheckTimes(ref calledDeleteConfirmation, AutoConsole.GetDeleteConfirmationCL);
            private static bool calledDeleteConfirmation = false;
            internal static string SetWorldSizeCL() => CheckTimes(ref calledSetWorldSize, AutoConsole.SetWorldSizeCL);
            private static bool calledSetWorldSize = false;
            internal static string SetWorldDifficultyCL() => CheckTimes(ref calledSetWorldDifficulty, AutoConsole.SetWorldDifficultyCL);
            private static bool calledSetWorldDifficulty = false;
            internal static string SetWorldEvilCL() => CheckTimes(ref calledSetWorldEvil, AutoConsole.SetWorldEvilCL);
            private static bool calledSetWorldEvil = false;
            internal static string SetWorldSeedCL() => CheckTimes(ref calledSetWorldSeed, AutoConsole.SetWorldSeedCL);
            private static bool calledSetWorldSeed = false;
            internal static string SetWorldNameCL() => CheckTimes(ref calledSetWorldName, AutoConsole.SetWorldNameCL);
            private static bool calledSetWorldName = false;
            internal static string SetMaxPlayerCL() => CheckTimes(ref calledSetMaxPlayer, AutoConsole.SetMaxPlayerCL);
            private static bool calledSetMaxPlayer = false;
            internal static string SetGamePortCL() => CheckTimes(ref calledSetGamePort, AutoConsole.SetGamePortCL);
            private static bool calledSetGamePort = false;
            internal static string SetPassworldCL() => CheckTimes(ref calledSetPassworld, AutoConsole.SetPassworldCL);
            private static bool calledSetPassworld = false;
            internal static string SetAutoForwardCL() => CheckTimes(ref calledSetAutoForward, AutoConsole.SetAutoForwardCL);
            private static bool calledSetAutoForward = false;
        }
    }
}