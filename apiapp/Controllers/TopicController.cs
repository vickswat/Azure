using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace apiapp.Controllers
{
    public class TopicController : ApiController
    {
        private static string _conn = "Endpoint=sb://mobservicbusnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Y5xN/Wvt6jNmP3rZMS0c5zYCsljoLleJNugI1yyXbr8=";
        private static string _topic = "t1";
        [HttpGet]
        [System.Web.Http.Route("Api/send")]
        public string send()
        {
            // Create a Long running task to do an infinite loop which will keep sending the server time
            // to the clients every 3 seconds.
            // string msgTopic = string.Empty;
            int i = 0;
            string msgTopic = string.Empty;
            while (true)
            {
                string timeNow = DateTime.Now.ToString();
                ////Sending the server time to all the connected clients on the client method SendServerTime()
                //Clients.All.SendServerTime(timeNow);


                //string msgTopic = servicebus();

                var subClient = SubscriptionClient.CreateFromConnectionString(_conn, _topic, "test");
                subClient.OnMessage(m =>
                {
                    msgTopic = m.Properties["Name"].ToString();

                });

                if (!string.IsNullOrEmpty(msgTopic))
                {
                    return msgTopic;
                }

                Task.Delay(3000);

            }

        }
        
        private string servicebus()
        {
            string msg = string.Empty;
            var subClient = SubscriptionClient.CreateFromConnectionString(_conn, _topic, "test");
            subClient.OnMessage(m =>
            {
                msg = m.Properties["Name"].ToString();

            });

            return msg;
        }
    }
}
