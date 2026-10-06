using System;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// Player 域消息基类：状态播报用（"XX 变了"）。具体状态消息继承它并携带简单字段。
    /// </summary>
    public abstract class PlayerMessage
    {
    }

    /// <summary>
    /// Player 域信息基底：Player 芯片内部的消息都走这里，不惊动全局总线（Game.Core.MessageBus）。
    /// 两条通道：
    ///   1. 状态通道（事件播报）：Subscribe/Publish——播报"XX 变了"，消息继承 PlayerMessage
    ///   2. 数据通道（传输 + 留底可查）：PublishData/SubscribeData/TryGetData——搬运数据（PlayerData&lt;T&gt;），
    ///      发出去的数据留一份底，任何时刻都能读到最新值（A 写 B 读，B 不知道谁写的，有就读，没有就拉倒）
    /// 域内静态类，Player 芯片内部专用，外部系统用全局总线。
    /// </summary>
    public static class PlayerMessageBus
    {
        // 状态通道：消息类型 → 订阅委托（纯转发，不缓存）
        private static readonly Dictionary<Type, Delegate> _stateHandlers = new Dictionary<Type, Delegate>();

        // 数据通道：数据类型 → 订阅委托
        private static readonly Dictionary<Type, Delegate> _dataHandlers = new Dictionary<Type, Delegate>();
        // 数据通道：数据类型 → 最新数据留底（写后可查）
        private static readonly Dictionary<Type, object> _dataCache = new Dictionary<Type, object>();

        // ═══════════════════ 状态通道（事件播报） ═══════════════════

        /// <summary>订阅状态消息</summary>
        public static void Subscribe<T>(Action<T> handler) where T : PlayerMessage
        {
            Type type = typeof(T);
            if (_stateHandlers.TryGetValue(type, out var existing))
                _stateHandlers[type] = Delegate.Combine(existing, handler);
            else
                _stateHandlers[type] = handler;
        }

        /// <summary>取消订阅状态消息</summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : PlayerMessage
        {
            Type type = typeof(T);
            if (_stateHandlers.TryGetValue(type, out var existing))
            {
                var newHandler = Delegate.Remove(existing, handler);
                if (newHandler == null) _stateHandlers.Remove(type);
                else _stateHandlers[type] = newHandler;
            }
        }

        /// <summary>发布状态消息，所有订阅者都会收到</summary>
        public static void Publish<T>(T message) where T : PlayerMessage
        {
            Type type = typeof(T);
            if (_stateHandlers.TryGetValue(type, out var handler))
                ((Action<T>)handler)?.Invoke(message);
        }

        // ═══════════════════ 数据通道（传输 + 留底可查） ═══════════════════

        /// <summary>订阅数据（PlayerData&lt;T&gt; 送达时触发）</summary>
        public static void SubscribeData<T>(Action<PlayerData<T>> handler)
        {
            Type type = typeof(PlayerData<T>);
            if (_dataHandlers.TryGetValue(type, out var existing))
                _dataHandlers[type] = Delegate.Combine(existing, handler);
            else
                _dataHandlers[type] = handler;
        }

        /// <summary>取消订阅数据</summary>
        public static void UnsubscribeData<T>(Action<PlayerData<T>> handler)
        {
            Type type = typeof(PlayerData<T>);
            if (_dataHandlers.TryGetValue(type, out var existing))
            {
                var newHandler = Delegate.Remove(existing, handler);
                if (newHandler == null) _dataHandlers.Remove(type);
                else _dataHandlers[type] = newHandler;
            }
        }

        /// <summary>发布数据：通知订阅者 + 留底（之后任何时刻 TryGetData 都能读到最新值）</summary>
        public static void PublishData<T>(PlayerData<T> data)
        {
            Type type = typeof(PlayerData<T>);
            _dataCache[type] = data.Value;

            if (_dataHandlers.TryGetValue(type, out var handler))
                ((Action<PlayerData<T>>)handler)?.Invoke(data);
        }

        /// <summary>读最新数据：有就读（返回 true），没有就返回 false，不报错不等待</summary>
        public static bool TryGetData<T>(out T value)
        {
            Type type = typeof(PlayerData<T>);
            if (_dataCache.TryGetValue(type, out var obj))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }
    }
}
