/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAssignedOvertimeRequest
*	Description: This stored procedure is used to fetch all cost centers where overtime is allowed
*
*	Date			Author		Revision No.	Comments:
*	28/08/2017		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetCostCenterOTAllowed
(
	@userEmpNo	INT 
)
AS
	
	SELECT	a.CompanyCode,
			a.BusinessUnit, 
			a.BusinessUnitName,
			a.ParentBU,
			a.Superintendent,
			a.CostCenterManager 
	FROM tas.Master_BusinessUnit_JDE_V2 a
	WHERE 
		LTRIM(RTRIM(a.BusinessUnit)) IN 
		(
			SELECT DISTINCT LTRIM(RTRIM(BusinessUnit)) 
			FROM tas.Master_Employee_JDE
		) 
		AND RTRIM(a.BusinessUnit) IN
		(
			SELECT RTRIM(LTRIM(MCMCU)) 
			FROM tas.syJDE_F0006
			WHERE   
				(MCSTYL IN ('*', ' ', 'BP', 'DA')) AND (MCCO IN ('00000', '00100', '00600'))
				AND ISNUMERIC(MCMCU) = 1
				AND (MCMCU BETWEEN 2110 AND 7910 OR MCMCU BETWEEN 6002000 AND 6007800)
				AND UPPER(RTRIM(ISNULL(MCRP06,''))) = 'Y'
		)
		AND RTRIM(a.BusinessUnit) IN 
		(
			SELECT RTRIM(PermitCostCenter) 
			FROM tas.syJDE_PermitCostCenter a
				INNER JOIN tas.syJDE_UserDefinedCode b on a.PermitAppID = b.UDCID
			WHERE RTRIM(b.UDCCode) = 'TAS3'
				AND PermitEmpNo = @userEmpNo
		)
	ORDER BY a.CompanyCode, a.BusinessUnit

GO 

/*	Debug:

	EXEC tas.Pr_GetCostCenterOTAllowed 10003653

*/