namespace AstroConverter.Models.Enums;

/// <summary>
/// Atmospheric composition category (when characterised).
/// </summary>
public enum AtmosphereType
{
    None,           // No atmosphere (e.g. Mercury-like)
    Thin,           // Very thin (e.g. Mars-like)
    RockyVolcanic,  // CO2-dominated
    WaterRich,      // Ocean world candidate
    Hydrogen,       // H₂/He envelope
    GasGiant,       // Thick H₂/He (Jupiter/Saturn-like)
    Unknown
}
