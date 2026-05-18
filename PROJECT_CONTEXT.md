# 🏋️ Project Context: Gym Center Manager System

Este documento centraliza la información técnica y funcional del proyecto **Gym Center Manager**. Debe ser utilizado como contexto base por cualquier agente o desarrollador que trabaje en el repositorio para asegurar consistencia, seguir las reglas de negocio y mantener la calidad arquitectónica.

---

## 🎯 Objetivos del Proyecto
1.  **Administración Multi-Tenant**: Gestionar múltiples gimnasios de forma aislada en una única plataforma.
2.  **Seguridad y Aislamiento**: Implementar un flujo de autenticación profesional con rotación de tokens y estricta separación de datos por `GymId`.
3.  **Experiencia Premium Personalizable**: Ofrecer una interfaz moderna (Glassmorphism) con branding dinámico (colores y logos) según el gimnasio.
4.  **Escalabilidad**: Mantener una arquitectura Clean Architecture que permita el crecimiento de la red de gimnasios.

---

## 💻 Stack Tecnológico
### Frontend
- **Framework**: Vue 3 (Composition API)
- **Estado**: Pinia
- **Routing**: Vue Router
- **Estilos**: TailwindCSS (Branding dinámico via CSS Variables: `--gym-primary`)
- **UI**: Glassmorphism, animaciones suaves y modo oscuro persistente.
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
| **Alumno** | Ver sus rutinas asignadas, editar su perfil, consultar su estado de acceso (`GET /api/memberships/me/access`). |
| **Profesor** | Todo lo del Alumno + Crear/Editar Ejercicios y Rutinas, Asignar rutinas a alumnos (dentro de su gimnasio). Consulta de membresías y estado de acceso de alumnos (solo lectura). |
| **Administrativo** | Gestión de Usuarios del gimnasio (Crear Alumnos/Profesores), Planes de membresía, Membresías, Pagos, ver estadísticas básicas. Hereda permisos de consulta de Profesor. |
| **Superusuario** | Control total: Gestión de Gimnasios (CRUD, colores, logos), Gestión global de Usuarios, Roles, Planes, Membresías y Pagos de todo el sistema. |

---

## 📋 Reglas de Negocio Críticas
- **Aislamiento**: Ningún usuario (excepto el Superusuario) puede ver o modificar datos de un gimnasio diferente al suyo.
- **Asignaciones**: Un alumno no puede tener la misma rutina asignada más de una vez simultáneamente.
- **Autoría**: Solo el profesor que creó una rutina puede editarla o eliminarla.
- **Gimnasios**: El Superusuario es el único capaz de crear gimnasios y definir su branding (Logo y Color Hexadecimal).
- **Auth**: Los Refresh Tokens tienen rotación; cada vez que se usa uno para obtener un nuevo Access Token, el Refresh Token viejo se invalida.
- **Primer Login**: El DNI es la contraseña inicial, pero el sistema obliga a cambiarla en el primer acceso.
- **Membresías**: Un alumno solo puede tener **una membresía activa** a la vez. Las renovaciones **no sobrescriben** el historial: la membresía anterior pasa a `Vencida` y se crea un registro nuevo.
- **Estado de acceso**: Se deriva de la membresía vigente (`Activo`, `Vencido`, `Moroso`, `Suspendido`, `Sin membresía`). **No** se persiste en la entidad `User`. `Moroso` = membresía activa con pagos en estado `Pendiente`.
- **Vencimiento automático**: Al consultar o modificar membresías, las activas con `FechaVencimiento` pasada se marcan como `Vencida`.
- **Planes**: Solo `Administrativo` y `Superusuario` gestionan planes. Un plan con membresías asociadas no se elimina; se desactiva (`Activo = false`).

### API de Membresías (Fase 2)
| Recurso | Ruta base | Roles |
| :--- | :--- | :--- |
| Planes | `GET/POST/PUT/DELETE /api/membershipplans` | Administrativo, Superusuario |
| Membresías | `GET/POST /api/memberships`, `POST .../renew`, `POST .../cancel` | Lectura: Profesor+. Escritura: Administrativo+ |
| Acceso alumno | `GET /api/memberships/me/access` | Alumno |
| Pagos | `GET/POST/PUT/DELETE /api/payments` | Administrativo, Superusuario |

---

## 📂 Estructura de Archivos

### Backend
- `src/GymAdmin.Api/Controllers/`: Endpoints organizados por recurso.
- `src/GymAdmin.Application/Services/`: Implementación de la lógica (ej: `RoutineService`).
- `src/GymAdmin.Infrastructure/Data/Configurations/`: Configuración Fluent API para cada entidad.
- `src/GymAdmin.Infrastructure/Seed/`: `DbSeeder` para datos iniciales.

### Frontend
- `src/api/`: Servicios de Axios espejo de los controladores del backend.
  - Membresías: `membership-plans.api.js`, `memberships.api.js`, `payments.api.js`
- `src/stores/`: Stores de Pinia (auth, routine, user, **membership**).
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
- Integración con **Cloudinary** para subir fotos de ejercicios reales.
- Generación de **PDFs** automáticos con códigos QR para las rutinas.
- Dashboard con **gráficos de evolución** física para alumnos.
- Módulo de **Asistencia** (QR Check-in).

---

## 🏆 Criterios de Calidad
1.  **DRY (Don't Repeat Yourself)**: Evitar código duplicado.
2.  **KISS (Keep It Simple, Stupid)**: No sobre-diseñar; priorizar la claridad.
3.  **Responsive**: La UI debe funcionar perfectamente en móviles.
4.  **Performante**: Minimizar peticiones redundantes y optimizar el tamaño de los bundles.
5.  **Seguro**: Cumplir con los principios de OWASP (protección contra XSS, CSRF, Inyección).

---

**Última actualización**: 2026-05-15
**Estado del Proyecto**: Arquitectura Multi-tenant completa, Branding dinámico operativo, **módulo de membresías (backend Fase 1–2)**.
