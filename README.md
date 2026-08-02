# MesaSitec

Prueba técnica para Desarrollador Junior - Sitecpro.

MesaSitec es una aplicación de mesa de servicio SaaS multi-organización donde diferentes organizaciones pueden gestionar solicitudes de soporte dentro de una misma instancia del sistema.

El sistema implementa aislamiento por organización (tenant), autenticación mediante JWT, gestión de solicitudes, categorías, estados y cálculo automático de SLA.


# Tecnologías utilizadas

## Backend

- .NET 8 Web API
- Entity Framework Core
- SQLite
- JWT Authentication
- Swagger
- xUnit


## Frontend

- Vue 3
- TypeScript
- Vite
- Pinia
- Vue Router


# Requisitos previos

Para ejecutar el proyecto se necesita:

- .NET 8 SDK
- Node.js
- npm


# Estructura del proyecto
MesaSitec
│
├── backend
│ │
│ ├── Controllers
│ ├── Data
│ ├── DTOs
│ ├── Enums
│ ├── Exceptions
│ ├── Helpers
│ ├── Interfaces
│ ├── Middleware
│ ├── Migrations
│ ├── Models
│ ├── Services
│ ├── AppDbContext.cs
│ ├── Program.cs
│ └── backend.csproj
│
├── backend.Tests
│ ├── PasswordServiceTests.cs
│ ├── SlaServiceTests.cs
│ └── SolicitudServiceTests.cs
│
├── frontend
│ ├── public
│ ├── src
│ ├── package.json
│ ├── tsconfig.json
│ └── vite.config.ts
│
├── MesaSitec.sln
├── .gitignore
└── README.md



# Ejecución del proyecto


## Backend

Restaurar dependencias:

dotnet restore

Ejecutar la API:

dotnet run

La API queda disponible en:

http://localhost:5022

Swagger:

http://localhost:5022/swagger

Al iniciar la aplicación se realiza automáticamente:

Creación de la base de datos SQLite.
Aplicación de migraciones.
Carga de datos iniciales mediante seed.

#  Frontend

Ingresar a la carpeta frontend:

cd frontend

Instalar dependencias:

npm install

Ejecutar:

npm run dev

Disponible en:

http://localhost:5173

#  Base de datos

El proyecto utiliza SQLite.

La base de datos se genera automáticamente al iniciar el backend.

No requiere instalación de un motor de base de datos externo.

# Usuarios de prueba

Contraseña utilizada para los usuarios semilla:

Sitec.2026

Usuarios disponibles:

admin@norte.test
agente1@norte.test
user1@norte.test

También existen usuarios pertenecientes a la organización Bufete Sur.

# Funcionalidades implementadas

# Backend

Autenticación mediante JWT.
Gestión de usuarios.
Roles de usuario.
Aislamiento de datos por organización (tenant).
Gestión de categorías.
Gestión de solicitudes.
Máquina de estados de solicitudes.
Cálculo automático de SLA.
Validaciones de reglas de negocio.
Manejo global de excepciones.
Migraciones automáticas.
Datos semilla.
Swagger configurado.

#  Endpoints implementados

POST   /auth/login

GET    /me

GET    /categorias

GET    /solicitudes

POST   /solicitudes

GET    /solicitudes/{id}

PUT    /solicitudes/{id}

POST   /solicitudes/{id}/transiciones

GET    /health

# Pruebas

Se implementaron pruebas unitarias utilizando xUnit.

Actualmente cuenta con 6 pruebas unitarias.

Ejecutar:

dotnet test

# Frontend

Implementado:

Proyecto Vue 3 con TypeScript.
Configuración inicial con Vite.
Estructura base del frontend.

Pendiente:

Conexión completa entre frontend y backend.
Implementación completa de las vistas requeridas.
Integración de autenticación desde frontend.

# Funcionalidades pendientes

Debido al tiempo disponible quedaron pendientes:

Conectar frontend con la API backend.
Completar las pantallas solicitadas en la consigna.
Completar las 8 pruebas unitarias requeridas.

# Uso de inteligencia artificial

Durante el desarrollo se utilizó inteligencia artificial (ChatGPT) como herramienta de apoyo para:

Interpretar la consigna técnica.
Resolver dudas sobre arquitectura.
Guiar la implementación de algunas funcionalidades.
Analizar errores encontrados durante el desarrollo.
Revisar posibles mejoras del código.

La IA fue utilizada como apoyo y guía durante el desarrollo. El código fue revisado y adaptado durante la implementación del proyecto.

# Notas finales

El backend puede ejecutarse de manera independiente y cuenta con la lógica principal del sistema implementada.

El frontend posee la estructura inicial del proyecto, pero la integración completa con el backend queda pendiente.


