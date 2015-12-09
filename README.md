# MsgingHubClient
C# Client sample for MsgingHub

**This is a proof of concept (work is in progress) :)** 

Simple [Lime Protocol](http://github.com/takenet/lime-csharp/) client.

All you need is:

```c#
var client = new MsgingHubClient()
                   .UsingAccessKey("myaccount", "myAccessKey")

//Is possible yet change all public properties using fluent-style construction

var executionTask = client.ConnectAsync();
```

To send a text message, after starting the client, just call:

```c#
await client.SendMessageAsync("some@node.io", "Hello guys");
```

Or send a message with generic document:
```c#
await client.SendMessageAsync("some@node.io", new Document {});
```

To receive some data you must only implement the IReceiver interface and set on client

```c#
await client.StartReceiving(new MyReceiver());
```

```c#
class MyReceiver : IReceiver
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
```


