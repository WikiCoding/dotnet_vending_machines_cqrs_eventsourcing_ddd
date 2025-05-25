## Summary

1. Project built around my interpretation of **Event Sourcing** based on **Domain Driven Design** and applying **CQRS** and **Layered Architecture**.
2. Other key aspect to this project was to implement a **Replay System (see RebuildQueriesDbController)** to recover a Read Database.
3. While at it, took the oportunity to look into what does it like to store **Polymorphic Objects** with **MongoDb** and the **.NET framework**. Up until now I've used EF Core with MongoDb but that doesn't actually seem to work properly with Polymorphic Objects and actually got convinced on using Mongo Driver instead.
4. The **main focus** on this project is really the **vendingmachines.commands service**.
5. The code ended up with quite some layers to be able to follow **SRP** but I got lazy where and there towards **OCP**, especially on the Queries service that definetly got less attention. On the other hand, I feel like I ended up in general with testable units (at least on the commands service) but I should considered returning data in each of the methods to make this test task easier.
6. It was interesting to experience that with this shift towards events and the domain taking care of enforcing business rules in most of the cases led to faster and smoother integration of new features in the code. You just need a new feature in the application unit, new command, new event, specify the business rules in the domain and make sure you're publishing your new domain event.
7. I really like the idea of getting the past events for a certain agregates, **rebuild the aggregate state** and then do whatever's next. Of course that comes with a cost but it's possible to have ways to go around this.
8. To achieve performance improvements, instead of fully reading the stream of events from beginning, I opted for a **snapshots** strategy persisting data in **Redis** with a very generous TimeSpan to keep the data. If it's an Id that is not getting used much then there's not many events so it's safe to rebuild state from scratch when that aggregate is being used.
9. With **snapshots** became more evident and critical the need for persisting and publishing messages in a single transaction or fail all together...
10. I wanted that when a product is being added to a machine or stock is being added to the machine to block any other requests to the server so I implemented a **semaphore** in which I'm limiting connections to 1 until the request get's completed
11. I took the chance to try out and implement **global exception handling**
12. I took the chance to try out and implement **automapper** to map objects
13. I took the change to try out and implement a read db with **FK constraint**, which actually may not really work on a production scenario, but due to racing conditions it might be catastrofic. If by any means the SaveProduct it's faster than SaveMachine, and it most likely will be, we break the app right due to FK constraint violation. This is fairly easy to experience if you start the commands app, add one machine and some products to that machine, piling up some messages in Kafka. Then the consumer when it's start will consume those messages in the right order but we don't really control to which topic the consumer subscribes first and there you go, you may be subscribed to Products first and the app breaks right away. This is prone to happen whenever we have more instances of the commands application and we get into those temporary out of sync records for example.
14. The last point is cool for an experiment, in a real world situation this doesn't make sense since in that queries service we're building and updating data that previously had rules enforced to it eventhough it eventually consistent, we're just querying, nothing more!
15. I took the change to try out and implement Concurrency checks within the domain and then on the EventStore.
16. It wasn't really possible to implement DDD by the book, but really close. To rebuild the aggregate state, I had to choose because being able to create the aggregate root object with a no args constructor, meaning in inconsist state, or create it normally with wrong data just to replace it afterwards which it really doesn't make sense at all. So whenever I'm rebuilding the aggregate, I'll temporarily will create an aggregate root in inconsistent state.

## To start the application

1. Along side with .NET and JDK21 you'll need to check your machine IPv4 Address and replace the one on the docker-compose.yml:

```
KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://192.168.1.91:9092 <-- your IP goes here
```

2. Then start the containers with `docker-compose up -d`
3. You can visit `localhost:8080` and create manually the topics in the Kafka UI or just run the commands app and when calling the endpoints the topics will get created.
4. The topics we have are:

- 4.1. machine-created-topic
- 4.2. product-added-topic
- 4.3. product-qty-updated-topic
- 4.4. product-ordered-topic

5. If you can't get a connection to mongodb you need to start the replica set manually going to the container terminal and running

```bash
docker exec -it <your_mongo_container_id> mongosh

mongosh

rs.initiate({
  _id: "rs0",
  members: [{ _id: 0, host: "host.docker.internal:27017" }]
});
```

6. At the queries service, it's recommended that you run the migrations before starting the application and disable in **Program.cs** the migrations on startup but it works either way, and in case you drop all the tables by accident you can rebuild the queries db by replaying all the events in the Commands service :)

7. I left the GET endpoint in the Commands service as a helper. You can get the machineId of a newly created machine from the MongoUI at http://localhost:8081 or from the logs in the console. Then you can use that helper for the Products also. Otherwise just start the Queries service, so you can get the same info.
