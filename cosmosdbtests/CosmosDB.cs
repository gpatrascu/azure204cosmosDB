#region

using Microsoft.Azure.Cosmos;
using Xunit.Abstractions;

#endregion

namespace cosmosdbtests;

public class CosmosDB : IAsyncLifetime
{
    private const string DatabaseName = "azureStudyDatabase";
    private const string Container = "orders";
    private static readonly string EndpointUri = "https://az204-study-orders-westeurope.documents.azure.com:443/";

    // Set variable to the Primary Key from earlier.
    private static readonly string PrimaryKey =
        "==";

    private readonly ITestOutputHelper _testOutputHelper;
    private Container container;

    private CosmosClient cosmosClient;
    private Database database;

    public CosmosDB(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public async Task InitializeAsync()
    {
        cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName);
        database = databaseResponse.Database;
        Print($"Created Database: {database.Id}\n");

        var containerResponse = await database.CreateContainerIfNotExistsAsync(Container, "/UserId");
        container = containerResponse.Container;
        Console.WriteLine("Created Container: {0}\n", container.Id);
    }

    public async Task DisposeAsync()
    {
    }

    private void Print(string message)
    {
        Console.WriteLine(message);
        _testOutputHelper.WriteLine(message);
    }

    [Fact]
    public async Task UpdateItem()
    {
        var item = await container.ReadItemAsync<Order>("d5ee9d78-3470-43e9-9e30-6970d01eb233",
            new PartitionKey("205e80ce-3a92-42e9-b536-2ba62528f679"));
        var itemResource = item.Resource;

        itemResource.OrderLines.Add(new OrderLine
        {
            Amount = 100,
            Quantity = 100,
            ProductId = itemResource.OrderLines.First().ProductId
        });

        await container.UpsertItemAsync(itemResource);
    }

    [Fact]
    public async Task CreateItem()
    {
        var order = new Order
        {
            id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Amount = new Random().Next(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 2
                },

                new()
                {
                    Amount = new Random().Next(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 5
                }
            }
        };
        await container.CreateItemAsync(order);
    }

    [Fact]
    public async Task QueryWithSql()
    {
        using FeedIterator<Order> feed = container.GetItemQueryIterator<Order>(
            "SELECT * FROM ORDERS"
        );

        // Iterate query result pages
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();

            // Iterate query results
            foreach (Order item in response) Print($"Found item:\t{item.id}");
        }
    }
}

public record Order
{
    public Guid id { get; set; }
    public Guid UserId { get; set; }

    public List<OrderLine> OrderLines { get; set; }
}

public record OrderLine
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
}