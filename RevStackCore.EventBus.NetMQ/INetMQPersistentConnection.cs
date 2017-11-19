using System;
using NetMQ;

namespace RevStackCore.EventBus.NetMQ
{
    public interface INetMQPersistentConnection : IDisposable
    {
        string ServiceBusConnectionString { get; }
        void Publish(string name, byte[] body);
        void Subscribe(string name);
        void Unsubscribe(string name);
        event EventHandler MessageRecieved;
    }
}