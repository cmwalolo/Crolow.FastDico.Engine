namespace Crolow.FastDico.Common.Interfaces.Users
{
    public interface IUserClientContext
    {
        static IUserClient Instance { get; set; }
    }
}