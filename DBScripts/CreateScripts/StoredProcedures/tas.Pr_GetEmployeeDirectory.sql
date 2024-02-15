/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeDirectory
*	Description: Get the data for the Weekly Overtime Report
*
*	Date			Author		Rev. #		Comments:
*	28/11/2016		Ervin		1.0			Created
*	13/08/2018		Ervin		1.1			Added WITH (NOLOCK) clause to enhance data retrieval performance
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeDirectory
(   		
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = '',
	@searchString	VARCHAR(100) = ''
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	IF ISNULL(@empNo, 0) = 0 
		SET @empNo = NULL 

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@searchString, '') = ''
		SET @searchString = NULL
	
	SELECT	a.EmpNo,
			a.EmpName,
			a.Position,
			a.SupervisorNo,
			RTRIM(c.EmpName) AS SupervisorName,
			a.BusinessUnit,
			b.MCDC AS BusinessUnitName,
			a.Religion,
			a.JobCategory,
			a.Sex,
			a.GradeCode,
			a.PayStatus,
			a.DateJoined,
			a.YearsOfService,
			a.DateOfBirth,
			a.Age,
			a.TelephoneExt,
			a.MobileNo,
			a.TelNo,
			a.FaxNo,
			a.EmpEmail
	FROM tas.Vw_EmployeeDirectory a WITH (NOLOCK)
		LEFT JOIN tas.syJDE_F0006 b WITH (NOLOCK) ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(b.MCMCU))
		LEFT JOIN tas.Master_Employee_JDE_View c WITH (NOLOCK) ON a.SupervisorNo = c.EmpNo
	WHERE 
		a.EmpNo > 10000000
		AND ISNUMERIC(a.PayStatus) = 1
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND
        (
			UPPER(RTRIM(a.EmpName)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR
			UPPER(RTRIM(a.JobCategory)) LIKE UPPER(RTRIM(@searchString)) + '%'
			OR
			UPPER(RTRIM(a.Religion)) LIKE UPPER(RTRIM(@searchString)) + '%'
			OR
			UPPER(RTRIM(a.Sex)) LIKE UPPER(RTRIM(@searchString)) + '%'
			OR
			UPPER(RTRIM(a.Position)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR
            UPPER(RTRIM(a.TelephoneExt)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR
            UPPER(RTRIM(a.MobileNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR
            UPPER(RTRIM(a.TelNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR
            UPPER(RTRIM(a.FaxNo)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR
            UPPER(RTRIM(a.EmpEmail)) LIKE '%' + UPPER(RTRIM(@searchString)) + '%'
			OR @searchString IS NULL
		)
	ORDER BY a.EmpNo

GO 

/*	Debug:

PARAMETERS:
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = '',
	@searchString	VARCHAR(100) = ''

	EXEC tas.Pr_GetEmployeeDirectory
	EXEC tas.Pr_GetEmployeeDirectory 10003632
	EXEC tas.Pr_GetEmployeeDirectory 0, '7600'
	EXEC tas.Pr_GetEmployeeDirectory 0, '', 'salary'


*/