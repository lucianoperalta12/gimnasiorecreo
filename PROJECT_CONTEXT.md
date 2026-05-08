# 🏋️ Project Context: Gym Manager System

Este documento centraliza la información técnica y funcional del proyecto **Gym Manager**. Debe ser utilizado como contexto base por cualquier agente o desarrollador que trabaje en el repositorio para asegurar consistencia, seguir las reglas de negocio y mantener la calidad arquitectónica.

---

## 🎯 Objetivos del Proyecto
1.  **Administración Centralizada**: Gestionar alumnos, profesores, ejercicios y rutinas en una única plataforma.
2.  **Seguridad Robusta**: Implementar un flujo de autenticación profesional con rotación de tokens.
3.  **Experiencia Premium**: Ofrecer una interfaz moderna, rápida y visualmente impactante (Red & Black Theme).
4.  **Escalabilidad**: Mantener una arquitectura desacoplada que permita futuras expansiones (ej: Multi-tenant).

---

## 💻 Stack Tecnológico
### Frontend
- **Framework**: Vue 3 (Composition API)
- **Estado**: Pinia
- **Routing**: Vue Router
- **Estilos**: TailwindCSS (Custom Palette: Red #950606 & Black #000000)
- **HTTP**: Axios (con interceptores para Refresh Token)

### Backend
- **Framework**: ASP.NET Core 8 Web API
- **ORM**: Entity Framework Core
- **Base de Datos**: PostgreSQL (Producción) / SQLite (Desarrollo)
- **Seguridad**: JWT + Refresh Tokens (Persistidos en DB)
- **Hashing**: BCrypt.Net-Next

---

## 🏛️ Arquitectura (Lightweight Clean Architecture)
El proyecto se divide en 4 proyectos (capas) dentro de la solución `.sln`:

1.  **GymAdmin.Domain**: Contiene las entidades base, enums e interfaces de dominio. Sin dependencias externas.
2.  **GymAdmin.Application**: Lógica de negocio, DTOs, Mapeos e interfaces de servicios.
3.  **GymAdmin.Infrastructure**: Implementación de persistencia (DbContext), configuraciones de EF Core, migraciones y seed de datos.
4.  **GymAdmin.Api**: Controladores, configuración de DI, Middleware de excepciones y configuración de Auth.

---

## 👥 Roles y Permisos

| Rol | Permisos |
| :--- | :--- |
| **Alumno** | Ver sus rutinas asignadas, editar su perfil. |
| **Profesor** | Todo lo del Alumno + Crear/Editar Ejercicios y Rutinas, Asignar rutinas a alumnos. |
| **Superusuario** | Todo lo del Profesor + Gestión de Usuarios (Cambio de roles, eliminación). |

---

## 📋 Reglas de Negocio Críticas
- **Asignaciones**: Un alumno no puede tener la misma rutina asignada más de una vez simultáneamente (validado en `AssignmentService`).
- **Autoría**: Solo el profesor que creó una rutina puede editarla o eliminarla (excepto el Superusuario).
- **Ejercicios**: Los ejercicios son globales; cualquier profesor puede usarlos para sus rutinas.
- **Auth**: Los Refresh Tokens tienen rotación; cada vez que se usa uno para obtener un nuevo Access Token, el Refresh Token viejo se invalida y se genera uno nuevo.

---

## 📂 Estructura de Archivos

### Backend
- `src/GymAdmin.Api/Controllers/`: Endpoints organizados por recurso.
- `src/GymAdmin.Application/Services/`: Implementación de la lógica (ej: `RoutineService`).
- `src/GymAdmin.Infrastructure/Data/Configurations/`: Configuración Fluent API para cada entidad.
- `src/GymAdmin.Infrastructure/Seed/`: `DbSeeder` para datos iniciales.

### Frontend
- `src/api/`: Servicios de Axios espejo de los controladores del backend.
- `src/stores/`: Stores de Pinia (auth, routine, user).
- `src/views/`: Vistas de la aplicación organizadas por módulos.
- `src/components/ui/`: Componentes atómicos (AppButton, AppInput, AppModal).

---

## 🛠️ Convenciones y Buenas Prácticas
- **Nomenclatura**: Backend en C# (PascalCase), Frontend en JS (camelCase), Componentes Vue (PascalCase).
- **Manejo de Errores**: Centralizado en el backend mediante `GlobalExceptionMiddleware`. El frontend debe capturar errores y mostrarlos vía `useNotification`.
- **Inyección de Dependencias**: Siempre usar interfaces para los servicios.
- **Estilos**: No usar colores arbitrarios; usar las clases `primary` y `dark` definidas en `tailwind.config.js`.

---

## ⚠️ NO HACER
- **No** duplicar lógica de validación de roles en los componentes si ya está en el router/api.
- **No** guardar contraseñas en texto plano (usar siempre BCrypt).
- **No** exponer entidades de dominio directamente en los controladores (usar DTOs).
- **No** usar `Any` en tipos de TypeScript/JavaScript si se puede evitar.
- **No** realizar peticiones API directamente desde los componentes; usar siempre los `stores` de Pinia o los módulos de `src/api/`.

---

## 🚀 Futuras Mejoras
- Implementar **Google OAuth** como alternativa de login.
- Agregar **Multi-tenancy** para soportar varios gimnasios.
- Integración con **Cloudinary** para subir fotos de ejercicios.
- Generación de **PDFs** para las rutinas.
- Gráficos de **progreso** para los alumnos.

---

## 🏆 Criterios de Calidad
1.  **DRY (Don't Repeat Yourself)**: Evitar código duplicado.
2.  **KISS (Keep It Simple, Stupid)**: No sobre-diseñar; priorizar la claridad.
3.  **Responsive**: La UI debe funcionar perfectamente en móviles.
4.  **Performante**: Minimizar peticiones redundantes y optimizar el tamaño de los bundles.
5.  **Seguro**: Cumplir con los principios de OWASP (protección contra XSS, CSRF, Inyección).

---

**Última actualización**: 2026-05-07
**Estado del Proyecto**: Base completa, Funcionalidades Core operativas.
