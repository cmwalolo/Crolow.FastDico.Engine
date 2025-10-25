using Kalow.Apps.Common.DataTypes;
using System.Security.Claims;

namespace Crolow.FastDico.Common.Interfaces.Users
{
    public interface IOAuthToken
    {
        List<Claim> Claims { get; set; }
        string? Email { get; }
        DateTime Expiration { get; set; }
        string? Name { get; }
        KalowId SerialNumber { get; }
        string DeviceId { get; }
        List<IUserSubscription>? Subscriptions { get; }
        string Token { get; set; }
    }
}