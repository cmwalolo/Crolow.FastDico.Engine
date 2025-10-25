using Crolow.TopMachine.Core.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;

namespace Crolow.FastDico.Common.Dto
{
    public class LoadDictionaryRequestDto
    {
        public IDictionaryModel Model { get; set; }

        public LoadDictionaryRequestDto()
        {
            Model = new DictionaryModel();
        }
    }
}
