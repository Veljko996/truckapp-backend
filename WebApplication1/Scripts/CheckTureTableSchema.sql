-- Provera sheme tabele Ture – greška "nvarchar to int" obično znači
-- da je neka kolona koja treba da bude INT u bazi tipa NVARCHAR.
-- Pokreni u SSMS / Azure Data Studio nad tvojom bazom.

SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'Ture'
ORDER BY c.ORDINAL_POSITION;

-- Kolone koje bi trebalo da budu INT (FK i ID):
-- TuraId, PrevoznikId, VoziloId, KlijentId, VrstaNadogradnjeId, KreiranPutniNalogId, itd.
-- Ako je DATA_TYPE 'nvarchar' ili 'varchar' za bilo koju od njih, to uzrokuje grešku.
-- Rešenje: ALTER TABLE Ture ALTER COLUMN ImeKolone INT [NULL];
