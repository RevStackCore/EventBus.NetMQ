using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventBusNetMQUnitTest.IntegrationEvents;
using RevStackCore.EventBus;

namespace EventBusNetMQUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private readonly ICatalogIntegrationEventService _service;

        public UnitTest1()
        {
            var container = StartupConfig.GetConfiguredContainer();
            _service = container.GetService<ICatalogIntegrationEventService>();

        }

        [TestMethod]
        public void Change_Price_Integration_Event()
        {
            var @event = new ProductPriceChangedIntegrationEvent("SKU-1234", 25, 17);
            _service.PublishThroughEventBusAsync(@event);
        }
    }
}
