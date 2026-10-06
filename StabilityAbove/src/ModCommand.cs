using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace StabilityAbove;

public static class ModCommand {
    public static void CreateDebugCommands(ICoreServerAPI Api, StabilityAbove Mod) {
        Api.ChatCommands.GetOrCreate("debug")
            .BeginSubCommand("stabilityabove")
            .WithDescription("Logs the current temporal stability with all modifiers to the chat.")
            .RequiresPlayer()
            .HandleWith(Args => {
                var Pos = Args.Caller.Pos;
                var SeaLevel = TerraGenConfig.seaLevel;
                var Config = Mod.Config!;
                var TransitionHeight = (int) (TerraGenConfig.seaLevel * Config.TransitionHeightPercentage);
                
                var StabilitySystem = Api.ModLoader.GetModSystem<SystemTemporalStability>();
                Mod.Enabled = false;
                var BaseStability = StabilitySystem.GetTemporalStability(Pos);
                Mod.Enabled = true;
                var ModdedStability = StabilitySystem.GetTemporalStability(Pos);

                var Output = new StringBuilder();
                Output.Append("Stability debug information:\n");
                Output.AppendFormat($"World height: {Api.WorldManager.MapSizeY}\n");
                Output.AppendFormat($"Sea level: {SeaLevel}\n");
                Output.Append("------------\n");
                Output.AppendFormat(Mod.Enabled ? $"The mod is currently enabled\n" : "The mod is currently disabled!\n");
                Output.AppendFormat($"The minimum stability is: {Config.StableStability}\n");
                Output.AppendFormat($"The transition zone starts at: {(int) (TerraGenConfig.seaLevel * Config.StabilityHeightPercentage) - TransitionHeight}\n");
                Output.AppendFormat($"The full stabilization start at: {(int) (TerraGenConfig.seaLevel * Config.StabilityHeightPercentage)}\n");
                Output.Append("------------\n");
                Output.AppendFormat($"Current position: {Pos}\n");
                Output.AppendFormat($"Current temporal stability values:\nBase Stability: {BaseStability}\nOverwritten Stability: {ModdedStability}\n");
                
                return TextCommandResult.Success(Output.ToString());
            }).EndSubCommand();
    }
}