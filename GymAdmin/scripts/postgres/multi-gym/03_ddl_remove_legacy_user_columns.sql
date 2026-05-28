-- =============================================================================
-- Multi-gimnasio | Paso 3: DDL - Eliminar GymId y Rol de Users
-- Requiere: pasos 01 y 02 ejecutados correctamente
-- =============================================================================
BEGIN;

DO $$
DECLARE
    users_with_gym integer;
    missing_assoc integer;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = 'GymUsers'
    ) THEN
        RAISE EXCEPTION 'La tabla GymUsers no existe.';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'Users'
          AND column_name = 'GymId'
    ) THEN
        RAISE NOTICE 'Users.GymId ya fue eliminada. Paso 3 omitido.';
        RETURN;
    END IF;

    SELECT COUNT(*) INTO users_with_gym
    FROM "Users" u
    WHERE u."GymId" IS NOT NULL AND u."GymId" > 0;

    SELECT COUNT(*) INTO missing_assoc
    FROM "Users" u
    WHERE u."GymId" IS NOT NULL
      AND u."GymId" > 0
      AND NOT EXISTS (
          SELECT 1 FROM "GymUsers" gu
          WHERE gu."UserId" = u."Id" AND gu."GymId" = u."GymId"
      );

    IF users_with_gym > 0 AND missing_assoc > 0 THEN
        RAISE EXCEPTION 'Hay % usuarios con GymId sin fila en GymUsers. Ejecute 02_dml_backfill_gym_users.sql.', missing_assoc;
    END IF;
END $$;

ALTER TABLE "Users" DROP CONSTRAINT IF EXISTS "FK_Users_Gyms_GymId";
DROP INDEX IF EXISTS "IX_Users_GymId";
ALTER TABLE "Users" DROP COLUMN IF EXISTS "Rol";
ALTER TABLE "Users" DROP COLUMN IF EXISTS "GymId";

COMMIT;

-- Rollback manual (restaura columnas; rellena desde la asociación activa más reciente):
-- BEGIN;
-- ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "GymId" integer NOT NULL DEFAULT 0;
-- ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "Rol" character varying(20) NOT NULL DEFAULT 'Alumno';
-- UPDATE "Users" u SET "GymId" = gu."GymId", "Rol" = gu."Rol"
-- FROM (
--     SELECT DISTINCT ON ("UserId") "UserId", "GymId", "Rol"
--     FROM "GymUsers" WHERE "Activo" = TRUE
--     ORDER BY "UserId", "FechaAsociacion" DESC
-- ) gu WHERE u."Id" = gu."UserId";
-- CREATE INDEX IF NOT EXISTS "IX_Users_GymId" ON "Users" ("GymId");
-- ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Gyms_GymId"
--     FOREIGN KEY ("GymId") REFERENCES "Gyms" ("Id") ON DELETE RESTRICT;
-- COMMIT;
