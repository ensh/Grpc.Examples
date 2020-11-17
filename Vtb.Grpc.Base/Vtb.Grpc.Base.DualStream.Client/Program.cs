namespace Vtb.Grpc.Base.Client
{
    using Grpc.Core;
    using Grpc.Net.Client;

    using System;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Service.Main.MainClient(channel);

            try
            {
                using (var dualMethod = client.Dual())
                {
                    var incomeStream = dualMethod.ResponseStream;
                    var outcomeStream = dualMethod.RequestStream;

                    var writer = Task.Run(
                        async () =>
                        {
                            while (true)
                            {
                                Console.ReadLine();

                                var guid = Guid.NewGuid().ToString();
                                Console.WriteLine(">" + guid);
                                await outcomeStream.WriteAsync(new Service.Data { Body = guid });
                            }
                        });

                    var reader = Task.Run(
                        async () =>
                        {
                            await foreach (var income in incomeStream.ReadAllAsync())
                            {
                                Console.WriteLine("<" + income.Body);
                            }
                        });

                    await Task.WhenAll(writer, reader);
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
