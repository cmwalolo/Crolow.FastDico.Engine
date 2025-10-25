namespace Crolow.FastDico.Search;

public class WordResults
{
    public List<Word> Words = new List<Word>();
    public class Word
    {
        public List<ResultTile> Tiles;
        public int Status;

        public Word()
        {
            Tiles = new List<ResultTile>();
        }
        public Word(List<ResultTile> tiles, int status = 0)
        {
            Tiles = tiles;
            Status = status;
        }

        public Word(Word copy)
        {
            Tiles = copy.Tiles.ToList();
        }
    }
    public class ResultTile
    {
        public byte Letter;
        public bool IsJoker;
        public int Status;

        public ResultTile(byte letter, bool isJoker, int status)
        {
            Letter = letter;
            IsJoker = isJoker;
            Status = status;
        }
    }
}
