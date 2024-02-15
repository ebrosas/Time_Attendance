/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsTASAdministrator
*	Description: This function is used to check if an employee is a system administrator
*
*	Date			Author		Rev. #		Comments:
*	16/08/2018		Ervin		1.0			Created
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnIsTASAdministrator 
(
	@empNo	INT
)
RETURNS @rtnTable 
TABLE 
(
	IsAdmin	BIT
) 
AS
BEGIN

    DECLARE @myTable TABLE 
	(
		IsAdmin	BIT
	)
    
	DECLARE	@isAdmin BIT 
	SET @isAdmin = 0

	IF EXISTS
    (
		SELECT DISTINCT DistMemEmpNo
		FROM 
		(
			--ICT personnel who are members of TAS system administrators group
			SELECT DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList
				WHERE UPPER(RTRIM(DistListCode)) = 'TASADMIN'		
			) 

			UNION

			--HR employees who has full access in all forms in TAS
			SELECT DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList
				WHERE UPPER(RTRIM(DistListCode)) = 'OTHRADMIN'		
			) 
		) a
		WHERE a.DistMemEmpNo = @empNo
	)
	SET @isAdmin = 1

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@isAdmin
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END


/*	Debugging:

	SELECT * FROM tas.fnIsTASAdministrator(10003632)	

*/