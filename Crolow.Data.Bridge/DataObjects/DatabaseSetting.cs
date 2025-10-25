namespace Crolow.TopMachine.Data.Bridge.DataObjects
{
    public enum DbItem
    {
        User,
        Topmachine,
        Dictionary
    }

    public class DatabaseSettings : IDatabaseSettings
    {
        public List<DatabaseSetting> Values { get; set; } = new();

        public List<DatabaseSetting> GetDbConfig(DbItem dbItem)
        {
            return Values.Where(x => x.Schema == dbItem.ToString()).ToList();
        }
    }

    public class DatabaseSetting : IDatabaseSetting
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }



    }
}
