/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetOTActualHourByCostCenter
*	Description: This function is used to calculate the total actual overtime work hours based on cost center
*
*	Date			Author		Rev. #		Comments:
*	14/03/2018		Ervin		1.0			Created
*	18/03/2018		Ervin		1.1			Commented the ff line of code: RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetOTActualHourByCostCenter 
(
	@fiscalYear		INT,
	@costCenter		VARCHAR(12)
)
RETURNS @rtnTable 
TABLE 
(
	FiscalYear			INT,
	CostCenter			VARCHAR(12),
	TotalActualOTHour	FLOAT
) 
AS
BEGIN

	DECLARE	@totalActualOTHour		FLOAT,
			@janActual				FLOAT,
			@febActual				FLOAT,
			@marActual				FLOAT,
			@aprActual				FLOAT,
			@mayActual				FLOAT,
			@junActual				FLOAT,
			@julActual				FLOAT,
			@augActual				FLOAT,
			@sepActual				FLOAT,
			@octActual				FLOAT,
			@novActual				FLOAT,
			@decActual				FLOAT

	--Initialize variables
	SELECT	@totalActualOTHour		= 0,
			@janActual				= 0,
			@febActual				= 0,
			@marActual				= 0,
			@aprActual				= 0,
			@mayActual				= 0,
			@junActual				= 0,
			@julActual				= 0,
			@augActual				= 0,
			@sepActual				= 0,
			@octActual				= 0,
			@novActual				= 0,
			@decActual				= 0

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL 

	--Populate the actual values per each month
	SELECT @janActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 1
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 1
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 1
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @febActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 2
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 2
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 2
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @marActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 3
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 3
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 3
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @aprActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 4
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 4
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 4
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @mayActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 5
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 5
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 5
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @junActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 6
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 6
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 6
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @julActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 7
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 7
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 7
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @augActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 8
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 8
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 8
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @sepActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 9
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 9
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 9
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @octActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 10
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 10
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 10
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @novActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 11
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 11
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 11
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a

	SELECT @decActual = SUM(OTDuration) 
	FROM
    (
		--Get the already paid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.Tran_Timesheet a
		WHERE 
			--a.IsLastRow = 1 AND 
			(a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
			AND MONTH(a.DT) = 12
			AND YEAR(a.DT) = @fiscalYear
			AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)

		UNION
        
		--Get the unpaid overtime
		SELECT SUM(CAST(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) / 60 AS FLOAT)) AS OTDuration
		FROM tas.OvertimeRequest a
		WHERE 
			--RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled') 
			RTRIM(a.StatusHandlingCode) = 'Open'
			AND a.OTApproved = 'Y'
			AND MONTH(a.DT) = 12
			AND YEAR(a.DT) = @fiscalYear
			AND a.TS_AutoID NOT IN
			(
				SELECT AutoID
				FROM tas.Tran_Timesheet 
				WHERE 
					--IsLastRow = 1 AND 
					(OTStartTime IS NOT NULL AND OTEndTime IS NOT NULL)
					AND MONTH(DT) = 12
					AND YEAR(DT) = @fiscalYear
					AND (RTRIM(BusinessUnit) = @costCenter OR @costCenter IS NULL)
			)
			AND (RTRIM(a.CostCenter) = @costCenter OR @costCenter IS NULL)
	) a		

	SELECT	@totalActualOTHour = ISNULL(@janActual, 0) + ISNULL(@febActual, 0) + ISNULL(@marActual, 0) + ISNULL(@aprActual, 0) + ISNULL(@mayActual, 0) + ISNULL(@junActual, 0) + ISNULL(@julActual, 0) + ISNULL(@augActual, 0) + ISNULL(@sepActual, 0) + ISNULL(@octActual, 0) + ISNULL(@novActual, 0) + ISNULL(@decActual, 0)

	--Populate data to the table
	INSERT INTO @rtnTable  
	SELECT	@fiscalYear,	
			@costCenter,
			@totalActualOTHour

	RETURN 

END

/*	Debug:

PARAMETERS:
	@fiscalYear		INT,
	@costCenter		VARCHAR(12)

	SELECT * FROM tas.fnGetOTActualHourByCostCenter(2018, '')
	SELECT * FROM tas.fnGetOTActualHourByCostCenter(2018, '2110')

*/
