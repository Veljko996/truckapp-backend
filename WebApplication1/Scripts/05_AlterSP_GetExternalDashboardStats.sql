-- ============================================================
-- 05: GetExternalDashboardStats — dodavanje @TenantId filtera
-- ============================================================

ALTER PROCEDURE [dbo].[GetExternalDashboardStats]
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = GETDATE();
    DECLARE @TodayStart DATETIME2(0) = DATEADD(DAY, DATEDIFF(DAY, 0, @Now), 0);
    DECLARE @TomorrowStart DATETIME2(0) = DATEADD(DAY, 1, @TodayStart);
    DECLARE @WeekStart DATETIME2(0) = DATEADD(DAY, -(DATEDIFF(DAY, '19000101', @TodayStart) % 7), @TodayStart);
    DECLARE @MonthStart DATETIME2(0) = DATEFROMPARTS(YEAR(@Now), MONTH(@Now), 1);

    ;WITH Base AS
    (
        SELECT
            StatusNaloga = LTRIM(RTRIM(ISNULL(n.StatusNaloga, N''))) COLLATE Latin1_General_100_CI_AI,
            n.CreatedAt,
            n.FinishedAt,
            IzlaznaCena = ISNULL(t.IzlaznaCena, 0),
            UlaznaCena   = ISNULL(t.UlaznaCena, 0),
            Valuta = CASE
                        WHEN t.Valuta IS NULL OR LTRIM(RTRIM(t.Valuta)) = '' THEN 'RSD'
                        ELSE UPPER(LTRIM(RTRIM(t.Valuta)))
                     END
        FROM dbo.Nalozi n
        INNER JOIN dbo.Prevoznici p ON p.PrevoznikId = n.PrevoznikId
        INNER JOIN dbo.Ture t ON t.TuraId = n.TuraId
        WHERE p.Interni = 0
          AND n.TenantId = @TenantId
    )
    SELECT
        UkupanBrojNaloga = COUNT(1),

        UkupanBrojIzvrsenihNaloga = SUM(CASE
            WHEN StatusNaloga = N'Zavrsen' THEN 1 ELSE 0 END),

        BrojAktivnihNaloga = SUM(CASE
            WHEN StatusNaloga NOT IN (N'Zavrsen', N'Ponisten', N'Storniran') THEN 1 ELSE 0 END),

        BrojNalogaDanas = SUM(CASE
            WHEN CreatedAt >= @TodayStart AND CreatedAt < @TomorrowStart THEN 1 ELSE 0 END),

        BrojNalogaNedelja = SUM(CASE
            WHEN CreatedAt >= @WeekStart AND CreatedAt < @TomorrowStart THEN 1 ELSE 0 END),

        BrojNalogaMesec = SUM(CASE
            WHEN CreatedAt >= @MonthStart AND CreatedAt < @TomorrowStart THEN 1 ELSE 0 END),

        ProfitDanasEUR = CAST(SUM(CASE
            WHEN StatusNaloga = N'Zavrsen'
             AND Valuta = 'EUR'
             AND FinishedAt >= @TodayStart
             AND FinishedAt < @TomorrowStart
            THEN IzlaznaCena - UlaznaCena ELSE 0 END) AS DECIMAL(18,2)),

        ProfitDanasRSD = CAST(SUM(CASE
            WHEN StatusNaloga = N'Zavrsen'
             AND Valuta = 'RSD'
             AND FinishedAt >= @TodayStart
             AND FinishedAt < @TomorrowStart
            THEN IzlaznaCena - UlaznaCena ELSE 0 END) AS DECIMAL(18,2)),

        ProfitNedeljaEUR = CAST(SUM(CASE
            WHEN StatusNaloga = N'Zavrsen'
             AND Valuta = 'EUR'
             AND FinishedAt >= @WeekStart
             AND FinishedAt < @TomorrowStart
            THEN IzlaznaCena - UlaznaCena ELSE 0 END) AS DECIMAL(18,2)),

        ProfitNedeljaRSD = CAST(SUM(CASE
            WHEN StatusNaloga = N'Zavrsen'
             AND Valuta = 'RSD'
             AND FinishedAt >= @WeekStart
             AND FinishedAt < @TomorrowStart
            THEN IzlaznaCena - UlaznaCena ELSE 0 END) AS DECIMAL(18,2)),

        ProfitMesecEUR = CAST(SUM(CASE
            WHEN StatusNaloga = N'Zavrsen'
             AND Valuta = 'EUR'
             AND FinishedAt >= @MonthStart
             AND FinishedAt < @TomorrowStart
            THEN IzlaznaCena - UlaznaCena ELSE 0 END) AS DECIMAL(18,2)),

        ProfitMesecRSD = CAST(SUM(CASE
            WHEN StatusNaloga = N'Zavrsen'
             AND Valuta = 'RSD'
             AND FinishedAt >= @MonthStart
             AND FinishedAt < @TomorrowStart
            THEN IzlaznaCena - UlaznaCena ELSE 0 END) AS DECIMAL(18,2))

    FROM Base;
END
GO
