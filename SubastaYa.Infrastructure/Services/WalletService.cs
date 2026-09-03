using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Core.DTOs;
using SubastaYa.Core.Entities;
using SubastaYa.Core.Enums;
using SubastaYa.Core.Interfaces;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WalletResponseDto> GetBalanceAsync(int userId)
        {
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == userId);

            if (billetera == null)
            {
                throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");
            }

            // El saldo disponible se calcula en memoria (Total - Retenido)
            var saldoDisponible = billetera.SaldoTotal - billetera.SaldoRetenido;

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, saldoDisponible);
        }

        public async Task<WalletResponseDto> DepositAsync(int userId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("El monto a depositar debe ser mayor a cero.", nameof(amount));
            }

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == userId);

            if (billetera == null)
            {
                throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");
            }

            // Sumar el amount a TotalBalance y actualizar SaldoDisponible
            billetera.Depositar(amount);

            // Registrar movimiento en TransaccionLedger con tipo DEPOSITO
            var transaccion = new TransaccionLedger(billetera.Id, TipoTransaccion.Deposito, amount);
            _context.TransaccionesLedger.Add(transaccion);

            await _context.SaveChangesAsync();

            // Saldo disponible calculado en memoria
            var saldoDisponible = billetera.SaldoTotal - billetera.SaldoRetenido;

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, saldoDisponible);
        }
    }
}
