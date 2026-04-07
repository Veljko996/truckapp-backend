-- ============================================================
-- 01: Kreiranje tabele Tenants + seed podrazumevanog tenanta
-- ============================================================
-- Idempotentna skripta - bezbedno se pokrece vise puta.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = N'Tenants' AND s.name = N'dbo'
)
BEGIN
    CREATE TABLE dbo.Tenants
    (
        TenantId    INT             NOT NULL IDENTITY(1,1),
        Naziv       NVARCHAR(200)   NOT NULL,
        PIB         NVARCHAR(20)    NULL,
        Email       NVARCHAR(200)   NULL,
        Telefon     NVARCHAR(50)    NULL,
        Adresa      NVARCHAR(300)   NULL,
        IsActive    BIT             NOT NULL CONSTRAINT DF_Tenants_IsActive DEFAULT 1,
        CreatedAt   DATETIME2       NOT NULL CONSTRAINT DF_Tenants_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (TenantId)
    );

    PRINT 'Tabela dbo.Tenants kreirana.';
END
ELSE
BEGIN
    PRINT 'Tabela dbo.Tenants vec postoji - preskoceno.';
END
GO

-- Seed podrazumevanog tenanta (koristi se za postojece podatke)
IF NOT EXISTS (SELECT 1 FROM dbo.Tenants WHERE TenantId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Tenants ON;

    INSERT INTO dbo.Tenants (TenantId, Naziv, IsActive, CreatedAt)
    VALUES (1, N'Default Tenant', 1, SYSUTCDATETIME());

    SET IDENTITY_INSERT dbo.Tenants OFF;

    PRINT 'Default tenant (TenantId = 1) unet.';
END
ELSE
BEGIN
    PRINT 'Default tenant (TenantId = 1) vec postoji.';
END
GO
