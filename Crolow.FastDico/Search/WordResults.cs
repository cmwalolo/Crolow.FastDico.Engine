namespace Crolow.FastDico.Search;

/// <summary>
/// Contains word-search results and their per-tile metadata.
/// </summary>
public class WordResults
{
    /// <summary>
    /// Gets the list of words returned by a search.
    /// </summary>
    public List<Word> Words = new List<Word>();

    /// <summary>
    /// Represents one candidate word and the status assigned by the search that produced it.
    /// </summary>
    public class Word
    {
        /// <summary>
        /// Gets the ordered tiles that compose the word.
        /// </summary>
        public List<ResultTile> Tiles;

        /// <summary>
        /// Gets the status associated with the word.
        /// </summary>
        public int Status;

        /// <summary>
        /// Initializes a new empty instance of the <see cref="Word"/> class.
        /// </summary>
        public Word()
        {
            Tiles = new List<ResultTile>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Word"/> class with the provided tiles and status.
        /// </summary>
        /// <param name="tiles">Tiles that compose the word.</param>
        /// <param name="status">Status associated with the word.</param>
        public Word(List<ResultTile> tiles, int status = 0)
        {
            Tiles = tiles;
            Status = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Word"/> class by copying another word.
        /// </summary>
        /// <param name="copy">Word to copy.</param>
        public Word(Word copy)
        {
            Tiles = copy.Tiles.ToList();
        }
    }

    /// <summary>
    /// Represents a tile returned by a search, including joker and status metadata.
    /// </summary>
    public class ResultTile
    {
        /// <summary>
        /// Gets the letter byte represented by this tile.
        /// </summary>
        public byte Letter;

        /// <summary>
        /// Gets a value indicating whether this tile was matched by a joker.
        /// </summary>
        public bool IsJoker;

        /// <summary>
        /// Gets the status assigned to this tile by the search operation.
        /// </summary>
        public int Status;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultTile"/> class.
        /// </summary>
        /// <param name="letter">Letter byte represented by the tile.</param>
        /// <param name="isJoker">Indicates whether the tile was matched by a joker.</param>
        /// <param name="status">Status assigned to the tile.</param>
        public ResultTile(byte letter, bool isJoker, int status)
        {
            Letter = letter;
            IsJoker = isJoker;
            Status = status;
        }
    }
}
