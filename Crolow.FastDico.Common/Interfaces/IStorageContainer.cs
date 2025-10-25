namespace Crolow.FastDico.Common.Interfaces
{
    public interface IStorageContainer
    {
        Task<T?> GetValue<T>(string key, bool encrypt);
        Task SetValue<T>(string key, T value, bool encrypt);
        Task RemoveValue(string key);
    }
}