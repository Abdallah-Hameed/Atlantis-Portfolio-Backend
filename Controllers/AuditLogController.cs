using Atlantis_Portfolio_Backend.DTOs.AuditLogs;
using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend_BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlantis_Portfolio_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly clsAuditLogService _auditLogService;

    public AuditLogController(clsAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("Get/All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAuditLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _auditLogService.GetAllAuditLogsAsync(pageNumber, pageSize);

        var items = result.Items.Select(log => new clsAuditLogResponseDto
        {
            LogId = log.LogId,
            UserId = log.UserId,
            ActionId = log.ActionId,
            Success = log.Success,
            CreatedAt = log.CreatedAt,
            Ipaddress = log.Ipaddress,
            ActionName = log.Action?.ActionName,
            UserEmail = log.User?.Email
        }).ToList();

        return Ok(new
        {
            Items = items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages
        });
    }
}