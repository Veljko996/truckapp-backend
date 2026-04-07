-- ============================================================
-- 06: GetDashboardMonthlyProfit — dodavanje @TenantId filtera
-- ============================================================

ALTER PROCEDURE [dbo].[GetDashboardMonthlyProfit]
    @MonthsBack INT = 12,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @MonthsBack IS NULL OR @MonthsBack < 1 SET @MonthsBack = 12;
    IF @MonthsBack > 60 SET @MonthsBack = 60;

    DECLARE @CurrentMonthStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @FromMonthStart DATE = DATEADD(MONTH, 1 - @MonthsBack, @CurrentMonthStart);
    DECLARE @ToMonthStart DATE = DATEADD(MONTH, 1, @CurrentMonthStart);

    ;WITH Months AS
    (
        SELECT 0 AS StepNo, @CurrentMonthStart AS MonthStart
        UNION ALL
        SELECT StepNo + 1, DATEADD(MONTH, -(StepNo + 1), @CurrentMonthStart)
        FROM Months
        WHERE StepNo + 1 < @MonthsBack
    ),
    ProfitByMonth AS
    (
        SELECT
            MonthStart = DATEFROMPARTS(
                YEAR(COALESCE(n.FinishedAt, n.CreatedAt)),
                MONTH(COALESCE(n.FinishedAt, n.CreatedAt)),
                1
            ),
            ProfitEUR = SUM(CASE
                WHEN t.Valuta = 'EUR' THEN ISNULL(t.IzlaznaCena, 0) - ISNULL(t.UlaznaCena, 0)
                ELSE 0 END),
            ProfitRSD = SUM(CASE
                WHEN t.Valuta = 'RSD' OR t.Valuta IS NULL THEN ISNULL(t.IzlaznaCena, 0) - ISNULL(t.UlaznaCena, 0)
                ELSE 0 END)
        FROM dbo.Nalozi n
        INNER JOIN dbo.Ture t ON t.TuraId = n.TuraId
        INNER JOIN dbo.Prevoznici p ON p.PrevoznikId = t.PrevoznikId
        WHERE n.StatusNaloga = N'Završen'
          AND p.Interni = 0
          AND COALESCE(n.FinishedAt, n.CreatedAt) >= @FromMonthStart
          AND COALESCE(n.FinishedAt, n.CreatedAt) < @ToMonthStart
          AND n.TenantId = @TenantId
        GROUP BY DATEFROMPARTS(
            YEAR(COALESCE(n.FinishedAt, n.CreatedAt)),
            MONTH(COALESCE(n.FinishedAt, n.CreatedAt)),
            1
        )
    )
    SELECT
        [Year] = YEAR(m.MonthStart),
        [Month] = MONTH(m.MonthStart),
        ProfitEUR = CAST(ISNULL(p.ProfitEUR, 0) AS DECIMAL(18, 2)),
        ProfitRSD = CAST(ISNULL(p.ProfitRSD, 0) AS DECIMAL(18, 2))
    FROM Months m
    LEFT JOIN ProfitByMonth p ON p.MonthStart = m.MonthStart
    ORDER BY m.MonthStart
    OPTION (MAXRECURSION 60);
END
GO
