namespace Crolow.TopMachine.Core.Client.Oauth.Models
{
    using Crolow.FastDico.Common.Interfaces.Users;
    using Kalow.Apps.Common.DataTypes;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public class UserSubscription : IUserSubscription
    {
        public string Language { get; set; } = "";
        public DateTime EndDate { get; set; }
    }

    public class OAuthToken : IOAuthToken
    {
        public string Token { get; set; }

        [JsonIgnore] public DateTime Expiration { get; set; }
        [JsonIgnore] public List<Claim> Claims { get; set; } = new List<Claim>();


        [JsonIgnore]
        public string? Name => Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        [JsonIgnore]
        public string? Email => Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        [JsonIgnore]
        public KalowId SerialNumber => new KalowId(Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value);

        [JsonIgnore]
        public string DeviceId => Claims.FirstOrDefault(c => c.Type == "DeviceId")?.Value;


        [JsonIgnore]

        public List<IUserSubscription>? Subscriptions
        {
            get
            {
                var subClaim = Claims.FirstOrDefault(c => c.Type == "Subscriptions")?.Value;
                if (string.IsNullOrWhiteSpace(subClaim))
                    return null;

                try
                {
                    return JsonConvert.DeserializeObject<List<UserSubscription>>(subClaim).ToList<IUserSubscription>();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
