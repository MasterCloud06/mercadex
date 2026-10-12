-- Mercadex 1.5.0 - verificacion previa a las migraciones de produccion.
-- La migracion 20261011020046_ProteccionDuplicadosMensuales crea un indice
-- UNICO sobre (UnidadComercialId, ConceptoCobroId, PeriodoAnio, PeriodoMes).
-- Si esta consulta devuelve filas, la migracion FALLARA: corrija o anule
-- primero las obligaciones duplicadas.

SELECT "UnidadComercialId",
       "ConceptoCobroId",
       "PeriodoAnio",
       "PeriodoMes",
       COUNT(*) AS cantidad
FROM obligaciones
WHERE "PeriodoAnio" IS NOT NULL
  AND "PeriodoMes" IS NOT NULL
GROUP BY 1, 2, 3, 4
HAVING COUNT(*) > 1
ORDER BY cantidad DESC;

-- Migraciones ya aplicadas en la base:
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1;
