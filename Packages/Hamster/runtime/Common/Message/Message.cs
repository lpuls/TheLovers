using System;
using System.Collections.Generic;

namespace Hamster {

    public interface IMessage {

    }

    interface IMessageProcessor {
    }
    

    class MessageProcessor<T> : IMessageProcessor where T : IMessage, new() {

        public event Action<T> Callback;

        public void Trigger(T message) {
            Callback?.Invoke(message);
        }
    }

    public class MessageManager {
        private Dictionary<Type, IMessageProcessor> _processors = new Dictionary<Type, IMessageProcessor>();

        public void Bind<T>(Action<T> callback) where T : IMessage, new() {
            MessageProcessor<T> messageProcessor = null; 
            if (_processors.TryGetValue(typeof(T), out IMessageProcessor processor)) {
                messageProcessor = processor as MessageProcessor<T>;
            }
            if (null == messageProcessor) {
                messageProcessor = new MessageProcessor<T>();
                _processors.Add(typeof(T), messageProcessor);
            }
            messageProcessor.Callback += callback;
        }

        public void Unbind<T>(Action<T> callback) where T : IMessage, new() {
            if (_processors.TryGetValue(typeof(T), out IMessageProcessor processor)) {
                MessageProcessor<T> messageProcessor = processor as MessageProcessor<T>;
                messageProcessor.Callback -= callback;
            }
            else {
                throw new Exception("Not bind event of " + typeof(T).Name); 
            }
        }

        public void Trigger<T>(T message) where T : IMessage, new() {
            if (_processors.TryGetValue(typeof(T), out IMessageProcessor processor)) {
                MessageProcessor<T> messageProcessor = processor as MessageProcessor<T>;
                messageProcessor.Trigger(message);
            }
            else {
                // UnityEngine.Debug.LogError("Not bind event" + typeof(T).Name);
            }
        }


    }

}
