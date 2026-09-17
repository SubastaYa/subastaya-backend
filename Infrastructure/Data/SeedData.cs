using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Usuarios.Any())
            {
                return;
            }

            
            var catTecnologia = new Categoria("Tecnología", "https://img.icons8.com/color/96/laptop.png");
            var catColeccionables = new Categoria("Coleccionables", "https://img.icons8.com/color/96/trophy.png");
            var catIndumentaria = new Categoria("Indumentaria", "https://img.icons8.com/color/96/t-shirt.png");
            var catVehiculos = new Categoria("Vehículos", "https://img.icons8.com/color/96/car.png");

            context.Categorias.AddRange(catTecnologia, catColeccionables, catIndumentaria, catVehiculos);
            context.SaveChanges();

           
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");
            var vendedor = new Usuario("vendedor@test.com", "Vendedor Test", passwordHash);
            var comprador1 = new Usuario("comprador1@test.com", "Comprador 1 Test", passwordHash);
            var comprador2 = new Usuario("comprador2@test.com", "Comprador 2 Test", passwordHash);
            var sinfondos = new Usuario("sinfondos@test.com", "Sin Fondos Test", passwordHash);
            var compradorHistorico = new Usuario("comprador.historico@test.com", "Comprador Histórico Test", passwordHash);

            context.Usuarios.AddRange(vendedor, comprador1, comprador2, sinfondos, compradorHistorico);
            context.SaveChanges();

            var billeteraVendedor = new Billetera(vendedor.Id, 0m, 0m);
            var billeteraComprador1 = new Billetera(comprador1.Id, 150000m, 45000m);
            var billeteraComprador2 = new Billetera(comprador2.Id, 200000m, 0m);
            var billeteraSinfondos = new Billetera(sinfondos.Id, 500m, 0m);
            var billeteraCompradorHistorico = new Billetera(compradorHistorico.Id, 50000m, 50000m);

            context.Billeteras.AddRange(billeteraVendedor, billeteraComprador1, billeteraComprador2, billeteraSinfondos, billeteraCompradorHistorico);
            context.SaveChanges();

            var subastaActivaEstandar = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catTecnologia.Id,
                titulo: "iPhone 15",
                descripcion: "Subasta Activa estándar de Smartphone de última generación. Cierra en 30 minutos.",
                urlImagen: "https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=800&q=80",
                precioBase: 30000m,
                incrementoMinimo: 1000m,
                fechaInicio: DateTime.UtcNow.AddHours(-2),
                fechaFin: DateTime.UtcNow.AddMinutes(30),
                estado: EstadoSubasta.Activa
            );

            var subastaActivaCritica = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catTecnologia.Id,
                titulo: "MacBook Pro",
                descripcion: "Subasta Activa crítica por cerrar en 1 minuto.",
                urlImagen: "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=800&q=80",
                precioBase: 100000m,
                incrementoMinimo: 5000m,
                fechaInicio: DateTime.UtcNow.AddHours(-1),
                fechaFin: DateTime.UtcNow.AddMinutes(1),
                estado: EstadoSubasta.Activa
            );

            var subastaProxima = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catVehiculos.Id,
                titulo: "Toyota Corolla 2019",
                descripcion: "Subasta Programada con fecha de inicio en 24 horas.",
                urlImagen: "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&w=800&q=80",
                precioBase: 5000000m,
                incrementoMinimo: 100000m,
                fechaInicio: DateTime.UtcNow.AddHours(24),
                fechaFin: DateTime.UtcNow.AddHours(48),
                estado: EstadoSubasta.Programada
            );

            // Subasta vencida con ganador para validar el cierre y liquidación del worker
            var subastaVencidaGanador = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catColeccionables.Id,
                titulo: "Reloj Casio",
                descripcion: "Subasta Vencida con ganador y ofertas registradas para liquidación del worker.",
                urlImagen: "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=800&q=80",
                precioBase: 50000m,
                incrementoMinimo: 2000m,
                fechaInicio: DateTime.UtcNow.AddDays(-3),
                fechaFin: DateTime.UtcNow.AddMinutes(-5),
                estado: EstadoSubasta.Activa
            );

            // Subasta vencida sin ofertas para validar pase a DESIERTA por el worker
            var subastaVencidaDesierta = new Subasta(
                vendedorId: vendedor.Id,
                categoriaId: catIndumentaria.Id,
                titulo: "Campera Adidas Originals",
                descripcion: "Subasta Vencida sin ofertas registradas para pase a DESIERTA por el worker.",
                urlImagen: "https://images.unsplash.com/photo-1544441893-675973e31985?auto=format&fit=crop&w=800&q=80",
                precioBase: 80000m,
                incrementoMinimo: 2000m,
                fechaInicio: DateTime.UtcNow.AddDays(-4),
                fechaFin: DateTime.UtcNow.AddMinutes(-10),
                estado: EstadoSubasta.Activa
            );

            context.Subastas.AddRange(
                subastaActivaEstandar,
                subastaActivaCritica,
                subastaProxima,
                subastaVencidaGanador,
                subastaVencidaDesierta
            );
            context.SaveChanges();

            var oferta1Activa = new Oferta(subastaActivaEstandar.Id, comprador2.Id, 40000m);
            var oferta2Activa = new Oferta(subastaActivaEstandar.Id, comprador1.Id, 45000m);
            var ofertaVencida = new Oferta(subastaVencidaGanador.Id, compradorHistorico.Id, 50000m);

            context.Ofertas.AddRange(oferta1Activa, oferta2Activa, ofertaVencida);
            context.SaveChanges();

            // Movimientos contables de la subasta activa: oferta inicial de comprador2 y superación de comprador1
            var depComprador1 = new TransaccionLedger(billeteraComprador1.Id, TipoTransaccion.Deposito, 150000m);
            var retComprador1 = new TransaccionLedger(billeteraComprador1.Id, TipoTransaccion.Retencion, 45000m, subastaActivaEstandar.Id);

            var depComprador2 = new TransaccionLedger(billeteraComprador2.Id, TipoTransaccion.Deposito, 200000m);
            var retComprador2 = new TransaccionLedger(billeteraComprador2.Id, TipoTransaccion.Retencion, 40000m, subastaActivaEstandar.Id);
            var libComprador2 = new TransaccionLedger(billeteraComprador2.Id, TipoTransaccion.Liberacion, 40000m, subastaActivaEstandar.Id);

            var depSinfondos = new TransaccionLedger(billeteraSinfondos.Id, TipoTransaccion.Deposito, 500m);

            // Depósito y retención para respaldar la liquidación de la subasta vencida
            var depCompradorHistorico = new TransaccionLedger(billeteraCompradorHistorico.Id, TipoTransaccion.Deposito, 50000m);
            var retCompradorHistorico = new TransaccionLedger(billeteraCompradorHistorico.Id, TipoTransaccion.Retencion, 50000m, subastaVencidaGanador.Id);

            context.TransaccionesLedger.AddRange(
                depComprador1,
                retComprador1,
                depComprador2,
                retComprador2,
                libComprador2,
                depSinfondos,
                depCompradorHistorico,
                retCompradorHistorico
            );
            context.SaveChanges();
        }
    }
}
