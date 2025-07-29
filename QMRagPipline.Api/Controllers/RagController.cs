using Microsoft.AspNetCore.Mvc;
using QMRagPipeline.Services;
using QMRagPipline.Api.Filters;

namespace QMRagPipline.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly RagPipelineRunner _runner;

        public RagController(RagPipelineRunner runner)
        {
            _runner = runner;
        }

        [HttpGet("build")]
        public async Task<IActionResult> BuildIndex()
        {
            await _runner.BuildIndexAsync();
            return NoContent();
        }

        [HttpGet("ask")]
        [SessionId]
        public async Task<IActionResult> Ask([FromQuery] string question)
        {
            var sessionId = HttpContext.Items["SessionId"]?.ToString()!;

            var result = await _runner.AnswerQuestionAsync(question, sessionId);
            return Ok(new { answer = result });
        }
    }
}
