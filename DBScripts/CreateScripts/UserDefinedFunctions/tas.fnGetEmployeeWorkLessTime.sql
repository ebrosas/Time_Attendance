/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetEmployeeWorkLessTime
*	Description: Converts minute input value into hours with the following format: HH:mm
*
*	Date:			Author:		Rev.#:		Comments:
*	10/07/2017		Ervin		1.0			Created
*******************************************************************************************************************************************************/

--CREATE FUNCTION tas.fnGetEmployeeWorkLessTime
--(
--	@startDate				DATETIME,
--	@endDate				DATETIME,
--	@costCenter				VARCHAR(12),
--	@occurenceLimit			INT,
--	@lateMinuteThreshold	INT 
--)
--RETURNS @rtnTable
--TABLE	
--(	
--	CostCenter VARCHAR(12),
--	EmpNo INT NOT NULL,
--	Occurence INT  
--)
--AS
--BEGIN

--	DECLARE @myTable TABLE 
--	(		
--		CostCenter VARCHAR(12),
--		EmpNo INT,
--		Occurence INT 
--	)
		
	SELECT	SUM(A.Duration_Worked_Diff) AS TotalMissingWork,
			A.EmpNo
	FROM
	(
		SELECT 
			CASE WHEN CAST(c.DepartFrom AS TIME) > CAST(c.ArrivalTo AS TIME)
				THEN DATEDIFF(MINUTE, c.ArrivalTo, c.DepartFrom)
				ELSE 1440 + DATEDIFF(MINUTE, c.ArrivalTo, c.DepartFrom)
			END AS Duration_Required,
			a.Duration_Worked_Cumulative,
			CASE WHEN CAST(c.DepartFrom AS TIME) > CAST(c.ArrivalTo AS TIME)
				THEN DATEDIFF(MINUTE, c.ArrivalTo, c.DepartFrom)
				ELSE 1440 + DATEDIFF(MINUTE, c.ArrivalTo, c.DepartFrom)
			END - a.Duration_Worked_Cumulative AS Duration_Worked_Diff,
			a.EmpNo
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			INNER JOIN tas.Master_ShiftTimes c ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(c.ShiftCode)
		WHERE 
			a.dtIN IS NOT NULL 
			AND a.dtOUT IS NOT NULL 
			AND ISNULL(a.LeaveType, '') = ''
			AND ISNULL(a.AbsenceReasonCode, '') = ''
			AND ISNULL(a.DIL_Entitlement, '') = ''
			AND a.ShiftCode <> 'O'
			--AND a.EmpNo > 10000000
			AND ISNUMERIC(b.PayStatus) = 1
			AND a.IsLastRow = 1
			AND a.Duration_Worked_Cumulative < 
				CASE WHEN CAST(c.DepartFrom AS TIME) > CAST(c.ArrivalTo AS TIME)
					THEN DATEDIFF(MINUTE, c.ArrivalTo, c.DepartFrom)
					ELSE 1440 + DATEDIFF(MINUTE, c.ArrivalTo, c.DepartFrom)
				END
			AND a.DT BETWEEN '01/01/2016' AND '07/01/2016'
			AND RTRIM(a.BusinessUnit) = '7600'
		--ORDER BY a.EmpNo, a.DT
	) A
	--WHERE SUM(A.Duration_Worked_Diff) > 120
	GROUP BY A.EmpNo
