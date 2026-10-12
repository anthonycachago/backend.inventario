/* =============================================================================
   01 - Esquema completo (tablas, índices, restricciones y vistas)
   Generado con: dotnet ef migrations script --idempotent
   Es re-ejecutable: solo aplica las migraciones que falten en __EFMigrationsHistory.
   Ejecutar sobre la base del sistema, p. ej.:
     sqlcmd -S servidor -d InventarioMariscos -I -f 65001 -i 01_Esquema_Idempotente.sql
   ============================================================================= */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    IF SCHEMA_ID(N'inv') IS NULL EXEC(N'CREATE SCHEMA [inv];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [NombreCompleto] nvarchar(150) NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Bodegas] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Direccion] nvarchar(250) NULL,
        [Ciudad] nvarchar(100) NULL,
        [Latitud] decimal(9,6) NULL,
        [Longitud] decimal(9,6) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Bodegas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Categorias] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(250) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Proveedores] (
        [Id] int NOT NULL IDENTITY,
        [Identificacion] nvarchar(20) NOT NULL,
        [RazonSocial] nvarchar(200) NOT NULL,
        [Telefono] nvarchar(20) NULL,
        [Email] nvarchar(150) NULL,
        [Direccion] nvarchar(250) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Ubicaciones] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(30) NOT NULL,
        [Tipo] nvarchar(30) NOT NULL,
        [Zona] nvarchar(20) NOT NULL,
        [Pasillo] nvarchar(10) NULL,
        [Rack] nvarchar(10) NULL,
        [Nivel] nvarchar(10) NULL,
        [CapacidadKg] decimal(18,3) NULL,
        [TemperaturaObjetivo] decimal(5,2) NULL,
        [Activo] bit NOT NULL,
        [BodegaId] int NOT NULL,
        CONSTRAINT [PK_Ubicaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ubicaciones_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [inv].[Bodegas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Productos] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(30) NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [NombreCientifico] nvarchar(150) NULL,
        [Presentacion] nvarchar(50) NULL,
        [UnidadMedida] nvarchar(20) NOT NULL,
        [EstadoConservacion] nvarchar(20) NOT NULL,
        [TemperaturaMinima] decimal(5,2) NULL,
        [TemperaturaMaxima] decimal(5,2) NULL,
        [DiasVidaUtil] int NOT NULL,
        [StockMinimo] decimal(18,3) NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [CategoriaId] int NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Productos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [inv].[Categorias] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[RegistrosTemperatura] (
        [Id] bigint NOT NULL IDENTITY,
        [Temperatura] decimal(5,2) NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [Observacion] nvarchar(250) NULL,
        [UbicacionId] int NOT NULL,
        [UsuarioId] nvarchar(450) NULL,
        CONSTRAINT [PK_RegistrosTemperatura] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RegistrosTemperatura_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RegistrosTemperatura_Ubicaciones_UbicacionId] FOREIGN KEY ([UbicacionId]) REFERENCES [inv].[Ubicaciones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Lotes] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(40) NOT NULL,
        [FechaIngreso] datetime2 NOT NULL,
        [FechaCaptura] datetime2 NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [Origen] nvarchar(200) NULL,
        [CertificadoSanitario] nvarchar(100) NULL,
        [CostoUnitario] decimal(18,4) NOT NULL,
        [ProductoId] int NOT NULL,
        [ProveedorId] int NULL,
        CONSTRAINT [PK_Lotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lotes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [inv].[Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Lotes_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [inv].[Proveedores] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[Existencias] (
        [Id] int NOT NULL IDENTITY,
        [Cantidad] decimal(18,3) NOT NULL,
        [FechaActualizacion] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [LoteId] int NOT NULL,
        [UbicacionId] int NOT NULL,
        CONSTRAINT [PK_Existencias] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Existencias_Cantidad] CHECK ([Cantidad] >= 0),
        CONSTRAINT [FK_Existencias_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [inv].[Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Existencias_Ubicaciones_UbicacionId] FOREIGN KEY ([UbicacionId]) REFERENCES [inv].[Ubicaciones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE TABLE [inv].[MovimientosInventario] (
        [Id] bigint NOT NULL IDENTITY,
        [Tipo] nvarchar(20) NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [Fecha] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Referencia] nvarchar(50) NULL,
        [Observacion] nvarchar(250) NULL,
        [LoteId] int NOT NULL,
        [UbicacionOrigenId] int NULL,
        [UbicacionDestinoId] int NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MovimientosInventario_Cantidad] CHECK ([Cantidad] > 0),
        CONSTRAINT [FK_MovimientosInventario_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [inv].[Lotes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_Ubicaciones_UbicacionDestinoId] FOREIGN KEY ([UbicacionDestinoId]) REFERENCES [inv].[Ubicaciones] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_Ubicaciones_UbicacionOrigenId] FOREIGN KEY ([UbicacionOrigenId]) REFERENCES [inv].[Ubicaciones] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[inv].[Categorias]'))
        SET IDENTITY_INSERT [inv].[Categorias] ON;
    EXEC(N'INSERT INTO [inv].[Categorias] ([Id], [Activo], [Descripcion], [Nombre])
    VALUES (1, CAST(1 AS bit), NULL, N''Pescado''),
    (2, CAST(1 AS bit), NULL, N''Crustáceo''),
    (3, CAST(1 AS bit), NULL, N''Molusco''),
    (4, CAST(1 AS bit), NULL, N''Cefalópodo'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[inv].[Categorias]'))
        SET IDENTITY_INSERT [inv].[Categorias] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bodegas_Codigo] ON [inv].[Bodegas] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categorias_Nombre] ON [inv].[Categorias] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Existencias_LoteId_UbicacionId] ON [inv].[Existencias] ([LoteId], [UbicacionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_Existencias_UbicacionId] ON [inv].[Existencias] ([UbicacionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_Lotes_FechaVencimiento] ON [inv].[Lotes] ([FechaVencimiento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Lotes_ProductoId_Codigo] ON [inv].[Lotes] ([ProductoId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_Lotes_ProveedorId] ON [inv].[Lotes] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_Fecha] ON [inv].[MovimientosInventario] ([Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_LoteId_Fecha] ON [inv].[MovimientosInventario] ([LoteId], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_UbicacionDestinoId] ON [inv].[MovimientosInventario] ([UbicacionDestinoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_UbicacionOrigenId] ON [inv].[MovimientosInventario] ([UbicacionOrigenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_UsuarioId] ON [inv].[MovimientosInventario] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaId] ON [inv].[Productos] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [inv].[Productos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proveedores_Identificacion] ON [inv].[Proveedores] ([Identificacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_RegistrosTemperatura_UbicacionId_FechaHora] ON [inv].[RegistrosTemperatura] ([UbicacionId], [FechaHora]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE INDEX [IX_RegistrosTemperatura_UsuarioId] ON [inv].[RegistrosTemperatura] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ubicaciones_BodegaId_Codigo] ON [inv].[Ubicaciones] ([BodegaId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013456_InicialInventarioMariscos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261009013456_InicialInventarioMariscos', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[Ubicaciones] ADD CONSTRAINT [CK_Ubicaciones_Capacidad] CHECK ([CapacidadKg] IS NULL OR [CapacidadKg] > 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[Productos] ADD CONSTRAINT [CK_Productos_DiasVidaUtil] CHECK ([DiasVidaUtil] >= 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[Productos] ADD CONSTRAINT [CK_Productos_RangoTemperatura] CHECK ([TemperaturaMinima] IS NULL OR [TemperaturaMaxima] IS NULL OR [TemperaturaMinima] <= [TemperaturaMaxima])');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[Productos] ADD CONSTRAINT [CK_Productos_StockMinimo] CHECK ([StockMinimo] >= 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[MovimientosInventario] ADD CONSTRAINT [CK_MovimientosInventario_Ubicaciones] CHECK (([Tipo] = ''Entrada'' AND [UbicacionOrigenId] IS NULL AND [UbicacionDestinoId] IS NOT NULL) OR ([Tipo] IN (''Salida'', ''Merma'') AND [UbicacionOrigenId] IS NOT NULL AND [UbicacionDestinoId] IS NULL) OR ([Tipo] = ''Transferencia'' AND [UbicacionOrigenId] IS NOT NULL AND [UbicacionDestinoId] IS NOT NULL AND [UbicacionOrigenId] <> [UbicacionDestinoId]) OR ([Tipo] = ''Ajuste'' AND ([UbicacionOrigenId] IS NULL OR [UbicacionDestinoId] IS NULL) AND COALESCE([UbicacionOrigenId], [UbicacionDestinoId]) IS NOT NULL))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[Lotes] ADD CONSTRAINT [CK_Lotes_CostoUnitario] CHECK ([CostoUnitario] >= 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'ALTER TABLE [inv].[Lotes] ADD CONSTRAINT [CK_Lotes_Vencimiento] CHECK (CAST([FechaVencimiento] AS date) >= CAST([FechaIngreso] AS date))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'CREATE OR ALTER VIEW [inv].[vw_StockPorProducto] AS
    SELECT p.Id AS ProductoId, p.Codigo, p.Nombre AS Producto, c.Nombre AS Categoria, p.UnidadMedida, p.StockMinimo,
           CAST(ISNULL(SUM(e.Cantidad), 0) AS decimal(18,3)) AS StockTotal,
           CAST(CASE WHEN ISNULL(SUM(e.Cantidad), 0) < p.StockMinimo THEN 1 ELSE 0 END AS bit) AS BajoMinimo
    FROM [inv].[Productos] p
    INNER JOIN [inv].[Categorias] c ON c.Id = p.CategoriaId
    LEFT JOIN [inv].[Lotes] l ON l.ProductoId = p.Id
    LEFT JOIN [inv].[Existencias] e ON e.LoteId = l.Id
    WHERE p.Activo = 1
    GROUP BY p.Id, p.Codigo, p.Nombre, c.Nombre, p.UnidadMedida, p.StockMinimo');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'CREATE OR ALTER VIEW [inv].[vw_StockPorUbicacion] AS
    SELECT e.Id AS ExistenciaId, b.Id AS BodegaId, b.Nombre AS Bodega,
           u.Id AS UbicacionId, u.Codigo AS Ubicacion, u.Zona, u.Tipo AS TipoUbicacion,
           l.Id AS LoteId, l.Codigo AS Lote, p.Id AS ProductoId, p.Nombre AS Producto, p.UnidadMedida,
           e.Cantidad, l.CostoUnitario, l.FechaVencimiento,
           DATEDIFF(DAY, CAST(GETDATE() AS date), CAST(l.FechaVencimiento AS date)) AS DiasParaVencer
    FROM [inv].[Existencias] e
    INNER JOIN [inv].[Lotes] l ON l.Id = e.LoteId
    INNER JOIN [inv].[Productos] p ON p.Id = l.ProductoId
    INNER JOIN [inv].[Ubicaciones] u ON u.Id = e.UbicacionId
    INNER JOIN [inv].[Bodegas] b ON b.Id = u.BodegaId
    WHERE e.Cantidad > 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    EXEC(N'CREATE OR ALTER VIEW [inv].[vw_OcupacionUbicaciones] AS
    SELECT u.Id AS UbicacionId, b.Id AS BodegaId, b.Nombre AS Bodega, u.Codigo, u.Zona, u.Pasillo, u.Rack, u.Nivel,
           u.Tipo, u.Activo, u.CapacidadKg,
           CAST(ISNULL(s.OcupadoKg, 0) AS decimal(18,3)) AS OcupadoKg,
           CAST(CASE WHEN u.CapacidadKg > 0 THEN ISNULL(s.OcupadoKg, 0) * 100.0 / u.CapacidadKg END AS decimal(9,2)) AS PorcentajeOcupacion,
           ISNULL(s.LotesDistintos, 0) AS LotesDistintos,
           u.TemperaturaObjetivo,
           t.Temperatura AS UltimaTemperatura,
           t.FechaHora AS FechaUltimaTemperatura
    FROM [inv].[Ubicaciones] u
    INNER JOIN [inv].[Bodegas] b ON b.Id = u.BodegaId
    OUTER APPLY (
        SELECT SUM(e.Cantidad * CASE p.UnidadMedida WHEN ''Kilogramo'' THEN 1.0 WHEN ''Libra'' THEN 0.45359237 ELSE 0 END) AS OcupadoKg,
               COUNT(DISTINCT e.LoteId) AS LotesDistintos
        FROM [inv].[Existencias] e
        INNER JOIN [inv].[Lotes] l ON l.Id = e.LoteId
        INNER JOIN [inv].[Productos] p ON p.Id = l.ProductoId
        WHERE e.UbicacionId = u.Id AND e.Cantidad > 0
    ) s
    OUTER APPLY (
        SELECT TOP (1) r.Temperatura, r.FechaHora
        FROM [inv].[RegistrosTemperatura] r
        WHERE r.UbicacionId = u.Id
        ORDER BY r.FechaHora DESC
    ) t');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011213936_LocalizacionVistasYReglas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011213936_LocalizacionVistasYReglas', N'10.0.12');
END;

COMMIT;
GO

