using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepsisNlp.Application.Patients.Queries.GetPatientRollback;

namespace SepsisNlp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly ISender _sender;

    public AuditController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("rollback/patient/{pseudonym}")]
    public async Task<IActionResult> GetPatientOriginalData(string pseudonym)
    {
        var query = new GetPatientRollbackQuery(pseudonym);
        var result = await _sender.Send(query);

        if (result == null)
            return NotFound(new { Message = "Pseudônimo não encontrado ou registro inexistente." });

        return Ok(result);
    }
}