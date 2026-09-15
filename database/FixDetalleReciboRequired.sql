CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE conceptos_cobro (
    "Id" uuid NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "Nombre" character varying(150) NOT NULL,
    "Descripcion" character varying(500),
    "Activo" boolean NOT NULL,
    CONSTRAINT "PK_conceptos_cobro" PRIMARY KEY ("Id")
);

CREATE TABLE monedas (
    "Id" uuid NOT NULL,
    "Codigo" character varying(10) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Simbolo" character varying(10) NOT NULL,
    "Activa" boolean NOT NULL,
    CONSTRAINT "PK_monedas" PRIMARY KEY ("Id")
);

CREATE TABLE pasillos (
    "Id" uuid NOT NULL,
    "Codigo" character varying(30) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Activo" boolean NOT NULL,
    CONSTRAINT "PK_pasillos" PRIMARY KEY ("Id")
);

CREATE TABLE socios (
    "Id" uuid NOT NULL,
    "Nombres" character varying(150) NOT NULL,
    "Apellidos" character varying(150) NOT NULL,
    "DocumentoIdentidad" character varying(50),
    "Telefono" character varying(50),
    "Activo" boolean NOT NULL,
    "FechaRegistro" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_socios" PRIMARY KEY ("Id")
);

CREATE TABLE pagos (
    "Id" uuid NOT NULL,
    "MonedaId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "FormaPago" integer NOT NULL,
    "NombrePagador" character varying(200) NOT NULL,
    "Referencia" character varying(150),
    "TipoCambio" numeric(18,6) NOT NULL,
    "FechaRegistro" timestamp with time zone NOT NULL,
    "Estado" integer NOT NULL,
    CONSTRAINT "PK_pagos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_pagos_monedas_MonedaId" FOREIGN KEY ("MonedaId") REFERENCES monedas ("Id") ON DELETE RESTRICT
);

CREATE TABLE unidades_comerciales (
    "Id" uuid NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "Tipo" integer NOT NULL,
    "PasilloId" uuid,
    "Bloque" character varying(50),
    "Activa" boolean NOT NULL,
    "FechaRegistro" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_unidades_comerciales" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_unidades_comerciales_pasillos_PasilloId" FOREIGN KEY ("PasilloId") REFERENCES pasillos ("Id") ON DELETE RESTRICT
);

CREATE TABLE asignaciones_unidades_comerciales (
    "Id" uuid NOT NULL,
    "SocioId" uuid NOT NULL,
    "UnidadComercialId" uuid NOT NULL,
    "TipoVinculo" integer NOT NULL,
    "FechaInicio" date NOT NULL,
    "FechaFin" date,
    CONSTRAINT "PK_asignaciones_unidades_comerciales" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_asignaciones_unidades_comerciales_socios_SocioId" FOREIGN KEY ("SocioId") REFERENCES socios ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_asignaciones_unidades_comerciales_unidades_comerciales_Unid~" FOREIGN KEY ("UnidadComercialId") REFERENCES unidades_comerciales ("Id") ON DELETE RESTRICT
);

CREATE TABLE obligaciones (
    "Id" uuid NOT NULL,
    "UnidadComercialId" uuid NOT NULL,
    "ConceptoCobroId" uuid NOT NULL,
    "MonedaId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "FechaEmision" date NOT NULL,
    "FechaVencimiento" date,
    "PeriodoAnio" integer,
    "PeriodoMes" integer,
    "Estado" integer NOT NULL,
    CONSTRAINT "PK_obligaciones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_obligaciones_conceptos_cobro_ConceptoCobroId" FOREIGN KEY ("ConceptoCobroId") REFERENCES conceptos_cobro ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_obligaciones_monedas_MonedaId" FOREIGN KEY ("MonedaId") REFERENCES monedas ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_obligaciones_unidades_comerciales_UnidadComercialId" FOREIGN KEY ("UnidadComercialId") REFERENCES unidades_comerciales ("Id") ON DELETE RESTRICT
);

CREATE TABLE recibos (
    "Id" uuid NOT NULL,
    "Numero" bigint NOT NULL,
    "UnidadComercialId" uuid NOT NULL,
    "PagoId" uuid NOT NULL,
    "FechaEmision" timestamp with time zone NOT NULL,
    "Estado" integer NOT NULL,
    "MotivoAnulacion" character varying(500),
    "FechaAnulacion" timestamp with time zone,
    CONSTRAINT "PK_recibos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_recibos_pagos_PagoId" FOREIGN KEY ("PagoId") REFERENCES pagos ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_recibos_unidades_comerciales_UnidadComercialId" FOREIGN KEY ("UnidadComercialId") REFERENCES unidades_comerciales ("Id") ON DELETE RESTRICT
);

CREATE TABLE detalles_recibo (
    "Id" uuid NOT NULL,
    "ObligacionId" uuid NOT NULL,
    "ConceptoCobroId" uuid NOT NULL,
    "Concepto" character varying(200) NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "PeriodoAnio" integer,
    "PeriodoMes" integer,
    "ReciboId" uuid,
    CONSTRAINT "PK_detalles_recibo" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_detalles_recibo_conceptos_cobro_ConceptoCobroId" FOREIGN KEY ("ConceptoCobroId") REFERENCES conceptos_cobro ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_detalles_recibo_obligaciones_ObligacionId" FOREIGN KEY ("ObligacionId") REFERENCES obligaciones ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_detalles_recibo_recibos_ReciboId" FOREIGN KEY ("ReciboId") REFERENCES recibos ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_asignaciones_unidades_comerciales_SocioId" ON asignaciones_unidades_comerciales ("SocioId");

CREATE INDEX "IX_asignaciones_unidades_comerciales_UnidadComercialId_SocioId~" ON asignaciones_unidades_comerciales ("UnidadComercialId", "SocioId", "FechaInicio");

CREATE UNIQUE INDEX "IX_conceptos_cobro_Codigo" ON conceptos_cobro ("Codigo");

CREATE INDEX "IX_conceptos_cobro_Nombre" ON conceptos_cobro ("Nombre");

CREATE INDEX "IX_detalles_recibo_ConceptoCobroId" ON detalles_recibo ("ConceptoCobroId");

CREATE INDEX "IX_detalles_recibo_ObligacionId" ON detalles_recibo ("ObligacionId");

CREATE INDEX "IX_detalles_recibo_ReciboId" ON detalles_recibo ("ReciboId");

CREATE UNIQUE INDEX "IX_monedas_Codigo" ON monedas ("Codigo");

CREATE INDEX "IX_obligaciones_ConceptoCobroId_PeriodoAnio_PeriodoMes" ON obligaciones ("ConceptoCobroId", "PeriodoAnio", "PeriodoMes");

CREATE INDEX "IX_obligaciones_MonedaId" ON obligaciones ("MonedaId");

CREATE INDEX "IX_obligaciones_UnidadComercialId_Estado" ON obligaciones ("UnidadComercialId", "Estado");

CREATE INDEX "IX_pagos_FechaRegistro" ON pagos ("FechaRegistro");

CREATE INDEX "IX_pagos_MonedaId" ON pagos ("MonedaId");

CREATE INDEX "IX_pagos_Referencia" ON pagos ("Referencia");

CREATE UNIQUE INDEX "IX_pasillos_Codigo" ON pasillos ("Codigo");

CREATE INDEX "IX_recibos_FechaEmision" ON recibos ("FechaEmision");

CREATE UNIQUE INDEX "IX_recibos_Numero" ON recibos ("Numero");

CREATE UNIQUE INDEX "IX_recibos_PagoId" ON recibos ("PagoId");

CREATE INDEX "IX_recibos_UnidadComercialId" ON recibos ("UnidadComercialId");

CREATE INDEX "IX_socios_DocumentoIdentidad" ON socios ("DocumentoIdentidad");

CREATE UNIQUE INDEX "IX_unidades_comerciales_Codigo" ON unidades_comerciales ("Codigo");

CREATE INDEX "IX_unidades_comerciales_PasilloId" ON unidades_comerciales ("PasilloId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260908020204_InitialCreate', '10.0.11');

COMMIT;

START TRANSACTION;
ALTER TABLE detalles_recibo ALTER COLUMN "ReciboId" SET NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260915193111_FixDetalleReciboRequired', '10.0.11');

COMMIT;

