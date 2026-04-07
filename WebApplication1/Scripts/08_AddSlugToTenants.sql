-- ============================================================
-- 08: Dodavanje Slug kolone na Tenants tabelu
-- ============================================================
-- Slug je kratki, case-insensitive identifikator firme
-- koji korisnik unosi pri loginu (npr. "suins", "tallteam").
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tenants') AND name = 'Slug')
BEGIN
    ALTER TABLE dbo.Tenants ADD Slug NVARCHAR(50) NULL;
    PRINT 'Tenants: kolona Slug dodata.';
END
GO

UPDATE dbo.Tenants SET Slug = 'suins' WHERE TenantId = 1 AND Slug IS NULL;
GO

-- Popuni ostale tenante koji nemaju slug (safety net)
UPDATE dbo.Tenants SET Slug = LOWER(REPLACE(Naziv, ' ', '')) WHERE Slug IS NULL;
GO

ALTER TABLE dbo.Tenants ALTER COLUMN Slug NVARCHAR(50) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tenants_Slug' AND object_id = OBJECT_ID('dbo.Tenants'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Tenants_Slug
        ON dbo.Tenants(Slug);
    PRINT 'Tenants: unique indeks na Slug kreiran.';
END
GO
