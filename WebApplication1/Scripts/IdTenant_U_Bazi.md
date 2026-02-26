-- =============================================================================
-- Gde u bazi se još uvek očekuje IdTenant (dok ne uvedeš tenant)
-- =============================================================================

-- 1) Tabela Ture
--    Kolona: IdTenant (int NOT NULL).
--    Aplikacija više ne šalje IdTenant pri insertu, pa INSERT može da padne sa
--    "Cannot insert the value NULL into column 'IdTenant'".
--    Rešenje (izaberi jedno):
--    • DEFAULT: ALTER TABLE Ture ADD CONSTRAINT DF_Ture_IdTenant DEFAULT (1) FOR IdTenant;
--    • ili dozvoli NULL: ALTER TABLE Ture ALTER COLUMN IdTenant INT NULL;

-- 2) Procedura dbo.GetNextDocumentNumber
--    Parametar: @idTenant INT (bez defaulta).
--    Aplikacija sada poziva proceduru sa literalom 1 kao prvi argument, tako da
--    procedura i dalje radi. Kad uvedeš tenant, u kodu vrati prosleđivanje
--    @idTenant iz aplikacije.

-- 3) Tabela DocumentCounters
--    Koristi je GetNextDocumentNumber; ima kolonu IdTenant i WHERE po njoj.
--    Ništa ne menjaš dok ne uvedeš tenant – za sada sve ide pod tenant 1
--    (ili vrednost koju proslediš u proceduru).

-- Provera: koje tabele imaju kolonu IdTenant
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'IdTenant'
ORDER BY TABLE_NAME;
