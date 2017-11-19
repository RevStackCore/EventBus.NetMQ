using System;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RevStackCore.EventBus.NetMQ
{
    public class NetMQPersistentConnection : INetMQPersistentConnection
    {
        private readonly string _serviceBusConnectionString;
        private PublisherSocket _pubSocket;
        private Dictionary<string, SubscriberSocket> _subSockets;
        bool _disposed;

        public NetMQPersistentConnection(string serviceBusConnectionString)
        { 
            _serviceBusConnectionString = serviceBusConnectionString ??
                throw new ArgumentNullException(nameof(serviceBusConnectionString));

            _subSockets = new Dictionary<string, SubscriberSocket>();

            _pubSocket = new PublisherSocket();
            _pubSocket.Bind(_serviceBusConnectionString);
            //IMPORTANT! let subscribers connect to the publisher before sending messages
            Thread.Sleep(500);
        }

        public string ServiceBusConnectionString => _serviceBusConnectionString;
        public PublisherSocket PublisherSocket => _pubSocket;

        public void Subscribe(string name)
        {
            Task.Run(() =>
            {
                using (var sub = new SubscriberSocket())
                {
                    sub.Connect(_serviceBusConnectionString);
                    sub.Subscribe(name);
                    //add to list
                    _subSockets.Add(name, sub);

                    while (true)
                    {
                        // receive event
                        var message = sub.ReceiveMultipartMessage();
                        //Console.WriteLine("2: " + message[0].ConvertToString() + " " + message[1].ConvertToString());
                        OnMessageRecieved(this, new NetMQMessageEventArgs(message[0].ConvertToString(), message[1].ConvertToString()));
                        
                    }
                }
            });
        }

        public void Unsubscribe(string name)
        {
            if (_subSockets.ContainsKey(name))
            {
                var sub = _subSockets[name];
                sub.Unsubscribe(name);
                _subSockets.Remove(name);
            }
        }

        public void Publish(string name, byte[] body)
        {
            _pubSocket.SendMoreFrame(name).SendFrame(body);
        }

        public event EventHandler MessageRecieved;
        protected virtual void OnMessageRecieved(object sender, NetMQMessageEventArgs e)
        {
            EventHandler handler = MessageRecieved;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        
        public void Dispose()
        {
            _pubSocket.Dispose();

            if (_disposed) return;

            _disposed = true;
        }
    }

    public class NetMQMessageEventArgs : EventArgs
    {
        public NetMQMessageEventArgs(string eventName, string message)
        {
            EventName = eventName;
            Message = message;
        }

        public string EventName { get; private set; }
        public string Message { get; private set; }
    }
}
