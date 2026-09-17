using Application.DTOs;
using Application.UseCases.AuditLogs.ObtenerAuditLogs;
using Application.UseCases.AuditLogs.RegistrarAuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/v1/audit-logs")]
    [Route("api/audit-logs")]
    [ApiController]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IQueryHandler<ObtenerAuditLogsQuery, IReadOnlyList<AuditLogDto>> _queryHandler;
        private readonly ICommandHandler<RegistrarAuditLogCommand, Guid> _commandHandler;

        public AuditLogsController(
            IQueryHandler<ObtenerAuditLogsQuery, IReadOnlyList<AuditLogDto>> queryHandler,
            ICommandHandler<RegistrarAuditLogCommand, Guid> commandHandler)
        {
            _queryHandler = queryHandler;
            _commandHandler = commandHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ObtenerAuditLogsQuery query, CancellationToken ct)
        {
            var logs = await _queryHandler.HandleAsync(query, ct);
            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAuditLogCommand command, CancellationToken ct)
        {
            var id = await _commandHandler.HandleAsync(command, ct);
            return CreatedAtAction(nameof(GetAll), new { id }, new { id });
        }
    }
}
