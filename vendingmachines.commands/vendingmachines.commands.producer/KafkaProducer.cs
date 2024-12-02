using Confluent.Kafka;

namespace vendingmachines.commands.producer;

public class KafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaConfig _kafkaConfig;

    public KafkaProducer(KafkaConfig kafkaConfig)
    {
        _kafkaConfig = kafkaConfig;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers,
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(string topic, string message, CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<Null, string> { Value = message };

        DeliveryResult<Null, string> result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

        Console.WriteLine("INFO: Message successfully delivered to Kafka");
    }
}
