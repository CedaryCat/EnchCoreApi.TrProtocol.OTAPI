using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using Terraria.GameContent.ItemDropRules;

#pragma warning disable CS8321 // Local function is declared but never used

Tuple<Type, MethodDefinition>[] drops;
[Modification(ModType.PostMerge, "make Item, NPC and Proj to virtual")]
[MonoMod.MonoModIgnore]
void HookDrops(ModFwModder modder)
{
    drops = new Tuple<Type, MethodDefinition>[]
    {
        new Tuple<Type, MethodDefinition>(typeof(DropPerPlayerOnThePlayer), modder.GetMethodDefinition(() => OTAPI.Hooks.Drop.InvokeCreateDropPerPlayerOnThePlayer(0, 0, 0, 0, null))),
        new Tuple<Type, MethodDefinition>(typeof(DropLocalPerClientAndResetsNPCMoneyTo0), modder.GetMethodDefinition(() => OTAPI.Hooks.Drop.InvokeCreateDropLocalPerClientAndResetsNPCMoneyTo0(0, 0, 0, 0, null))),
        new Tuple<Type, MethodDefinition>(typeof(ItemDropWithConditionRule), modder.GetMethodDefinition(() => OTAPI.Hooks.Drop.InvokeCreateItemDropWithConditionRule(0, 0, 0, 0, null, 0))),
        new Tuple<Type, MethodDefinition>(typeof(CommonDropWithRerolls), modder.GetMethodDefinition(() => OTAPI.Hooks.Drop.InvokeCreateCommonDropWithRerolls(0, 0, 0, 0, 0))),
        new Tuple<Type, MethodDefinition>(typeof(CommonDropNotScalingWithLuck), modder.GetMethodDefinition(() => OTAPI.Hooks.Drop.InvokeCreateCommonDropNotScalingWithLuck(0, 0, 0, 0))),
        new Tuple<Type, MethodDefinition>(typeof(CommonDrop), modder.GetMethodDefinition(() => OTAPI.Hooks.Drop.InvokeCreateCommonDrop(0, 0, 0, 0, 0))),
    };
    modder.OnRewritingMethodBody += (modder, body, instr, instri) => {
        if (instr.OpCode == OpCodes.Newobj)
        {
            var operandMethod = instr.Operand as MethodReference;
            foreach (var model in drops)
            {
                if (operandMethod.DeclaringType.FullName == model.Item1.FullName && body.Method.DeclaringType.Namespace != "OTAPI")
                {
                    instr.OpCode = OpCodes.Call;

                    //Find the appropriate create tile call, depending on the constructor parameters
                    instr.Operand = modder.Module.ImportReference(model.Item2);
                }
            }
        }
    };
}

namespace OTAPI
{
    public static partial class Hooks
    {
        public static partial class Drop
        {
            public static CommonDrop InvokeCreateCommonDrop(int itemId, int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1)
            {
                return Hooks.Drop.CreateCommonDrop?.Invoke(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator) ?? new CommonDrop(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator);
            }
            public static CommonDropNotScalingWithLuck InvokeCreateCommonDropNotScalingWithLuck(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum)
            {
                return Hooks.Drop.CreateCommonDropNotScalingWithLuck?.Invoke(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum) ?? new CommonDropNotScalingWithLuck(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum);
            }
            public static CommonDropWithRerolls InvokeCreateCommonDropWithRerolls(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, int rerolls)
            {
                return Hooks.Drop.CreateCommonDropWithRerolls?.Invoke(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, rerolls) ?? new CommonDropWithRerolls(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, rerolls);
            }
            public static ItemDropWithConditionRule InvokeCreateItemDropWithConditionRule(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition condition, int chanceNumerator = 1)
            {
                return Hooks.Drop.CreateItemDropWithConditionRule?.Invoke(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, condition, chanceNumerator) ?? new ItemDropWithConditionRule(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, condition, chanceNumerator);
            }
            public static DropLocalPerClientAndResetsNPCMoneyTo0 InvokeCreateDropLocalPerClientAndResetsNPCMoneyTo0(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition optionalCondition)
            {
                return Hooks.Drop.CreateDropLocalPerClientAndResetsNPCMoneyTo0?.Invoke(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, optionalCondition) ?? new DropLocalPerClientAndResetsNPCMoneyTo0(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, optionalCondition);
            }
            public static DropPerPlayerOnThePlayer InvokeCreateDropPerPlayerOnThePlayer(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition optionalCondition)
            {
                return Hooks.Drop.CreateDropPerPlayerOnThePlayer?.Invoke(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, optionalCondition) ?? new DropPerPlayerOnThePlayer(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, optionalCondition);
            }
            public static CommonDrop_RollLuckHandler CommonDropRollLuck;
            public static CommonDrop_TryDroppingItemHandler CommonDropTryDroppingItem;
            public static CommonDropWithRerolls_RerollTimesHandler RerollsCommonDropRerollTimes;
            public static CommonDropWithRerolls_RollLuckHandler RerollsCommonDropRollLuck;
            public static CommonDropWithRerolls_TryDroppingItemHandler RerollsCommonDropTryDroppingItem;
            public static CommonDropNotScalingWithLuck_TryDroppingItemHandler NotLuckScaleCommonDropTryDroppingItem;

            public static CreateCommonDropHandler CreateCommonDrop;
            public static CreateCommonDropNotScalingWithLuckHandler CreateCommonDropNotScalingWithLuck;
            public static CreateCommonDropWithRerollsHandler CreateCommonDropWithRerolls;
            public static CreateItemDropWithConditionRuleHandler CreateItemDropWithConditionRule;
            public static CreateDropLocalPerClientAndResetsNPCMoneyTo0Handler CreateDropLocalPerClientAndResetsNPCMoneyTo0;
            public static CreateDropPerPlayerOnThePlayerHandler CreateDropPerPlayerOnThePlayer;


            public delegate int CommonDrop_RollLuckHandler(CommonDrop dropData, DropAttemptInfo info);
            public delegate HookResult CommonDrop_TryDroppingItemHandler(CommonDrop dropData, ref DropAttemptInfo info, ref ItemDropAttemptResult result);
            public delegate void CommonDropWithRerolls_RerollTimesHandler(CommonDropWithRerolls dropData, DropAttemptInfo info, ref int times);
            public delegate int CommonDropWithRerolls_RollLuckHandler(CommonDropWithRerolls dropData, DropAttemptInfo info);
            public delegate HookResult CommonDropWithRerolls_TryDroppingItemHandler(CommonDropWithRerolls dropData, ref DropAttemptInfo info, ref ItemDropAttemptResult result);
            public delegate HookResult CommonDropNotScalingWithLuck_TryDroppingItemHandler(CommonDropNotScalingWithLuck dropData, ref DropAttemptInfo info, ref ItemDropAttemptResult result);

            public delegate CommonDrop CreateCommonDropHandler(int itemId, int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1);
            public delegate CommonDropNotScalingWithLuck CreateCommonDropNotScalingWithLuckHandler(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum);
            public delegate CommonDropWithRerolls CreateCommonDropWithRerollsHandler(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, int rerolls);
            public delegate ItemDropWithConditionRule CreateItemDropWithConditionRuleHandler(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition condition, int chanceNumerator = 1);
            public delegate DropLocalPerClientAndResetsNPCMoneyTo0 CreateDropLocalPerClientAndResetsNPCMoneyTo0Handler(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition optionalCondition);
            public delegate DropPerPlayerOnThePlayer CreateDropPerPlayerOnThePlayerHandler(int itemId, int chanceDenominator, int amountDroppedMinimum, int amountDroppedMaximum, IItemDropRuleCondition optionalCondition);
        }
    }
}
