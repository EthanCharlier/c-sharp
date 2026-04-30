namespace AstroConverter.Models.Enums;

/// <summary>
/// Method used to detect or confirm the exoplanet.
/// </summary>
public enum DetectionMethod
{
    Transit,
    RadialVelocity,
    DirectImaging,
    Astrometry,
    Microlensing,
    PulsarTiming,
    TransitTimingVariation,
    Polarimetry,
    Other
}
