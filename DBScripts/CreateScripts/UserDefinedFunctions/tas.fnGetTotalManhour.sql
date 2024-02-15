/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetTotalManhour
*	Description: Get the total man-hour by pay period
*
*	Date:			Author:		Rev.#:		Comments:
*	16/06/2016		Ervin		1.0			Created
*	02/08/2016		Ervin		1.1			Refactored the code to check for LTI record when calculating the total man-hour
*	10/11/2016		Ervin		1.2			Added filter condition to exclude Visitors and Guest in the manhour count
*	05/03/2017		Ervin		1.3			Commented filter condition to exclude visitors and guests in the man-hour count
*******************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetTotalManhour
(
	@startDate		DATETIME,
	@endDate		DATETIME
)
RETURNS FLOAT
AS
BEGIN

	DECLARE @totalManhour FLOAT

	--Check if there is LTI record within the date range
	--IF EXISTS
 --   (
	--	SELECT a.LogID FROM tas.ManhourHistory a
	--	WHERE a.StartDate = @startDate
	--		AND a.EndDate = @endDate
	--		AND a.IsLTI = 1
	--)
	--BEGIN

	--	--Set @startDate equal to the MIR Report Date plus 1 day
	--	SELECT @startDate = DATEADD(DAY, 1, a.EndDate)
	--	FROM tas.ManhourHistory a
	--	WHERE a.StartDate = @startDate
	--		AND a.EndDate = @endDate
	--		AND a.IsLTI = 1
 --   END 

	SELECT @totalManhour = SUM(TotalHoursWorked) 
	FROM
    (
		SELECT	DISTINCT
				A.BusinessUnit, 
				A.BusinessUnitName, 
				A.EmpNo, 
				A.EmpName, 
				ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) AS TotalHoursWorked,
				ShiftPatCode,
				GroupCode 
		FROM
		(					
			SELECT 
				a.BusinessUnit, 
				c.BusinessUnitName, 
				a.EmpNo, 
				CASE WHEN a.EmpNo > 10000000
					THEN b.EmpName
					ELSE d.ContractorEmpName
				END AS EmpName, 
				a.GradeCode, 
				a.Duration_Worked_Cumulative, 
				a.Duration_Required,
				d.ShiftPatCode,
				d.GroupCode
			FROM tas.Tran_Timesheet a
				LEFT JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
				LEFT JOIN tas.Master_ContractEmployee d ON a.EmpNo = d.EmpNo AND UPPER(RTRIM(d.GroupCode)) <> 'V'
			WHERE 
				a.DT BETWEEN @startDate AND @endDate
				AND a.IsLastRow = 1
				AND a.Duration_Worked_Cumulative > 0
				AND ISNULL(a.LeaveType, '') NOT IN ('SLP', 'SLU', 'IL')		
				
				/* Start of Rev. #1.3 
				AND	--Rev. #1.2
				(
					a.EmpNo >= 50000
					AND
					NOT a.EmpNo BETWEEN 60000 AND 69999
				)
				End of Rev. #1.3	*/
		) A
		GROUP BY A.BusinessUnit, A.BusinessUnitName, A.EmpNo, A.EmpName, A.ShiftPatCode, A.GroupCode
		HAVING ROUND(CONVERT(FLOAT, SUM(A.Duration_Worked_Cumulative)) / 60, 0) > 0
			AND ISNULL(A.EmpNo, 0) > 0		
	) MainTable

	RETURN ISNULL(@totalManhour, 0)

END

/*	Testing:

PARAMETERS:
	@startDate			DATETIME,
	@endDate			DATETIME
	
	SELECT tas.fnGetTotalManhour('07/16/2016', '08/15/2016')
	SELECT tas.fnGetTotalManhour('07/16/2016', '07/28/2016')
	SELECT tas.fnGetTotalManhour('07/30/2016', '08/15/2016')
	SELECT tas.fnGetTotalManhour('02/16/2017', '03/15/2017')

*/
