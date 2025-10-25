using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Entities;
public class BoardGridModel : DataObject, IBoardGridModel
{

    public class MultiplierData : IMultiplierData
    {
        public int Multiplier { get; set; }
        public List<int[]> Positions { get; set; }
    }

    public string Name { get; set; }
    public int SizeH { get; set; }
    public int SizeV { get; set; }
    public List<IMultiplierData> Configuration { get; set; }
}
