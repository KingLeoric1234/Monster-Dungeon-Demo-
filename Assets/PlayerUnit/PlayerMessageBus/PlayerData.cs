namespace Player
{
    /// <summary>
    /// Player 域数据基底类：走 PlayerMessageBus 数据通道搬运数据。
    /// 泛型 T 是载荷类型——PlayerData&lt;Vector2&gt;（位置）、PlayerData&lt;int&gt;（数值）等。
    /// A 写 B 读：发布方只负责写（PublishData），读取方不知道谁写的，有就读（TryGetData），没有就拉倒。
    /// </summary>
    public class PlayerData<T>
    {
        /// <summary>数据载荷（唯一内容）</summary>
        public T Value;

        public PlayerData(T value)
        {
            Value = value;
        }
    }
}
