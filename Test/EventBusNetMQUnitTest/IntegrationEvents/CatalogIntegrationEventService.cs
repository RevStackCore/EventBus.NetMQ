using RevStackCore.EventBus;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace EventBusNetMQUnitTest.IntegrationEvents
{
    public class CatalogIntegrationEventService : ICatalogIntegrationEventService
    {
        //private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        //private readonly CatalogContext _catalogContext;
        //private readonly IIntegrationEventLogService _eventLogService;

        public CatalogIntegrationEventService(IEventBus eventBus)
        {
            //_catalogContext = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            //_eventLogService = _integrationEventLogServiceFactory(_catalogContext.Database.GetDbConnection());
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            await Task.Run(()=> _eventBus.Publish(evt));

            //Log event published
            //await _eventLogService.MarkEventAsPublishedAsync(evt);
        }
        
    }
}
