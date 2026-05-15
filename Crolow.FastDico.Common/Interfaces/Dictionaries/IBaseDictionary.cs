namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    /// <summary>
    /// Represents a disposable base dictionary that stores words in a graph of letter nodes (a trie-like structure) and
    /// supports building, inserting, and persisting entries to files or streams.   
    /// </summary>
    /// <remarks>Implementations construct and maintain an in-memory letter-node graph. Build populates the
    /// structure from a sequence of words and Insert adds a single word. ReadFromFile/ReadFromStream and
    /// SaveToFile/WriteToStream provide persistence; the concrete serialization format is implementation-defined.
    /// BuildNodeId exposes the identifier used during construction. Dispose releases any resources held by the
    /// implementation.</remarks>
    public interface IBaseDictionary : IDisposable
    {
        int BuildNodeId { get; set; }
        ILetterNode Root { get; }
        ILetterNode RootBuild { get; }

        void Build(IEnumerable<string> words);
        void Insert(string word);
        void ReadFromFile(string filePath);
        void SaveToFile(string filePath);
        Stream WriteToStream(Stream stream);
        void ReadFromStream(Stream stream);

    }
}