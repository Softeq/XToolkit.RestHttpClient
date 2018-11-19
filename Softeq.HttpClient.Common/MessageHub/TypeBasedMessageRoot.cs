using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Softeq.HttpClient.Common.MessageHub
{
    public class TypeBasedMessageRoot : IMessageRoot
    {
        private readonly ConcurrentDictionary<Type, HashSet<object>> _eventToHandlersMap;

        public TypeBasedMessageRoot()
        {
            _eventToHandlersMap = new ConcurrentDictionary<Type, HashSet<object>>();
        }

        public void Subscribe<T>(IMessageHandler<T> handler)
        {
            var eventType = typeof(T);

            Subscribe(handler, eventType);
        }

        public void Subscribe<T>(IAsyncMessageHandler<T> handler)
        {
            var eventType = typeof(T);

            Subscribe(handler, eventType);
        }

        public void Unsubscribe<T>(IMessageHandler<T> handler)
        {
            var eventType = typeof(T);

            Unsubscribe(handler, eventType);
        }

        public void Unsubscribe<T>(IAsyncMessageHandler<T> handler)
        {
            var eventType = typeof(T);

            Unsubscribe(handler, eventType);
        }

        public void Raise<T>(T obj)
        {
            var eventType = typeof(T);

            if (!_eventToHandlersMap.ContainsKey(eventType))
            {
                return;
            }

            var handlers = _eventToHandlersMap[eventType].ToList();

            foreach (var handler in handlers)
            {
                if (handler is IAsyncMessageHandler<T> messageHandler)
                {
                    messageHandler.HandleAsync(obj);

                    continue;
                }

                (handler as IMessageHandler<T>)?.Handle(obj);
            }
        }

        public async Task RaiseAsync<T>(T obj)
        {
            var eventType = typeof(T);

            if (!_eventToHandlersMap.ContainsKey(eventType))
            {
                return;
            }

            var handlers = _eventToHandlersMap[eventType].ToList();

            foreach (var handler in handlers)
            {
                if (handler is IAsyncMessageHandler<T> messageHandler)
                {
                    await messageHandler.HandleAsync(obj);

                    continue;
                }

                (handler as IMessageHandler<T>)?.Handle(obj);
            }
        }

        private void Unsubscribe(object handler, Type eventType)
        {
            if (!_eventToHandlersMap.ContainsKey(eventType))
            {
                return;
            }

            _eventToHandlersMap[eventType].RemoveWhere(t => t == handler);
        }

        private void Subscribe(object handler, Type eventType)
        {
            if (!_eventToHandlersMap.ContainsKey(eventType))
            {
                _eventToHandlersMap.AddOrUpdate(eventType, new HashSet<object>(), (key, value) => value);
            }

            if (!_eventToHandlersMap[eventType].Contains(handler))
            {
                _eventToHandlersMap[eventType].Add(handler);
            }
        }
    }
}