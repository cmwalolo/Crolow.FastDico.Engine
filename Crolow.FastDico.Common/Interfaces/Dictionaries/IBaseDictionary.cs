namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
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