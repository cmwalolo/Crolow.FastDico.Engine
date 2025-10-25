namespace Crolow.TopMachine.Data.Bridge
{
    public interface IDocumentSession : IDisposable
    {
        Task AbortAsync();
        Task CommitAsync();
    }
}