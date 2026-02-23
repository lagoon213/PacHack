using UnityEngine;

/// <summary>
/// Huidige gedragsmodus van een ghost.
/// 
/// Bepaalt:
/// - target-keuze
/// - snelheid
/// - animaties
/// </summary>
public enum GhostMode
{
    /// <summary>
    /// Scatter:
    /// ghost gaat naar zijn vaste hoek.
    /// </summary>
    Scatter,

    /// <summary>
    /// Chase:
    /// ghost jaagt actief op Pac-Man.
    /// </summary>
    Chase,

    /// <summary>
    /// Frightened:
    /// power pellet actief,
    /// ghost beweegt traag en willekeurig.
    /// </summary>
    Frightened,

    /// <summary>
    /// Returning (eyes):
    /// ghost is opgegeten en keert terug
    /// naar het ghost house.
    /// </summary>
    Returning
}