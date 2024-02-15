/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCanAccessDependentInfo
*	Description: This function is used to check if an employee is allowed to view the Dependent Information tab in the "Employee Self Service" form
*
*	Date			Author		Rev. #		Comments:
*	15/08/2018		Ervin		1.0			Created
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnCanAccessDependentInfo 
(
	@empNo	INT
)
RETURNS @rtnTable 
TABLE 
(
	IsAllowedAccess BIT
) 
AS
BEGIN

    DECLARE @myTable TABLE 
	(
		IsAllowedAccess	BIT
	)
    
	DECLARE	@isAllowedAccess BIT 
	SET @isAllowedAccess = 0

	IF EXISTS
    (
		SELECT DISTINCT EmpNo
		FROM 
		(
			--Get all HR employees
			SELECT a.EmpNo 
			FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
			WHERE ISNUMERIC(a.PayStatus) = 1
				AND RTRIM(a.BusinessUnit) = '7500'

			UNION
    
			--Get all managers (Grade 12 and above)
			SELECT a.EmpNo 
			FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
			WHERE ISNUMERIC(a.PayStatus) = 1
				AND a.GradeCode >= 12
		) a
		WHERE a.EmpNo = @empNo
	)
	SET @isAllowedAccess = 1

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@isAllowedAccess
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END

/*	Debug:

	SELECT * FROM tas.fnCanAccessDependentInfo(10001988) 

*/
