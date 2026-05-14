# Gym Management System

Este repositorio contiene una aplicación completa de gestión de gimnasios con un backend en .NET 8 y un frontend en Vue.js.

## Estructura del Proyecto

- `/GymAdmin`: Backend API desarrollado en .NET 8.
- `/gym-frontend`: Frontend SPA desarrollado en Vue.js + Vite.

## Despliegue en Railway

El proyecto está configurado para desplegarse automáticamente en [Railway](https://railway.app/) utilizando Docker.

### Configuración de Railway

1. Conecta tu repositorio de GitHub a un nuevo proyecto en Railway.
2. Railway detectará automáticamente el archivo `railway.json` y el `Dockerfile`.
3. Configura las siguientes variables de entorno en el dashboard de Railway:

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a la base de datos (SQLite o PostgreSQL) | `Data Source=gymadmin.db` |
| `Jwt__Key` | Clave secreta para JWT | `tu_clave_secreta_super_larga_y_segura` |
| `Jwt__Issuer` | Emisor del token | `GymAdmin` |
| `Jwt__Audience` | Audiencia del token | `GymAdminUsers` |

### Notas de Despliegue

- El backend sirve los archivos estáticos del frontend desde la carpeta `wwwroot`.
- Se incluye un Health Check en `/api/health` para monitorear el estado de la aplicación.
- La base de datos por defecto es SQLite, pero se recomienda usar PostgreSQL en producción configurando la variable `ConnectionStrings__DefaultConnection`.

## Desarrollo Local

### Backend
```bash
cd GymAdmin/src/GymAdmin.Api
dotnet run
```

### Frontend
```bash
cd gym-frontend
npm install
npm run dev
```
