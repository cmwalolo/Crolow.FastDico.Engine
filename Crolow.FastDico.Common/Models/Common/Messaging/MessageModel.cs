using Crolow.FastDico.Common.Enums;

namespace Crolow.FastDico.Common.Models.Common.Messaging
{
    public partial class MessageModel
    {

        public MessageType Type { get; set; }
        public bool MessageResult { get; set; }
        public object MessageObject { get; set; }
        public string Message { get; set; }

        public T GetMessage<T>()
        {
            return MessageObject != null ? (T)MessageObject : default;
        }
    }
}
