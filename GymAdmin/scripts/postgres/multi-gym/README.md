# Migración multi-gimnasio (PostgreSQL)

Scripts para producción. Ejecutar **en orden** y en una ventana de mantenimiento.

| Orden | Archivo | Descripción |
|-------|---------|-------------|
| 1 | `01_ddl_gym_users.sql` | Crea tabla `GymUsers`, índices y FKs |
| 2 | `02_dml_backfill_gym_users.sql` | Copia `Users.GymId` / `Users.Rol` a `GymUsers` |
| 3 | `03_ddl_remove_legacy_user_columns.sql` | Elimina FK, índice y columnas legacy en `Users` |

Cada script usa `BEGIN` / `COMMIT` y validaciones previas. Los comentarios al final de cada archivo indican cómo revertir manualmente.

**Desarrollo local:** también puede aplicarse con EF Core:

```bash
dotnet ef database update --project src/GymAdmin.Infrastructure --startup-project src/GymAdmin.Api
```

Migración EF equivalente (crea `GymUsers`, backfill y elimina columnas legacy): `20260528150853_AddGymUsers`.
