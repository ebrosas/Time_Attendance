/****************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCanAccessEmergencyContact
*	Description: This function is used to check if an employee is allowed to access the "Employee Emergency Contact" form
*
*	Date			Author		Rev. #		Comments:
*	15/08/2018		Ervin		1.0			Created
*	21/11/2018		Ervin		1.1			Modified the list of allowed people who can access the form
*	03/01/2019		Ervin		1.2			As per Helpdesk No. 98197, access ti the Emergency Contact form in TAS should be given to all HR and Medical Services employees only. The rest will be commented. 
****************************************************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCanAccessEmergencyContact 
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
		SELECT DISTINCT DistMemEmpNo
		FROM 
		(
			--HR employees who has full access to all OT forms
			--SELECT DistMemEmpNo 
			--FROM tas.syJDE_DistributionMember a WITH (NOLOCK)
			--	INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.DistMemEmpNo = b.EmpNo 
			--	LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			--WHERE DistMemDistListID = 
			--(
			--	SELECT TOP 1 DistListID 
			--	FROM tas.syJDE_DistributionList
			--	WHERE UPPER(RTRIM(DistListCode)) = 'OTHRADMIN'		--Rev. #1.1
			--) 

			--Get all employees from HR and Medical departments
			SELECT a.EmpNo AS DistMemEmpNo
			FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
			WHERE ISNUMERIC(a.PayStatus) = 1
				AND RTRIM(a.BusinessUnit) IN ('7500', '7250')

			/******************************************** Start of Rev. #1.2 ************************************************/
			/*
			UNION

			--Get the Shift Supervisors and Safety Officers that belong to Safety & Security departments
			SELECT DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a WITH (NOLOCK)
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList WITH (NOLOCK)
				WHERE UPPER(RTRIM(DistListCode)) = 'SAFETYEMER'		
			) 

			UNION
    
			--Get all managers
			SELECT a.EmpNo AS DistMemEmpNo
			FROM tas.Master_Employee_JDE_View_V2 a WITH (NOLOCK)
			WHERE ISNUMERIC(a.PayStatus) = 1
				AND a.GradeCode >= 12

			UNION
    
			--Get all Shift Superintendents
			SELECT DistMemEmpNo 
			FROM tas.syJDE_DistributionMember a WITH (NOLOCK)
				INNER JOIN tas.Master_Employee_JDE_View b WITH (NOLOCK) ON a.DistMemEmpNo = b.EmpNo 
				LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON RTRIM(LTRIM(b.BusinessUnit)) = RTRIM(LTRIM(c.BusinessUnit))
			WHERE DistMemDistListID = 
			(
				SELECT TOP 1 DistListID 
				FROM tas.syJDE_DistributionList WITH (NOLOCK)
				WHERE UPPER(RTRIM(DistListCode)) = 'SHIFTMANGR'		
			) 
			*/
			/********************************************* End of Rev. #1.2 ***********************************************/
		) a
		WHERE a.DistMemEmpNo = @empNo
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

	SELECT * FROM tas.fnCanAccessEmergencyContact(10003632) 

*/
