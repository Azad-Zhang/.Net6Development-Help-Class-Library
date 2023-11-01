using Microsoft.AspNetCore.Mvc;
using Zack.EventBus;

namespace 发送端.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class testController : ControllerBase
    {
        private IEventBus eventBus;
        public testController(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }
        [HttpPost]
        public string AddUser(string userName, int userAge)
        {
            //1. 模拟数据库操作
            //2. DB成功后，执行消息的发布
            eventBus.Publish("userAdd", new { userName = userName, userAge = userAge });
            return "ok";
        }
    }
}
