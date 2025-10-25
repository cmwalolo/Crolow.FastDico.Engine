namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class PlayableSolution
{
    public string KpiRate { get; set; }
    public List<Tile> Tiles { get; set; }
    public List<Tile> OnBoardTiles { get; set; }

    public Position Position { get; set; }
    public int Points { get; set; }
    public float PlayedTime { get; set; }
    public int Bonus { get; set; }
    public int Pivot { get; set; }

    public PlayerRack Rack { get; set; }
    public bool Finalized { get; set; }

    public PlayableSolution()
    {
        Tiles = new List<Tile>(100);
        Position = new Position(0, 0, 0);
    }
    public PlayableSolution(PlayableSolution copy)
    {
        Tiles = copy.Tiles.ToList();
        Pivot = copy.Pivot;
        Position = new Position(copy.Position);
    }
}
