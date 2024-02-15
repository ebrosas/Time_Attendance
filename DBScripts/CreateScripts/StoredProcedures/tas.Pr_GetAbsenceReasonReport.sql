/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAbsenceReasonReport
*	Description: Get data for the Absence Reason report
*
*	Date			Author		Rev. #		Comments:
*	20/11/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAbsenceReasonReport
(   	
	@startDate		DATETIME,
	@endDate		DATETIME,
	@employeeType	TINYINT = 0,	--(Note: 0 = All; 1 = Non-Salary Staff; 2 = Salary Staff)
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0	
)
AS

	--Validate parameters
	IF ISNULL(@employeeType, 0) = 0
		SET @employeeType = 0

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL			

	SELECT a.* 
	FROM tas.Vw_EmployeeAbsences a
	WHERE 
		a.DT BETWEEN @startDate AND @endDate
		AND 
		(
			(a.GradeCode <= 9 AND @employeeType = 1)
			OR
            (a.GradeCode >= 9 AND @employeeType = 2)
			OR
            (@employeeType = 0)
		)
		--AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
		--AND (a.EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY a.EmpNo, a.DT 

GO 


/*	Debugging:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@employeeType	TINYINT = 0	--(Note: 0 = All; 1 = Non-Salary Staff; 2 = Salary Staff)
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0	

	EXEC tas.Pr_GetAbsenceReasonReport '16/02/2016', '15/03/2016'		--All
	EXEC tas.Pr_GetAbsenceReasonReport '16/02/2016', '15/03/2016', 1	--Non-salary Staff
	EXEC tas.Pr_GetAbsenceReasonReport '16/02/2016', '15/03/2016', 2	--Salary Staff

	EXEC tas.Pr_GetAbsenceReasonReport '11/16/2016', '12/15/2016', 0, '7600'		

*/
