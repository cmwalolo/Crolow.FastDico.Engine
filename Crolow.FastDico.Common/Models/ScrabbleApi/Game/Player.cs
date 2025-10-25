namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class Player
{
    public string Name { get; }
    public PlayerRack Rack { get; set; } // List of letters (bytes) on the player's rack
    public Player(string name)
    {
        Name = name;
    }
}
