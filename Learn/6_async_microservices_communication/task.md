# Task 6 (SQS & SNS, Async Microservices Communication)

## Prerequisites

---

- The task is a continuation of [Task 4](../4_integration_with_nosql_database/task.md) and should be done in the same repo
- Task goal is to migrate sevice commutication abstraction to SQS or SNS.

## Architecture

Find the entire program architecture: [here](../Architecture.pdf).

## Tasks

 
---

### Task 6.1
1. Select appropriate for this task
2. If you selected SNS
   - Create an SNS topic `eShopEventsTopic`
   - Create a subscription for this SNS topic with an `http` endpoint type .
   - Create new implemantion of interface IEventBus EventBusServiceBus to use SNS
3. If you selected SQS
   - Create an SQS queue `eShopEvents`
   - Impllement a subscription for this SQS topic.
   - Create new implemantion of interface IEventBus EventBusServiceBus to use SQS
4. Replace usage of EventBusRabbitMQ with IEventBus implementation that you just created
5. Add health check to replace 'rabbitmq' section with 'awsqueue' section with same structure and check with cloud mentor