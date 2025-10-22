using LearnLangs.Services.Chat;
using Microsoft.AspNetCore.Mvc;

namespace LearnLangs.Controllers
{
    // MVC Controller cho view
    public class ConversationController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }

    // API Controller
    [ApiController]
    [Route("api/conversation")]
    public class ConversationApiController : ControllerBase
    {
        // Lưu lịch sử chat theo sessionId
        private static readonly Dictionary<string, List<(string role, string content)>> _memory
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly IGeminiChatService _chatService;
        private readonly ILogger<ConversationApiController> _logger;

        public ConversationApiController(
            IGeminiChatService chatService,
            ILogger<ConversationApiController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public record SendRequest(
            string SessionId,
            string Text,
            string? TargetLanguage = null,
            string? Level = null,
            string? Goal = null
        );

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendRequest req, CancellationToken ct)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(req.SessionId))
                    return BadRequest(new { error = "sessionId is required" });

                if (string.IsNullOrWhiteSpace(req.Text))
                    return BadRequest(new { error = "text is required" });

                _logger.LogInformation("Conversation Send: SessionId={SessionId}, Text={Text}",
                    req.SessionId, req.Text);

                // Lấy hoặc tạo history
                if (!_memory.TryGetValue(req.SessionId, out var history))
                {
                    history = new List<(string role, string content)>();
                    _memory[req.SessionId] = history;
                }

                // Thêm tin nhắn user vào history
                history.Add(("user", req.Text));

                // Gọi Gemini API
                var reply = await _chatService.ReplyAsync(
                    history,
                    req.Text,
                    req.TargetLanguage ?? "English",
                    req.Level ?? "beginner",
                    req.Goal ?? "conversation",
                    ct
                );

                _logger.LogInformation("Gemini Reply: {Reply}", reply);

                // Lưu reply vào history
                history.Add(("assistant", reply));

                // Trả về response
                return Ok(new
                {
                    success = true,
                    text = reply,
                    sessionId = req.SessionId
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Gemini API error");
                return StatusCode(502, new
                {
                    error = "AI service error",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Conversation error");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    details = ex.Message
                });
            }
        }

        [HttpGet("history/{sessionId}")]
        public IActionResult GetHistory(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest(new { error = "sessionId is required" });

            _memory.TryGetValue(sessionId, out var history);
            return Ok(new
            {
                sessionId,
                messages = history ?? new List<(string role, string content)>()
            });
        }

        [HttpPost("clear")]
        public IActionResult ClearHistory([FromBody] string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest(new { error = "sessionId is required" });

            _memory.Remove(sessionId);
            return Ok(new { success = true });
        }
    }
}