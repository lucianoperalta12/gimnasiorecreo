-- =============================================================================
-- Multi-gimnasio | Paso 2: DML - Backfill desde Users (GymId / Rol legacy)
-- Requiere: paso 01 ejecutado y columnas legacy aún presentes en Users
-- =============================================================================
BEGIN;

DO $$
DECLARE
    missing_gym_count integer;
    orphan_gym_count integer;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = 'GymUsers'
    ) THEN
        RAISE EXCEPTION 'La tabla GymUsers no existe. Ejecute 01_ddl_gym_users.sql primero.';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'Users'
          AND column_name = 'GymId'
    ) THEN
        RAISE EXCEPTION 'La columna Users.GymId ya fue eliminada. El backfill no aplica.';
    END IF;

    SELECT COUNT(*) INTO missing_gym_count
    FROM "Users" u
    WHERE u."GymId" IS NOT NULL
      AND u."GymId" > 0
      AND NOT EXISTS (SELECT 1 FROM "Gyms" g WHERE g."Id" = u."GymId");

    IF missing_gym_count > 0 THEN
        RAISE EXCEPTION 'Hay % usuarios con GymId inexistente en Gyms. Corrija datos antes de migrar.', missing_gym_count;
    END IF;

    SELECT COUNT(*) INTO orphan_gym_count
    FROM "Users" u
    WHERE (u."GymId" IS NULL OR u."GymId" <= 0)
      AND u."Rol"::text <> 'Superusuario';

    IF orphan_gym_count > 0 THEN
        RAISE WARNING 'Hay % usuarios sin gimnasio asignado (no Superusuario). Revisar manualmente.', orphan_gym_count;
    END IF;
END $$;

INSERT INTO "GymUsers" ("GymId", "UserId", "Rol", "Activo", "FechaAsociacion")
SELECT
    u."GymId",
    u."Id",
    u."Rol",
    u."Activo",
    COALESCE(u."FechaCreacion", CURRENT_TIMESTAMP)
FROM "Users" u
WHERE u."GymId" IS NOT NULL
  AND u."GymId" > 0
  AND NOT EXISTS (
      SELECT 1
      FROM "GymUsers" gu
      WHERE gu."GymId" = u."GymId"
        AND gu."UserId" = u."Id"
  );

DO $$
DECLARE
    users_with_gym integer;
    gym_users_rows integer;
BEGIN
    SELECT COUNT(*) INTO users_with_gym
    FROM "Users" u
    WHERE u."GymId" IS NOT NULL AND u."GymId" > 0;

    SELECT COUNT(*) INTO gym_users_rows
    FROM "GymUsers";

    RAISE NOTICE 'Usuarios con GymId legacy: % | Filas en GymUsers: %', users_with_gym, gym_users_rows;

    IF users_with_gym > 0 AND gym_users_rows = 0 THEN
        RAISE EXCEPTION 'Backfill falló: no se insertaron asociaciones.';
    END IF;
END $$;

COMMIT;

-- Rollback manual del backfill (no elimina la tabla):
-- BEGIN;
-- DELETE FROM "GymUsers" gu
-- USING "Users" u
-- WHERE gu."UserId" = u."Id"
--   AND gu."GymId" = u."GymId"
--   AND gu."Rol" = u."Rol";
-- COMMIT;
