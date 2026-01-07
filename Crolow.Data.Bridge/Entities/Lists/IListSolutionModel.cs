namespace Crolow.TopMachine.Core.Entities.Lists
{
    public interface IListSolutionModel
    {
        int PrefixLength { get; set; }
        int SuffixLength { get; set; }
        string Prefix { get; set; }
        string Suffix { get; set; }
        string Solution { get; set; }
    }
}