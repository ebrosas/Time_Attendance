/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpShiftPatternInfo_Excel
*	Description: Get the shift pattern information of an employee
*
*	Date			Author		Revision No.	Comments:
*	03/11/2016		Ervin		1.0			Created
*	12/03/2018		Ervin		1.1			Added filter to exclude employees who already resigned from the company
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmpShiftPatternInfo_Excel
(   
	@autoID			INT = 0,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''
)
AS

	--Validate parameters
	--Validate parameters
	IF ISNULL(@autoID, 0) = 0
		SET @autoID = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SELECT	a.AutoID,
			a.EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,
			LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS Position,
			a.ShiftPatCode,
			a.ShiftPointer,
			a.WorkingBusinessUnit,
			LTRIM(RTRIM(f.BUname)) AS WorkingBusinessUnitName, 
			CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
				WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
			END AS ParentCostCenter,
			a.SpecialJobCatg,
			a.LastUpdateUser,
			a.LastUpdateTime 
	FROM tas.Master_EmployeeAdditional a
		INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = CAST(b.YAAN8 AS INT) 
		LEFT JOIN tas.syJDE_F0101 c ON b.YAAN8 = c.ABAN8
		LEFT JOIN tas.syJDE_F00092 d ON b.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = 'WH' AND LTRIM(RTRIM(d.T3SDB)) = 'E'
		LEFT JOIN tas.syJDE_F08001 e on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
		LEFT JOIN tas.Master_BusinessUnit_JDE_view f ON RTRIM(a.WorkingBusinessUnit) = RTRIM(f.BU)
		INNER JOIN tas.Master_Employee_JDE g ON a.EmpNo = g.EmpNo	
	WHERE 
		ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END) = 1
		AND (a.AutoID = @autoID OR @autoID IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (tas.fnGetEmployeeCostCenter(a.EmpNo) = @costCenter OR @costCenter IS NULL)
		AND g.DateResigned IS NULL		--Rev. #1.1
	ORDER BY a.LastUpdateTime DESC, a.EmpNo


/*	Debugging:

PARAMETERS:
	@autoID			INT = 0,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''

	EXEC tas.Pr_GetEmpShiftPatternInfo_Excel 
	EXEC tas.Pr_GetEmpShiftPatternInfo_Excel 1840
	EXEC tas.Pr_GetEmpShiftPatternInfo_Excel 0, 10003632
	EXEC tas.Pr_GetEmpShiftPatternInfo_Excel 0, 0, '7600'

*/
