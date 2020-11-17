using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;


namespace Vtb.Grpc.Base.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Service.Main.MainClient(channel);

            try
            {
                using (var outcomeMethod = client.Outcome())
                {
                    var outcomeStream = outcomeMethod.RequestStream;

                    while (outcomeMethod.GetStatus().Equals(Status.DefaultSuccess))
                    {
                        Console.ReadLine();

                        var guid = Guid.NewGuid().ToString();
                        Console.WriteLine(">" + guid);
                        await outcomeStream.WriteAsync(new Service.Data { Body = guid });
                    }
                }

                Console.WriteLine("disconnect....");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }
    }
}
