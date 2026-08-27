using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepsisNlp.Application.Inferences.Commands;

namespace SepsisNlp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InferencesController : ControllerBase
{
    private readonly ISender _sender;

    public InferencesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("execute-full-dataset-inference")]
    public async Task<IActionResult> ExecuteFullDatasetInference()
    {
        // Dispara o nosso Maestro!
        var result = await _sender.Send(new RunExperimentCommand());

        return Ok(new { Message = result });
    }
}