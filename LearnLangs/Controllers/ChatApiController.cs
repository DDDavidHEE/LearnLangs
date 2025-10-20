using LearnLangs.Hubs;
using LearnLangs.Services.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LearnLangs.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatApiController : ControllerBase
    {
        // sessionId -> history
        private static readonly Dictionary<string, List<(string role, string content)>> _mem
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly IGeminiChatService _chat;
        private readonly IHubContext<ChatHub> _hub;

        public ChatApiController(IGeminiChatService chat, IHubContext<ChatHub> hub)
        {
            _chat = chat;
            _hub = hub;
        }

        public record ChatSend(string SessionId, string Text);

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatSend req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.SessionId) || string.IsNullOrWhiteSpace(req.Text))
                return BadRequest(new { error = "sessionId and text are required." });

            if (!_mem.TryGetValue(req.SessionId, out var history))
                _mem[req.SessionId] = history = new();

            // add user message (giữ tối đa 20)
            history.Add(("user", req.Text));
            if (history.Count > 20) history.RemoveRange(0, history.Count - 20);

            try
            {
                var reply = await _chat.ReplyAsync(history, req.Text, ct);
                history.Add(("assistant", reply));
                if (history.Count > 20) history.RemoveRange(0, history.Count - 20);

                await _hub.Clients.Group(req.SessionId).SendAsync("assistant",
                    new { sessionId = req.SessionId, text = reply }, ct);

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                await _hub.Clients.Group(req.SessionId).SendAsync("assistant",
                    new { sessionId = req.SessionId, text = "⚠️ Sorry, something went wrong." }, ct);
                Console.Error.WriteLine($"[ChatApi] {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history/{sessionId}")]
        public IActionResult History(string sessionId)
        {
            _mem.TryGetValue(sessionId, out var h);
            return Ok(h ?? new());
        }

        [HttpPost("reset")]
        public IActionResult Reset([FromBody] string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest(new { error = "sessionId is required." });

            _mem.Remove(sessionId);
            return Ok(new { ok = true });
        }
    }
}
