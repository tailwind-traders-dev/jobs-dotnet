using Azure.Messaging.ServiceBus;
using Azure.Identity;


async void SendMessages(string sbHostname, string sbQueueName)
{
    var clientOptions = new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    };

    var client = new ServiceBusClient(
        sbHostname,
        new DefaultAzureCredential(),
        clientOptions);

    var sender = client.CreateSender(sbQueueName);
    var numOfMessages = 3;

    using ServiceBusMessageBatch messageBatch = sender.CreateMessageBatchAsync().GetAwaiter().GetResult();

    for (int i = 1; i <= numOfMessages; i++)
    {
        if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
        {
            throw new Exception($"The message {i} is too large to fit in the batch.");
        }
    }

    try
    {
        sender.SendMessagesAsync(messageBatch).GetAwaiter().GetResult();
        Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
    }
    finally
    {
        await sender.DisposeAsync();
        await client.DisposeAsync();
    }
}

async void ReceiveMessages(string sbHostname, string sbQueueName)
{
    var clientOptions = new ServiceBusClientOptions()
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    };
    var client = new ServiceBusClient(sbHostname, new DefaultAzureCredential(), clientOptions);
    var processor = client.CreateProcessor(sbQueueName, new ServiceBusProcessorOptions());

    async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");
        await args.CompleteMessageAsync(args.Message);
    }

    Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;
    await processor.StartProcessingAsync();
}

void Pause()
{
    var cancellationTokenSource = new CancellationTokenSource();

    // Register the event to cancel the token when Ctrl+C is pressed
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        Console.WriteLine("Ctrl+C detected. Cancelling the operation.");
        cancellationTokenSource.Cancel();
    };

    // Register the event to cancel the token when the process is being terminated
    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
    {
        Console.WriteLine("Process is exiting. Cancelling the operation.");
        cancellationTokenSource.Cancel();
    };

    cancellationTokenSource.Token.WaitHandle.WaitOne();
}

void Hello() {
    Console.WriteLine("hello, world...");
}

if (args.Length == 0)
{
    Console.WriteLine("No command provided.");
    return 0;
}

var sbHostname = Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_HOSTNAME");
var sbQueueName = Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_QUEUE_NAME");

switch (args[0])
{
    case "send":
        Console.WriteLine("Sending.");
        if (sbHostname == null || sbQueueName == null)
        {
            Console.WriteLine("Please set the AZURE_SERVICEBUS_HOSTNAME and AZURE_SERVICEBUS_QUEUE_NAME environment variables.");
            return 1;
        }
        SendMessages(sbHostname, sbQueueName);
        return 0;

    case "receive":
        Console.WriteLine("Receiving.");
        if (sbHostname == null || sbQueueName == null)
        {
            Console.WriteLine("Please set the AZURE_SERVICEBUS_HOSTNAME and AZURE_SERVICEBUS_QUEUE_NAME environment variables.");
            return 1;
        }
        ReceiveMessages(sbHostname, sbQueueName);
        Pause();
        return 0;

    case "hello":
        Hello();
        return 0;

    default:
        Console.WriteLine($"Invalid command: {args[0]}. Valid commands: send, receive");
        return 1;
}
