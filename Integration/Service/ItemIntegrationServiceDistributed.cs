using Integration.Backend;
using Integration.Common;
using StackExchange.Redis;

namespace Integration.Service;

public sealed class ItemIntegrationServiceDistributed
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public ItemIntegrationServiceDistributed()
    {
        _redis = ConnectionMultiplexer.Connect("localhost:6379");
        _db = _redis.GetDatabase();
    }

    public Result SaveItem(string itemContent)
    {
        var lockKey = $"lock:item:{itemContent}";

        bool lockAcquired = _db.StringSet(lockKey, "locked", TimeSpan.FromMinutes(1), When.NotExists);

        if (lockAcquired)
        {
            try
            {
                // Check the backend to see if the content is already saved.
                if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
                {
                    return new Result(false, $"Duplicate item received with content {itemContent}.");
                }

                var item = ItemIntegrationBackend.SaveItem(itemContent);
                return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
            }
            finally
            {
                _db.KeyDelete(lockKey);
            }
        }
        else
        {
            return new Result(false, $"Failed to acquire lock for item with content {itemContent}.");
        }
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}
}