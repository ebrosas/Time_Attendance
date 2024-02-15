/*******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpWithUnentitledDayoff
*	Description: Retrieve the list of employees with unentitled dayoff
*
*	Date:			Author:		Rev. #:		Comments:
*	28/04/2019		Ervin		1.0			Created
*******************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmpWithUnentitledDayoff
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SELECT b.ActualCostCenter, a.EmpNo, b.EmpName, d.DayOffArray, d.LeaveType
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND ISNUMERIC(b.PayStatus) = 1 AND b.DateResigned IS NULL
		INNER JOIN tas.Tran_ShiftPatternUpdates c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT = c.DateX
		CROSS APPLY tas.fnCheckIfEntitledtoDayoff(a.EmpNo, a.DT) d
	WHERE a.IsLastRow = 1
		AND RTRIM(c.Effective_ShiftCode) = 'O'
		AND ISNULL(d.DayOffArray, '') <> ''
		AND NOT EXISTS
        (
			SELECT 1 FROM tas.DayOffUnpaidLeaveLog WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND DT = a.DT
		)
		AND NOT 
		(
			ISNULL(a.CorrectionCode, '') <> '' AND SUBSTRING(RTRIM(a.CorrectionCode), 0, 2) <> 'RA'
		)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		AND a.DT BETWEEN @startDate AND @endDate
	ORDER BY b.ActualCostCenter, a.EmpNo, a.DT 



GO 

/*	Debug:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT,
	@costCenter		VARCHAR(12)

	--Live database
	EXEC tas.Pr_GetEmpWithUnentitledDayoff '08/16/2020', '09/15/2020', 0, '' 
	EXEC tas.Pr_GetEmpWithUnentitledDayoff '07/16/2020', '08/15/2020', 0, '' 

*/