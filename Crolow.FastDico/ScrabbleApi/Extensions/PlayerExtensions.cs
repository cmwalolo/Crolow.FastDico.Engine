using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

/// <summary>
/// Provides helper methods for player rack assignment and play actions.
/// </summary>
public static class PlayerExtensions
{
    /// <summary>
    /// Adds the supplied letters to the player's rack.
    /// </summary>
    /// <param name="p">Player whose rack is updated.</param>
    /// <param name="Letters">Letters to add to the rack.</param>
    public static void SetRack(this Player p, Tile[] Letters)
    {
        p.Rack.Tiles.AddRange(Letters);
    }

    /// <summary>
    /// Plays a word for the player.
    /// </summary>
    /// <param name="p">Player performing the action.</param>
    /// <param name="word">Tiles that compose the played word.</param>
    /// <returns><c>true</c> when the play action is accepted.</returns>
    public static bool PlayWord(this Player p, List<Tile> word)
    {
        return true;
    }
}
