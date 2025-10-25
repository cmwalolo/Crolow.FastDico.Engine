namespace Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi
{
    public interface IMultiplierData
    {
        int Multiplier { get; set; }
        List<int[]> Positions { get; set; }
    }
}