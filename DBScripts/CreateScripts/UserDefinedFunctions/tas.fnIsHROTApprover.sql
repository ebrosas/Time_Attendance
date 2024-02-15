/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsHROTApprover
*	Description: This function is used to check if an employee is an HR approver or validator
*
*	Date			Author		Rev. #		Comments:
*	12/12/2017		Ervin		1.0			Created
*	09/05/2018		Ervin		1.1			Added "OTHRADMIN" distribution group members
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnIsHROTApprover 
(
	@empNo	INT
)
RETURNS @rtnTable 
TABLE 
(
	IsHRApprover	BIT
) 
AS
BEGIN

    DECLARE @myTable TABLE 
	(
		IsHRApprover	BIT
	)
    
	DECLARE	@isHRApprover BIT 
	SET @isHRApprover = 0

	IF EXISTS
    (
		SELECT DISTINCT DistMemEmpNo
		FROM 
		(
			--Get the HR Validator for OT and Meal Voucher Request
			SELECT TOP 1 DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList
				WHERE UPPER(RTRIM(DistListCode)) = 'OTHRVALIDR'
			) 

			UNION 

			--HR Final Approver for OT and Meal Voucher Request
			SELECT TOP 1 DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList
				WHERE UPPER(RTRIM(DistListCode)) = 'OTHRAPROVE'
			) 

			UNION

			--HR employees who has full access to all OT forms
			SELECT DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a
				INNER JOIN tas.Master_Employee_JDE_View b ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList
				WHERE UPPER(RTRIM(DistListCode)) = 'OTHRADMIN'		--Rev. #1.1
			) 
		) a
		WHERE a.DistMemEmpNo = @empNo
	)
	SET @isHRApprover = 1

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@isHRApprover
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END


/*	Debugging:

	SELECT * FROM tas.fnIsHROTApprover(10003656)	--OTHRVALIDR
	SELECT * FROM tas.fnIsHROTApprover(10003512)	--OTHRAPROVE
	SELECT * FROM tas.fnIsHROTApprover(10003632)	

*/