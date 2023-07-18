using Confluent.Kafka;
using CommandDotNet;

namespace Logerfo.Kafka;

public class Program
{
    const string host = "localhost:29092";

    static int Main(string[] args)
    {
        AppRunner<Program> runner = new();
        runner.UseTypoSuggestions();
        return runner.Run(args);
    }

    [Command(Description = "Produces a message to the specified Kafka topic with the given value.")]
    public async Task produce(string topic, string value)
    {
        ProducerConfig config = new() { BootstrapServers = host };
        // If serializers are not specified, default serializers from
        // `Confluent.Kafka.Serializers` will be automatically used where
        // available. Note: by default strings are encoded as UTF8.
        using var p = new ProducerBuilder<Null, string>(config).Build();
        try
        {
            var dr = await p.ProduceAsync(topic, new Message<Null, string> { Value = value });
            Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}' at {topic}");
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    }

    [Command(Description = "Consumes messages from the specified Kafka topic.")]
    public void consume(string topic)
    {
        ConsumerConfig config = new()
        {
            GroupId = "test-consumer-group",
            BootstrapServers = host,
            // Note: The AutoOffsetReset property determines the start offset in the event
            // there are not yet any committed offsets for the consumer group for the
            // topic/partitions of interest. By default, offsets are committed
            // automatically, so in this example, consumption will only start from the
            // earliest message in the topic the first time you run the program.
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var c = new ConsumerBuilder<Ignore, string>(config).Build();
        c.Subscribe(topic);

        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        try
        {
            while (true)
                try
                {
                    var cr = c.Consume(cts.Token);
                    Console.WriteLine($"Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occured: {e.Error.Reason}");
                }
        }
        catch (OperationCanceledException)
        {
            // Ensure the consumer leaves the group cleanly and final offsets are committed.
            c.Close();
        }
    }
}
