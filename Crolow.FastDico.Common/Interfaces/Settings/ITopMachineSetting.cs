using Crolow.TopMachine.Data;

namespace Crolow.FastDico.Common.Interfaces.Settings
{
    public interface ITopMachineSetting
    {
        public string OutputFolderPath { get; set; }

        public string AppDataFolderPath { get; set; }
        public string UrlApi { get; set; }
        public string OutputPathHtml { get; set; }
        public IDatabaseSettings DatabaseSettings { get; set; }
        public string JWTToken { get; set; }
        public List<string> Languages { get; set; }
        public int FreeDays { get; set; }
    }
}
