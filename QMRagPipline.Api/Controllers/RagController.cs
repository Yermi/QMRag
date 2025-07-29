using Microsoft.AspNetCore.Mvc;
using QMRagPipeline.Services;

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
        public async Task<IActionResult> Ask([FromQuery] string question)
        {
            var result = await _runner.AnswerQuestionAsync(question);
            return Ok(new { answer = result });
        }
    }
}
