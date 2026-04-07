-- ============================================================
-- 04: GetNextDocumentNumber — dodavanje @TenantId parametra
-- ============================================================
-- Brojac je sada per-tenant: svaka firma ima svoj niz brojeva.
-- ============================================================

ALTER PROCEDURE [dbo].[GetNextDocumentNumber]
    @DocumentType NVARCHAR(20),
    @TenantId INT,
    @Result NVARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Year SMALLINT = YEAR(GETUTCDATE());
    DECLARE @NextNumber INT;

    BEGIN TRAN;

    IF EXISTS (
        SELECT 1 FROM dbo.DocumentCounters
        WHERE DocumentType = @DocumentType AND [Year] = @Year AND TenantId = @TenantId
    )
    BEGIN
        UPDATE dbo.DocumentCounters
        SET LastNumber = LastNumber + 1
        WHERE DocumentType = @DocumentType AND [Year] = @Year AND TenantId = @TenantId;

        SELECT @NextNumber = LastNumber
        FROM dbo.DocumentCounters
        WHERE DocumentType = @DocumentType AND [Year] = @Year AND TenantId = @TenantId;
    END
    ELSE
    BEGIN
        SET @NextNumber = 1;
        INSERT INTO dbo.DocumentCounters (DocumentType, [Year], LastNumber, TenantId)
        VALUES (@DocumentType, @Year, 1, @TenantId);
    END

    COMMIT;

    SET @Result =
        RIGHT('000' + CAST(@NextNumber AS VARCHAR(10)), 3)
        + '/'
        + RIGHT(CAST(@Year AS VARCHAR(4)), 2);
END
GO
