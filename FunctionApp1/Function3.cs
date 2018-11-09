using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionApp1
{
    public static class Function3
    {
        [FunctionName("Function3")]
        public static void Run([BlobTrigger("images/{name}", Connection = "DefaultEndpointsProtocol=https;AccountName=functappdemosaccount;AccountKey=spslceII1MjqEqRBz9CTecSHo8HSuYS9Dtz07sdMtXS3T6k/zC0Oz2ay82fRZsWPL9m1Soo1zn1YKLAstqqRgQ==;EndpointSuffix=core.windows.net")]Stream myBlob, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
