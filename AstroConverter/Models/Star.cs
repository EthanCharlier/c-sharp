using AstroConverter.Models.Enums;

namespace AstroConverter.Models;

/// <summary>
/// Represents a star, potentially hosting exoplanets.
/// </summary>
public class Star
{
    // --- Identity ---
    public int StarId { get; init; }
    public string Name { get; init; }
    public string? CatalogueId { get; init; }  // e.g. HD 209458, Gliese 667C
    public int GalaxyId { get; init; }  // Foreign key → Galaxy
    public string Constellation { get; init; }

    // --- Classification ---
    public SpectralType SpectralType { get; init; }
    public LuminosityClass LuminosityClass { get; init; }

    // --- Physical properties ---
    public double DistanceLy { get; init; }  // Distance from Earth in light-years
    public double MassSolar { get; init; }  // Mass in solar masses
    public double RadiusSolar { get; init; }  // Radius in solar radii
    public double LuminositySolar { get; init; }  // Luminosity in solar luminosities
    public double TemperatureK { get; init; }  // Effective surface temperature in Kelvin
    public double? AgeGyr { get; init; }  // Age in billions of years  (null if unknown)
    public double? Metallicity { get; init; }  // [Fe/H] relative to Sun  (null if unknown)

    // --- Kinematics ---
    public double? RadialVelocityKms { get; init; } // Radial velocity in km/s
    public double? ProperMotionMasyr { get; init; } // Proper motion in mas/yr

    // --- System ---
    public bool IsMultiStar { get; init; } // Part of a multiple-star system?
    public int? KnownPlanetCount { get; init; } // Confirmed planets  (null if not surveyed)

    public Star(
        int starId,
        string name,
        string? catalogueId,
        int galaxyId,
        string constellation,
        SpectralType spectralType,
        LuminosityClass luminosityClass,
        double distanceLy,
        double massSolar,
        double radiusSolar,
        double luminositySolar,
        double temperatureK,
        double? ageGyr,
        double? metallicity,
        double? radialVelocityKms,
        double? properMotionMasyr,
        bool isMultiStar,
        int? knownPlanetCount)
    {
        if (starId <= 0) throw new ArgumentOutOfRangeException(nameof(starId), "ID must be positive.");
        if (galaxyId <= 0) throw new ArgumentOutOfRangeException(nameof(galaxyId), "Galaxy ID must be positive.");
        if (distanceLy < 0) throw new ArgumentOutOfRangeException(nameof(distanceLy), "Distance cannot be negative.");
        if (massSolar <= 0) throw new ArgumentOutOfRangeException(nameof(massSolar), "Mass must be positive.");
        if (radiusSolar <= 0) throw new ArgumentOutOfRangeException(nameof(radiusSolar), "Radius must be positive.");
        if (luminositySolar <= 0) throw new ArgumentOutOfRangeException(nameof(luminositySolar), "Luminosity must be positive.");
        if (temperatureK <= 0) throw new ArgumentOutOfRangeException(nameof(temperatureK), "Temperature must be positive.");
        if (ageGyr.HasValue && ageGyr.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(ageGyr), "Age cannot be negative.");
        if (knownPlanetCount.HasValue && knownPlanetCount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(knownPlanetCount), "Planet count cannot be negative.");

        StarId = starId;
        GalaxyId = galaxyId;
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Constellation = Guard.NotNullOrWhiteSpace(constellation, nameof(constellation));
        CatalogueId = catalogueId?.Trim();
        SpectralType = spectralType;
        LuminosityClass = luminosityClass;
        DistanceLy = distanceLy;
        MassSolar = massSolar;
        RadiusSolar = radiusSolar;
        LuminositySolar = luminositySolar;
        TemperatureK = temperatureK;
        AgeGyr = ageGyr;
        Metallicity = metallicity;
        RadialVelocityKms = radialVelocityKms;
        ProperMotionMasyr = properMotionMasyr;
        IsMultiStar = isMultiStar;
        KnownPlanetCount = knownPlanetCount;
    }

    /// <summary>
    /// Full MK spectral designation (e.g. G2V).
    /// </summary>
    public string MKDesignation => $"{SpectralType}{LuminosityClass}";

    /// <summary>
    /// Habitable zone inner edge in AU (Kopparapu et al. 2013 — conservative).
    /// </summary>
    public double HabitableZoneInnerAu => Math.Sqrt(LuminositySolar / 1.1);

    /// <summary>
    /// Habitable zone outer edge in AU.
    /// </summary>
    public double HabitableZoneOuterAu => Math.Sqrt(LuminositySolar / 0.53);

    /// <summary>
    /// Returns true if the star is a main-sequence dwarf.
    /// </summary>
    public bool IsMainSequence => LuminosityClass == LuminosityClass.V;

    public override string ToString() =>
        $"[Star #{StarId}] {Name} ({MKDesignation}) — {DistanceLy:N1} ly | {TemperatureK:N0} K";
}
