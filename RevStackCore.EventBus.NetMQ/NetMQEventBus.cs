using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autofac;
using System.Reflection;

namespace RevStackCore.EventBus.NetMQ
{
    public class NetMQEventBus : IEventBus
    {
        private readonly INetMQPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionsManager _subsManager;

        private readonly ILifetimeScope _autofac;
        private readonly string _clientScopeName;

        public NetMQEventBus(INetMQPersistentConnection persistentConnection, IEventBusSubscriptionsManager subsManager, string clientScopeName, ILifetimeScope autofac)
        {
            _persistentConnection = persistentConnection;
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _clientScopeName = clientScopeName;
            _autofac = autofac;
            _persistentConnection.MessageRecieved += Client_MessageRecieved;
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _persistentConnection.Publish(eventName, body);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);
            _subsManager.AddSubscription<T, TH>();
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            DoInternalSubscription(eventName);
            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            _persistentConnection.Unsubscribe(eventName);
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _subsManager.Clear();
        }

        #region "private"

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                _persistentConnection.Subscribe(eventName);
            }
        }

        private void Client_MessageRecieved(object sender, EventArgs e)
        {
            var eventArgs = (NetMQMessageEventArgs)e;
            string eventName = eventArgs.EventName;
            string message = eventArgs.Message;
            ProcessEvent(eventName, message);
        }

        private void ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _autofac.BeginLifetimeScope(_clientScopeName))
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            var handler = scope.ResolveOptional(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                            dynamic eventData = JObject.Parse(message);
                            handler.Handle(eventData);
                        }
                        else
                        {
                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var handler = scope.ResolveOptional(subscription.HandlerType);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
        }

        #endregion
    }
}
