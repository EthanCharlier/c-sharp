namespace AstroConverter.Models.Enums;

/// <summary>
/// Harvard spectral classification of stars (Morgan–Keenan system).
/// </summary>
public enum SpectralType
{
    O,  // > 30,000 K — Blue supergiant
    B,  // > 10,000 K — Blue-white
    A,  //  > 7,500 K — White
    F,  //  > 6,000 K — Yellow-white
    G,  //  > 5,200 K — Yellow (solar type)
    K,  //  > 3,700 K — Orange
    M,  //  ≤ 3,700 K — Red dwarf
    L,  //            — Brown dwarf (cool)
    T,  //            — Brown dwarf (methane)
    Y   //            — Ultra-cool brown dwarf
}
