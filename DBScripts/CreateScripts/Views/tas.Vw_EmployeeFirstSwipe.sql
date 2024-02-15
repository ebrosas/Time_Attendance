/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EmployeeFirstSwipe
*	Description: Get the employee first swipe attendance record
*
*	Date:			Author:		Rev. #:		Comments:
*	04/12/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_EmployeeFirstSwipe
AS
	
	SELECT	a.EmpNo,
			a.DT,
			a.BusinessUnit,
			CONVERT(TIME, MIN(a.dtIN)) AS dtIN,
			a.RemarkCode,
			a.CorrectionCode
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE d ON a.EmpNo = d.EmpNo AND d.DateResigned IS NULL AND d.GradeCode > 0
	GROUP BY 
		a.EmpNo, 
		a.DT, 
		a.BusinessUnit,
		a.RemarkCode,
		a.CorrectionCode

GO 

/*	Testing:

	SELECT * FROM tas.Vw_EmployeeFirstSwipe a
	WHERE a.EmpNo = 10003662 AND a.DT = '11/30/2016'

	SELECT * FROM tas.Vw_EmployeeFirstSwipe a
	WHERE a.DT = '11/30/2016'
		and a.BusinessUnit = '7600'

*/
