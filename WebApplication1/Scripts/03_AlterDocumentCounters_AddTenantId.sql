-- ============================================================
-- 03: Dodavanje TenantId na DocumentCounters tabelu
-- ============================================================
-- Brojac dokumenata mora biti po tenantu.
-- Svaka firma dobija svoj niz (001/26, 002/26...).
-- ============================================================

-- Korak 1: Dodaj kolonu (nullable)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DocumentCounters') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.DocumentCounters ADD TenantId INT NULL;
    PRINT 'DocumentCounters: kolona TenantId dodata.';
END
GO

-- Korak 2: Popuni postojece redove default tenantom
UPDATE dbo.DocumentCounters SET TenantId = 1 WHERE TenantId IS NULL;
GO

-- Korak 3: NOT NULL
ALTER TABLE dbo.DocumentCounters ALTER COLUMN TenantId INT NOT NULL;
GO

-- Korak 4: FK ka Tenants
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DocumentCounters_Tenants')
    ALTER TABLE dbo.DocumentCounters ADD CONSTRAINT FK_DocumentCounters_Tenants
        FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO

-- Korak 5: Azuriranje unique constraint-a
--   Stari kljuc je verovatno (DocumentType, Year).
--   Novi mora biti (DocumentType, Year, TenantId).
--   Dropujemo stari index/constraint ako postoji, pa kreiramo novi.

-- Pronalazenje i brisanje starog unique indeksa/constrainta na (DocumentType, Year)
DECLARE @OldIndexName NVARCHAR(256);
SELECT TOP 1 @OldIndexName = i.name
FROM sys.indexes i
INNER JOIN sys.index_columns ic1 ON ic1.object_id = i.object_id AND ic1.index_id = i.index_id
INNER JOIN sys.columns c1 ON c1.object_id = ic1.object_id AND c1.column_id = ic1.column_id AND c1.name = 'DocumentType'
INNER JOIN sys.index_columns ic2 ON ic2.object_id = i.object_id AND ic2.index_id = i.index_id
INNER JOIN sys.columns c2 ON c2.object_id = ic2.object_id AND c2.column_id = ic2.column_id AND c2.name = 'Year'
WHERE i.object_id = OBJECT_ID('dbo.DocumentCounters')
  AND i.is_unique = 1
  AND i.is_primary_key = 0;

IF @OldIndexName IS NOT NULL
BEGIN
    EXEC('DROP INDEX [' + @OldIndexName + '] ON dbo.DocumentCounters');
    PRINT 'DocumentCounters: stari unique indeks [' + @OldIndexName + '] obrisan.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_DocumentCounters_Type_Year_Tenant' AND object_id = OBJECT_ID('dbo.DocumentCounters'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_DocumentCounters_Type_Year_Tenant
        ON dbo.DocumentCounters(DocumentType, [Year], TenantId);
    PRINT 'DocumentCounters: novi unique indeks (DocumentType, Year, TenantId) kreiran.';
END
GO
