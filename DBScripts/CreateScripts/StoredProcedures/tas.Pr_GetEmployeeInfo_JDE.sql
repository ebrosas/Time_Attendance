/************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetVisitorPassLog
*	Description: Retrieves visitor's log records
*
*	Date:			Author:		Rev. #:			Comments:
*	14/04/2016		Ervin		1.0				Created
*	31/12/2016		Ervin		1.1				Added "Position" in the query results
************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeInfo_JDE
(	
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = NULL,
	@isActiveOnly	BIT = NULL   
)
AS

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL 

	IF ISNULL(@isActiveOnly, 0) = 0
		SET @isActiveOnly = NULL 

	SELECT	a.EmpNo,
			a.EmpName,
			a.ReligionCode,
			a.JobCategoryCode,
			a.SexCode,
			a.BusinessUnit AS CostCenter,
			RTRIM(b.BusinessUnitName) AS CostCenterName,
			a.Company,
			a.GradeCode,
			a.DateJoined,
			a.DateResigned,
			a.PayStatus,
			a.DateOfBirth,
			a.ActualCostCenter,
			a.YearsOfService,
			a.Position
	FROM tas.Master_Employee_JDE_View_V2 a
		LEFT JOIN tas.Master_BusinessUnit_JDE b ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
	WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND 
		(
			(ISNUMERIC(a.PayStatus) = 1 AND @isActiveOnly = 1)
			OR
            @isActiveOnly IS NULL   
		)

GO


/*	Debugging:
	
	EXEC tas.Pr_GetEmployeeInfo_JDE 10003632
	EXEC tas.Pr_GetEmployeeInfo_JDE 0, '', 1

*/