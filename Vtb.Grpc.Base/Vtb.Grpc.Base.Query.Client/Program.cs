﻿using System;
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
                while (true)
                {
                    Console.ReadLine();
                    var guid = Guid.NewGuid().ToString();

                    Console.WriteLine(">" + guid);
                    var result = client.Query(new Service.Data { Body = guid });
                    Console.WriteLine("<" + result.Body);
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
