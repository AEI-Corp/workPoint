# WorkPoint API

## Descripción

WorkPoint es una plataforma que permite a personas y empresas **publicar espacios disponibles para alquiler** y a su vez permite que otras personas o empresas **consulten disponibilidad y realicen reservas** para trabajar.

El sistema contempla dos roles principales:
- **Admin**: administra y publica espacios y gestiona recursos internos (por ejemplo, fotos).
- **User**: consume el catálogo, consulta disponibilidad y crea/cancela reservas.

---

## Enlaces del sistema (despliegue)

- **Frontend (producción):** https://proyecto-integrador-eight-pi.vercel.app/Home
- **API (Swagger / documentación):** https://workpoint-fd50fee8e731.herokuapp.com/index.html
- **Plataforma de despliegue (API):** Heroku

---

## Arquitectura

Este proyecto está construido con **Arquitectura en Capas** siguiendo principios de **Clean Architecture** con una filosofía **DDD (Domain-Driven Design)**. La solución separa responsabilidades por proyectos, manteniendo reglas de dependencia claras: las capas internas no dependen de las externas.

### Estructura de la solución

- **workpoint.Api**
  - Capa de presentación (ASP.NET Core Web API).
  - Controllers, Middleware, configuración (Program.cs, appsettings) y Dockerfile.
  - Expone endpoints HTTP y aplica autenticación/autorización.

- **workpoint.Application**
  - Capa de aplicación (casos de uso).
  - DTOs, Interfaces, Services y Messages.
  - Coordina la lógica por casos de uso y define contratos hacia Infrastructure.

- **workpoint.Domain**
  - Capa de dominio (núcleo).
  - Entities e Interfaces.
  - Modelo del negocio y reglas del dominio. No depende de frameworks externos.

- **workpoint.Infrastructure**
  - Capa de infraestructura.
  - Data, Repositories, Services, Messaging y Extensions.
  - Persistencia con EF Core + MySQL e integraciones externas (por ejemplo, Cloudinary).

- **workpoint.Test**
  - Proyecto de pruebas.

### Regla de dependencias

- `Domain` no depende de ninguna otra capa.
- `Application` depende de `Domain`.
- `Infrastructure` depende de `Application` y `Domain`.
- `Api` depende de `Application` e `Infrastructure` (y referencias necesarias a `Domain`).

---

## Lenguaje

- **C#**

---

## Frameworks

- **.NET 9 (net9.0)**
- **ASP.NET Core Web API**
- **Entity Framework Core 9**
- **JWT Bearer Authentication**

---

## Librerías (NuGet)

### workpoint.Api (net9.0)
- AutoMapper (12.0.1)
- AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)
- Microsoft.AspNetCore.OpenApi (9.0.11)
- Microsoft.EntityFrameworkCore.Design (9.0.0)
- Swashbuckle.AspNetCore (9.0.0)
- Swashbuckle.AspNetCore.SwaggerGen (9.0.0)
- Swashbuckle.AspNetCore.SwaggerUI (9.0.0)

### workpoint.Application (net9.0)
- AutoMapper (12.0.1)
- AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)
- BCrypt (1.0.0)
- BCrypt.Net-Next (4.0.3)
- Microsoft.AspNetCore.Http (2.3.0)
- Microsoft.Extensions.Configuration (10.0.0)
- Microsoft.Extensions.Configuration.Abstractions (10.0.0)
- System.IdentityModel.Tokens.Jwt (8.15.0)

### workpoint.Domain (net9.0)
- Sin paquetes NuGet adicionales.

### workpoint.Infrastructure (net9.0)
- CloudinaryDotNet (1.27.9)
- Microsoft.EntityFrameworkCore (9.0.0)
- Microsoft.EntityFrameworkCore.Design (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (9.0.0)
- Pomelo.EntityFrameworkCore.MySql (9.0.0)

---

## Seguridad: autenticación y autorización

La API utiliza **JWT Bearer**.

Header requerido para endpoints protegidos:
`Authorization: Bearer {token}`

### Matriz de permisos (alto nivel)

- **Auth**: público (login/register/refresh/revoke según políticas del sistema).
- **Space**: restringido a **Admin**.
- **Booking**: **User** puede gestionar el flujo completo (crear/consultar/actualizar/cancelar).
- **Photos**: restringido a **Admin**.
- **WebhookSubscription**: no aplica / omitido en esta documentación.

---

## Reglas del negocio (reservas y disponibilidad)

- No se permite reservar el **mismo espacio** en el **mismo rango de tiempo** (no se permiten solapamientos).
- El campo `Available` representa estado/disponibilidad de la reserva.
- El endpoint de cancelación **no elimina** la reserva: la **marca como cancelada**.

---

## Modelo de datos (resumen)

El modelo está organizado alrededor de alquiler de espacios y gestión de reservas:

- **Users**: información del usuario y autenticación.
- **Roles**: control de permisos (Admin/User).
- **Spaces**: espacios publicados para alquiler (relaciona con categoría, sede/sucursal y usuario).
- **Bookings**: reservas por usuario/espacio con `StartHour` y `EndHour`, además de estado y auditoría.
- **Photos**: fotografías asociadas a espacios (URL).
- Catálogos/ubicación: **DocumentsTypes**, **Departments**, **Cities**, **Branches**, **Categories**.
- El modelo contempla **Payments** y **PaymentMethods** a nivel de base de datos.

---

## Endpoints

<img width="1813" height="809" alt="image" src="https://github.com/user-attachments/assets/68e5a1da-aeab-4888-a246-254dc39a2f84" />
<img width="1816" height="763" alt="image" src="https://github.com/user-attachments/assets/4de675c5-9e90-4ae2-8376-f970b616b96f" />


## Estado del proyecto

- Base de datos: **MySQL**.
- El sistema se encuentra desplegado en producción; no requiere configuración local para su uso desde el cliente final.

 ## Diagramas UML y entidad relacion
<img width="1389" height="3471" alt="workPoint drawio" src="https://github.com/user-attachments/assets/dafb7cab-de65-4b55-807b-c1bb97ee6a93" />

## Diagrama caso de uso 
<img width="476" height="823" alt="image" src="https://github.com/user-attachments/assets/4680eb1d-3291-458c-b08c-5787b095a27b" />
