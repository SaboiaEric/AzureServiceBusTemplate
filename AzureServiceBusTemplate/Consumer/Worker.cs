using Azure.Messaging.ServiceBus;

namespace Consumer
{
    public class Worker : IWorker
    {
        private readonly string _connectionString;
        private readonly string _queue;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public Worker(string connectionString, string queue)
        {
            _connectionString = connectionString;
            _queue = queue;

            if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(_queue))
            {
                throw new ArgumentException("Favor informar a connection string e o nome da fila do Azure Service Bus");
            }

            var clientOptions = new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets };

            _client = new ServiceBusClient(_connectionString, clientOptions);

            _processor = _client.CreateProcessor(_queue, new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorMessageHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Iniciando o processamento de mensagens...");

            await _processor.StartProcessingAsync();
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.CloseAsync();

            await _processor.DisposeAsync();

            await _client.DisposeAsync();

            await Console.Out.WriteLineAsync("Conexao com o Azure Service Bus fechada!");
        }

        private static Task ErrorMessageHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine("[Falha] " + args.Exception.GetType().FullName + " " + args.Exception.Message);

            return Task.CompletedTask;
        }

        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            Console.WriteLine("[Nova mensagem recebida] " + args.Message.Body.ToString());

            await args.CompleteMessageAsync(args.Message);

            Console.WriteLine("ACK realizado com sucesso!");
        }
    }
}