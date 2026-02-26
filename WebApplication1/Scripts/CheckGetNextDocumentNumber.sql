-- Provera procedure GetNextDocumentNumber – greška "nvarchar to int" često
-- nastaje ovde (npr. brojač u tabeli tipa nvarchar koji se koristi u aritmetici ili CONVERT(int, ...)).
-- Pokreni u SSMS / Azure Data Studio.

-- 1) Definicija procedure
EXEC sp_helptext 'dbo.GetNextDocumentNumber';

-- 2) Ako procedura koristi tabelu brojača (npr. DocumentNumbers, Sequence, itd.),
--    proveri tipove kolona te tabele:
SELECT 
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME IN (
    SELECT OBJECT_NAME(referenced_id) 
    FROM sys.sql_expression_dependencies 
    WHERE referencing_id = OBJECT_ID('dbo.GetNextDocumentNumber')
)
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;

-- U proceduri traži: CONVERT(int, ...), CAST(... AS int), ili SET @Broj = @Broj + 1
-- ako je @Broj / kolona tipa nvarchar. Rešenje: kolona za broj treba da bude INT,
-- ili eksplicitno CONVERT(int, RTRIM(NvarcharKolona)) uz proveru da vrednost jeste broj.
