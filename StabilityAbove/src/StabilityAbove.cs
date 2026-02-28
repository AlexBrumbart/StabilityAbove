using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using JetBrains.Annotations;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace StabilityAbove;

[UsedImplicitly]
public class StabilityAbove : ModSystem {
    private GetTemporalStabilityDelegate? StoryStructureStabilityOverwrite;
    
    public override double ExecuteOrder() => 0.2F; // Has to load after StoryStructuresSpawnConditions to override their stability delegate!

    public override void Start(ICoreAPI Api) {
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
        Contract.Assert(StoryStructureStabilityOverwrite != null);

        var OverwriteStability = Stability;
        var OverwriteStartHeight = TerraGenConfig.seaLevel - 10;
        if (Stability < 1.0 && Y >= OverwriteStartHeight) {
            var InterpolationFactor = (float) Math.Min((Y - OverwriteStartHeight) / 10, 1.0F);
            OverwriteStability = Stability * (1 - InterpolationFactor) + 1.0F * InterpolationFactor;
        }
        
        // This delegate overrides the delegate of StoryStructuresSpawnConditions, which needs to be included here.
        var StoryStructureStability = StoryStructureStabilityOverwrite(Stability, X, Y, Z);
        
        return Math.Max(OverwriteStability, StoryStructureStability);
    }
}