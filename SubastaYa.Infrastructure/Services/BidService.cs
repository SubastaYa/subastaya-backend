using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Core.Entities;
using SubastaYa.Core.Enums;
using SubastaYa.Core.Exceptions;
using SubastaYa.Core.Interfaces;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Services
{
    public class BidService : IBidService
    {
        private readonly ApplicationDbContext _context;

        public BidService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task PlaceBidAsync(int auctionId, int buyerId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("El monto de la puja debe ser mayor a cero.", nameof(amount));
            }

            var subasta = await _context.Subastas
                .Include(s => s.Pujas)
                .FirstOrDefaultAsync(s => s.Id == auctionId);

            if (subasta == null)
            {
                throw new KeyNotFoundException($"La subasta con ID {auctionId} no existe.");
            }

            if (subasta.Estado != EstadoSubasta.Activa)
            {
                throw new InvalidOperationException("La subasta no se encuentra activa para recibir ofertas.");
            }

            decimal montoMinimoRequerido = (subasta.Pujas != null && subasta.Pujas.Any())
                ? subasta.Pujas.Max(p => p.Monto) + subasta.IncrementoMinimo
                : subasta.PrecioBase;

            if (amount < montoMinimoRequerido)
            {
                throw new InvalidOperationException($"El monto de la oferta ({amount}) debe ser mayor o igual al mínimo requerido ({montoMinimoRequerido}).");
            }

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == buyerId);

            if (billetera == null)
            {
                throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {buyerId}.");
            }

            var availableBalance = billetera.SaldoTotal - billetera.SaldoRetenido;
            if (availableBalance < amount)
            {
                var auditLog = new AuditLog(
                    "INTENTO_PUJA_FALLIDO_SALDO",
                    $"Saldo insuficiente para el usuario {buyerId} en la subasta {auctionId}. Requerido: {amount}, Disponible: {availableBalance}.",
                    "Billetera",
                    billetera.Id.ToString()
                );
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                throw new SaldoInsuficienteException($"Saldo insuficiente para realizar la oferta. Saldo disponible: {availableBalance}, monto requerido: {amount}.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ultimaPuja = subasta.Pujas != null && subasta.Pujas.Any()
                    ? subasta.Pujas.OrderByDescending(p => p.Monto).ThenByDescending(p => p.FechaPuja).FirstOrDefault()
                    : null;

                if (ultimaPuja != null)
                {
                    var billeteraAnterior = (ultimaPuja.CompradorId == buyerId)
                        ? billetera
                        : await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == ultimaPuja.CompradorId);

                    if (billeteraAnterior != null)
                    {
                        billeteraAnterior.Liberar(ultimaPuja.Monto);
                        var transaccionLiberacion = new TransaccionLedger(
                            billeteraAnterior.Id,
                            TipoTransaccion.Liberacion,
                            ultimaPuja.Monto,
                            auctionId
                        );
                        _context.TransaccionesLedger.Add(transaccionLiberacion);
                    }
                }

                billetera.Retener(amount);
                var transaccionRetencion = new TransaccionLedger(
                    billetera.Id,
                    TipoTransaccion.Retencion,
                    amount,
                    auctionId
                );
                _context.TransaccionesLedger.Add(transaccionRetencion);

                var nuevaPuja = new Puja(auctionId, buyerId, amount);
                _context.Pujas.Add(nuevaPuja);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
