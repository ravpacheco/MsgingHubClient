using Lime.Protocol;
using System.Threading.Tasks;

namespace Takenet.MsgingNet.Client
{
    public interface IReceiver
    {
        Task ReceiveCommandAsync(Command command);
        Task ReceiveMessageAsync(Message message);
        Task ReceiveNotificationAsync(Notification notification);
    }
}