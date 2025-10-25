using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Interfaces.Users
{
    public interface IUserClient
    {
        IOAuthToken OAuthToken { get; set; }
        string Password { get; set; }
        string UserName { get; set; }
        string DeviceId { get; set; }
        KalowId UserId { get; set; }
    }
}