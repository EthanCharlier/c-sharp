using AstroConverter.Models.Enums;

namespace AstroConverter.Models;

/// <summary>
/// Represents a confirmed exoplanet orbiting a host star.
/// </summary>
public class Exoplanet
{
    // --- Identity ---
    public int ExoplanetId { get; init; }
    public string Name { get; init; }
    public string? AlternativeName { get; init; }  // e.g. Kepler-452b, TOI-700d
    public int StarId { get; init; }  // Foreign key → Star

    // --- Classification ---
    public DetectionMethod DetectionMethod { get; init; }
    public AtmosphereType AtmosphereType { get; init; }

    // --- Physical properties ---
    public double MassEarth { get; init; }  // Mass in Earth masses  (M⊕)
    public double RadiusEarth { get; init; }  // Radius in Earth radii  (R⊕)
    public double? DensityGcm3 { get; init; }  // Bulk density in g/cm³  (null if unresolved)
    public double? SurfaceGravityG { get; init; }  // Surface gravity in g  (null if unresolved)
    public double? EscapeVelocityKms { get; init; }  // Escape velocity in km/s

    // --- Orbital mechanics ---
    public double OrbitalPeriodDays { get; init; }  // Orbital period in days
    public double DistanceFromStarAu { get; init; }  // Semi-major axis in AU
    public double? OrbitalEccentricity { get; init; }  // 0 = circular, <1 = elliptical
    public double? InclinationDeg { get; init; }  // Orbital inclination in degrees
    public double? OrbitalSpeedKms { get; init; }  // Mean orbital speed in km/s

    // --- Thermal properties ---
    public double? EquilibriumTempK { get; init; }  // Equilibrium temperature in K
    public double? SurfaceTempK { get; init; }  // Estimated surface temperature in K
    public bool IsInHabitableZone { get; init; }  // Within host star's habitable zone?

    // --- Discovery metadata ---
    public int? DiscoveryYear { get; init; }
    public string? DiscoveredBy { get; init; }  // Telescope / mission name
    public bool IsConfirmed { get; init; }  // False = candidate only

    public Exoplanet(
        int exoplanetId,
        string name,
        string? alternativeName,
        int starId,
        DetectionMethod detectionMethod,
        AtmosphereType atmosphereType,
        double massEarth,
        double radiusEarth,
        double? densityGcm3,
        double? surfaceGravityG,
        double? escapeVelocityKms,
        double orbitalPeriodDays,
        double distanceFromStarAu,
        double? orbitalEccentricity,
        double? inclinationDeg,
        double? orbitalSpeedKms,
        double? equilibriumTempK,
        double? surfaceTempK,
        bool isInHabitableZone,
        int? discoveryYear,
        string? discoveredBy,
        bool isConfirmed)
    {
        if (exoplanetId <= 0) throw new ArgumentOutOfRangeException(nameof(exoplanetId), "ID must be positive.");
        if (starId <= 0) throw new ArgumentOutOfRangeException(nameof(starId), "Star ID must be positive.");
        if (massEarth <= 0) throw new ArgumentOutOfRangeException(nameof(massEarth), "Mass must be positive.");
        if (radiusEarth <= 0) throw new ArgumentOutOfRangeException(nameof(radiusEarth), "Radius must be positive.");
        if (orbitalPeriodDays <= 0) throw new ArgumentOutOfRangeException(nameof(orbitalPeriodDays), "Orbital period must be positive.");
        if (distanceFromStarAu <= 0) throw new ArgumentOutOfRangeException(nameof(distanceFromStarAu), "Distance from star must be positive.");
        if (densityGcm3.HasValue && densityGcm3.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(densityGcm3), "Density must be positive.");
        if (orbitalEccentricity.HasValue && (orbitalEccentricity.Value < 0 || orbitalEccentricity.Value >= 1))
            throw new ArgumentOutOfRangeException(nameof(orbitalEccentricity), "Eccentricity must be in [0, 1[.");
        if (surfaceTempK.HasValue && surfaceTempK.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(surfaceTempK), "Temperature cannot be negative.");
        if (equilibriumTempK.HasValue && equilibriumTempK.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(equilibriumTempK), "Temperature cannot be negative.");
        if (discoveryYear.HasValue && (discoveryYear.Value < 1992 || discoveryYear.Value > DateTime.Now.Year))
            throw new ArgumentOutOfRangeException(nameof(discoveryYear), "Discovery year is out of valid range.");

        ExoplanetId = exoplanetId;
        StarId = starId;
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        AlternativeName = alternativeName?.Trim();
        DetectionMethod = detectionMethod;
        AtmosphereType = atmosphereType;
        MassEarth = massEarth;
        RadiusEarth = radiusEarth;
        DensityGcm3 = densityGcm3;
        SurfaceGravityG = surfaceGravityG;
        EscapeVelocityKms = escapeVelocityKms;
        OrbitalPeriodDays = orbitalPeriodDays;
        DistanceFromStarAu = distanceFromStarAu;
        OrbitalEccentricity = orbitalEccentricity;
        InclinationDeg = inclinationDeg;
        OrbitalSpeedKms = orbitalSpeedKms;
        EquilibriumTempK = equilibriumTempK;
        SurfaceTempK = surfaceTempK;
        IsInHabitableZone = isInHabitableZone;
        DiscoveryYear = discoveryYear;
        DiscoveredBy = discoveredBy?.Trim();
        IsConfirmed = isConfirmed;
    }

    /// <summary>
    /// Size classification derived from radius (Earth radii).
    /// </summary>
    public PlanetSizeClass SizeClass => RadiusEarth switch
    {
        < 0.5 => Enums.PlanetSizeClass.SubEarth,
        <= 1.25 => PlanetSizeClass.Terrestrial,
        <= 2.0 => PlanetSizeClass.SuperEarth,
        <= 4.0 => PlanetSizeClass.MiniNeptune,
        <= 10.0 => PlanetSizeClass.NeptuneClass,
        <= 15.0 => PlanetSizeClass.Jupiter,
        _ => PlanetSizeClass.SuperJupiter
    };

    /// <summary>
    /// Returns true if this planet could plausibly support liquid water.
    /// </summary>
    public bool IsPotentiallyHabitable =>
        IsInHabitableZone &&
        IsConfirmed &&
        SizeClass is PlanetSizeClass.Terrestrial or PlanetSizeClass.SuperEarth &&
        (EquilibriumTempK is null or (>= 200 and <= 320));

    public override string ToString() =>
        $"[Exoplanet #{ExoplanetId}] {Name} — {SizeClass} | {MassEarth:N2} M⊕ | {OrbitalPeriodDays:N1} d | {(IsInHabitableZone ? "HZ" : "—")}";
}
