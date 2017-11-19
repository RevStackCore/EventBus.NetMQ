using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventBusNetMQUnitTest.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RevStackCore.EventBus;
using RevStackCore.EventBus.NetMQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBusNetMQUnitTest
{
    public class StartupConfig
    {
        #region Container
        private static Lazy<IServiceProvider> container = new Lazy<IServiceProvider>(() =>
        {
            // create service collection
            var serviceCollection = new ServiceCollection();
            var serviceProvider = ConfigureServices(serviceCollection);
            // create service provider
            //var serviceProvider = serviceCollection.BuildServiceProvider();
            // logging
            //ConfigureLogging(serviceProvider);
            // event bus subscribers
            ConfigureEventBus(serviceProvider);
            return serviceProvider;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IServiceProvider GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //setup our DI
            //service.AddLogging();

            //services.AddAutofac();

            // add services
            services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

            services.AddSingleton<INetMQPersistentConnection>(sp =>
            {
                var serviceBusConnection = "tcp://127.0.0.1:7777";

                return new NetMQPersistentConnection(serviceBusConnection);
            });

            RegisterEventBus(services);
            
            //configure autofac
            var container = new ContainerBuilder();
            container.Populate(services);
            
            // build the Autofac container
            ApplicationContainer = container.Build();

            // creating the IServiceProvider out of the Autofac container
            return new AutofacServiceProvider(ApplicationContainer);
        }

        public static IContainer ApplicationContainer { get; private set; }

        private static void RegisterEventBus(IServiceCollection services)
        {
            services.AddSingleton<IEventBus, NetMQEventBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<INetMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                var subscriptionClientName = "Test_SubscriptionClientName";

                return new NetMQEventBus(serviceBusPersisterConnection,
                    eventBusSubcriptionsManager, subscriptionClientName, iLifetimeScope);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
        }

        private static void ConfigureEventBus(IServiceProvider serviceProvider)
        {
            var eventBus = serviceProvider.GetService<IEventBus>();
            eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();
        }

        private static void ConfigureLogging(IServiceProvider serviceProvider)
        {
            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<UnitTest1>();
            logger.LogDebug("UnitTest1 application");
        }
    }
}
