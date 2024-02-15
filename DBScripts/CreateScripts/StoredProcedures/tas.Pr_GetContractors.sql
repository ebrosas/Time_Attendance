/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractors
*	Description: Get the list of contractors
*
*	Date			Author		Revision No.	Comments:
*	14/07/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContractors
(   
	@empNo		INT = 0,
	@empName	VARCHAR(40) = NULL
)
AS

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@empName, '') = ''
		SET @empName = NULL
		 
	SELECT	a.AutoID,
			a.EmpNo,
			a.ContractorEmpName,
			a.GroupCode,
			a.ContractorNumber,
			a.DateJoined,
			a.DateResigned,
			a.ShiftPatCode,
			a.ShiftPointer,
			a.ReligionCode,
			a.LastUpdateUser,
			a.LastUpdateTime
	FROM tas.Master_ContractEmployee a 
	WHERE (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (UPPER(RTRIM(a.ContractorEmpName)) LIKE '%' + RTRIM(@empName) + '%' OR @empName IS NULL) 
	ORDER BY a.EmpNo

GO 

/*	Debugging:

	EXEC tas.Pr_GetContractors
	EXEC tas.Pr_GetContractors 10014
	EXEC tas.Pr_GetContractors 0, 'talib'

*/


