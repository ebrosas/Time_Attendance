/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetFireTeamMember
*	Description: Get all Fire Team members
*
*	Date			Author		Revision No.	Comments:
*	15/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetFireTeamMember
AS

	SELECT	CONVERT(INT, a.FName) + 10000000 AS EmpNo, 
			RTRIM(a.LName) AS EmpName
	FROM tas.sy_FireTeamMembers a 
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON (CONVERT(INT, a.FName) + 10000000) = b.EmpNo
	WHERE 
		ISNUMERIC(b.PayStatus) = 1
	ORDER BY CONVERT(INT, a.FName) + 10000000

GO 

/*	Debugging:

	EXEC tas.Pr_GetFireTeamMember

	SELECT * FROM tas.sy_FireTeamMembers
	SELECT * FROM tas.Master_Employee_JDE_View_V2

*/


