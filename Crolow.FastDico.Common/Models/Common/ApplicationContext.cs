using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.Common
{
    public class ApplicationContext
    {
        public static ILetterConfigModel DefaultLetterConfig { get; set; }
        public static CurrentGame CurrentGame { get; set; }

    }
}
