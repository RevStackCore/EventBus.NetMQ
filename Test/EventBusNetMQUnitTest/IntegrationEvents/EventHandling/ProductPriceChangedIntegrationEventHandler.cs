using RevStackCore.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventBusNetMQUnitTest.IntegrationEvents
{
    public class ProductPriceChangedIntegrationEventHandler :
        IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
    {
        public ProductPriceChangedIntegrationEventHandler()
        {
        }

        public async Task Handle(ProductPriceChangedIntegrationEvent command)
        {
            await Task.Run(() => { Console.WriteLine("Handle ProductId=" + command.ProductId); });
        }
    }
}
