/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EmployeeAbsences
*	Description: Get the employee absences records
*
*	Date:			Author:		Rev. #:		Comments:
*	20/11/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_EmployeeAbsences
AS
	
	SELECT  c.DT,
			RTRIM(a.BusinessUnit) AS CostCenter,
			RTRIM(d.BusinessUnitName) AS CostCenterName,
			a.EmpNo,
			a.EmpName,
			a.GradeCode,
			c.RemarkCode,
			c.CorrectionCode,
			b.ShiftPatCode
			--e.Effective_ShiftPatCode,
			--e.Effective_ShiftCode
	FROM tas.Master_Employee_JDE a
		INNER JOIN tas.Master_EmployeeAdditional b ON a.EmpNo = b.EmpNo
		INNER JOIN tas.Tran_Timesheet c ON a.EmpNo = c.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE d ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(d.BusinessUnit))
		--INNER JOIN tas.Tran_ShiftPatternUpdates e ON b.EmpNo = e.EmpNo AND c.DT = e.DateX
	WHERE
		a.DateResigned IS NULL
		AND RTRIM(c.RemarkCode) = (SELECT RTRIM(Code_Remark_Absent) FROM tas.System_Values)
		AND ISNULL(c.CorrectionCode, '') = ''
		AND ISNULL(b.ShiftPatCode, '') <> ''
		AND NOT EXISTS(SELECT MappingID FROM tas.EmployeeContractorMapping WHERE EmpNo = a.EmpNo AND PrimaryIDNoType = 1)       

GO 

/*	Debug:

	--All
	SELECT * FROM tas.Vw_EmployeeAbsences a
	WHERE a.DT BETWEEN '01/01/2015' AND '31/05/2015'
	ORDER BY a.EmpNo, a.DT 

	--Non-Salary Staff
	SELECT * FROM tas.Vw_EmployeeAbsences a
	WHERE a.DT BETWEEN '01/01/2015' AND '31/05/2015'
		AND a.GradeCode <= 9
	ORDER BY a.EmpNo, a.DT 

	--Salary Staff
	SELECT * FROM tas.Vw_EmployeeAbsences a
	WHERE a.DT BETWEEN '01/01/2015' AND '31/05/2015'
		AND a.GradeCode >= 9
	ORDER BY a.EmpNo, a.DT 

*/