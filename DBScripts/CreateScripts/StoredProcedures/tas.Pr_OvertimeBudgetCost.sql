/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_OvertimeBudgetCost
*	Description: This stored procedure is used to fetch overtime budget allocated to each fiscal year
*
*	Date:			Author:		Rev. #:		Comments:
*	07/03/2018		Ervin		1.0			Created
*****************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_OvertimeBudgetCost
(	
	@loadType			TINYINT,	
	@fiscalYear			INT = 0,
	@costCenter			VARCHAR(12) = NULL
)
AS

	--Validate the parameters
	IF ISNULL(@fiscalYear, 0) = 0
		SET @fiscalYear = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @loadType = 0	--Get overtime budget amount by month
	BEGIN
		
		SELECT	a.FiscalYear,
				SUM(a.January) AS JanBudget,
				SUM(a.February) AS FebBudget,
				SUM(a.March) AS MarBudget,
				SUM(a.April) AS AprBudget,
				SUM(a.May) AS MayBudget,
				SUM(a.June) AS JunBudget,
				SUM(a.July) AS JulBudget,
				SUM(a.August) AS AugBudget,
				SUM(a.September) AS SepBudget,
				SUM(a.October) AS OctBudget,
				SUM(a.November) AS NovBudget,
				SUM(a.December)  AS DecBudget
		FROM tas.Vw_OTBudgetAmount a
		WHERE (a.FiscalYear = @fiscalYear OR @fiscalYear IS NULL)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.FiscalYear
		ORDER BY a.FiscalYear
	END 

	ELSE IF @loadType = 1	--Get overtime actual amount by month
	BEGIN
		
		SELECT	a.FiscalYear,
				SUM(a.January) AS JanActual,
				SUM(a.February) AS FebActual,
				SUM(a.March) AS MarActual,
				SUM(a.April) AS AprActual,
				SUM(a.May) AS MayActual,
				SUM(a.June) AS JunActual,
				SUM(a.July) AS JulActual,
				SUM(a.August) AS AugActual,
				SUM(a.September) AS SepActual,
				SUM(a.October) AS OctActual,
				SUM(a.November) AS NovActual,
				SUM(a.December)  AS DecActual
		FROM tas.Vw_OTActualAmount a
		WHERE (a.FiscalYear = @fiscalYear OR @fiscalYear IS NULL)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.FiscalYear
		ORDER BY a.FiscalYear
	END 

	ELSE IF @loadType = 2	--Get total overtime budget amount
	BEGIN
		
		SELECT	a.FiscalYear,
				SUM(a.January) + SUM(a.February) + SUM(a.March) + SUM(a.April) + SUM(a.May) + SUM(a.June) + SUM(a.July) + SUM(a.August) + SUM(a.September) + SUM(a.October) + SUM(a.November) + SUM(a.December) AS TotalBudgetAmount
		FROM tas.Vw_OTBudgetAmount a
		WHERE (a.FiscalYear = @fiscalYear OR @fiscalYear IS NULL)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.FiscalYear
		ORDER BY a.FiscalYear
	END 

	ELSE IF @loadType = 3	--Get total overtime actual amount
	BEGIN
		
		SELECT	a.FiscalYear,
				SUM(a.January) + SUM(a.February) + SUM(a.March) + SUM(a.April) + SUM(a.May) + SUM(a.June) + SUM(a.July) + SUM(a.August) + SUM(a.September) + SUM(a.October) + SUM(a.November) + SUM(a.December) AS TotalActualAmount
		FROM tas.Vw_OTActualAmount a
		WHERE (a.FiscalYear = @fiscalYear OR @fiscalYear IS NULL)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.FiscalYear
		ORDER BY a.FiscalYear
	END 

	ELSE IF @loadType = 4	--Get total overtime budget and actual amount
	BEGIN
		
		DECLARE	@totalBudgetAmount	FLOAT,
				@totalActualAmount	FLOAT
		
		SELECT @totalBudgetAmount = SUM(a.January) + SUM(a.February) + SUM(a.March) + SUM(a.April) + SUM(a.May) + SUM(a.June) + SUM(a.July) + SUM(a.August) + SUM(a.September) + SUM(a.October) + SUM(a.November) + SUM(a.December) 
		FROM tas.Vw_OTBudgetAmount a
		WHERE a.FiscalYear = @fiscalYear 
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.FiscalYear

		SELECT @totalActualAmount = SUM(a.January) + SUM(a.February) + SUM(a.March) + SUM(a.April) + SUM(a.May) + SUM(a.June) + SUM(a.July) + SUM(a.August) + SUM(a.September) + SUM(a.October) + SUM(a.November) + SUM(a.December) 
		FROM tas.Vw_OTActualAmount a
		WHERE a.FiscalYear = @fiscalYear 
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		GROUP BY a.FiscalYear

		SELECT	@fiscalYear AS FiscalYear,
				ISNULL(@totalBudgetAmount, 0) AS TotalBudgetAmount,
				ISNULL(@totalActualAmount, 0) AS TotalActualAmount,
				ISNULL(@totalBudgetAmount, 0) - ISNULL(@totalActualAmount, 0) AS TotalBalanceAmount
	END 

	ELSE IF @loadType = 5	--Get distinct fiscal years
	BEGIN
		
		SELECT	DISTINCT a.FiscalYear
		FROM tas.Vw_OTBudgetAmount a
		WHERE (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
		ORDER BY a.FiscalYear DESC
	END 

	ELSE IF @loadType = 6	--Get total overtime budget amount grouped by cost center
	BEGIN
		
		SELECT	a.FiscalYear,
				a.CostCenter,
				SUM(a.January) + SUM(a.February) + SUM(a.March) + SUM(a.April) + SUM(a.May) + SUM(a.June) + SUM(a.July) + SUM(a.August) + SUM(a.September) + SUM(a.October) + SUM(a.November) + SUM(a.December) AS TotalBudgetAmount
		FROM tas.Vw_OTBudgetAmount a
		WHERE (a.FiscalYear = @fiscalYear OR @fiscalYear IS NULL)			
		GROUP BY a.FiscalYear, a.CostCenter
		ORDER BY a.FiscalYear, a.CostCenter
	END 

	ELSE IF @loadType = 7	--Get total overtime actual amount grouped by cost center
	BEGIN
		
		SELECT	a.FiscalYear,
				a.CostCenter,
				SUM(a.January) + SUM(a.February) + SUM(a.March) + SUM(a.April) + SUM(a.May) + SUM(a.June) + SUM(a.July) + SUM(a.August) + SUM(a.September) + SUM(a.October) + SUM(a.November) + SUM(a.December) AS TotalActualAmount
		FROM tas.Vw_OTActualAmount a
		WHERE (a.FiscalYear = @fiscalYear OR @fiscalYear IS NULL)			
		GROUP BY a.FiscalYear, a.CostCenter
		ORDER BY a.FiscalYear, a.CostCenter
	END 

	ELSE IF @loadType = 8	--Get distinct cost center by fiscal year
	BEGIN
		
		SELECT	DISTINCT a.FiscalYear, a.CostCenter
		FROM tas.Vw_OTBudgetAmount a
		WHERE a.FiscalYear = @fiscalYear
		ORDER BY a.CostCenter 
	END 

GO

/*	Debug:

PARAMETERS:
	@loadType			TINYINT,	
	@fiscalYear			INT = 0,
	@costCenter			VARCHAR(12) = NULL

	EXEC tas.Pr_OvertimeBudgetCost 0, 2018		--Budget by month
	EXEC tas.Pr_OvertimeBudgetCost 1, 2018		--Actuals by month

	EXEC tas.Pr_OvertimeBudgetCost 8, 2018		--Get all cost center
	EXEC tas.Pr_OvertimeBudgetCost 6, 2018		--Budget by cost center
	EXEC tas.Pr_OvertimeBudgetCost 7, 2018		--Actuals by cost center

	EXEC tas.Pr_OvertimeBudgetCost 0, 0
	EXEC tas.Pr_OvertimeBudgetCost 1, 0

	EXEC tas.Pr_OvertimeBudgetCost 2, 2018
	EXEC tas.Pr_OvertimeBudgetCost 3, 2018
	EXEC tas.Pr_OvertimeBudgetCost 4, 2018
	EXEC tas.Pr_OvertimeBudgetCost 5
	EXEC tas.Pr_OvertimeBudgetCost 5, 0, '2110'
	
	EXEC tas.Pr_OvertimeBudgetCost 0, 2018, '2110'
	EXEC tas.Pr_OvertimeBudgetCost 1, 2018, '2110'

*/

