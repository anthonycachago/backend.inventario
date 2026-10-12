/* =============================================================================
   02 - Datos de demostración (opcional)
   Ejecutar sobre la base del sistema después de 01_Esquema_Idempotente.sql.
   Es re-ejecutable: solo inserta si la bodega BOD-GYE no existe.
   Los lotes se cargan con su existencia y su movimiento de Entrada para que
   el kardex cuadre con el stock.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Usuario técnico al que se atribuyen los movimientos (también lo crea la app al iniciar)
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = '00000000-0000-0000-0000-000000000001')
    INSERT INTO dbo.AspNetUsers (Id, NombreCompleto, Activo, UserName, NormalizedUserName, EmailConfirmed,
                                 SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled,
                                 LockoutEnabled, AccessFailedCount)
    VALUES ('00000000-0000-0000-0000-000000000001', N'Usuario del sistema', 1, N'sistema', N'SISTEMA', 0,
            CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), 0, 0, 1, 0);

IF EXISTS (SELECT 1 FROM inv.Bodegas WHERE Codigo = 'BOD-GYE')
BEGIN
    PRINT 'Los datos de demostración ya existen. No se hizo ningún cambio.';
    RETURN;
END

BEGIN TRANSACTION;

DECLARE @usuario nvarchar(450) = '00000000-0000-0000-0000-000000000001';
DECLARE @hoy date = CAST(GETDATE() AS date);

------------------------------------------------------------------ Proveedores
INSERT INTO inv.Proveedores (Identificacion, RazonSocial, Telefono, Email, Direccion, Activo) VALUES
 ('0991234567001', N'Camaronera El Oro S.A.',          '072 960 100', 'ventas@camaroneraeloro.ec', N'Vía Puerto Bolívar km 4, Machala', 1),
 ('1391234567001', N'Pesquera Manabí Cía. Ltda.',      '052 620 300', 'despachos@pesqueramanabi.ec', N'Puerto pesquero de Manta', 1),
 ('0992345678001', N'Cooperativa Pescadores Anconcito', '042 900 200', NULL, N'Anconcito, Santa Elena', 1);

------------------------------------------------------------------ Productos
-- Categorías sembradas por la migración: 1 Pescado, 2 Crustáceo, 3 Molusco, 4 Cefalópodo
INSERT INTO inv.Productos (Codigo, Nombre, NombreCientifico, Presentacion, UnidadMedida, EstadoConservacion,
                           TemperaturaMinima, TemperaturaMaxima, DiasVidaUtil, StockMinimo, Activo, CategoriaId) VALUES
 ('CAM-ENT-2030', N'Camarón blanco entero 20/30',  N'Litopenaeus vannamei', N'Entero',   'Kilogramo', 'Congelado',  -25, -18, 365, 2000, 1, 2),
 ('CAM-COL-3140', N'Camarón cola 31/40',           N'Litopenaeus vannamei', N'Cola',     'Kilogramo', 'Congelado',  -25, -18, 365, 1500, 1, 2),
 ('ATU-LOM',      N'Lomo de atún aleta amarilla',  N'Thunnus albacares',    N'Lomo',     'Kilogramo', 'Congelado',  -25, -18, 270,  800, 1, 1),
 ('DOR-FIL',      N'Filete de dorado',             N'Coryphaena hippurus',  N'Filete',   'Kilogramo', 'Congelado',  -25, -18, 240,  500, 1, 1),
 ('COR-FRE',      N'Corvina fresca',               N'Cynoscion albus',      N'Entero',   'Kilogramo', 'Refrigerado',  0,   4,  10,  150, 1, 1),
 ('PUL-ENT',      N'Pulpo entero',                 N'Octopus mimus',        N'Entero',   'Kilogramo', 'Congelado',  -25, -18, 300,  300, 1, 4),
 ('CAL-ANI',      N'Calamar en anillos',           N'Dosidicus gigas',      N'Anillos',  'Kilogramo', 'Congelado',  -25, -18, 300,  400, 1, 4),
 ('CON-NEG',      N'Concha negra',                 N'Anadara tuberculosa',  N'En valva', 'Unidad',    'Fresco',       2,   6,   5, 1000, 1, 3);

-- Precios de venta (IVA 0 %: mariscos frescos/congelados en estado natural; 15 %: procesados)
UPDATE p SET PrecioVenta = v.Precio, TarifaIva = v.Iva
FROM inv.Productos p
JOIN (VALUES ('CAM-ENT-2030', 6.90, 0), ('CAM-COL-3140', 8.75, 0), ('ATU-LOM', 10.50, 0), ('DOR-FIL', 7.80, 0),
             ('COR-FRE', 4.95, 0), ('PUL-ENT', 9.60, 0), ('CAL-ANI', 6.20, 15), ('CON-NEG', 0.30, 0)) v(Codigo, Precio, Iva)
  ON v.Codigo = p.Codigo;

------------------------------------------------------------------ Clientes
IF NOT EXISTS (SELECT 1 FROM inv.Clientes WHERE Identificacion = '9999999999999')
    INSERT INTO inv.Clientes (TipoIdentificacion, Identificacion, RazonSocial, LimiteCredito, DiasCredito, Activo)
    VALUES ('ConsumidorFinal', '9999999999999', N'CONSUMIDOR FINAL', 0, 0, 1);

-- RUC ficticios con formato válido (verificados como inexistentes en el SRI)
INSERT INTO inv.Clientes (TipoIdentificacion, Identificacion, RazonSocial, Telefono, Email, Direccion, LimiteCredito, DiasCredito, Activo) VALUES
 ('Ruc', '0990000001001', N'Restaurante La Cevichería Demo S.A.', '042 300 100', 'compras@cevicheria-demo.ec', N'Av. Francisco de Orellana, Guayaquil', 5000, 30, 1),
 ('Ruc', '1390000004001', N'Distribuidora Marisquera Demo Cía. Ltda.', '052 610 200', 'pagos@marisquera-demo.ec', N'Av. 4 de Noviembre, Manta', 15000, 45, 1),
 ('Ruc', '1790000003001', N'Supermercados Demo S.A.', '022 450 300', NULL, N'Av. Amazonas, Quito', 0, 0, 1);

------------------------------------------------------------------ Bodegas
INSERT INTO inv.Bodegas (Codigo, Nombre, Direccion, Ciudad, Latitud, Longitud, Activo) VALUES
 ('BOD-GYE', N'Centro de frío Guayaquil', N'Km 10.5 vía a Daule', N'Guayaquil', -2.090500, -79.937800, 1),
 ('BOD-MTA', N'Bodega puerto Manta',      N'Av. Malecón y calle 103', N'Manta', -0.942600, -80.731700, 1);

DECLARE @gye int = (SELECT Id FROM inv.Bodegas WHERE Codigo = 'BOD-GYE');
DECLARE @mta int = (SELECT Id FROM inv.Bodegas WHERE Codigo = 'BOD-MTA');

------------------------------------------------------------------ Ubicaciones
-- Guayaquil: cámara de congelación CC1 (2 pasillos x 2 racks x 2 niveles), refrigeración CR1, andén y cuarentena
INSERT INTO inv.Ubicaciones (Codigo, Tipo, Zona, Pasillo, Rack, Nivel, CapacidadKg, TemperaturaObjetivo, Activo, BodegaId)
SELECT CONCAT('CC1-', p.v, '-', r.v, '-', n.v), 'CamaraCongelacion', 'CC1', p.v, r.v, n.v, 2000, -20, 1, @gye
FROM (VALUES ('P01'), ('P02')) p(v)
CROSS JOIN (VALUES ('R01'), ('R02')) r(v)
CROSS JOIN (VALUES ('N1'), ('N2')) n(v);

INSERT INTO inv.Ubicaciones (Codigo, Tipo, Zona, Pasillo, Rack, Nivel, CapacidadKg, TemperaturaObjetivo, Activo, BodegaId) VALUES
 ('CR1-P01-R01', 'CamaraRefrigeracion', 'CR1', 'P01', 'R01', NULL,  800,  2, 1, @gye),
 ('CR1-P01-R02', 'CamaraRefrigeracion', 'CR1', 'P01', 'R02', NULL,  800,  2, 1, @gye),
 ('AND-01',      'Anden',               'AND', NULL,  NULL,  NULL, 5000, NULL, 1, @gye),
 ('CUA-01',      'Cuarentena',          'CUA', NULL,  NULL,  NULL, 1000, -20, 1, @gye),
 ('CC1-P01-R01', 'CamaraCongelacion',   'CC1', 'P01', 'R01', NULL, 3000, -20, 1, @mta),
 ('CC1-P01-R02', 'CamaraCongelacion',   'CC1', 'P01', 'R02', NULL, 3000, -20, 1, @mta),
 ('CR1-P01-R01', 'CamaraRefrigeracion', 'CR1', 'P01', 'R01', NULL,  600,  2, 1, @mta);

------------------------------------------------------------------ Lotes + existencias + kardex
DECLARE @carga TABLE (Producto varchar(30), Lote varchar(40), Proveedor varchar(20), Bodega int, Ubicacion varchar(30),
                      Cantidad decimal(18,3), Costo decimal(18,4), DiasIngreso int, DiasVence int, Origen nvarchar(200));
INSERT INTO @carga VALUES
 ('CAM-ENT-2030', 'L-CAM-2501', '0991234567001', @gye, 'CC1-P01-R01-N1', 1200, 4.85, -40, 325, N'Piscina 12, Puerto Bolívar'),
 ('CAM-ENT-2030', 'L-CAM-2502', '0991234567001', @gye, 'CC1-P01-R01-N2',  650, 4.90, -12, 353, N'Piscina 07, Puerto Bolívar'),
 ('CAM-COL-3140', 'L-COL-2501', '0991234567001', @gye, 'CC1-P01-R02-N1', 1800, 6.20, -25, 340, N'Piscina 03, Machala'),
 ('ATU-LOM',      'L-ATU-0907', '1391234567001', @mta, 'CC1-P01-R01',    1400, 7.40, -60, 210, N'B/P Don Julio, Pacífico oriental'),
 ('ATU-LOM',      'L-ATU-0911', '1391234567001', @gye, 'CC1-P02-R01-N1',  300, 7.55,  -5, 265, N'B/P Don Julio, Pacífico oriental'),
 ('DOR-FIL',      'L-DOR-0915', '1391234567001', @gye, 'CC1-P02-R01-N2',  180, 5.10, -230, 5, N'Pesca artesanal, Manta'),
 ('COR-FRE',      'L-COR-1009', '0992345678001', @gye, 'CR1-P01-R01',      90, 3.20,  -2,  3, N'Anconcito'),
 ('PUL-ENT',      'L-PUL-0820', '0992345678001', @mta, 'CC1-P01-R02',     520, 6.80, -50, 250, N'Santa Elena'),
 ('CAL-ANI',      'L-CAL-0901', '1391234567001', @gye, 'CC1-P02-R02-N1',  260, 3.95, -35, 265, N'Flota calamarera, Manta'),
 ('CON-NEG',      'L-CON-1010', '0992345678001', @gye, 'CR1-P01-R02',     800, 0.18,  -1,  4, N'Manglares de El Oro');

INSERT INTO inv.Lotes (Codigo, FechaIngreso, FechaCaptura, FechaVencimiento, Origen, CertificadoSanitario, CostoUnitario, ProductoId, ProveedorId)
SELECT c.Lote, DATEADD(DAY, c.DiasIngreso, GETDATE()), DATEADD(DAY, c.DiasIngreso - 1, @hoy), DATEADD(DAY, c.DiasVence, @hoy),
       c.Origen, CONCAT('ARCSA-', RIGHT(c.Lote, 4)), c.Costo, p.Id, pr.Id
FROM @carga c
JOIN inv.Productos p ON p.Codigo = c.Producto
JOIN inv.Proveedores pr ON pr.Identificacion = c.Proveedor;

INSERT INTO inv.Existencias (Cantidad, FechaActualizacion, LoteId, UbicacionId)
SELECT c.Cantidad, l.FechaIngreso, l.Id, u.Id
FROM @carga c
JOIN inv.Productos p ON p.Codigo = c.Producto
JOIN inv.Lotes l ON l.ProductoId = p.Id AND l.Codigo = c.Lote
JOIN inv.Ubicaciones u ON u.BodegaId = c.Bodega AND u.Codigo = c.Ubicacion;

INSERT INTO inv.MovimientosInventario (Tipo, Cantidad, Fecha, Referencia, Observacion, LoteId, UbicacionOrigenId, UbicacionDestinoId, UsuarioId)
SELECT 'Entrada', c.Cantidad, l.FechaIngreso, CONCAT('FAC-001-', RIGHT(c.Lote, 4)), N'Carga inicial (datos demo)', l.Id, NULL, u.Id, @usuario
FROM @carga c
JOIN inv.Productos p ON p.Codigo = c.Producto
JOIN inv.Lotes l ON l.ProductoId = p.Id AND l.Codigo = c.Lote
JOIN inv.Ubicaciones u ON u.BodegaId = c.Bodega AND u.Codigo = c.Ubicacion;

------------------------------------------------------------------ Lecturas de temperatura
INSERT INTO inv.RegistrosTemperatura (Temperatura, FechaHora, Observacion, UbicacionId, UsuarioId)
SELECT CASE WHEN u.Codigo = 'CC1-P02-R02-N1' THEN -15.5 ELSE u.TemperaturaObjetivo + v.delta END,
       DATEADD(HOUR, v.horas, GETDATE()),
       CASE WHEN u.Codigo = 'CC1-P02-R02-N1' THEN N'Puerta mal cerrada; se avisó a mantenimiento' END,
       u.Id, @usuario
FROM inv.Ubicaciones u
CROSS JOIN (VALUES (-8, -0.4), (-4, 0.3), (-1, -0.2)) v(horas, delta)
WHERE u.TemperaturaObjetivo IS NOT NULL;

COMMIT TRANSACTION;
PRINT 'Datos de demostración cargados.';
