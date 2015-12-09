﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Transport.Tcp;
using System.Threading;
using Lime.Protocol.Client;
using Lime.Protocol.Security;
using Lime.Messaging.Resources;
using Lime.Protocol.Network;
using Lime.Messaging.Contents;

namespace Takenet.MsgingNet.Client
{
    class MsgingConnection : IMsgingConnection, IDisposable
    {
        public Node Node { get; set; }
        public string ApiKey { get; set; }
        public string HostName { get; set; }
        public int PortNumber { get; set; }

        //Build can insert more props here

        ClientChannel _clientChannel;
        TcpTransport _tcpTransport;
        Session _session;
        CancellationTokenSource _receiveTokenSource;

        public MsgingConnection(Node yourNode, string apiKey)
        {
            Node = yourNode;
            ApiKey = apiKey;
            HostName = "msging.net";
            PortNumber = 55321;
        }

        public async Task ConnectAsync()
        {
            try
            {
                // Creates a new transport and connect to the server
                var serverUri = new Uri(string.Format("net.tcp://{0}:{1}", HostName, PortNumber));
                _tcpTransport = new TcpTransport();

                await _tcpTransport.OpenAsync(serverUri, CancellationToken.None).ConfigureAwait(false);

                // Creates a new client channel
                var sendTimeout = TimeSpan.FromSeconds(60);

                _clientChannel = new ClientChannel(_tcpTransport, sendTimeout);
                
                using (var authentication = new KeyAuthentication
                {
                    Key = ApiKey
                })
                {
                    _session = await _clientChannel.EstablishSessionAsync(
                        (compressionOptions) => SessionCompression.None,     // Compression selector 
                        (encryptionOptions) => SessionEncryption.TLS,       // Encryption selector
                        Node,                                                   // Client identity
                        (authenticationSchemes, roundtrip) => authentication,             // Authentication
                        Node.Instance ?? "default",
                        CancellationToken.None);

                    if (_session.State != SessionState.Established)
                    {
                        throw new Exception(_session.Reason.ToString());
                    }

                    var finishedSessionTask = _clientChannel
                            .ReceiveFinishedSessionAsync(CancellationToken.None)
                            .ContinueWith(HandleReceiveFinishedSession);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during establishing session.");
            }
        }

        public async void Receive(IReceiver receiverListener)
        {
            try
            {
                if (_session.State == SessionState.Established)
                {
                    var delay = TimeSpan.FromSeconds(5);

                    using (
                    var cts = new CancellationTokenSource(delay))
                    {
                        await _clientChannel.SetResourceAsync(new LimeUri("/presence"), new Presence { Status = PresenceStatus.Available, RoutingRule = Node.Instance == null ? RoutingRule.Identity : RoutingRule.Instance }, cts.Token);
                    }

                    _receiveTokenSource = new CancellationTokenSource();
                    var consumeMessagesTask = ConsumeMessagesAsync(receiverListener, _receiveTokenSource.Token).WithPassiveCancellation();
                    //var consumeCommandsTask = ConsumeCommandsAsync(receiverListener, _receiverTokenSource.Token).WithPassiveCancellation();
                    //var consumeNotificationsTask = ConsumeNotificationsAsync(receiverListener, _receiverTokenSource.Token).WithPassiveCancellation();

                    Console.WriteLine("Session established. Id: {0} - Local node: {1} - Remote node: {2}", _session.Id, _session.To, _session.From);
                }
                else
                {
                    Console.Write("Could not establish the session. ");
                    if (_session.Reason != null)
                    {
                        Console.Write("Reason: {0}", _session.Reason);
                    }
                    Console.WriteLine();
                    throw new Exception(_session.Reason.ToString());
                }

                await _clientChannel.Transport.CloseAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
            }
        }

        async Task ConsumeMessagesAsync(IReceiver listener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _clientChannel.ReceiveMessageAsync(cancellationToken);

                await listener.ReceiveMessageAsync(message);

                Console.WriteLine("Message with id '{0}' received from '{1}': {2}", message.Id, message.From ?? _clientChannel.RemoteNode, message.Content);
            }
        }

        async Task ConsumeCommandAsync(IReceiver listener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var command = await _clientChannel.ReceiveCommandAsync(cancellationToken);

                await listener.ReceiveCommandAsync(command);

                Console.WriteLine("Command with id '{0}' received from '{1}': {2}", command.Id, command.From ?? _clientChannel.RemoteNode, command.Method);
            }
        }

        async Task ConsumeNotificationAsync(IReceiver listener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var notification = await _clientChannel.ReceiveNotificationAsync(cancellationToken);

                await listener.ReceiveNotificationAsync(notification);

                Console.WriteLine("Notification with id '{0}' received from '{1}': {2}", notification.Id, notification.From ?? _clientChannel.RemoteNode, notification.Event);
            }
        }

        public async Task SendAsync(Node to, Document genericDocument)
        {
            if (_clientChannel.State == SessionState.Established)
            {
                var message = new Message
                {
                    To = to,
                    Content = genericDocument
                };

                await _clientChannel.SendMessageAsync(message).ConfigureAwait(false);
            }
            else
            {
                throw new Exception(_session.Reason.ToString());
            }
        }

        public async Task SendAsync(Node to, string text)
        {
            if (_clientChannel.State == SessionState.Established)
            {
                var message = new Message
                {
                    To = to,
                    Content = new PlainText
                    {
                        Text = text
                    }
                };

                await _clientChannel.SendMessageAsync(message).ConfigureAwait(false);
            }
            else
            {
                throw new Exception(_session.Reason.ToString());
            }
        }

        void HandleReceiveFinishedSession(Task<Session> session)
        {
            Console.WriteLine($"The session was finished with status: {session.Status}");
            CancelReceiveTasks();
        }

        void CancelReceiveTasks()
        {
            _receiveTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _clientChannel.Dispose();
        }
    }

    public static class TaskExtensions
    {
        public static Task WithPassiveCancellation(this Task task)
        {
            return task.ContinueWith(t => t, TaskContinuationOptions.None);
        }
    }
}