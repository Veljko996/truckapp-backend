-- ============================================================
-- 07: GetDashboardStats — dodavanje @TenantId filtera
-- ============================================================

ALTER PROCEDURE [dbo].[GetDashboardStats]
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = GETDATE();
    DECLARE @TodayStart DATETIME2(0) = DATEADD(DAY, DATEDIFF(DAY, 0, @Now), 0);
    DECLARE @TomorrowStart DATETIME2(0) = DATEADD(DAY, 1, @TodayStart);
    DECLARE @WeekStart DATETIME2(0) = DATEADD(DAY, - (DATEDIFF(DAY, '19000101', @TodayStart) % 7), @TodayStart);
    DECLARE @MonthStart DATETIME2(0) = DATEFROMPARTS(YEAR(@Now), MONTH(@Now), 1);

    ;WITH Base AS
    (
        SELECT
            NormalizedStatus = LTRIM(RTRIM(ISNULL(n.StatusNaloga, N''))) COLLATE Latin1_General_100_CI_AI,
            n.CreatedAt,
            EffectiveFinishedAt = COALESCE(n.FinishedAt, n.CreatedAt),
            t.IzlaznaCena,
            t.UlaznaCena,
            t.Valuta
        FROM dbo.Nalozi n
        INNER JOIN dbo.Ture t ON t.TuraId = n.TuraId
        WHERE n.TenantId = @TenantId
    )
    SELECT
        UkupanBrojNaloga = COUNT(1),
        UkupanBrojIzvrsenihNaloga = SUM(CASE WHEN NormalizedStatus = N'Zavrsen' THEN 1 ELSE 0 END),
        BrojAktivnihNaloga = SUM(CASE WHEN NormalizedStatus NOT IN (N'Zavrsen', N'Ponisten', N'Storniran') THEN 1 ELSE 0 END),

        BrojNalogaDanas = SUM(CASE
            WHEN CreatedAt >= @TodayStart AND CreatedAt < @TomorrowStart THEN 1 ELSE 0 END),
        BrojNalogaNedelja = SUM(CASE
            WHEN CreatedAt >= @WeekStart AND CreatedAt < @TomorrowStart THEN 1 ELSE 0 END),
        BrojNalogaMesec = SUM(CASE
            WHEN CreatedAt >= @MonthStart AND CreatedAt < @TomorrowStart THEN 1 ELSE 0 END),

        PrihodDanas = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND EffectiveFinishedAt >= @TodayStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2)),
        PrihodNedelja = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND EffectiveFinishedAt >= @WeekStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2)),
        PrihodMesec = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND EffectiveFinishedAt >= @MonthStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2)),

        ProfitNedeljaEUR = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND Valuta = 'EUR'
                 AND EffectiveFinishedAt >= @WeekStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) - ISNULL(UlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2)),
        ProfitMesecEUR = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND Valuta = 'EUR'
                 AND EffectiveFinishedAt >= @MonthStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) - ISNULL(UlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2)),
        ProfitNedeljaRSD = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND (Valuta = 'RSD' OR Valuta IS NULL)
                 AND EffectiveFinishedAt >= @WeekStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) - ISNULL(UlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2)),
        ProfitMesecRSD = CAST(SUM(CASE
            WHEN NormalizedStatus = N'Zavrsen'
                 AND (Valuta = 'RSD' OR Valuta IS NULL)
                 AND EffectiveFinishedAt >= @MonthStart AND EffectiveFinishedAt < @TomorrowStart
            THEN ISNULL(IzlaznaCena, 0) - ISNULL(UlaznaCena, 0) ELSE 0 END) AS DECIMAL(18, 2))
    FROM Base;
END
GO
