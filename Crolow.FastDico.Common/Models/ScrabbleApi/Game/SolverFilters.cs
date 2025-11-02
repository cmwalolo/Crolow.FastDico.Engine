namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game
{
    public class SolverFilters
    {
        public bool PickallResults { get; set; }
        public List<int>[] Filters { get; set; } = new List<int>[2];

        public List<Tile> LettersInRack { get; set; } = new List<Tile>();
        public List<Tile> MandatoryLettersInRack { get; set; } = new List<Tile>();

        public int ForceStartBoostRound { get; set; } = 0;
        public int MinimalRack { get; set; } = 0;

        public SolverFilters()
        {
            Filters[0] = new List<int>();
            Filters[1] = new List<int>();
        }
    }
}
