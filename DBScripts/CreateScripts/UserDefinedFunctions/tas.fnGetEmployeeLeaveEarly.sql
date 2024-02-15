/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetEmployeeLeaveEarly
*	Description: This user-defined function is used to fetch employees who leave early at work
*
*	Date:			Author:		Rev.#:		Comments:
*	16/07/2017		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetEmployeeLeaveEarly
(
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenter				VARCHAR(12),
	@occurenceLimit			INT,
	@lateMinuteThreshold	INT 
)
RETURNS @rtnTable
TABLE	
(	
	CostCenter VARCHAR(12),
	EmpNo INT NOT NULL,
	Occurence INT  
)
AS
BEGIN

	DECLARE @myTable TABLE 
	(		
		CostCenter VARCHAR(12),
		EmpNo INT,
		Occurence INT 
	)

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@occurenceLimit, 0) = 0
		SET @occurenceLimit = 3			--(Note: Default number of occurence is 3)

	IF ISNULL(@lateMinuteThreshold, 0) = 0
		SET @lateMinuteThreshold = 5	--(Note: Default number of minutes is 5)
		
	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	A.CostCenter,
			A.EmpNo,
			COUNT(A.EmpNo) AS Occurence
	FROM
    (
		SELECT	a.EmpNo,
				RTRIM(a.BusinessUnit) AS CostCenter,
				a.ShiftPatCode,
				ISNULL(a.Actual_ShiftCode, a.ShiftPatCode) AS ShiftCode,
				a.DT,
				c.dtIN AS FirstTimeIn,
				a.dtOUT AS LastTimeOut,
				CASE WHEN CAST(d.DepartFrom AS TIME) > CAST(d.ArrivalTo AS TIME)
					THEN DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
					ELSE 1440 + DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
				END AS Duration_Required,
				CASE WHEN e.SettingID IS NOT NULL
					THEN DATEADD
						(
							MINUTE, 
							CASE WHEN CAST(d.DepartFrom AS TIME) > CAST(d.ArrivalTo AS TIME)
								THEN DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
								ELSE 1440 + DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
							END, 
							c.dtIN
						)
					ELSE d.DepartFrom
				END AS Required_TimeOut
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			CROSS APPLY
			(
				SELECT TOP 1 dtIN
				FROM tas.Tran_Timesheet 
				WHERE DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL 
					AND EmpNo = a.EmpNo
			) c
			LEFT JOIN tas.Master_ShiftTimes d ON RTRIM(a.ShiftPatCode) = RTRIM(d.ShiftPatCode) AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(d.ShiftCode)
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
		WHERE 
			a.dtIN IS NOT NULL 
			AND a.dtOUT IS NOT NULL 
			AND ISNULL(a.LeaveType, '') = ''
			AND ISNULL(a.AbsenceReasonCode, '') = ''
			AND ISNULL(a.DIL_Entitlement, '') = ''
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND a.ShiftCode <> 'O'		
			AND ISNUMERIC(b.PayStatus) = 1
			AND a.IsLastRow = 1
			AND CONVERT(TIME, A.dtOUT) < CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
															THEN DATEADD
																(
																	MINUTE, 
																	CASE WHEN CAST(d.DepartFrom AS TIME) > CAST(d.ArrivalTo AS TIME)
																		THEN DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
																		ELSE 1440 + DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
																	END, 
																	c.dtIN
																)
															ELSE d.DepartFrom
														END)
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
	) A
	GROUP BY A.CostCenter, A.EmpNo 
	HAVING (COUNT(A.EmpNo) >= @occurenceLimit)

	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 
	ORDER BY CostCenter, EmpNo

	RETURN 
END 

/*	Debugging:

PARAMETERS:
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenter				VARCHAR(12),
	@occurenceLimit			INT,
	@lateMinuteThreshold	INT 

	SELECT * FROM tas.fnGetEmployeeLeaveEarly('01/01/2016', '07/01/2016', '7700', 0, 0)		--By cost center
	SELECT * FROM tas.fnGetEmployeeLeaveEarly('07/02/2017', '07/08/2017', '5300', 2)		--Filtered by cost center

*/