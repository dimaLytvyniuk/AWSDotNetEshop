using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Basket.API.Repositories
{
    public class DynamoBasketRepository : IBasketRepository
    {
        private const string _columnId = "BuyerId";
        private const string _columnData = "BasketData";
        private const string _tableName = "Basket";

        private readonly ILogger<DynamoBasketRepository> _logger;
        private readonly IAmazonDynamoDB _database;

        public DynamoBasketRepository(ILogger<DynamoBasketRepository> logger, IAmazonDynamoDB database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<bool> DeleteBasketAsync(string id)
        {
            var request = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    {  _columnId, new AttributeValue { S = id }  }
                }
            };
            var result = await _database.DeleteItemAsync(request);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<CustomerBasket> GetBasketAsync(string customerId)
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>() { { _columnId, new AttributeValue { S = customerId } } },
            };
            var response = await _database.GetItemAsync(request);


            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonSerializer.Deserialize<CustomerBasket>(response.Item[_columnData].S, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public IEnumerable<string> GetUsers()
        {
            var request = new ScanRequest
            {
                TableName = _tableName,
                ProjectionExpression = _columnId
            };

            var response = _database.ScanAsync(request).Result;

            return response.Items?.Select(k => k[_columnId].S);
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {
            await CreateTableIfNotExist(_database, _tableName);

            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>()
                    {
                        { _columnId, new AttributeValue { S = basket.BuyerId }},
                        { _columnData, new AttributeValue { S = JsonSerializer.Serialize(basket) }},
                    }
            };
            var created = await _database.PutItemAsync(request);

            if (created.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogInformation("Problem occur persisting the item.");

                return null;
            }

            _logger.LogInformation("Basket item persisted successfully.");

            return await GetBasketAsync(basket.BuyerId);
        }

        private async Task CreateBasketTableAsync(IAmazonDynamoDB database, string tableName)
        {
            var request = new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = _columnId,
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = _columnId,
                        KeyType = "HASH"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                },
                TableName = tableName
            };

            await database.CreateTableAsync(request);
        }

        private async Task CreateTableIfNotExist(IAmazonDynamoDB database, string tableName)
        {
            if (await IsDynamoDbTableExistsAsync(database) == false)
                await CreateBasketTableAsync(database, tableName);
        }

        private async Task<bool> IsDynamoDbTableExistsAsync(IAmazonDynamoDB database)
        {
            string lastTableNameEvaluated = null;

            do
            {
                var request = new ListTablesRequest
                {
                    Limit = 100,
                    ExclusiveStartTableName = lastTableNameEvaluated
                };

                var response = await database.ListTablesAsync(request);

                if (response.TableNames.Any(name => name == _tableName))
                {
                    return true;
                }

                lastTableNameEvaluated = response.LastEvaluatedTableName;

            } while (lastTableNameEvaluated != null);

            return false;
        }
    }
}
