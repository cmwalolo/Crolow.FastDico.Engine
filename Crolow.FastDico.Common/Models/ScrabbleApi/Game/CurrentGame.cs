namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class CurrentGame
{
    public string FileName { get; set; }

    public GameObjects GameObjects { get; set; }
    public GameControllersSetup ControllersSetup { get; set; }

    public GameHistory History { get; set; } = new GameHistory();
    public List<KeyValuePair<string, Exception>> DebugInfo { get; set; }
}
