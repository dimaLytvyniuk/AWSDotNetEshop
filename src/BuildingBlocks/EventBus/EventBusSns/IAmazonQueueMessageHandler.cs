using System.Threading.Tasks;

namespace EventBusSns;

public interface IAmazonQueueMessageHandler
{
    Task HandleMessageAsync(string serializedMessage);
}
