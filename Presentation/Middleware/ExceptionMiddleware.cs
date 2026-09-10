using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (statusCode, title, detail, isWarning) = ex switch
                {
                    AuthenticationFailedException => (HttpStatusCode.Unauthorized, "Autenticación fallida", ex.Message, true),
                    UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado", ex.Message, true),
                    SaldoInsuficienteException => (HttpStatusCode.UnprocessableEntity, "Saldo insuficiente", ex.Message, true),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado", ex.Message, true),
                    DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Conflicto de concurrencia", "El recurso fue modificado simultáneamente por otro usuario o proceso. Por favor, reintente la operación.", true),
                    DomainValidationException => (HttpStatusCode.BadRequest, "Error de validación de dominio", ex.Message, true),
                    PujaInvalidaException => (HttpStatusCode.BadRequest, "Oferta inválida", ex.Message, true),
                    SubastaNoActivaException => (HttpStatusCode.Conflict, "Subasta inactiva", ex.Message, true),
                    SubastaVencidaException => (HttpStatusCode.Conflict, "Subasta finalizada", ex.Message, true),
                    ArgumentException => (HttpStatusCode.BadRequest, "Solicitud inválida", ex.Message, true),
                    InvalidOperationException => (HttpStatusCode.BadRequest, "Operación no permitida", ex.Message, true),
                    _ => (HttpStatusCode.InternalServerError, "Error interno del servidor", "Ha ocurrido un error inesperado al procesar su solicitud.", false)
                };

                if (isWarning)
                {
                    _logger.LogWarning(ex, "{Title}: {Detail}", title, detail);
                }
                else
                {
                    _logger.LogError(ex, "Error crítico no controlado: {Mensaje}", ex.Message);
                }

                await WriteProblemDetailsAsync(context, statusCode, title, detail);
            }
        }

        private static async Task WriteProblemDetailsAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string title,
            string detail)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(json);
        }
    }
}
