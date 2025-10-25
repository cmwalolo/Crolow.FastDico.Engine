namespace Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi
{
    public interface IBoardGridModel : IDataObject
    {
        List<IMultiplierData> Configuration { get; set; }
        string Name { get; set; }
        int SizeH { get; set; }
        int SizeV { get; set; }
    }
}