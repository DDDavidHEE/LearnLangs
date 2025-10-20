using Microsoft.AspNetCore.Mvc;

namespace LearnLangs.Controllers
{
    // Truy cập: /Conversation  hoặc /Conversation/Index
    [Route("Conversation")]
    public class ChatController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index() => View(); // Views/Chat/Index.cshtml
    }
}
