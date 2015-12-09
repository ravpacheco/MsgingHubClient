using System.Threading.Tasks;
using Lime.Protocol;

namespace Takenet.MsgingNet.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Init().Wait();
        }

        static async Task Init()
        {
            using (var client = new MsgingHubClient("someHost")
                   .UsingAccessKey(new Node("name", "domain", "instance"), "myAccessKey"))
            {
                await client.ConnectAsync();

                client.StartReceiving(new MyReceiver());

                await client.SendMessageAsync(new Node(), "Hello world!");

                var jsonDocument = new JsonDocument(MediaType.Parse("application/json")){
                                    { "property1", "string value" },
                                    { "property2", 2 },
                                    { "property3", true },
                                };

                await client.SendMessageAsync(new Node(), jsonDocument);
            }
        }
    }
}
