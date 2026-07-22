using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StabilityAbove;

public static class ModCommand {
    public static void CreateDebugCommand(ICoreAPI Api, StabilityAbove Mod) {
        Api.ChatCommands.GetOrCreate("debug")
            .BeginSubCommand("stabilityabove")
            .WithDescription("Logs the current temporal stability with all modifiers to the chat.")
            .RequiresPlayer()
            .HandleWith(Args => {
                var Pos = Args.Caller.Pos;
                var StabilitySystem = Api.ModLoader.GetModSystem<SystemTemporalStability>();
                
                Mod.Enabled = false;
                var BaseStability = StabilitySystem.GetTemporalStability(Pos);
                Mod.Enabled = true;
                var ModdedStability = StabilitySystem.GetTemporalStability(Pos);
                
                return TextCommandResult.Success($"Current temporal stability values:\nBase Stability: {BaseStability}\nOverwritten Stability: {ModdedStability}");
            }).EndSubCommand();
    }
}