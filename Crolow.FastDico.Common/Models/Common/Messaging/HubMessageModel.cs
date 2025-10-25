namespace Crolow.FastDico.Common.Models.Common.Messaging
{
    public class HubMessageModel
    {
        public string HubSource { get; set; }
        public string Type { get; set; }
        public object MessageObject { get; set; }

        public T GetObject<T>()
        {
            return MessageObject != null ? (T)MessageObject : default;
        }
    }
}
