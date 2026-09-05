using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Subastas.Commands.CrearSubasta
{
    public class CrearSubastaCommandHandler
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CrearSubastaCommandHandler(
            ISubastaRepository subastaRepository,
            ICategoriaRepository categoriaRepository)
        {
            _subastaRepository = subastaRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<int> HandleAsync(CrearSubastaCommand command, CancellationToken ct = default)
        {
            var categoriaExiste = await _categoriaRepository.ExisteAsync(command.CategoriaId, ct);
            if (!categoriaExiste)
            {
                throw new DomainValidationException($"La categoría con ID {command.CategoriaId} no existe.");
            }

            var subasta = Subasta.Crear(
                command.VendedorId,
                command.CategoriaId,
                command.Titulo,
                command.Descripcion,
                command.UrlImagen,
                command.PrecioBase,
                command.IncrementoMinimo,
                command.FechaInicio,
                command.FechaFin
            );

            await _subastaRepository.AgregarAsync(subasta, ct);
            await _subastaRepository.GuardarCambiosAsync(ct);

            return subasta.Id;
        }
    }
}
