# Estado de despliegue (automático)

Fecha: 2026-05-08

- Backend: GymAdmin.Api ejecutándose en http://localhost:5000
- Frontend: gym-frontend (Vite) ejecutándose en http://localhost:5173/

Credenciales seed verificadas:
- admin / admin
- profesor / profesor
- alumno / alumno

Comandos usados para correr localmente:
```powershell
cd GymAdmin\src\GymAdmin.Api
$env:ASPNETCORE_URLS='http://localhost:5000'
dotnet run --no-launch-profile

cd gym-frontend
$env:PORT=5173
npm run dev
```

Nota: el seeder `src/GymAdmin.Infrastructure/Seed/DbSeeder.cs` establece las cuentas arriba.
