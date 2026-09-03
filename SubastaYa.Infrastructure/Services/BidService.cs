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

            decimal montoMinimoRequerido;
            if (subasta.Pujas != null && subasta.Pujas.Any())
            {
                var pujaActualMasAlta = subasta.Pujas.Max(p => p.Monto);
                montoMinimoRequerido = pujaActualMasAlta + subasta.IncrementoMinimo;
            }
            else
            {
                montoMinimoRequerido = subasta.PrecioBase;
            }

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
                throw new SaldoInsuficienteException($"Saldo insuficiente para realizar la oferta. Saldo disponible: {availableBalance}, monto requerido: {amount}.");
            }

            
        }
    }
}
