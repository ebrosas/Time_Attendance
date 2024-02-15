/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeEmailAddress
*	Description: This stored procedure is used to get the email address of the employee from JDE
*
*	Date			Author		Revision No.	Comments:
*	02/08/2017		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeEmailAddress
(   
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''
)
AS
	
	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SELECT	a.EmpNo,
			a.EmpName,
			a.Position,
			a.EmpEmail 
	FROM tas.Master_Employee_JDE_View_V2 a
	WHERE ISNUMERIC(a.PayStatus) = 1
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
	ORDER BY a.BusinessUnit, a.EmpNo

	

GO

/*	Debug:

	EXEC tas.Pr_GetEmployeeEmailAddress 10002162
	EXEC tas.Pr_GetEmployeeEmailAddress 0, '7600'

*/