using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace StabilityAbove;

public class ModConfig {
    /**
     * The stability value to which the world gets stabilized.
     * Valid between 0.0 and 2.0.
     */
    public float StableStability { get; private set; } = 1.0F;
    
    /**
     * The percentage of the sea-level height, above which the world is stable.
     * Valid between 0.0 and 2.5.
     */
    public float StabilityHeightPercentage { get; private set; } = 1.0F;

    /**
     * The percentage of the sea-level height used as a transition zone below the stable zone.
     * Valid between 0.0 and 1.0.
     */
    public float TransitionHeightPercentage { get; private set; } = 0.10F;
    
    /**
     * States if the mod should be disabled during a temporal storm.
     * Valid either "true" or "false".
     */
    public bool DisableDuringTemporalStorm { get; private set; } = true;
    
    /* ----- Config functionality ----- */
    
    private const string KeyStableStability = "StabilityAbove.StableStability";
    private const string KeyStabilityHeightPercentage = "StabilityAbove.StabilityHeightPercentage";
    private const string KeyTransitionHeightPercentage = "StabilityAbove.TransitionHeightPercentage";
    private const string KeyDisableDuringTemporalStorm = "StabilityAbove.DisableDuringTemporalStorm";
    
    /**
     * Sets the mod config options as world config values, which can be used for synchronization to the client.
     */
    public void SetWorldConfig(ICoreServerAPI Api) {
        Api.World.Config.SetFloat(KeyStableStability, StableStability);
        Api.World.Config.SetFloat(KeyStabilityHeightPercentage, StabilityHeightPercentage);
        Api.World.Config.SetFloat(KeyTransitionHeightPercentage, TransitionHeightPercentage);
        Api.World.Config.SetBool(KeyDisableDuringTemporalStorm, DisableDuringTemporalStorm);
    }

    /**
     * Loads the mod config options from the world config, which can be used for synchronization from the server.
     */
    public void LoadWorldConfig(ICoreClientAPI Api) {
        if (Api.World.Config.HasAttribute(KeyStableStability))
            StableStability = Api.World.Config.GetFloat(KeyStableStability);
        
        if (Api.World.Config.HasAttribute(KeyStabilityHeightPercentage))
            StabilityHeightPercentage = Api.World.Config.GetFloat(KeyStabilityHeightPercentage);
        
        if (Api.World.Config.HasAttribute(KeyTransitionHeightPercentage))
            TransitionHeightPercentage = Api.World.Config.GetFloat(KeyTransitionHeightPercentage);
        
        if (Api.World.Config.HasAttribute(KeyDisableDuringTemporalStorm))
            DisableDuringTemporalStorm = Api.World.Config.GetBool(KeyDisableDuringTemporalStorm);
    }
    
    /**
     * Tries to load this config file. If no config file exists or if some error occured
     * during loading, a new config with default values is returned.
     */
    public static ModConfig TryLoad(ICoreAPI Api, ILogger ErrorLogger) {
        try {
            var LoadedConfig = Api.LoadModConfig<ModConfig>("StabilityAbove.json") ?? new ModConfig();
            LoadedConfig.CorrectValues();
            
            Api.StoreModConfig(LoadedConfig, "StabilityAbove.json");

            return LoadedConfig;
        } catch (Exception Exception) {
            ErrorLogger.Error("Could not load config! Loading default settings instead.");
            ErrorLogger.Error(Exception);
            
            return new ModConfig();
        }
    }

    private void CorrectValues() {
        StableStability = Math.Clamp(StableStability, 0.0F, 2.0F);
        StabilityHeightPercentage = Math.Clamp(StabilityHeightPercentage, 0.0F, 2.5F);
        TransitionHeightPercentage = Math.Clamp(TransitionHeightPercentage, 0.0F, 1.0F);
    }
}