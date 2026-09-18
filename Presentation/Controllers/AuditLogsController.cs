using Application.DTOs;
using Application.UseCases.AuditLogs.ObtenerAuditLogs;
using Application.UseCases.AuditLogs.RegistrarAuditLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Route("api/v1/audit-logs")]
    [Route("api/audit-logs")]
    [ApiController]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private const string AuditorEmail = "auditoria@test.com";
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
            var email = User.GetUserEmail();
            if (!string.Equals(email, AuditorEmail, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensaje = "Acceso denegado. Solo el usuario de auditoría (auditoria@test.com) tiene autorización para acceder al registro de actividades."
                });
            }

            var logs = await _queryHandler.HandleAsync(query, ct);
            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAuditLogCommand command, CancellationToken ct)
        {
            var email = User.GetUserEmail();
            if (!string.Equals(email, AuditorEmail, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensaje = "Acceso denegado. Solo el usuario de auditoría (auditoria@test.com) tiene autorización para registrar eventos de auditoría."
                });
            }

            var id = await _commandHandler.HandleAsync(command, ct);
            return CreatedAtAction(nameof(GetAll), new { id }, new { id });
        }
    }
}
