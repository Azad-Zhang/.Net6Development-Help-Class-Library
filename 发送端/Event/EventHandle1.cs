using Zack.EventBus;

namespace 发送端.Event
{
    /// <summary>
    /// 事件接收类1
    /// IIntegrationEventHandler，将发送的消息序列化成json字符串
    /// </summary>
    [EventName("userAdd")]
    [EventName("userEdit")]
    public class EventHandle1 : DynamicIntegrationEventHandler
    {
        public override Task HandleDynamic(string eventName, dynamic eventData)
        {
            if (eventName == "userAdd")
            {
                Console.WriteLine($"收到了{eventName}消息，消息为：{eventData}");
            }
            return Task.CompletedTask;
        }

    }
}
