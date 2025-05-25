using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace vendingmachines.commands.producer;

public class KafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaConfig _kafkaConfig;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(ILogger<KafkaProducer> logger, KafkaConfig kafkaConfig)
    {
        _logger = logger;
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

        _logger.LogInformation("Message successfully delivered to Kafka");
    }
}
