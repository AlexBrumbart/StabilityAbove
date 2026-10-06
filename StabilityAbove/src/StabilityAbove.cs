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

    public ModConfig? Config { get; private set; }
    
    private SystemTemporalStability? TemporalStabilitySystem;
    private GetTemporalStabilityDelegate? StoryStructureStabilityOverwrite;
    
    public override double ExecuteOrder() => 0.2F; // Has to load after StoryStructuresSpawnConditions to override their stability delegate!
    
    /* ----- Lifecycle methods ----- */

    public override void Start(ICoreAPI Api) {
        TemporalStabilitySystem = Api.ModLoader.GetModSystem<SystemTemporalStability>();
        CaptureStoryStructureDelegate(Api);
        
        Api.ModLoader.GetModSystem<SystemTemporalStability>().OnGetTemporalStability += ClampStabilityOverground;
    }
    
    public override void StartServerSide(ICoreServerAPI Api) {
        Config = ModConfig.TryLoad(Api, Mod.Logger);
        Config.SetWorldConfig(Api);

        ModCommand.CreateDebugCommands(Api, this);
    }
    
    public override void StartClientSide(ICoreClientAPI Api) {
        Config = ModConfig.TryLoad(Api, Mod.Logger);
        Config.LoadWorldConfig(Api);
    }
    
    /* ----- Class methods ----- */
    
    private void CaptureStoryStructureDelegate(ICoreAPI Api) {
        var StoryStructureSpawn = Api.ModLoader.GetModSystem<StoryStructuresSpawnConditions>();
        var StabilityDelegate = StoryStructureSpawn.GetType().GetMethod("ResoArchivesSpawnConditions_OnGetTemporalStability", BindingFlags.Instance | BindingFlags.NonPublic);
        if (StabilityDelegate == null)
            throw new InvalidOperationException("StoryStructuresSpawnConditions_OnGetTemporalStability method not found");
        
        // Captures the base games stability overwrite inside story structures into a callable delegate.
        StoryStructureStabilityOverwrite = (GetTemporalStabilityDelegate) Delegate.CreateDelegate(typeof(GetTemporalStabilityDelegate), StoryStructureSpawn, StabilityDelegate);
    }
    
    /*
     * Delegate called from the game to allow adjusting the temporal stability.
     *
     * As only one delegate can be set, the StoryStructuresSpawnConditions overwrite, which should be included manually!
     */
    private float ClampStabilityOverground(float Stability, double X, double Y, double Z) {
        Contract.Assert(StoryStructureStabilityOverwrite != null);
        
        if (!Enabled || DisableForStorm())
            return Stability;

        var OverwriteStability = CalculateStability(Stability, Y);
        var StoryStructureStability = StoryStructureStabilityOverwrite(Stability, X, Y, Z);
        
        return Math.Max(OverwriteStability, StoryStructureStability);
    }
    
    private bool DisableForStorm() {
        Contract.Assert(Config != null);
        Contract.Assert(TemporalStabilitySystem != null);
        
        return Config.DisableDuringTemporalStorm && TemporalStabilitySystem.StormData.nowStormActive;
    }

    private float CalculateStability(float Stability, double Height) {
        Contract.Assert(Config != null);
        
        var OverwriteStability = Stability;
        var TransitionHeight = (int) (TerraGenConfig.seaLevel * Config.TransitionHeightPercentage);
        var OverwriteStartHeight = (TerraGenConfig.seaLevel * Config.StabilityHeightPercentage) - TransitionHeight;
        if (Stability < Config.StableStability && Height >= OverwriteStartHeight) {
            var InterpolationFactor = (float) Math.Min((Height - OverwriteStartHeight) / TransitionHeight, 1.0F);
            OverwriteStability = Stability * (1.0F - InterpolationFactor) + Config.StableStability * InterpolationFactor;
        }

        return OverwriteStability;
    }
}