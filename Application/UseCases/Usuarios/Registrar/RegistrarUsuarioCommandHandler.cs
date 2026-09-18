using Application.DTOs.Auth;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Usuarios.Registrar
{
    public class RegistrarUsuarioCommandHandler : ICommandHandler<RegistrarUsuarioCommand, AuthResponseDto>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarUsuarioCommandHandler(
            IUsuarioRepository usuarioRepository,
            IBilleteraRepository billeteraRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _billeteraRepository = billeteraRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponseDto> HandleAsync(RegistrarUsuarioCommand command, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                throw new DomainValidationException("El correo electrónico es requerido.");
            }

            if (string.IsNullOrWhiteSpace(command.Nombre))
            {
                throw new DomainValidationException("El nombre es requerido.");
            }

            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            {
                throw new DomainValidationException("La contraseña debe tener al menos 6 caracteres.");
            }

            var emailNormalizado = command.Email.Trim().ToLowerInvariant();

            var usuarioExistente = await _usuarioRepository.ObtenerPorEmailAsync(emailNormalizado, ct);
            if (usuarioExistente is not null)
            {
                throw new EmailYaRegistradoException(emailNormalizado);
            }

            var hash = _passwordHasher.Hash(command.Password);
            var usuario = new Usuario(emailNormalizado, command.Nombre.Trim(), hash);

            await _usuarioRepository.AgregarAsync(usuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Crear e inicializar billetera asociada al nuevo usuario con saldo 0
            var billetera = new Domain.Entities.Billetera(usuario.Id, 0m, 0m);
            await _billeteraRepository.AgregarAsync(billetera, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var token = _jwtProvider.GenerarToken(usuario);

            return new AuthResponseDto(token, usuario.Email, usuario.Id, usuario.Nombre);
        }
    }
}
