using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepsisNlp.Application.Evolutions.Queries.GetRecentEvolutions;

namespace SepsisNlp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvolutionsController : ControllerBase
{
    private readonly ISender _sender;

    public EvolutionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentEvolutions()
    {
        var result = await _sender.Send(new GetRecentEvolutionsQuery());
        return Ok(result);
    }
}