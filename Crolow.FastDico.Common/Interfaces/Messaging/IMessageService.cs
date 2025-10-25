using Crolow.FastDico.Common.Models.Common.Messaging;

namespace Crolow.FastDico.Common.Interfaces.Messaging
{
    public interface IMessageService
    {
        event Action<MessageModel> OnMessage;
        event Action<HubMessageModel> OnHubMessage;

        void SendMessage(MessageModel message);
        void SendHubMessage(HubMessageModel message);
    }
}