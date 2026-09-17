using Application.DTOs;
using Application.DTOs.Auth;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.LiquidarSubastasVencidas;
using Application.UseCases.Subastas.ObtenerCatalogo;
using Application.UseCases.Subastas.ObtenerDetalle;
using Application.UseCases.Usuarios.Login;
using Application.UseCases.Ofertas.CrearOferta;
using Application.UseCases.Ofertas.ObtenerOfertaPorId;
using Application.UseCases.Ofertas.ObtenerOfertasPorSubastaId;
using Application.UseCases.Categorias.ObtenerCategorias;
using Application.UseCases.Billetera.DepositarFondos;
using Application.UseCases.Billetera.ObtenerBalance;
using Application.UseCases.Billetera.ObtenerMovimientos;
using Application.UseCases.AuditLogs.ObtenerAuditLogs;
using Application.UseCases.AuditLogs.RegistrarAuditLog;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.WebSockets;
using Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();
builder.Services.AddScoped<IOfertaRepository, OfertaRepository>();
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IQueryHandler<ObtenerAuditLogsQuery, IReadOnlyList<AuditLogDto>>, ObtenerAuditLogsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<RegistrarAuditLogCommand, Guid>, RegistrarAuditLogCommandHandler>();

builder.Services.AddScoped<ICommandHandler<LiquidarSubastasVencidasCommand>, LiquidarSubastasVencidasCommandHandler>();
builder.Services.AddHostedService<SubastasBackgroundWorker>();

builder.Services.AddScoped<ICommandHandler<CrearSubastaCommand, int>, CrearSubastaCommandHandler>();
builder.Services.AddScoped<IQueryHandler<ObtenerCatalogoQuery, IReadOnlyList<SubastaListDto>>, ObtenerCatalogoQueryHandler>();
builder.Services.AddScoped<IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDetalleDto?>, ObtenerSubastaPorIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CrearOfertaCommand, int>, CrearOfertaCommandHandler>();
builder.Services.AddScoped<IQueryHandler<ObtenerOfertaPorIdQuery, OfertaResumenDto?>, ObtenerOfertaPorIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<ObtenerOfertasPorSubastaIdQuery, IReadOnlyList<OfertaResumenDto>>, ObtenerOfertasPorSubastaIdQueryHandler>();

builder.Services.AddScoped<IQueryHandler<ObtenerBalanceQuery, WalletResponseDto>, ObtenerBalanceQueryHandler>();
builder.Services.AddScoped<ICommandHandler<DepositarFondosCommand, WalletResponseDto>, DepositarFondosCommandHandler>();
builder.Services.AddScoped<IQueryHandler<ObtenerMovimientosQuery, IReadOnlyList<TransaccionLedgerDto>>, ObtenerMovimientosQueryHandler>();

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IQueryHandler<ObtenerCategoriasQuery, IReadOnlyList<CategoriaDto>>, ObtenerCategoriasQueryHandler>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, AuthResponseDto>, LoginCommandHandler>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IAuctionHubService, AuctionHubService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa tu token JWT en el formato: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Aplica las migraciones pendientes automáticamente al iniciar
    context.Database.Migrate();
    DbInitializer.Initialize(context);
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
