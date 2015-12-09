using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MsgingNet.Client
{
    public class MyReceiver : IReceiver
    {
        public Task ReceiveCommandAsync(Command command)
        {
            Console.WriteLine("Received a command");
            return Task.FromResult(0);
        }

        public Task ReceiveMessageAsync(Message message)
        {
            Console.WriteLine("Received a message");
            return Task.FromResult(0);
        }

        public Task ReceiveNotificationAsync(Notification notification)
        {
            Console.WriteLine("Received a notification");
            return Task.FromResult(0);
        }
    }
}
