# Project Context: Gym Center Manager System

Este documento centraliza la informacion tecnica y funcional del proyecto **Gym Center Manager**. Debe utilizarse como contexto base por cualquier agente o desarrollador que trabaje en el repositorio para asegurar consistencia, respetar las reglas de negocio y mantener la calidad arquitectonica.

---

## Objetivos del Proyecto
1. **Administracion Multi-Tenant**: Gestionar multiples gimnasios de forma aislada en una unica plataforma.
2. **Seguridad y Aislamiento**: Implementar un flujo de autenticacion profesional con rotacion de tokens y estricta separacion de datos por `GymId`.
3. **Experiencia Premium Personalizable**: Ofrecer una interfaz moderna con branding dinamico por gimnasio.
4. **Escalabilidad**: Mantener una arquitectura Clean Architecture que permita evolucionar modulos nuevos sin acoplamiento innecesario.
5. **Control de Acceso Fisico**: Registrar ingresos de alumnos desde usuarios tipo terminal, respetando membresias y limites de uso.

---

## Stack Tecnologico
### Frontend
- **Framework**: Vue 3 (Composition API)
- **Estado**: Pinia
- **Routing**: Vue Router
- **Estilos**: TailwindCSS con branding dinamico via CSS variables
- **HTTP**: Axios con interceptores para Refresh Token

### Backend
- **Framework**: ASP.NET Core 8 Web API
- **ORM**: Entity Framework Core
- **Base de Datos**: PostgreSQL (produccion) / SQLite (desarrollo)
- **Seguridad**: JWT + Refresh Tokens persistidos en DB
- **Hashing**: BCrypt.Net-Next

---

## Arquitectura
El proyecto se divide en 4 proyectos dentro de la solucion:

1. **GymAdmin.Domain**: Entidades base, enums e interfaces de dominio. Sin dependencias externas.
2. **GymAdmin.Application**: Logica de negocio, DTOs, mapeos e interfaces de servicios.
3. **GymAdmin.Infrastructure**: Persistencia, `DbContext`, configuraciones de EF Core, migraciones y seed.
4. **GymAdmin.Api**: Controladores, configuracion de DI, middleware de excepciones y autenticacion/autorizacion.

---

## Roles y Permisos

| Rol | Permisos |
| :--- | :--- |
| **Alumno** | Ver sus rutinas asignadas, editar su perfil, consultar su estado de acceso (`GET /api/memberships/me/access`). |
| **Profesor** | Todo lo del Alumno + Crear/Editar ejercicios y rutinas, asignar rutinas a alumnos dentro de su gimnasio, consultar membresias y estado de acceso de alumnos en modo lectura. |
| **Administrativo** | Gestion de usuarios del gimnasio, planes de membresia, membresias, pagos e ingresos. Puede consultar y auditar movimientos operativos del gimnasio. |
| **Superusuario** | Control total del sistema: gimnasios, usuarios, roles, planes, membresias, pagos e ingresos globales. |
| **Terminal** | Registrar ingresos de alumnos en recepcion o molinete usando DNI. No administra usuarios, rutinas ni membresias. Solo puede operar sobre alumnos de su propio gimnasio. |

### Alcance operativo del rol Terminal
- Accede al endpoint `POST /api/ingresos/registrar`.
- Debe estar autenticado como usuario con rol `Terminal`.
- El ingreso registrado queda asociado al usuario terminal que lo ejecuto.
- No tiene acceso al listado global de ingresos.

---

## Reglas de Negocio Criticas
- **Aislamiento**: Ningun usuario, excepto el `Superusuario`, puede ver o modificar datos de un gimnasio diferente al suyo.
- **Asignaciones**: Un alumno no puede tener la misma rutina asignada mas de una vez simultaneamente.
- **Autoria**: Solo el profesor que creo una rutina puede editarla o eliminarla.
- **Gimnasios**: El `Superusuario` es el unico capaz de crear gimnasios y definir su branding.
- **Auth**: Los Refresh Tokens tienen rotacion; cuando se usa uno para renovar sesion, el token anterior queda invalidado.
- **Primer Login**: El DNI es la contrasena inicial, pero el sistema obliga a cambiarla en el primer acceso.
- **Membresias**: Un alumno solo puede tener **una membresia activa** a la vez. Las renovaciones no sobrescriben historial: la anterior pasa a `Vencida` y se crea un nuevo registro.
- **Estado de acceso**: Se deriva de la membresia vigente (`Activo`, `Vencido`, `Moroso`, `Suspendido`, `Sin membresia`). No se persiste en `User`.
- **Vencimiento automatico**: Al consultar o modificar membresias, las activas con `FechaVencimiento` vencida se marcan como `Vencida`.
- **Planes**: Un plan con membresias asociadas no se elimina; se desactiva.
- **Ingresos**: Cada ingreso debe quedar asociado a un alumno, una membresia valida, un gimnasio y un usuario `Terminal`.

### Validaciones de Ingresos
- Solo un usuario con rol `Terminal` puede registrar ingresos.
- El DNI ingresado es obligatorio.
- El alumno debe existir, estar activo y tener rol `Alumno`.
- La terminal solo puede registrar ingresos para alumnos de su mismo gimnasio.
- El alumno debe tener una membresia activa en ese gimnasio.
- Si la membresia esta vencida, el ingreso se rechaza.
- Si el plan no es `PaseLibre`, el sistema valida ingresos disponibles antes de registrar.
- Si el plan define `DiasPorSemana`, el sistema bloquea ingresos que superen el limite semanal.
- Cuando el ingreso es valido, se incrementa `IngresosUtilizados` en membresias que no sean `PaseLibre`.

### Validaciones de Gimnasio
- `Nombre` es obligatorio.
- `DuenoNombreApellido` es obligatorio.
- `ColorPrincipalHex` debe tener formato `#RRGGBB`.
- Si `Moneda` no se informa, el backend usa `ARS` por defecto.

---

## API Principal

### Membresias
| Recurso | Ruta base | Roles |
| :--- | :--- | :--- |
| Planes | `GET/POST/PUT/DELETE /api/membershipplans` | Administrativo, Superusuario |
| Membresias | `GET/POST /api/memberships`, `POST /api/memberships/{studentId}/renew`, `POST /api/memberships/{id}/cancel` | Lectura: Profesor+. Escritura: Administrativo+ |
| Acceso alumno | `GET /api/memberships/me/access` | Alumno |
| Acceso de alumno puntual | `GET /api/memberships/student/{studentId}/access` | Profesor, Administrativo, Superusuario |
| Pagos | `GET/POST/PUT/DELETE /api/payments` | Administrativo, Superusuario |

### Ingresos
| Recurso | Ruta base | Roles |
| :--- | :--- | :--- |
| Listado de ingresos | `GET /api/ingresos` | Superusuario, Administrativo |
| Registro de ingreso | `POST /api/ingresos/registrar` | Terminal |

---

## Estructura de Archivos

### Backend
- `GymAdmin/src/GymAdmin.Api/Controllers/`: Endpoints organizados por recurso.
- `GymAdmin/src/GymAdmin.Application/Services/`: Implementacion de logica de negocio.
- `GymAdmin/src/GymAdmin.Infrastructure/Data/Configurations/`: Configuracion Fluent API por entidad.
- `GymAdmin/src/GymAdmin.Infrastructure/Seed/`: Seed de datos iniciales.

### Frontend
- `GymAdmin/src/api/`: Servicios Axios espejo del backend.
- `GymAdmin/src/stores/`: Stores de Pinia.
- `GymAdmin/src/views/`: Vistas organizadas por modulo.
- `GymAdmin/src/components/ui/`: Componentes atomicos reutilizables.

---

## Convenciones y Buenas Practicas
- **Nomenclatura**: Backend en C# con PascalCase; frontend en JS con camelCase; componentes Vue en PascalCase.
- **Errores**: El backend centraliza errores en `GlobalExceptionMiddleware`. El frontend debe mostrarlos via notificaciones.
- **Dependencias**: Usar interfaces para servicios de aplicacion.
- **Roles**: No duplicar validacion de permisos en componentes si ya existe en router, store o API.
- **Estilos**: Reutilizar tokens y clases del sistema visual; evitar colores arbitrarios fuera del branding definido.

---

## No Hacer
- No duplicar logica de negocio de membresias o ingresos en el frontend.
- No guardar contrasenas en texto plano.
- No exponer entidades de dominio directamente desde controladores.
- No usar `any` si existe una alternativa tipada razonable.
- No hacer llamadas API directas desde componentes si ya existe store o modulo `src/api/`.
- No permitir que una terminal opere sobre alumnos de otro gimnasio.

---

## Futuras Mejoras
- Google OAuth como alternativa de login.
- Integracion con Cloudinary para fotos de ejercicios.
- PDFs con QR para rutinas.
- Dashboard con metricas fisicas para alumnos.
- Modulo de asistencia extendido sobre ingresos y check-in.

---

## Criterios de Calidad
1. **DRY**: Evitar codigo duplicado.
2. **KISS**: Priorizar claridad sobre sobre-diseno.
3. **Responsive**: La UI debe funcionar correctamente en moviles.
4. **Performante**: Minimizar peticiones redundantes y peso innecesario.
5. **Seguro**: Respetar principios OWASP.

---

**Ultima actualizacion**: 2026-05-19
**Estado del Proyecto**: Arquitectura multi-tenant operativa, modulo de membresias activo, y modulo de ingresos con rol `Terminal` y validaciones de acceso/membresia implementadas.
