# LinkUp - Red Social con Battleship

## Descripción
LinkUp es una red social desarrollada en ASP.NET Core MVC (.NET 8) con arquitectura Onion, que incluye publicaciones, amistades, solicitudes de amistad y el juego Battleship multijugador.

## Arquitectura
```
LinkUp.Domain         → Entidades, Enums (núcleo)
LinkUp.Application    → Interfaces, DTOs, ViewModels, Servicios
LinkUp.Infrastructure → EF Core, Repositorios, Email (SMTP)
LinkUp.Shared         → EmailSenderOptions
LinkUp.Web            → Controllers, Views, UI
```

## Ejecución

```bash
cd LinkUp.Web
dotnet ef migrations add InitialCreate --project ../LinkUp.Infrastructure
dotnet ef database update --project ../LinkUp.Infrastructure
dotnet run
```

O desde la raíz:
```bash
dotnet build
dotnet run --project LinkUp.Web
```

La aplicación estará disponible en `https://localhost:5001`

## Usuarios de Prueba (Seed Data)
| Usuario   | Contraseña      | Email                |
|-----------|----------------|----------------------|
| pruebas     | c-1234   | pruebas@linkup.com     |
| alb3rtsontl  | c-1234   | alb3rtsontl@gmail.com     |