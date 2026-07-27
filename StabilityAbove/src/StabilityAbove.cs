using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using JetBrains.Annotations;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace StabilityAbove;

[UsedImplicitly]
public class StabilityAbove : ModSystem {
    public bool Enabled { get; set; } = true;

    private ModConfig? Config;
    private SystemTemporalStability TemporalStabilitySystem;
    private GetTemporalStabilityDelegate? StoryStructureStabilityOverwrite;
    
    public override double ExecuteOrder() => 0.2F; // Has to load after StoryStructuresSpawnConditions to override their stability delegate!

    public override void Start(ICoreAPI Api) {
        TemporalStabilitySystem = Api.ModLoader.GetModSystem<SystemTemporalStability>();
        CaptureStoryStructureDelegate(Api);
        
        Api.ModLoader.GetModSystem<SystemTemporalStability>().OnGetTemporalStability += ClampStabilityOverground;
    }
    
    private void CaptureStoryStructureDelegate(ICoreAPI Api) {
        var StoryStructureSpawn = Api.ModLoader.GetModSystem<StoryStructuresSpawnConditions>();
        var StabilityDelegate = StoryStructureSpawn.GetType().GetMethod("ResoArchivesSpawnConditions_OnGetTemporalStability", BindingFlags.Instance | BindingFlags.NonPublic);
        if (StabilityDelegate == null)
            throw new InvalidOperationException("StoryStructuresSpawnConditions_OnGetTemporalStability method not found");
        
        // Captures the base games stability overwrite inside story structures into a callable delegate.
        StoryStructureStabilityOverwrite = (GetTemporalStabilityDelegate) Delegate.CreateDelegate(typeof(GetTemporalStabilityDelegate), StoryStructureSpawn, StabilityDelegate);
    }
    
    private float ClampStabilityOverground(float Stability, double X, double Y, double Z) {
        Contract.Assert(Config != null);
        Contract.Assert(StoryStructureStabilityOverwrite != null);
        
        if (!Enabled)
            return Stability;
        
        if (Config.DisableDuringTemporalStorm && TemporalStabilitySystem.StormData.nowStormActive)
            return Stability;

        var OverwriteStability = Stability;
        var TransitionHeight = (int) (TerraGenConfig.seaLevel * Config.TransitionHeightPercentage);
        var OverwriteStartHeight = (TerraGenConfig.seaLevel * Config.StabilityHeightPercentage) - TransitionHeight;
        if (Stability < 1.0 && Y >= OverwriteStartHeight) {
            var InterpolationFactor = (float) Math.Min((Y - OverwriteStartHeight) / TransitionHeight, 1.0F);
            OverwriteStability = Stability * (1 - InterpolationFactor) + 1.0F * InterpolationFactor;
        }
        
        // This delegate overrides the delegate of StoryStructuresSpawnConditions, which needs to be included here.
        var StoryStructureStability = StoryStructureStabilityOverwrite(Stability, X, Y, Z);
        
        return Math.Max(OverwriteStability, StoryStructureStability);
    }
    
    public override void StartServerSide(ICoreServerAPI Api) {
        TryLoadConfig(Api);
        
        Api.World.Config.SetFloat("StabilityAbove.StabilityHeightPercentage", Config!.StabilityHeightPercentage);
        Api.World.Config.SetFloat("StabilityAbove.TransitionHeightPercentage", Config!.TransitionHeightPercentage);
        Api.World.Config.SetBool("StabilityAbove.DisableDuringTemporalStorm", Config!.DisableDuringTemporalStorm);

        ModCommand.CreateDebugCommand(Api, this);
    }

    private void TryLoadConfig(ICoreAPI Api) {
        try {
            Config = Api.LoadModConfig<ModConfig>("StabilityAbove.json") ?? new ModConfig();
            Api.StoreModConfig(Config, "StabilityAbove.json");
        } catch (Exception Exception) {
            Mod.Logger.Error("Could not load config! Loading default settings instead.");
            Mod.Logger.Error(Exception);
            
            Config = new ModConfig();
        }
    }

    public override void StartClientSide(ICoreClientAPI Api) {
        TryLoadConfig(Api);
        
        if (Api.World.Config.HasAttribute("StabilityAbove.StabilityHeightPercentage"))
            Config!.StabilityHeightPercentage = Api.World.Config.GetFloat("StabilityAbove.StabilityHeightPercentage");
        
        if (Api.World.Config.HasAttribute("StabilityAbove.TransitionHeightPercentage"))
            Config!.TransitionHeightPercentage = Api.World.Config.GetFloat("StabilityAbove.TransitionHeightPercentage");
        
        if (Api.World.Config.HasAttribute("StabilityAbove.DisableDuringTemporalStorm"))
            Config!.DisableDuringTemporalStorm = Api.World.Config.GetBool("StabilityAbove.DisableDuringTemporalStorm");
    }
}