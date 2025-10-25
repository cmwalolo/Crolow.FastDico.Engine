using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;


public class AllSolutions
{
    private readonly PriorityQueue<PlayableSolution, int> _queue;
    private readonly int _capacity;

    public AllSolutions(int capacity)
    {
        _queue = new PriorityQueue<PlayableSolution, int>();
        _capacity = capacity;
    }

    public void Add(PlayableSolution sol)
    {
        if (_queue.Count < _capacity)
        {
            _queue.Enqueue(sol, sol.Points);
        }
        else if (_queue.Peek().Points < sol.Points) // Compare with smallest
        {
            _queue.Dequeue();
            _queue.Enqueue(sol, sol.Points);
        }
    }

    public List<PlayableSolution> GetBest()
    {
        var list = new List<PlayableSolution>(_queue.UnorderedItems.Count);
        foreach (var (element, _) in _queue.UnorderedItems)
            list.Add(element);

        list.Sort((a, b) => b.Points.CompareTo(a.Points)); // Highest first
        return list;
    }

    public void Clear()
    {
        _queue.Clear();
    }
}

public class PlayedRounds
{
    public IGameConfigModel Config { get; set; }
    public int MaxPoints { get; set; }
    public int MaxSubTopPoints { get; set; }
    public int PickAll { get; set; }

    public List<PlayableSolution> Tops { get; set; }
    public List<PlayableSolution> SubTops { get; set; }
    public AllSolutions AllRounds { get; set; }
    public PlayableSolution CurrentRound { get; set; }

    public PlayerRack PlayerRack { get; set; }

    public PlayedRounds(IGameConfigModel config, List<Tile> rack, int pickAll)
    {
        Config = config;
        Tops = new List<PlayableSolution>();
        SubTops = new List<PlayableSolution>();
        AllRounds = new AllSolutions(10000);
        CurrentRound = new PlayableSolution();
        PlayerRack = new PlayerRack(rack);
        PickAll = pickAll;
    }
}
