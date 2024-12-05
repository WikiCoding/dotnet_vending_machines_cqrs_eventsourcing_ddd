

## Mongo docker without auth

```yml
mongodb:
    image: mongo
    container_name: mongo1
    restart: always
    ports:
      - 27017:27017
    environment:
      MONGO_REPLICA_SET_NAME: "rs0"
    command: ["mongod", "--replSet", "rs0", "--bind_ip_all", "--noauth"]
```

## Configure and start mongo replica set
```bash
rs.initiate( { _id: "rs0", members: [ { _id: 0, host: "mongo1:27017" } ] } );
```

## Configuration for mongo source - http POST to http://localhost:8083/connectors

```json
{
  "name": "mongo-source-connector",
  "config": {
    "connector.class": "io.debezium.connector.mongodb.MongoDbConnector",
    "mongodb.connection.string": "mongodb://mongodb:27017/vending_machines?replicaSet=rs0",
    "mongodb.name": "vending_machines",
    "topic.prefix": "machines",
    "collection.include.list": "vending_machines.machines",
    "pipeline": "[{'$match': {'operationType': {'$in': ['insert', 'update', 'replace'], }}},{'$project': {'_id': 1,'fullDocument': 1,'ns': 1,}}]",
    "publish.full.document.only": "true",
    "topic.namespace.map": "{\"*\":\"vending_machines.machines\"}",
    "copy.existing": "true"
  }
}
```

## Configuration for jdbc sink with psql (Not working for the moment)

```json
{
  "name": "psql-sink-connector",
  "config": {
    "connector.class": "io.debezium.connector.jdbc.JdbcSinkConnector",
    "tasks.max": "1",
    "connection.url": "jdbc:postgresql://postgres:5432/vending_machines",
    "connection.username": "postgres",
    "connection.password": "postgres",
    "insert.mode": "upsert",
    "delete.enabled": "false",
    "primary.key.mode": "record_value",
    "primary.key.fields": "id",
    "schema.evolution": "basic",
    "database.time_zone": "UTC",
    "hibernate.dialect": "org.hibernate.dialect.PostgreSQLDialect",
    "topics": "machines.vending_machines.machines",
    "value.converter": "org.apache.kafka.connect.json.JsonConverter",
    "value.converter.schemas.enable": "true",
    "key.converter": "org.apache.kafka.connect.json.JsonConverter",
    "key.converter.schemas.enable": "false",
    "table.name.format": "machines",
    "transforms": "unwrap",
    "transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState",
    "transforms.unwrap.drop.tombstones": "true",
    "transforms.unwrap.delete.handling.mode": "rewrite",
    "transforms.unwrap.field": "after"
  }
}
```
