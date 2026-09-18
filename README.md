# SubastaYa — Backend API

> **Proyecto desarrollado por**: Antunes Julián y Florentin Javier.

<div align="center">
  <img src="https://skillicons.dev/icons?i=cs" height="40" alt="C# logo" />
  &nbsp;&nbsp;
  <img src="https://skillicons.dev/icons?i=dotnet" height="40" alt=".NET 8 logo" />
  &nbsp;&nbsp;
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/microsoftsqlserver/microsoftsqlserver-plain.svg" height="40" alt="SQL Server logo" />
  &nbsp;&nbsp;
  <img src="https://skillicons.dev/icons?i=docker" height="40" alt="Docker logo" />
  &nbsp;&nbsp;
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/swagger/swagger-original.svg" height="40" alt="Swagger logo" />
  &nbsp;&nbsp;
  <img src="https://skillicons.dev/icons?i=postman" height="40" alt="Postman logo" />
  &nbsp;&nbsp;
  <img src="https://skillicons.dev/icons?i=powershell" height="40" alt="PowerShell logo" />
  &nbsp;&nbsp;
  <img src="https://skillicons.dev/icons?i=git" height="40" alt="Git logo" />
</div>

<br />

Plataforma de subastas en tiempo real desarrollada con **.NET 8**, **ASP.NET Core Web API**, **Entity Framework Core (Code-First)**, **SQL Server** y **SignalR**, siguiendo los principios de **Clean Architecture**, **CQRS** y **Concurrencia Optimista**.

---

## 🛠️ Tecnologías y Herramientas

* **Lenguaje y Framework**: C# 12 / .NET 8 SDK
* **Acceso a Datos**: Entity Framework Core 8.0 (SQL Server Provider, Migraciones Code-First)
* **Base de Datos**: Microsoft SQL Server 2022 (vía Docker)
* **Comunicación en Tiempo Real**: ASP.NET Core SignalR (WebSockets con soporte de autenticación JWT)
* **Autenticación y Seguridad**: JWT (JSON Web Tokens) con algoritmo HMAC-SHA256 y hashing de contraseñas adaptativo con **BCrypt.Net-Next**
* **Procesamiento en Segundo Plano**: Background Worker (`IHostedService`) con timers asíncronos periódicos
* **Documentación de API**: Swagger / OpenAPI (Swashbuckle)
* **Contenedores**: Docker & Docker Compose

---

## 🏛️ Arquitectura del Proyecto

El sistema está estructurado en cuatro capas concéntricas con inversión de dependencias estricta:

```
subastaya-backend/
│
├── Domain/                 # Entidades, Value Objects, Enums y Excepciones de Dominio (Cero dependencias)
│   ├── Entities/           # Subasta, Oferta, Billetera, TransaccionLedger, Usuario, Categoria, AuditLog
│   ├── Enums/              # EstadoSubasta, TipoTransaccion
│   └── Exceptions/         # DomainValidationException, SaldoInsuficienteException, etc.
│
├── Application/            # Casos de Uso, DTOs, Interfaces y Lógica de Aplicación (CQRS)
│   ├── Common/Helpers/     # UsuarioHelper (ofuscación centralizada de seudónimos)
│   ├── DTOs/               # Contratos inmutables de transferencia (Records)
│   ├── Interfaces/         # IUnitOfWork, Repositorios e interfaces de servicios
│   └── UseCases/           # Commands y Queries segregados por módulo (Subastas, Ofertas, Billetera, etc.)
│
├── Infrastructure/         # Persistencia, Migraciones, Servicios Externos y Workers
│   ├── Data/               # ApplicationDbContext, Configuraciones Fluent API, Migraciones y SeedData
│   ├── Repositories/       # Implementación de repositorios con consultas optimizadas en LINQ
│   ├── Services/           # AuctionHubService, JwtProvider, BcryptPasswordHasher
│   ├── WebSockets/         # AuctionHub (SignalR)
│   └── Workers/            # SubastasBackgroundWorker (liquidación y activación automática)
│
└── Presentation/           # API REST, Middleware, Inyección de Dependencias y Controladores
    ├── Controllers/        # Subastas, Ofertas, Billetera, Categorías, Auth y AuditLogs (Doble ruteo /api/v1/)
    ├── Extensions/         # ClaimsPrincipalExtensions
    └── Middleware/         # ExceptionMiddleware (captura centralizada y mapeo a códigos HTTP estándar)
```

### Patrones de Diseño Clave
* **CQRS (Command Query Responsibility Segregation)**: Las operaciones que modifican estado (`ICommand<T>`) están completamente separadas de las operaciones de consulta (`IQuery<T>`).
* **Unit of Work & Transaccionalidad ACID**: Bloques transaccionales con `BeginTransactionAsync`, `CommitTransactionAsync` y `RollbackTransactionAsync` que garantizan que las operaciones de oferta, retención en garantía y libro mayor contable se ejecuten de manera atómica.
* **Concurrencia Optimista (`RowVersion`)**: La entidad `Subasta` posee un token binario administrado por SQL Server para evitar condiciones de carrera (Race Conditions) cuando múltiples usuarios ofertan en el mismo milisegundo, retornando `HTTP 409 Conflict`.
* **Proyección SQL Pura**: Las consultas leen directamente a DTOs mediante `.Select()` de LINQ, optimizando los comandos generados a nivel de base de datos sin sobrecarga de reflexión.

---

## 📋 Requisitos Previos

Antes de ejecutar el proyecto, asegurarse de contar con las siguientes herramientas instaladas:

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (con Docker Compose habilitado)
* [Git](https://git-scm.com/)
* Terminal PowerShell 7+ (Windows) o Bash (Linux / macOS)

---

## 🚀 Despliegue y Puesta en Marcha

### 1. Clonar el repositorio
```bash
git clone https://github.com/SubastaYa/subastaya-backend.git
cd subastaya-backend
```

### 2. Configurar Variables de Entorno
Copiar el archivo `.env.example` como `.env`:
```bash
cp .env.example .env
```
Verificar que `SA_PASSWORD` coincida con una contraseña segura requerida por SQL Server (por ejemplo: `TuPasswordSeguroAqui123!`).

### 3. Levantar la Base de Datos en Docker
Ejecutar el contenedor de SQL Server 2022 en segundo plano:
```bash
docker compose up -d
```
Para comprobar que el contenedor está saludable:
```bash
docker ps
```

### 4. Configurar la Cadena de Conexión
En `Presentation/appsettings.json` (o `appsettings.Development.json`), asegurarse de que la cadena de conexión coincida con las credenciales de tu contenedor:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=subastaya-sqlserver;User Id=sa;Password=TuPasswordSeguroAqui123!;TrustServerCertificate=True;"
}
```

### 5. Compilar y Ejecutar el Backend
```bash
dotnet run --project Presentation
```

> **Nota de Despliegue Automático (Zero-Touch)**:  
> El sistema incluye `context.Database.Migrate()` y `DbInitializer.Initialize(context)` en el método de arranque de `Program.cs`. Esto significa que **todas las tablas, columnas, índices y datos semilla de prueba se aplican automáticamente** la primera vez que se ejecuta la aplicación, sin necesidad de ejecutar comandos manuales de Entity Framework.

### 6. Acceso a la Documentación Interactiva (Swagger)
Una vez iniciado el servidor, abrir el navegador en:
* **HTTP**: `http://localhost:5017/swagger`
* **HTTPS**: `https://localhost:7076/swagger`

---

## 👥 Datos Semilla de Prueba (Seed Data)

El sistema incluye usuarios y escenarios precargados para pruebas funcionales y de evaluación. Todas las cuentas tienen la contraseña: `123456`.

| Email | Contraseña | Rol / Perfil | Saldo Total | Saldo Retenido | Saldo Disponible | Propósito de Prueba |
| :--- | :---: | :--- | :---: | :---: | :---: | :--- |
| `vendedor@test.com` | `123456` | Vendedor | $0 | $0 | $0 | Creación y administración de publicaciones. |
| `comprador1@test.com` | `123456` | Comprador Habilitado | $150.000 | $45.000 | $105.000 | Postor líder actual en la subasta activa estándar. |
| `comprador2@test.com` | `123456` | Comprador Habilitado | $200.000 | $0 | $200.000 | Postor con fondos suficientes para pujar. Historial con retención y liberación previa de $40.000. |
| `sinfondos@test.com` | `123456` | Comprador sin saldo | $500 | $0 | $500 | Validación de rechazos por saldo insuficiente (`HTTP 400`). |
| `comprador.historico@test.com` | `123456` | Cuenta del sistema | $50.000 | $50.000 | $0 | Respalda la liquidación de la subasta vencida en el arranque del worker. |
| `auditoria@test.com` | `123456` | Auditor del Sistema | $0 | $0 | $0 | Acceso exclusivo al Registro de Actividades. Bloqueado para ofertar, publicar o cargar saldo. |

### Subastas Preconfiguradas
1. **iPhone 15 Pro Max (ID 1)**: Subasta activa estándar ($100.000 base). Recibió oferta inicial de `comprador2` ($40.000) y superación de `comprador1` ($45.000).
2. **MacBook Pro M3 (ID 2)**: Subasta activa crítica (vence en menos de 2 minutos). Ideal para comprobar la regla **Anti-Sniping** (extensión automática de 2 minutos ante ofertas en el último minuto).
3. **PlayStation 5 Pro (ID 3)**: Subasta en estado **Programada** (inicia en el futuro). Ideal para validar el temporizador regresivo de apertura.
4. **Reloj Casio Vintage (ID 4)**: Subasta vencida con ofertas. Liquidada automáticamente por el Background Worker al iniciar el servidor.
5. **Campera Adidas Originals (ID 5)**: Subasta vencida sin ofertas. Declarada desierta automáticamente por el Worker.

---

## ⚡ Prueba de Concurrencia Optimista (Stress Test)

El sistema implementa **Optimistic Locking** mediante la propiedad `RowVersion` en la tabla `Subastas`. Cuando dos usuarios envían ofertas concurrentes sobre el mismo lote con milisegundos de diferencia, el primer request actualiza la fila e incrementa la versión binaria, mientras que el segundo colisiona, detonando una `DbUpdateConcurrencyException` que el middleware captura para responder `HTTP 409 Conflict`.

### Instrucciones para Ejecutar la Prueba

1. Asegurarse de que el backend esté corriendo (`dotnet run --project Presentation`).
2. Abrir una consola de PowerShell y ejecutar el script automatizado:

```powershell
.\Test\concurrency_test.ps1 -baseUrl "http://localhost:5017"
```

### ¿Qué hace el script?
1. Inicia sesión automáticamente con `comprador1@test.com` y `comprador2@test.com` y obtiene sus tokens JWT.
2. Consulta el precio actual de la subasta ID 1.
3. Dispara **dos peticiones HTTP POST en hilos paralelos independientes (Start-Job)** con el mismo monto exacto.
4. Muestra la respuesta en pantalla evidenciando:
   * **Petición Ganadora**: `HTTP 201 Created`
   * **Petición Concurrente Derrotada**: `HTTP 409 Conflict` (con mensaje de colisión y registro automático en auditoría)

---

## 🛡️ Módulo de Auditoría y Trazabilidad

El sistema incorpora un registro inmutable de eventos para asegurar la transparencia, auditoría y trazabilidad operativa de todas las acciones del ciclo de vida de las subastas.

### Eventos Auditados Automáticamente
* **Activación por Worker (`ACTIVACION_WORKER`)**: Registro del cambio automático de subastas de estado *Programada* a *Activa*.
* **Intentos Fallidos de Puja (`INTENTO_OFERTA_FALLIDO_*`)**: Monitoreo de intentos de autopuja por el vendedor, pujas redundantes del postor líder o intentos sin saldo suficiente.
* **Colisiones de Concurrencia**: Detección y registro de conflictos de ofertas simultáneas (`HTTP 409 Conflict`).
* **Movimientos de Billetera (`ACREDITACION_SALDO`)**: Auditoría de depósitos y transacciones contables del Ledger.
* **Notas Manuales (`NOTA_AUDITORIA`)**: Eventos creados expresamente por el auditor.

### Política de Acceso y Restricciones de Seguridad
1. **Acceso Exclusivo**: Únicamente el usuario **`auditoria@test.com`** tiene autorización para consultar (`GET /api/v1/audit-logs`) y registrar (`POST /api/v1/audit-logs`) eventos de auditoría. Cualquier otro usuario recibe `HTTP 403 Forbidden`.
2. **Rol de Solo Consulta Operativa**: Por diseño y segregación de funciones, la cuenta `auditoria@test.com` tiene **estrictamente bloqueada** la capacidad de:
   * Realizar ofertas en subastas (`HTTP 403 Forbidden`).
   * Realizar cargas o depósitos de dinero (`HTTP 403 Forbidden`).
   * Publicar nuevas subastas (`HTTP 403 Forbidden`).
3. **Navegación Dedicada**: En el frontend, la barra de navegación superior muestra el botón **"Actividades"** únicamente para esta cuenta, ocultando las funciones transaccionales.

---

## 📡 Endpoints Principales y Rutas RESTful

Todos los endpoints admiten versionado canónico con prefijo `/api/v1/` y mantienen compatibilidad con `/api/`.

### Autenticación
* `POST /api/v1/auth/login` — Autenticación con email y password. Retorna token JWT y perfil básico (`id`, `nombre`, `email`).

### Subastas
* `GET /api/v1/subastas` — Catálogo con filtros (`categoriaId`, `estado`, `busqueda`, `precioMin`, `precioMax`, `orden`, `page`, `pageSize`).
* `GET /api/v1/subastas/{id}` — Detalle completo de una subasta con últimas ofertas y `postorLiderId`.
* `POST /api/v1/subastas` — Publicación de una nueva subasta (`[Authorize]`).
* `GET /api/v1/users/me/auctions` — Listado de publicaciones del vendedor autenticado con recaudación obtenida y nombre de ganador.
* `GET /api/v1/users/me/bids` — Listado de ofertas del comprador autenticado indicando si es líder o si ganó el lote.

### Ofertas (Bids)
* `GET /api/v1/subastas/{auctionId}/ofertas` — Historial de ofertas de una subasta con nombres ofuscados (`J***z`).
* `POST /api/v1/subastas/{auctionId}/ofertas` — Enviar oferta validando saldo, incremento mínimo y anti-autopuja (`[Authorize]`).

### Billetera Virtual
* `GET /api/v1/billetera/balance` — Consulta de saldo total, retenido y disponible (`[Authorize]`).
* `POST /api/v1/billetera/deposito` — Acreditación manual de fondos (`[Authorize]`).
* `GET /api/v1/billetera/movimientos` — Historial de transacciones contables del libro mayor (Ledger) (`[Authorize]`).

### Auditoría (CQRS)
* `GET /api/v1/audit-logs` — Consulta paginada y filtrada de eventos de auditoría inmutables (Exclusivo `auditoria@test.com` - `[Authorize]`).
* `POST /api/v1/audit-logs` — Registro manual de eventos de trazabilidad (Exclusivo `auditoria@test.com` - `[Authorize]`).

### Categorías
* `GET /api/v1/categorias` — Listado de categorías disponibles.

---

## 🔄 WebSocket Hub (SignalR)

* **Ruta de Conexión**: `/hubs/auctions` (Requiere enviar el token JWT en el query string: `?access_token={token}`).
* **Grupos de Sala**: Cada subasta posee su grupo en tiempo real. Para unirse: invocar método `JoinAuctionGroup(auctionId)`.
* **Eventos Emitidos por el Servidor**:
  * `ReceiveNewBid` / `ReceiveNewOffer`: `{ subastaId, monto, postor, timestamp, buyerId }`
  * `TimeExtension`: `{ subastaId, nuevaFechaFin }`
  * `AuctionClosed`: `{ subastaId, estado, ganador, montoFinal }`

---

## 🧪 Pruebas con Postman

Se incluye una colección de Postman lista para importar en el directorio:
```
postman/collections/SubastaYa - API/
```
Permite probar todos los flujos de autenticación, publicación, ofertas simultáneas y transacciones contables de manera inmediata.
