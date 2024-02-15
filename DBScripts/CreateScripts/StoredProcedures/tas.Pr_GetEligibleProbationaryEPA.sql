/***********************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEligibleProbationaryEPA
*	Description: This stored procedure is used to fetch Probationary employees who are eligible for appraisal
*
*	Date:			Author:		Rev.#:		Comments:
*	18/08/2016		Ervin		1.0			Created

**********************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEligibleProbationaryEPA	
(
	@processDate	DATETIME
)
AS
	
	SELECT	tblA.*,		
			LTRIM(RTRIM(tblC.ABALPH)) AS SupervisorSubstituteName,
			LTRIM(RTRIM(ISNULL(tblB.EAEMAL, ''))) AS SupervisorSubstituteEmail 
	FROM
	(
		SELECT	a.EmpNo, 
				a.EmpName,
				a.Position,
				a.PayGrade,
				a.PayStatus,
				a.CostCenter,
				a.ActualCostCenter,
				a.Company,
				a.SupervisorNo,
				a.SupervisorName,
				a.SupervisorEmail,
				a.ManagerNo,
				a.ManagerName,
				a.ManagerEmail,							
				a.EmpJoinDate,
				DATEDIFF(DAY, a.EmpJoinDate, @processDate) AS DaysOfService,
				CASE WHEN ISNULL(b.EmpNo, 0) > 0 THEN 1 ELSE 0 END AS IsSupervisorOnLeave,
				CASE WHEN ISNULL(b.EmpNo, 0) > 0
					THEN tas.fnGetEmpOnLeaveSubstitute(a.SupervisorNo, RTRIM(a.CostCenter), 'WFEPA') 
					ELSE 0
				END AS SupervisorSubstituteNo			
		FROM tas.Vw_ProbationaryEmployee a
			LEFT JOIN tas.Vw_EmployeeAvailability b ON a.SupervisorNo = b.EmpNo AND CONVERT(DATETIME, @processDate, 101) BETWEEN FromDate AND ToDate	
		WHERE ISNUMERIC(a.PayStatus) = 1
			AND a.PayStatus BETWEEN 0 AND 7
			AND DATEDIFF(DAY, a.EmpJoinDate, @processDate) BETWEEN 60 AND 90
			AND a.ActualCostCenter <> '7920'
			AND a.PayGrade > 0
	) tblA
	LEFT JOIN tas.syJDE_F01151 tblB ON tblA.SupervisorSubstituteNo = tblB.EAAN8 AND tblB.EAIDLN = 0 AND tblB.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(tblB.EAETP))) = 'E' 
	LEFT JOIN tas.syJDE_F0101 tblC ON tblA.SupervisorSubstituteNo = tblC.ABAN8 

GO 

/*	Debugging:

	EXEC tas.Pr_GetEligibleProbationaryEPA '07/18/2016'	
	EXEC tas.Pr_GetEligibleProbationaryEPA '08/18/2016'	

*/
		