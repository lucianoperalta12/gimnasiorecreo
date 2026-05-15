Instrucciones de despliegue

Contenido de esta carpeta:
- backend\  -> archivos publicados de la API (.NET)
- frontend\ -> build estático del frontend (contenido de dist)
- Start-All.bat -> script para arrancar backend y servir frontend en localhost

Requisitos en la máquina del usuario:
- .NET 8 Runtime instalado (para ejecutar GymAdmin.Api.dll)
- Node.js + npx instalado (para el servidor estático `serve` que usa Start-All.bat)

Uso:
1. Copiar toda la carpeta `Deploy` al equipo del usuario o pendrive.
2. Ejecutar `Start-All.bat` (doble-click). El backend quedará en http://localhost:5000 y el frontend en http://localhost:5173.

Credenciales de prueba (seed): admin/admin, profesor/profesor, alumno/alumno
