using Crolow.FastDico.Common.Interfaces.Users;
using Crolow.FastDico.Common.Models.ScrabbleApi.Entities;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Core.Client.Oauth.Models;
using Crolow.TopMachine.Core.Entities.Definitions;
using Crolow.TopMachine.Core.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Newtonsoft.Json.Serialization;

namespace Crolow.TopMachine.Core.Json
{
    public class InterfaceContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, Type> _interfaceToConcreteMap;

        public InterfaceContractResolver()
        {
            _interfaceToConcreteMap = new Dictionary<Type, Type>
            {
                { typeof(IGameConfigModel), typeof(GameConfigModel) },
                { typeof(IBoardGridModel), typeof(BoardGridModel) },
                { typeof(ILetterConfigModel), typeof(LetterConfigModel) },
                { typeof(IWordEntryModel), typeof(WordEntryModel) },
                { typeof(IDictionaryModel), typeof(DictionaryModel) },
                { typeof(IDefinitionModel), typeof(DefinitionModel) },
                { typeof(IListConfigurationModel), typeof(ListConfigurationModel) },
                { typeof(IListItemModel), typeof(ListItemModel) },

                { typeof(IGameDetailModel), typeof(GameDetailModel) },
                { typeof(IGameUserDetailModel), typeof(GameUserDetailModel) },
                { typeof(IPlayableSolutionModel), typeof(PlayableSolutionModel) },

                { typeof(IUserClient), typeof(UserClient) },
                { typeof(IOAuthToken), typeof(OAuthToken) },
                { typeof(IUserSubscription), typeof(UserSubscription) },
            };
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType.IsInterface && _interfaceToConcreteMap.TryGetValue(objectType, out var concreteType))
            {
                return base.CreateContract(concreteType);
            }
            return base.CreateContract(objectType);
        }
    }
}
