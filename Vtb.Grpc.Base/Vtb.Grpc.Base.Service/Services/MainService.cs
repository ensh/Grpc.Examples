namespace Vtb.Grpc.Base.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.Extensions.Logging;

    public class MainService : Main.MainBase
    {
        // потом это нужно инЪектировать через контейнер!
        private static readonly ServiceState<string> s_state = new ServiceState<string>(); 

        private readonly ILogger<MainService> m_logger;

        public MainService(ILogger<MainService> logger)
        {
            m_logger = logger;
        }

        public override Task<Empty> Command(Data request, ServerCallContext context)
        {
            s_state.OnNext(request.Body ?? "");
            return Task.FromResult(new Empty());
        }

        public override Task<Data> Query(Data request, ServerCallContext context)
        {
            s_state.OnNext(request.Body ?? "");
            return Task.FromResult(new Data { Body = request.Body ?? "" });
        }

        public override async Task<Empty> Outcome(IAsyncStreamReader<Data> requestStream, ServerCallContext context)
        {
            await foreach(var income in requestStream.ReadAllAsync())
            {
                s_state.OnNext(income.Body);
            }

            return new Empty();
        }

        public override async Task Dual(IAsyncStreamReader<Data> requestStream, IServerStreamWriter<Data> responseStream, ServerCallContext context)
        {
            var writer = Task.Run(async () => await Income(new Empty(), responseStream, context));
            var reader = Task.Run(async () => await Outcome(requestStream, context));

            await Task.WhenAll(reader, writer);
        }

        public override async Task Income(Empty request, IServerStreamWriter<Data> responseStream, ServerCallContext context)
        {
            var queue = new ConcurrentQueue<string>();
            using (var ready = new ManualResetEvent(false))
            {
                using (var s = s_state.Subscribe(new StateObserver(queue, ready)))
                {
                    while (!context.CancellationToken.IsCancellationRequested)
                    {
                        if (ready.WaitOne(1000))
                        {
                            ready.Reset();

                            while (queue.TryDequeue(out var state))
                            {
                                var result = new Data { Body = state, };

                                m_logger.LogInformation($"Отправлено:[{state}]");

                                await responseStream.WriteAsync(result);
                            }
                        }
                    }
                }
            }
        }

        private class StateObserver : IStateObserver<string>
        {
            private ConcurrentQueue<string> m_queue;
            private EventWaitHandle m_ready;

            public string Identity { get; }

            public StateObserver(ConcurrentQueue<string> queue, EventWaitHandle ready)
            {
                m_queue = queue;
                m_ready = ready;
                Identity = Guid.NewGuid().ToString();
            }

            public void OnCompleted()
            {
                m_ready.Dispose();
            }

            public void OnError(Exception error)
            {
                m_ready.Dispose();
            }

            public void OnNext(string value)
            {
                m_queue.Enqueue(value);
                m_ready.Set();
            }
        }
    }
}
