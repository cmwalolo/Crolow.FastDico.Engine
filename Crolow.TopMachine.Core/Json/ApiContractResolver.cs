using Crolow.FastDico.Common.Interfaces.Users;
using Crolow.FastDico.Common.Models.ScrabbleApi.Entities;
using Crolow.FastDico.Common.Models.ScrabbleApi.Entities.Partials;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Common.Models.ScrabbleApi.Kpi;
using Crolow.FastDico.Data.Bridge.Entities.Puzzling;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Core.Client.Oauth.Models;
using Crolow.TopMachine.Core.Entities.Definitions;
using Crolow.TopMachine.Core.Entities.Lists;
using Crolow.TopMachine.Core.Entities.Puzzling;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Newtonsoft.Json.Serialization;
using static Crolow.FastDico.Common.Models.ScrabbleApi.Entities.BoardGridModel;

namespace Crolow.TopMachine.Core.Json
{
    public class ApiContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            return objectType switch
            {

                // Map interface types to concrete types
                { } when objectType == typeof(ITileConfig) => base.CreateContract(typeof(TileConfig)),
                { } when objectType == typeof(IMultiplierData) => base.CreateContract(typeof(MultiplierData)),
                { } when objectType == typeof(IGameConfigModel) => base.CreateContract(typeof(GameConfigModel)),
                { } when objectType == typeof(IBoardGridModel) => base.CreateContract(typeof(BoardGridModel)),
                { } when objectType == typeof(IEvaluatorConfig) => base.CreateContract(typeof(EvaluatorConfig)),
                { } when objectType == typeof(ILetterConfigModel) => base.CreateContract(typeof(LetterConfigModel)),
                { } when objectType == typeof(IDictionaryModel) => base.CreateContract(typeof(DictionaryModel)),
                { } when objectType == typeof(IListConfigurationModel) => base.CreateContract(typeof(ListConfigurationModel)),
                { } when objectType == typeof(IListItemModel) => base.CreateContract(typeof(ListItemModel)),
                { } when objectType == typeof(IListSolutionModel) => base.CreateContract(typeof(ListSolutionModel)),

                { } when objectType == typeof(IGameDetailModel) => base.CreateContract(typeof(GameDetailModel)),
                { } when objectType == typeof(IGameUserDetailModel) => base.CreateContract(typeof(GameUserDetailModel)),
                { } when objectType == typeof(IPlayableSolutionModel) => base.CreateContract(typeof(PlayableSolutionModel)),
                { } when objectType == typeof(IGameUserStatisticsModel) => base.CreateContract(typeof(GameUserStatisticsModel)),
                { } when objectType == typeof(IKpiRateSummary) => base.CreateContract(typeof(KpiRateSummary)),
                { } when objectType == typeof(IKpiRateSummaryItem) => base.CreateContract(typeof(KpiRateSummaryItem)),

                { } when objectType == typeof(IGameRollerConfigModel) => base.CreateContract(typeof(GameRollerConfigModel)),

                { } when objectType == typeof(IUserClient) => base.CreateContract(typeof(UserClient)),
                { } when objectType == typeof(IOAuthToken) => base.CreateContract(typeof(OAuthToken)),


                _ => base.CreateContract(objectType) // Default case
            };
        }
    }
}