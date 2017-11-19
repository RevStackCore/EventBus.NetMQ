using RevStackCore.EventBus;
using System.Threading.Tasks;

namespace EventBusNetMQUnitTest.IntegrationEvents
{
    public interface ICatalogIntegrationEventService
    {
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
