namespace Crolow.TopMachine.Data.Bridge.DataObjects
{
    public interface IDatabaseSetting
    {
        string ConnectionString { get; set; }
        string Database { get; set; }
        string Name { get; set; }
        string Schema { get; set; }
    }
}