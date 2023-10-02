# Task 4 (Integration with NoSQL Database)

## Architecture

Find the entire program architecture: [here](../Architecture.pdf).

## Tasks

---

### Task 4.1

1. Use AWS Console to create database tables in DynamoDB. Expected schemas for products and stocks:

Product model:

```
  Basket:
    BuyerId -  HASH (Primary key)
```

Use folowing code as an example:

```
aws dynamodb create-table --table-name Basket --attribute-definitions  AttributeName=BuyerId,AttributeType=S --key-schema AttributeName=BuyerId,KeyType=HASH --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 --table-class STANDARD --endpoint-url http://yourhost:8000
```

2. Check that Basket  table was created successfully

```
aws dynamodb list-tables --endpoint-url http://yourhost:8000
```

3. Reconfigure services to use this database instead on local dynamo DB in Docker container

4. Manually check that services runnung correctly by making postman requests to application gateway using mogified request 'Health Check Module 4\Http Agg [Basket Health Check]'

5. Verify using cloud mentor pipeline that you deployed services correctly

6. Optional. Setup instances in multiple regions and configure load balancing

7. Optional. Change deployment scripts to create/configure of Dynamo DB in pipleline.