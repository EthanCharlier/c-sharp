using AstroConverter.Models.Enums;

namespace AstroConverter.Models;

/// <summary>
/// Represents a galaxy in the observable universe.
/// </summary>
public class Galaxy
{
    // --- Identity ---
    public int GalaxyId { get; init; }
    public string Name { get; init; }
    public string? AlsoKnownAs { get; init; }  // Alternate designation  (e.g. M31, NGC 224)
    public string Constellation { get; init; }

    // --- Classification ---
    public GalaxyType Type { get; init; }

    // --- Physical properties ---
    public double DistanceMly { get; init; }  // Distance in millions of light-years
    public double DiameterKly { get; init; }  // Diameter in thousands of light-years
    public long? StarCount { get; init; }  // Estimated star count  (null if unknown)
    public double? MassSolar { get; init; }  // Total mass in solar masses × 10⁹  (null if unknown)
    public double? RedShift { get; init; }  // Cosmological redshift z  (null if not measured)

    // --- Observational data ---
    public double? ApparentMagnitude { get; init; }  // Visual brightness as seen from Earth
    public string? DiscoveredBy { get; init; }
    public int? DiscoveryYear { get; init; }

    public Galaxy(
        int galaxyId,
        string name,
        string? alsoKnownAs,
        string constellation,
        GalaxyType type,
        double distanceMly,
        double diameterKly,
        long? starCount,
        double? massSolar,
        double? redShift,
        double? apparentMagnitude,
        string? discoveredBy,
        int? discoveryYear)
    {
        if (galaxyId <= 0) throw new ArgumentOutOfRangeException(nameof(galaxyId), "ID must be positive.");
        if (distanceMly < 0) throw new ArgumentOutOfRangeException(nameof(distanceMly), "Distance cannot be negative.");
        if (diameterKly <= 0) throw new ArgumentOutOfRangeException(nameof(diameterKly), "Diameter must be positive.");
        if (starCount.HasValue && starCount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(starCount), "Star count cannot be negative.");
        if (massSolar.HasValue && massSolar.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(massSolar), "Mass must be positive.");
        if (discoveryYear.HasValue && (discoveryYear.Value < 1600 || discoveryYear.Value > DateTime.Now.Year))
            throw new ArgumentOutOfRangeException(nameof(discoveryYear), "Discovery year is out of valid range.");

        GalaxyId = galaxyId;
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Constellation = Guard.NotNullOrWhiteSpace(constellation, nameof(constellation));
        AlsoKnownAs = alsoKnownAs?.Trim();
        Type = type;
        DistanceMly = distanceMly;
        DiameterKly = diameterKly;
        StarCount = starCount;
        MassSolar = massSolar;
        RedShift = redShift;
        ApparentMagnitude = apparentMagnitude;
        DiscoveredBy = discoveredBy?.Trim();
        DiscoveryYear = discoveryYear;
    }

    /// <summary>
    /// Returns true if the galaxy is within the Local Group (~3 Mly).
    /// </summary>
    public bool IsLocalGroup => DistanceMly <= 3.0;

    public override string ToString() =>
        $"[Galaxy #{GalaxyId}] {Name}{(AlsoKnownAs is not null ? $" ({AlsoKnownAs})" : "")} — {Type} | {DistanceMly:N0} Mly";
}
