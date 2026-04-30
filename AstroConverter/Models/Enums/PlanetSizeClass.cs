namespace AstroConverter.Models.Enums;

/// <summary>
/// Size classification of an exoplanet based on its radius (Earth radii).
/// </summary>
public enum PlanetSizeClass
{
    SubEarth,       // R < 0.5
    Terrestrial,    // 0.5 ≤ R ≤ 1.25
    SuperEarth,     // 1.25 < R ≤ 2.0
    MiniNeptune,    // 2.0  < R ≤ 4.0
    NeptuneClass,   // 4.0  < R ≤ 10.0
    Jupiter,        // 10.0 < R ≤ 15.0
    SuperJupiter    // R > 15.0
}
