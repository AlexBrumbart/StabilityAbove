using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace StabilityAbove;

public class ModConfig {
    // The stability value to which the world gets stabilized.
    public float StableStability { get; private set; } = 1.0F;
    
    // The percentage of the sea-level height, above which the world is stable.
    public float StabilityHeightPercentage { get; private set; } = 1.0F;

    // The percentage of the sea-level height used as a transition zone below the stable zone.
    public float TransitionHeightPercentage { get; private set; } = 0.10F;
    
    // States if the mod should be disabled during a temporal storm.
    public bool DisableDuringTemporalStorm { get; private set; } = true;
    
    /* ----- Class methods ----- */
    
    /**
     * Sets the mod config options as world config values, which can be used for synchronization to the client.
     */
    public void SetWorldConfig(ICoreServerAPI Api) {
        Api.World.Config.SetFloat("StabilityAbove.StableStability", StableStability);
        Api.World.Config.SetFloat("StabilityAbove.StabilityHeightPercentage", StabilityHeightPercentage);
        Api.World.Config.SetFloat("StabilityAbove.TransitionHeightPercentage", TransitionHeightPercentage);
        Api.World.Config.SetBool("StabilityAbove.DisableDuringTemporalStorm", DisableDuringTemporalStorm);
    }

    /**
     * Loads the mod config options from the world config, which can be used for synchronization from the server.
     */
    public void LoadWorldConfig(ICoreClientAPI Api) {
        if (Api.World.Config.HasAttribute("StabilityAbove.StableStability"))
            StableStability = Api.World.Config.GetFloat("StabilityAbove.StableStability");
        
        if (Api.World.Config.HasAttribute("StabilityAbove.StabilityHeightPercentage"))
            StabilityHeightPercentage = Api.World.Config.GetFloat("StabilityAbove.StabilityHeightPercentage");
        
        if (Api.World.Config.HasAttribute("StabilityAbove.TransitionHeightPercentage"))
            TransitionHeightPercentage = Api.World.Config.GetFloat("StabilityAbove.TransitionHeightPercentage");
        
        if (Api.World.Config.HasAttribute("StabilityAbove.DisableDuringTemporalStorm"))
            DisableDuringTemporalStorm = Api.World.Config.GetBool("StabilityAbove.DisableDuringTemporalStorm");
    }
    
    /**
     * Tries to load this config file. If no config file exists or if some error occured
     * during loading, a new config with default values is returned.
     */
    public static ModConfig TryLoad(ICoreAPI Api, ILogger ErrorLogger) {
        try {
            var LoadedConfig = Api.LoadModConfig<ModConfig>("StabilityAbove.json") ?? new ModConfig();
            Api.StoreModConfig(LoadedConfig, "StabilityAbove.json");

            return LoadedConfig;
        } catch (Exception Exception) {
            ErrorLogger.Error("Could not load config! Loading default settings instead.");
            ErrorLogger.Error(Exception);
            
            return new ModConfig();
        }
    }
}