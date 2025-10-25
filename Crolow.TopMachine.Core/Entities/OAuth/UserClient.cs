using Crolow.FastDico.Common.Interfaces.Users;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Client.Oauth.Models
{
    public class UserClient : IUserClient
    {
        public IOAuthToken OAuthToken { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public KalowId UserId { get; set; } = KalowId.Empty;
    }
}
