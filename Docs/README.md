# WorkPoint API

## Description

WorkPoint is a platform that allows individuals and companies to **publish spaces available for rent**, and also allows other individuals or companies to **check availability and create bookings** for work purposes.

The system includes two main roles:
- **Admin**: manages and publishes spaces and manages internal resources (for example, photos).
- **User**: browses the catalog, checks availability, and creates/cancels bookings.

---

## System Links (Deployment)

- **Frontend (Production):** https://proyecto-integrador-eight-pi.vercel.app/Home
- **API (Swagger / Documentation):** https://workpoint-fd50fee8e731.herokuapp.com/index.html
- **Deployment Platform (API):** Heroku

---

## Architecture

This project is built using a **Layered Architecture** following **Clean Architecture** principles with a **DDD (Domain-Driven Design)** philosophy. The solution separates responsibilities by projects, enforcing clear dependency rules: inner layers do not depend on outer layers.

### Solution Structure

- **workpoint.Api**
  - Presentation layer (ASP.NET Core Web API).
  - Controllers, Middleware, configuration (Program.cs, appsettings) and Dockerfile.
  - Exposes HTTP endpoints and applies authentication/authorization.

- **workpoint.Application**
  - Application layer (use cases).
  - DTOs, Interfaces, Services, and Messages.
  - Coordinates business logic per use case and defines contracts toward Infrastructure.

- **workpoint.Domain**
  - Domain layer (core).
  - Entities and Interfaces.
  - Business model and domain rules. Does not depend on external frameworks.

- **workpoint.Infrastructure**
  - Infrastructure layer.
  - Data, Repositories, Services, Messaging, and Extensions.
  - Persistence with EF Core + MySQL and external integrations (for example, Cloudinary).

- **workpoint.Test**
  - Test project.

### Dependency Rule

- `Domain` does not depend on any other layer.
- `Application` depends on `Domain`.
- `Infrastructure` depends on `Application` and `Domain`.
- `Api` depends on `Application` and `Infrastructure` (and required references to `Domain`).

---

## Language

- **C#**

---

## Frameworks

- **.NET 9 (net9.0)**
- **ASP.NET Core Web API**
- **Entity Framework Core 9**
- **JWT Bearer Authentication**

---

## Libraries (NuGet)

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
- No additional NuGet packages.

### workpoint.Infrastructure (net9.0)
- CloudinaryDotNet (1.27.9)
- Microsoft.EntityFrameworkCore (9.0.0)
- Microsoft.EntityFrameworkCore.Design (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (9.0.0)
- Pomelo.EntityFrameworkCore.MySql (9.0.0)

---

## Security: Authentication and Authorization

The API uses **JWT Bearer**.

Required header for protected endpoints:
`Authorization: Bearer {token}`

### High-Level Permission Matrix

- **Auth**: public (login/register/refresh/revoke depending on system policies).
- **Space**: restricted to **Admin**.
- **Booking**: **User** can manage the complete flow (create/read/update/cancel).
- **Photos**: restricted to **Admin**.
- **WebhookSubscription**: not applicable / omitted from this documentation.

---

## Business Rules (Bookings and Availability)

- Booking the **same space** for the **same time range** is not allowed (no overlaps).
- The `Available` field represents booking state/availability.
- The cancellation endpoint does **not delete** the booking; it **marks it as canceled**.

---

## Data Model (Summary)

The data model is organized around renting spaces and managing bookings:

- **Users**: user information and authentication.
- **Roles**: permission control (Admin/User).
- **Spaces**: rentable spaces (related to categories, branches/locations, and a user/owner).
- **Bookings**: reservations per user/space with `StartHour` and `EndHour`, plus status and auditing.
- **Photos**: photos associated with spaces (URL).
- Location/catalog entities: **DocumentsTypes**, **Departments**, **Cities**, **Branches**, **Categories**.
- The model includes **Payments** and **PaymentMethods** at database level.

---

## Endpoints

<img width="1813" height="809" alt="image" src="https://github.com/user-attachments/assets/68e5a1da-aeab-4888-a246-254dc39a2f84" />
<img width="1816" height="763" alt="image" src="https://github.com/user-attachments/assets/4de675c5-9e90-4ae2-8376-f970b616b96f" />
<img width="1824" height="413" alt="image" src="https://github.com/user-attachments/assets/baa07491-0dfc-42f2-8336-968a91bb0742" />

---

## Project Status

- Database: **MySQL**.
- The system is fully deployed in production; it does not require local configuration for end-user access.

---

## UML and Entity-Relationship Diagrams

<img width="1389" height="3471" alt="workPoint drawio" src="https://github.com/user-attachments/assets/dafb7cab-de65-4b55-807b-c1bb97ee6a93" />

---

## Use Case Diagram

<img width="476" height="823" alt="image" src="https://github.com/user-attachments/assets/4680eb1d-3291-458c-b08c-5787b095a27b" />

# WorkPoint Platform (Frontend + API)

## 1. Overview

WorkPoint is a full-stack platform that allows individuals and companies to **publish spaces available for rent** and enables other users to **browse availability and create bookings** to work in those spaces.

The solution is composed of:
- **Frontend (Web App)** built with Next.js.
- **Backend (REST API)** built with ASP.NET Core.
- Both layers are exposed and consumed through an **API Gateway**, which centralizes access and standardizes the communication between the client application and the services.

---

## 2. Production Links

- **Frontend (Production):** https://proyecto-integrador-eight-pi.vercel.app/Home  
- **Backend API (Swagger / Docs):** https://workpoint-fd50fee8e731.herokuapp.com/index.html  
- **Backend Deployment Platform:** Heroku  

---

## 3. Platform Architecture

### 3.1 High-Level Architecture (Frontend + Backend)

The platform follows a full-stack architecture where:

- The **Frontend** provides the user interface, navigation, session handling, validation, and user feedback.
- The **Backend** provides the business logic, domain model, persistence, security, and the REST API surface.
- An **API Gateway** sits in front of the backend services and provides a single entry point for the frontend.

### 3.2 API Gateway Role (How Both Sides Are Consumed)

Both the frontend application and any external client consume the backend **through an API Gateway**. In practical terms, the API Gateway is responsible for:

- **Single entry point** for client requests (the frontend does not call internal services directly).
- **Request routing** to the appropriate backend endpoints.
- **Security enforcement** (centralized authentication/authorization policies when applicable).
- **Cross-cutting concerns** such as rate limiting, monitoring, and request logging (depending on the gateway configuration).
- **Abstraction** of internal backend changes, allowing the frontend to keep a stable contract.

> Note: The exact gateway provider/technology is not included in this documentation, but the architectural responsibility remains the same.

---

## 4. User Roles

- **Admin**
  - Creates and manages spaces.
  - Manages internal resources such as photos.
- **User**
  - Browses the catalog.
  - Checks availability.
  - Creates, updates, and cancels bookings.

---

## 5. Frontend Documentation (Web App)

### 5.1 Language
- **TypeScript**

### 5.2 Frameworks
- **Next.js (App Router + Turbopack)**
- **React**

### 5.3 Libraries and Tooling
- **Tailwind CSS** (styling)
- **NextAuth.js** (authentication and sessions)
- **Prisma ORM** (typed models and data access)
- **Cloudinary** (image management and optimization)
- **MercadoPago SDK** (payments integration)
- **Yup** (form validation)
- **React Toastify** (notifications)
- **PostCSS + Autoprefixer** (CSS processing)
- **ESLint** (code quality)

### 5.4 Backend Integration
- The frontend performs requests to the WorkPoint backend for business operations **through the API Gateway**.
- Payments are handled via **MercadoPago** according to the provided scope.

### 5.5 Main Module
- **Space Creation (Admin)**:
  - Space creation forms with validation.
  - Optional image handling (Cloudinary).
  - Submission to the backend through the API Gateway.
  - User feedback via notifications.

### 5.6 Next.js Template Reference (Kept as in the original repository)
This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

#### Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
