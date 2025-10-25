namespace Crolow.TopMachine.Data.Bridge.Entities.Definitions
{
    public interface IWordEntryModel : IDataObject
    {
        List<IDefinitionModel> Definitions { get; set; }
        string Etymology { get; set; }
        string NormalizedWord { get; set; }
        string Source { get; set; }
        string Word { get; set; }

        int RateComplexity { get; set; }
        int RatePopularity { get; set; }
        int RateGlobal { get; set; }
        int DifficultyLevel { get; set; }
    }
}