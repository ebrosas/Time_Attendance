/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeAbsences
*	Description: Retrieve the employee absence records
*
*	Date:			Author:		Rev.#		Comments:
*	07/08/2018		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeAbsences
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0
)
AS
	
	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 
    
	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	a.AutoID,
			a.BusinessUnit,
			RTRIM(c.BUname) AS BusinessUnitName,
			a.EmpNo,
			b.EmpName,
			b.Position,
			b.GradeCode AS PayGrade,
			b.SupervisorNo,
			LTRIM(RTRIM(e.YAALPH)) AS SupervisorName,
			a.DT,
			a.ShiftPatCode,
			a.ShiftCode,
			'Absent' as Remarks,
			'EmployeeStatus' = CASE WHEN d.YAPAST IN ('R', 'T', 'E', 'X') and GETDATE() < tas.ConvertFromJulian(d.YADT) THEN '0' ELSE d.YAPAST END
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(c.BU)
		INNER JOIN tas.syJDE_F060116 d WITH (NOLOCK) ON a.EmpNo = d.YAAN8
		LEFT JOIN tas.syJDE_F060116 e WITH (NOLOCK) ON b.SupervisorNo = e.YAAN8
	WHERE 
		ISNULL(a.RemarkCode, '') = 'A' 				
		AND ISNULL(IsLastRow, 0) = 1			
		AND ISNUMERIC(CASE WHEN d.YAPAST IN ('R', 'T', 'E', 'X') and GETDATE() < tas.ConvertFromJulian(d.YADT) THEN '0' ELSE d.YAPAST END) = 1
		AND a.DT BETWEEN @startDate AND @endDate
		AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY b.BusinessUnit, a.EmpNo, a.DT

GO

/*	Debugging:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0
		
	EXEC tas.Pr_GetEmployeeAbsences '01/01/2018', '12/31/2018'
	EXEC tas.Pr_GetEmployeeAbsences '07/16/2018', '08/15/2018'

*/

