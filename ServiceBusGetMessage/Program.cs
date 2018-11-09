using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusGetMessage
{
    class Program
    {
        private static string _conn = "Endpoint=sb://mobservicbusnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Y5xN/Wvt6jNmP3rZMS0c5zYCsljoLleJNugI1yyXbr8=";
        private static string _topic = "t1";
        static void Main(string[] args)
        {
            ReadMessage();
            Console.ReadLine();
        }
        static void ReadMessage()
        {
            var subClient = SubscriptionClient.CreateFromConnectionString(_conn, _topic, "test");
            subClient.OnMessage(m =>
            {
                Console.WriteLine(m.Properties["Name"]);
                
            });
        }
}
}
