// Assets/Scripts/Core/MessageBus.cs
using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// 通信秘书：秘书之间传话的总线，纯转发，不包含任何业务逻辑
    /// </summary>
    public static class MessageBus // 静态类，全局唯一
    {
        // 字典：消息类型 → 对应的所有处理方法，且字典本身无法被替换
        private static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 订阅某类消息
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : GameMessage
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[type] = handler;
            }
        }

        /// <summary>
        /// 取消订阅某类消息
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : GameMessage
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var newHandler = Delegate.Remove(existing, handler);
                if (newHandler == null)
                {
                    _handlers.Remove(type);
                }
                else
                {
                    _handlers[type] = newHandler;
                }
            }
        }

        /// <summary>
        /// 发布消息，所有订阅者都会收到
        /// </summary>
        public static void Publish<T>(T message) where T : GameMessage
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var handler))
            {
                ((Action<T>)handler)?.Invoke(message);
            }
        }
    }
}