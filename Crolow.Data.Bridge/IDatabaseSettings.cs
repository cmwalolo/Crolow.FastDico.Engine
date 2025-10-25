
using Crolow.TopMachine.Data.Bridge.DataObjects;

namespace Crolow.TopMachine.Data
{
    public interface IDatabaseSettings
    {
        List<DatabaseSetting> Values { get; set; }
        List<DatabaseSetting> GetDbConfig(DbItem dbItem);
    }
}