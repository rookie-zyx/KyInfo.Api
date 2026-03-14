using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KyInfo.Contracts.AiChat;
using KyInfo.Infrastructure.Ai;

namespace KyInfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly AiChatGateway _aiChat;
    private readonly AiGroundingService _grounding;

    public AiController(AiChatGateway aiChat, AiGroundingService grounding)
    {
        _aiChat = aiChat;
        _grounding = grounding;
    }

    /// <summary>调用 OpenAI 兼容接口进行对话（需登录）。</summary>
    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("请求体不能为空。");
        }

        var list = new List<AiChatMessageDto>();
        if (request.Messages is { Count: > 0 })
        {
            list.AddRange(request.Messages);
        }
        else if (!string.IsNullOrWhiteSpace(request.Message))
        {
            list.Add(new AiChatMessageDto { Role = "user", Content = request.Message });
        }
        else
        {
            return BadRequest("请提供 message 或 messages。");
        }

        try
        {
            // 1) 优先尝试：从数据库直接检索并生成“带引用”的答案
            var lastUser = list.LastOrDefault(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase));
            var directReply = lastUser is null
                ? null
                : await _grounding.TryAnswerFromDbAsync(lastUser.Content, cancellationToken);

            if (!string.IsNullOrWhiteSpace(directReply))
            {
                return new AiChatResponseDto { Reply = directReply };
            }

            // 2) 否则走 LLM：由大模型生成通用回答
            var reply = await _aiChat.ChatAsync(list, cancellationToken);
            return new AiChatResponseDto { Reply = reply };
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }
}
