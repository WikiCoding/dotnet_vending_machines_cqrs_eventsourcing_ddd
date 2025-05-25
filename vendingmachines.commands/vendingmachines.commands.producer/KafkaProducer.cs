using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace vendingmachines.commands.producer;

public class KafkaProducer
{
    private readonly IProducer<string, string> _producer;
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

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(string topic, string key, string message, CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<string, string> { Key = key, Value = message };

        DeliveryResult<string, string> result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

        _logger.LogInformation("Message successfully delivered to Kafka");
    }
}
