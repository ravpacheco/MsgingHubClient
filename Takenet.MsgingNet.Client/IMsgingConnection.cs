using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MsgingNet.Client
{
    interface IMsgingConnection
    {
        Task ConnectAsync();

        Task SendAsync(Node to, string text);

        Task SendAsync(Node to, Document genericDocument);

        //Set presence only here
        void Receive(IReceiver receiverListener);
    }
}
