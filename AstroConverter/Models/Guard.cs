using AstroConverter.Models.Enums;

namespace AstroConverter.Models;

/// <summary>
/// Shared validation utility used across all models.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws <see cref="ArgumentException"/> if the value is null or whitespace.
    /// </summary>
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' cannot be null or empty.", paramName);
        return value.Trim();
    }
}

/// <summary>
/// Fully joined read-only projection of Exoplanet + Star + Galaxy.
/// Produced by LINQ joins in the Service layer — contains no business logic.
/// </summary>
public class ExoplanetView
{
    // --- Exoplanet ---
    public int ExoplanetId { get; init; }
    public string ExoplanetName { get; init; } = "";
    public string? AlternativeName { get; init; }
    public double MassEarth { get; init; }
    public double RadiusEarth { get; init; }
    public double? DensityGcm3 { get; init; }
    public double? SurfaceGravityG { get; init; }
    public double OrbitalPeriodDays { get; init; }
    public double DistanceFromStarAu { get; init; }
    public double? OrbitalEccentricity { get; init; }
    public double? EquilibriumTempK { get; init; }
    public double? SurfaceTempK { get; init; }
    public bool IsInHabitableZone { get; init; }
    public bool IsConfirmed { get; init; }
    public bool IsPotentiallyHabitable { get; init; }
    public PlanetSizeClass SizeClass { get; init; }
    public DetectionMethod DetectionMethod { get; init; }
    public AtmosphereType AtmosphereType { get; init; }
    public int? DiscoveryYear { get; init; }
    public string? DiscoveredBy { get; init; }

    // --- Star ---
    public int StarId { get; init; }
    public string StarName { get; init; } = "";
    public string? StarCatalogueId { get; init; }
    public SpectralType SpectralType { get; init; }
    public LuminosityClass LuminosityClass { get; init; }
    public string MKDesignation { get; init; } = "";
    public double StarDistanceLy { get; init; }
    public double StarMassSolar { get; init; }
    public double StarTemperatureK { get; init; }
    public double HabitableZoneInnerAu { get; init; }
    public double HabitableZoneOuterAu { get; init; }

    // --- Galaxy ---
    public int GalaxyId { get; init; }
    public string GalaxyName { get; init; } = "";
    public GalaxyType GalaxyType { get; init; }
    public double GalaxyDistanceMly { get; init; }
}
