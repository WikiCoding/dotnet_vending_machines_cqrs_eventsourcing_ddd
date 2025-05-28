package com.wikicoding.notifications.infrastructure;

import lombok.extern.slf4j.Slf4j;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.stereotype.Component;

@Component
@Slf4j
public class KafkaConsumer {
    private final String topic = "machines-topic";
    private final Logger logger = LoggerFactory.getLogger(KafkaConsumer.class);

    @KafkaListener(topics = topic)
    public void listen(String message) {
        logger.warn("Received Message: {}", message);

        logger.warn("Dispatching email saying: {}", message);
    }
}
