﻿using Lime.Protocol;
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

        Task SendMessageAsync(Node to, string text);

        Task SendMessageAsync(Node to, Document genericDocument);

        //Set presence only here
        void SetReceiver(IReceiver receiverListener);
    }
}
