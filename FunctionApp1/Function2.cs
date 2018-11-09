using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace FunctionApp1
{
    public static class Function2
    {
        [FunctionName("Function2")]
        public static void Run([ServiceBusTrigger("t1", "test", AccessRights.Manage, Connection = "")]string mySbMsg, TraceWriter log)
        {
            log.Info($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
        }
    }
}
