namespace StabilityAbove;

public class ModConfig {
    // The percentage of the sea-level height, above which the world is stable.
    public float StabilityHeightPercentage { get; set; } = 1.0F;

    // The percentage of the sea-level height used as a transition zone below the stable zone.
    public float TransitionHeightPercentage { get; set; } = 0.10F;
}