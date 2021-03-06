﻿namespace EventBusNetMQUnitTest.IntegrationEvents
{
    using RevStackCore.EventBus;

    public class ProductPriceChangedIntegrationEvent : IntegrationEvent
    {        
        public string ProductId { get; private set; }

        public decimal NewPrice { get; private set; }

        public decimal OldPrice { get; private set; }

        public ProductPriceChangedIntegrationEvent(string productId, decimal newPrice, decimal oldPrice)
        {
            ProductId = productId;
            NewPrice = newPrice;
            OldPrice = oldPrice;
        }
    }
}