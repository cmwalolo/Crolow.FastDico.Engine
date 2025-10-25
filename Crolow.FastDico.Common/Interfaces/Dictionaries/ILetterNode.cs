namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    public interface ILetterNode
    {
        public List<ILetterNode> Children { get; set; }
        byte Control { get; set; }
        byte Letter { get; set; }
        bool IsEnd { get; }
        bool IsPivot { get; }

        void SetEnd();
    }
}