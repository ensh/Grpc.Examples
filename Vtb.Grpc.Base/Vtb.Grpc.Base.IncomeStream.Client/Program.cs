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
                using (var incomeMethod = client.Income(new Empty()))
                {
                    var incomeStream = incomeMethod.ResponseStream;

                    await foreach (var income in incomeStream.ReadAllAsync())
                    {
                        Console.WriteLine("<" + income.Body);
                    }

                    Console.WriteLine("disconnect....");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }
    }
}
