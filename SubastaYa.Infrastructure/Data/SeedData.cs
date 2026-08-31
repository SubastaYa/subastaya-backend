using System;
using System.Collections.Generic;
using System.Linq;
using SubastaYa.Core.Entities;
using SubastaYa.Core.Enums;

namespace SubastaYa.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Usuarios.Any())
            {
                return;
            }

            // 1. Insertar Categorías
            var catTecnologia = new Categoria("Tecnología", "https://img.icons8.com/color/96/laptop.png");
            var catColeccionables = new Categoria("Coleccionables", "https://img.icons8.com/color/96/trophy.png");
            var catIndumentaria = new Categoria("Indumentaria", "https://img.icons8.com/color/96/t-shirt.png");
            var catVehiculos = new Categoria("Vehículos", "https://img.icons8.com/color/96/car.png");

            context.Categorias.AddRange(catTecnologia, catColeccionables, catIndumentaria, catVehiculos);
            context.SaveChanges();

            // 2. Insertar Usuarios
            var vendedor = new Usuario("vendedor@test.com", "Vendedor Test", "123456");
            var comprador1 = new Usuario("comprador1@test.com", "Comprador 1 Test", "123456");
            var comprador2 = new Usuario("comprador2@test.com", "Comprador 2 Test", "123456");
            var sinfondos = new Usuario("sinfondos@test.com", "Sin Fondos Test", "123456");

            context.Usuarios.AddRange(vendedor, comprador1, comprador2, sinfondos);
            context.SaveChanges();

            // 3. Insertar Billeteras
            var billeteraVendedor = new Billetera(vendedor.Id, 0m, 0m);
            var billeteraComprador1 = new Billetera(comprador1.Id, 150000m, 45000m);
            var billeteraComprador2 = new Billetera(comprador2.Id, 200000m, 0m);
            var billeteraSinfondos = new Billetera(sinfondos.Id, 500m, 0m);

            context.Billeteras.AddRange(billeteraVendedor, billeteraComprador1, billeteraComprador2, billeteraSinfondos);
            context.SaveChanges();

            // 4. Insertar Subastas
            var subastaActivaEstandar = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catTecnologia.Id,
                titulo: "iPhone 15 Pro Max 256GB",
                descripcion: "Subasta Activa estándar de Smartphone de última generación. Cierra en 30 minutos.",
                urlImagen: "/images/default-subasta.jpg",
                precioBase: 30000m,
                incrementoMinimo: 1000m,
                fechaInicio: DateTime.UtcNow.AddHours(-2),
                fechaFin: DateTime.UtcNow.AddMinutes(30),
                estado: EstadoSubasta.Activa
            );

            var subastaActivaCritica = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catTecnologia.Id,
                titulo: "MacBook Pro M3 16-inch",
                descripcion: "Subasta Activa crítica por cerrar en 1 minuto.",
                urlImagen: "/images/default-subasta.jpg",
                precioBase: 100000m,
                incrementoMinimo: 5000m,
                fechaInicio: DateTime.UtcNow.AddHours(-1),
                fechaFin: DateTime.UtcNow.AddMinutes(1),
                estado: EstadoSubasta.Activa
            );

            var subastaProxima = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catVehiculos.Id,
                titulo: "Tesla Model 3 2023 Long Range",
                descripcion: "Subasta Programada con fecha de inicio en 24 horas.",
                urlImagen: "/images/default-subasta.jpg",
                precioBase: 5000000m,
                incrementoMinimo: 100000m,
                fechaInicio: DateTime.UtcNow.AddHours(24),
                fechaFin: DateTime.UtcNow.AddHours(48),
                estado: EstadoSubasta.Programada
            );

            var subastaVencidaGanador = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catColeccionables.Id,
                titulo: "Reloj Rolex Submariner Vintage 1980",
                descripcion: "Subasta Finalizada con ganador y ofertas registradas.",
                urlImagen: "/images/default-subasta.jpg",
                precioBase: 50000m,
                incrementoMinimo: 2000m,
                fechaInicio: DateTime.UtcNow.AddDays(-3),
                fechaFin: DateTime.UtcNow.AddDays(-1),
                estado: EstadoSubasta.Finalizada
            );

            var subastaVencidaDesierta = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catIndumentaria.Id,
                titulo: "Campera de Cuero Exclusiva Edición Limitada",
                descripcion: "Subasta Finalizada sin pujas registradas (Desierta).",
                urlImagen: "/images/default-subasta.jpg",
                precioBase: 80000m,
                incrementoMinimo: 2000m,
                fechaInicio: DateTime.UtcNow.AddDays(-4),
                fechaFin: DateTime.UtcNow.AddDays(-2),
                estado: EstadoSubasta.Desierta
            );

            context.Subastas.AddRange(
                subastaActivaEstandar,
                subastaActivaCritica,
                subastaProxima,
                subastaVencidaGanador,
                subastaVencidaDesierta
            );
            context.SaveChanges();

            // 5. Insertar Pujas
            var puja1Activa = new Puja(subastaActivaEstandar.Id, comprador2.Id, 40000m);
            var puja2Activa = new Puja(subastaActivaEstandar.Id, comprador1.Id, 45000m);

            var pujaVencida = new Puja(subastaVencidaGanador.Id, comprador1.Id, 60000m);

            context.Pujas.AddRange(puja1Activa, puja2Activa, pujaVencida);
            context.SaveChanges();

            // 6. Insertar Transacción Ledger
            var transaccionLedger = new TransaccionLedger(
                billeteraId: billeteraComprador1.Id,
                tipo: TipoTransaccion.Retencion,
                monto: 45000m,
                subastaId: subastaActivaEstandar.Id
            );

            context.TransaccionesLedger.Add(transaccionLedger);
            context.SaveChanges();
        }
    }
}
