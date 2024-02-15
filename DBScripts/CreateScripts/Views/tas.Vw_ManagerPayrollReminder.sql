/*******************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ManagerPayrollReminder
*	Description: This view returns the list of all people who shal receive the HR reminder notification
*
*	Date:			Author:		Rev. #:		Comments:
*	13/04/2021		Ervin		1.0			Created
********************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ManagerPayrollReminder
AS
	
	SELECT	a.EmpNo,
			a.EmpName,
			a.Position,
			a.BusinessUnit,
			a.GradeCode,
			a.EmpEmail 
	FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
	WHERE (a.GradeCode >= 12 AND a.GradeCode < 15)
		AND ISNUMERIC(a.PayStatus) = 1
		AND a.DateResigned IS NULL 
		AND a.EmpNo = 10001668

GO 

/*	Debug:

	SELECT * FROM tas.Vw_ManagerPayrollReminder a
	ORDER BY a.GradeCode, a.EmpNo

*/