using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Api.Middleware
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
            catch (AuthenticationFailedException ex)
            {
                _logger.LogWarning(ex, "Autenticación fallida: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.Unauthorized, "Autenticación fallida", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.Unauthorized, "No autorizado", ex.Message);
            }
            catch (SaldoInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Saldo insuficiente: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.UnprocessableEntity, "Saldo insuficiente", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso no encontrado: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.NotFound, "Recurso no encontrado", ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflicto de concurrencia detectado.");
                await WriteProblemDetailsAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "Conflicto de concurrencia",
                    "El recurso fue modificado simultáneamente por otro usuario o proceso. Por favor, reintente la operación."
                );
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(ex, "Validación de dominio fallida: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Error de validación de dominio", ex.Message);
            }
            catch (PujaInvalidaException ex)
            {
                _logger.LogWarning(ex, "Puja inválida: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Oferta inválida", ex.Message);
            }
            catch (SubastaNoActivaException ex)
            {
                _logger.LogWarning(ex, "Subasta no activa: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Subasta inactiva", ex.Message);
            }
            catch (SubastaVencidaException ex)
            {
                _logger.LogWarning(ex, "Subasta vencida: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Subasta finalizada", ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumento inválido: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Solicitud inválida", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Operación no permitida", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado: {Mensaje}", ex.Message);
                await WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError, "Error interno del servidor", "Ha ocurrido un error inesperado al procesar su solicitud.");
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
