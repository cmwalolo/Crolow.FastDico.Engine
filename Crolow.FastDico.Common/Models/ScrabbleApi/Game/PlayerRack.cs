namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class PlayerRack
{
    public List<Tile> Tiles { get; set; }

    public PlayerRack()
    {
        Tiles = new List<Tile>();
    }

    public PlayerRack(PlayerRack oldRack)
    {
        Tiles = new List<Tile>();
        foreach (var l in oldRack.Tiles)
        {
            Tiles.Add(l);
        }
    }
    public PlayerRack(List<Tile> tiles)
    {
        Tiles = tiles.ToList();
    }
}
