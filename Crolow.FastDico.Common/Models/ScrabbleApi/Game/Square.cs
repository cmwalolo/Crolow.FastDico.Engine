namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game
{

    public class Square
    {
        public int LetterMultiplier { get; set; } = 1;
        public int WordMultiplier { get; set; } = 1;
        public bool IsBorder { get; set; } = true;
        public Tile CurrentLetter { get; set; }

        public int Status { get; set; } = -1;
        public uint[] Pivots { get; set; } = new uint[2] { uint.MaxValue, uint.MaxValue };
        public int[] PivotPoints { get; set; } = new int[2];
        public int[] PivotLetters { get; set; } = new int[2];
    }
}
