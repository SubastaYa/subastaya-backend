using Application.DTOs;
using Application.UseCases.Categorias.ObtenerCategorias;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/categorias")]
    [Route("api/v1/categories")]
    [Route("api/categorias")]
    [Route("api/categories")]
    public class CategoriasController : ControllerBase
    {
        private readonly IQueryHandler<ObtenerCategoriasQuery, IReadOnlyList<CategoriaDto>> _categoriasHandler;

        public CategoriasController(IQueryHandler<ObtenerCategoriasQuery, IReadOnlyList<CategoriaDto>> categoriasHandler)
        {
            _categoriasHandler = categoriasHandler;
        }

        // Endpoint público para cargar dinámicamente las categorías en el catálogo y formulario de subastas
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var resultado = await _categoriasHandler.HandleAsync(new ObtenerCategoriasQuery(), ct);
            return Ok(resultado);
        }
    }
}