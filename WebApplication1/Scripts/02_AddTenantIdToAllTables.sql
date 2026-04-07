-- ============================================================
-- 02: Dodavanje TenantId kolone na sve tenant-scoped tabele
-- ============================================================
-- Idempotentna skripta - bezbedno se pokrece vise puta.
--
-- Strategija:
--   1. Dodaje TenantId kao NULLABLE kolonu
--   2. Popunjava postojece redove sa TenantId = 1 (default tenant)
--   3. Menja kolonu na NOT NULL
--   4. Dodaje FK ka dbo.Tenants
--   5. Dodaje indeks na TenantId
--   6. Azurira unique indekse da budu per-tenant
--
-- GLOBALNE tabele (BEZ TenantId):
--   - Roles
--   - VrsteNadogradnje
--   - TipDokumenta
--   - TipTroska
-- ============================================================

-- ==========================================
-- Helper: lista svih tenant-scoped tabela
-- ==========================================
-- Users, Employees, Ture, Nalozi, NasaVozila,
-- Prevoznici, Klijenti, Poslovnice, Vinjete,
-- NalogTroskovi, NalogPrihodi, NalogDokumenti,
-- GorivoZapisi, NasaVoziloVozacAssignments, Logs


-- ##########################################################
--  KORAK 1: Dodaj TenantId kolonu (nullable) + popuni + NOT NULL
-- ##########################################################

-- ---------- Users ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Users ADD TenantId INT NULL;
    PRINT 'Users: kolona TenantId dodata.';
END
GO
UPDATE dbo.Users SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Users ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Employees ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Employees') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Employees ADD TenantId INT NULL;
    PRINT 'Employees: kolona TenantId dodata.';
END
GO
UPDATE dbo.Employees SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Employees ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Ture ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Ture') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Ture ADD TenantId INT NULL;
    PRINT 'Ture: kolona TenantId dodata.';
END
GO
UPDATE dbo.Ture SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Ture ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Nalozi ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Nalozi') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Nalozi ADD TenantId INT NULL;
    PRINT 'Nalozi: kolona TenantId dodata.';
END
GO
UPDATE dbo.Nalozi SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Nalozi ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- NasaVozila ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NasaVozila') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.NasaVozila ADD TenantId INT NULL;
    PRINT 'NasaVozila: kolona TenantId dodata.';
END
GO
UPDATE dbo.NasaVozila SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.NasaVozila ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Prevoznici ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Prevoznici') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Prevoznici ADD TenantId INT NULL;
    PRINT 'Prevoznici: kolona TenantId dodata.';
END
GO
UPDATE dbo.Prevoznici SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Prevoznici ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Klijenti ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Klijenti') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Klijenti ADD TenantId INT NULL;
    PRINT 'Klijenti: kolona TenantId dodata.';
END
GO
UPDATE dbo.Klijenti SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Klijenti ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Poslovnice ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Poslovnice') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Poslovnice ADD TenantId INT NULL;
    PRINT 'Poslovnice: kolona TenantId dodata.';
END
GO
UPDATE dbo.Poslovnice SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Poslovnice ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Vinjete ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Vinjete') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Vinjete ADD TenantId INT NULL;
    PRINT 'Vinjete: kolona TenantId dodata.';
END
GO
UPDATE dbo.Vinjete SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Vinjete ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- NalogTroskovi ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NalogTroskovi') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.NalogTroskovi ADD TenantId INT NULL;
    PRINT 'NalogTroskovi: kolona TenantId dodata.';
END
GO
UPDATE dbo.NalogTroskovi SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.NalogTroskovi ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- NalogPrihodi ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NalogPrihodi') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.NalogPrihodi ADD TenantId INT NULL;
    PRINT 'NalogPrihodi: kolona TenantId dodata.';
END
GO
UPDATE dbo.NalogPrihodi SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.NalogPrihodi ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- NalogDokumenti ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NalogDokumenti') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.NalogDokumenti ADD TenantId INT NULL;
    PRINT 'NalogDokumenti: kolona TenantId dodata.';
END
GO
UPDATE dbo.NalogDokumenti SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.NalogDokumenti ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- GorivoZapisi ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.GorivoZapisi') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.GorivoZapisi ADD TenantId INT NULL;
    PRINT 'GorivoZapisi: kolona TenantId dodata.';
END
GO
UPDATE dbo.GorivoZapisi SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.GorivoZapisi ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- NasaVoziloVozacAssignments ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.NasaVoziloVozacAssignments') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.NasaVoziloVozacAssignments ADD TenantId INT NULL;
    PRINT 'NasaVoziloVozacAssignments: kolona TenantId dodata.';
END
GO
UPDATE dbo.NasaVoziloVozacAssignments SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.NasaVoziloVozacAssignments ALTER COLUMN TenantId INT NOT NULL;
GO

-- ---------- Logs ----------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Logs') AND name = 'TenantId')
BEGIN
    ALTER TABLE dbo.Logs ADD TenantId INT NULL;
    PRINT 'Logs: kolona TenantId dodata.';
END
GO
UPDATE dbo.Logs SET TenantId = 1 WHERE TenantId IS NULL;
GO
ALTER TABLE dbo.Logs ALTER COLUMN TenantId INT NOT NULL;
GO


-- ##########################################################
--  KORAK 2: Foreign key ka dbo.Tenants
-- ##########################################################

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Tenants')
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Employees_Tenants')
    ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ture_Tenants')
    ALTER TABLE dbo.Ture ADD CONSTRAINT FK_Ture_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Nalozi_Tenants')
    ALTER TABLE dbo.Nalozi ADD CONSTRAINT FK_Nalozi_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NasaVozila_Tenants')
    ALTER TABLE dbo.NasaVozila ADD CONSTRAINT FK_NasaVozila_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Prevoznici_Tenants')
    ALTER TABLE dbo.Prevoznici ADD CONSTRAINT FK_Prevoznici_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Klijenti_Tenants')
    ALTER TABLE dbo.Klijenti ADD CONSTRAINT FK_Klijenti_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Poslovnice_Tenants')
    ALTER TABLE dbo.Poslovnice ADD CONSTRAINT FK_Poslovnice_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vinjete_Tenants')
    ALTER TABLE dbo.Vinjete ADD CONSTRAINT FK_Vinjete_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NalogTroskovi_Tenants')
    ALTER TABLE dbo.NalogTroskovi ADD CONSTRAINT FK_NalogTroskovi_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NalogPrihodi_Tenants')
    ALTER TABLE dbo.NalogPrihodi ADD CONSTRAINT FK_NalogPrihodi_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NalogDokumenti_Tenants')
    ALTER TABLE dbo.NalogDokumenti ADD CONSTRAINT FK_NalogDokumenti_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GorivoZapisi_Tenants')
    ALTER TABLE dbo.GorivoZapisi ADD CONSTRAINT FK_GorivoZapisi_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NasaVoziloVozacAssignments_Tenants')
    ALTER TABLE dbo.NasaVoziloVozacAssignments ADD CONSTRAINT FK_NasaVoziloVozacAssignments_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Logs_Tenants')
    ALTER TABLE dbo.Logs ADD CONSTRAINT FK_Logs_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId);
GO

PRINT 'Svi FK constraints ka dbo.Tenants dodati.';
GO


-- ##########################################################
--  KORAK 3: Non-clustered indeksi na TenantId
-- ##########################################################

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_TenantId')
    CREATE NONCLUSTERED INDEX IX_Users_TenantId ON dbo.Users(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_TenantId')
    CREATE NONCLUSTERED INDEX IX_Employees_TenantId ON dbo.Employees(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ture_TenantId')
    CREATE NONCLUSTERED INDEX IX_Ture_TenantId ON dbo.Ture(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Nalozi_TenantId')
    CREATE NONCLUSTERED INDEX IX_Nalozi_TenantId ON dbo.Nalozi(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NasaVozila_TenantId')
    CREATE NONCLUSTERED INDEX IX_NasaVozila_TenantId ON dbo.NasaVozila(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Prevoznici_TenantId')
    CREATE NONCLUSTERED INDEX IX_Prevoznici_TenantId ON dbo.Prevoznici(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Klijenti_TenantId')
    CREATE NONCLUSTERED INDEX IX_Klijenti_TenantId ON dbo.Klijenti(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Poslovnice_TenantId')
    CREATE NONCLUSTERED INDEX IX_Poslovnice_TenantId ON dbo.Poslovnice(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vinjete_TenantId')
    CREATE NONCLUSTERED INDEX IX_Vinjete_TenantId ON dbo.Vinjete(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NalogTroskovi_TenantId')
    CREATE NONCLUSTERED INDEX IX_NalogTroskovi_TenantId ON dbo.NalogTroskovi(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NalogPrihodi_TenantId')
    CREATE NONCLUSTERED INDEX IX_NalogPrihodi_TenantId ON dbo.NalogPrihodi(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NalogDokumenti_TenantId')
    CREATE NONCLUSTERED INDEX IX_NalogDokumenti_TenantId ON dbo.NalogDokumenti(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GorivoZapisi_TenantId')
    CREATE NONCLUSTERED INDEX IX_GorivoZapisi_TenantId ON dbo.GorivoZapisi(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NasaVoziloVozacAssignments_TenantId')
    CREATE NONCLUSTERED INDEX IX_NasaVoziloVozacAssignments_TenantId ON dbo.NasaVoziloVozacAssignments(TenantId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Logs_TenantId')
    CREATE NONCLUSTERED INDEX IX_Logs_TenantId ON dbo.Logs(TenantId);
GO

PRINT 'Svi IX_*_TenantId indeksi dodati.';
GO


-- ##########################################################
--  KORAK 4: Azuriranje UNIQUE indeksa da budu per-tenant
-- ##########################################################

-- --- Users.Username: globalno unique -> unique per tenant ---
-- Stari indeks iz EF: IX_Users_Username (unique)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('dbo.Users'))
BEGIN
    DROP INDEX IX_Users_Username ON dbo.Users;
    PRINT 'IX_Users_Username obrisan.';
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_TenantId_Username' AND object_id = OBJECT_ID('dbo.Users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_TenantId_Username
        ON dbo.Users(TenantId, Username);
    PRINT 'IX_Users_TenantId_Username kreiran (unique per tenant).';
END
GO

-- --- Ture.RedniBroj: globalno unique -> unique per tenant ---
-- Stari indeks iz EF: IX_Ture_RedniBroj (unique)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ture_RedniBroj' AND object_id = OBJECT_ID('dbo.Ture'))
BEGIN
    DROP INDEX IX_Ture_RedniBroj ON dbo.Ture;
    PRINT 'IX_Ture_RedniBroj obrisan.';
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ture_TenantId_RedniBroj' AND object_id = OBJECT_ID('dbo.Ture'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Ture_TenantId_RedniBroj
        ON dbo.Ture(TenantId, RedniBroj);
    PRINT 'IX_Ture_TenantId_RedniBroj kreiran (unique per tenant).';
END
GO

-- --- Employees.EmployeeNumber: filtered unique -> unique per tenant ---
-- Stari indeks iz EF: IX_Employees_EmployeeNumber (unique, filtered)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_EmployeeNumber' AND object_id = OBJECT_ID('dbo.Employees'))
BEGIN
    DROP INDEX IX_Employees_EmployeeNumber ON dbo.Employees;
    PRINT 'IX_Employees_EmployeeNumber obrisan.';
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employees_TenantId_EmployeeNumber' AND object_id = OBJECT_ID('dbo.Employees'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Employees_TenantId_EmployeeNumber
        ON dbo.Employees(TenantId, EmployeeNumber)
        WHERE [EmployeeNumber] IS NOT NULL;
    PRINT 'IX_Employees_TenantId_EmployeeNumber kreiran (unique per tenant, filtered).';
END
GO

-- --- Nalozi.TuraId: conditional unique -> dodaj TenantId u indeks ---
-- Stari: IX_Nalozi_TuraId unique with filter
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Nalozi_TuraId' AND object_id = OBJECT_ID('dbo.Nalozi'))
BEGIN
    DROP INDEX IX_Nalozi_TuraId ON dbo.Nalozi;
    PRINT 'IX_Nalozi_TuraId obrisan.';
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Nalozi_TenantId_TuraId' AND object_id = OBJECT_ID('dbo.Nalozi'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Nalozi_TenantId_TuraId
        ON dbo.Nalozi(TenantId, TuraId)
        WHERE [StatusNaloga] <> 'Storniran' AND [StatusNaloga] <> 'Ponisten';
    PRINT 'IX_Nalozi_TenantId_TuraId kreiran (unique per tenant, filtered).';
END
GO

PRINT '================================================';
PRINT ' Migracija zavrsena.';
PRINT ' Svi postojeci podaci dodeljeni TenantId = 1.';
PRINT '================================================';
GO
