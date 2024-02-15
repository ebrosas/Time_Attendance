/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsAllowedToViewOTBudget
*	Description: This function is used to check if an employee has the authority to view the overtime budget statistics
*
*	Date			Author		Rev. #		Comments:
*	08/03/2018		Ervin		1.0			Created
*	12/03/2018		Ervin		1.1			Changed the distribution code from "OTBUDGETVW" to "OTBUDGTADM"
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnIsAllowedToViewOTBudget 
(
	@empNo	INT
)
RETURNS @rtnTable 
TABLE 
(
	CanViewOTBudget	BIT
) 
AS
BEGIN

    DECLARE @myTable TABLE 
	(
		CanViewOTBudget	BIT
	)
    
	DECLARE	@canViewOTBudget BIT 
	SET @canViewOTBudget = 0

	IF EXISTS
    (
		SELECT DISTINCT DistMemEmpNo
		FROM 
		(
			--Get the employees who are allowed to view the overtime budget
			SELECT DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList
				WHERE UPPER(RTRIM(DistListCode)) = 'OTBUDGTADM'		--Rev. #1.1
			) 
		) a
		WHERE a.DistMemEmpNo = @empNo
	)
	SET @canViewOTBudget = 1

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@canViewOTBudget
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END


/*	Debugging:

	SELECT * FROM tas.fnIsAllowedToViewOTBudget(10003191)	

*/