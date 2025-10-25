namespace Crolow.TopMachine.Data.Bridge.Entities.Definitions
{
    public interface IDefinitionModel
    {
        string CatGram { get; set; }
        List<string> Definitions { get; set; }
        List<string> Domains { get; set; }
        List<string> Usages { get; set; }
        string Word { get; set; }
    }
}