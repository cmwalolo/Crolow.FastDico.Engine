namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class LetterBag
{
    public List<Tile> Letters { get; set; }

    public int RemainingLetters => Letters.Count;
    public bool IsEmpty => Letters.Count == 0;

    public LetterBag(GameObjects currentGame)
    {
        Letters = new List<Tile>();

        // Populate the bag according to the distribution
        foreach (var kvp in currentGame.Configuration.TileConfig.LettersByByte)
        {
            for (var i = 0; i < kvp.Value.TotalLetters; i++)
            {
                Letters.Add(new Tile(kvp.Value, null));
            }
        }
    }

    public LetterBag(LetterBag bag)
    {
        Letters = new List<Tile>();
        Letters.AddRange(bag.Letters);
    }
}
