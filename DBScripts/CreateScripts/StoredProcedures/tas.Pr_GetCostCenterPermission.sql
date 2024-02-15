/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetCostCenterPermission
*	Description: Get the cost center permissions given to a user
*
*	Date			Author		Rev. #		Comments:
*	18/01/2017		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetCostCenterPermission
(   		
	@loadType		TINYINT,	--(Note: 0 = Summary; 1 = Cost Center List)
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''
)
AS

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL


	IF @loadType = 0
	BEGIN
    
		SELECT	DISTINCT
				a.PermitEmpNo AS EmpNo,
				b.EmpName,
				RTRIM(b.BusinessUnit) AS CostCenter,
				RTRIM(c.BUname) AS CostCenterName
		FROM tas.syJDE_PermitCostCenter a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.PermitEmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON LTRIM(RTRIM(b.BusinessUnit)) = LTRIM(RTRIM(c.BU))
		WHERE 
			ISNUMERIC(b.PayStatus) = 1
			AND a.PermitAppID = (SELECT UDCID FROM tas.syJDE_UserDefinedCode WHERE UDCUDCGID = 17 AND RTRIM(UDCCode) = 'TAS3')
			AND (a.PermitEmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.PermitCostCenter) = @costCenter OR @costCenter IS NULL)
		ORDER BY a.PermitEmpNo

	END
    
	ELSE IF @loadType = 1
	BEGIN

		SELECT	a.PermitID,
				a.PermitEmpNo AS EmpNo,
				LTRIM(RTRIM(b.YAALPH)) AS EmpName,
				a.PermitCostCenter AS CostCenter,
				RTRIM(c.BUname) AS CostCenterName,
				d.UDCDesc1 AS ApplicationName,
				a.PermitCreatedBy AS CreatedByEmpNo,
				LTRIM(RTRIM(e.YAALPH)) AS CreatedByEmpName,
				a.PermitCreatedDate AS CreatedDate,
				a.PermitModifiedBy AS ModifiedByEmpNo,
				LTRIM(RTRIM(f.YAALPH)) AS ModifiedByEmpName,
				a.PermitModifiedDate AS ModifiedDate
		FROM tas.syJDE_PermitCostCenter a
			INNER JOIN tas.syJDE_F060116 b ON a.PermitEmpNo = CAST(b.YAAN8 AS INT)
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON LTRIM(RTRIM(a.PermitCostCenter)) = LTRIM(RTRIM(c.BU))
			INNER JOIN tas.syJDE_UserDefinedCode d ON a.PermitAppID = d.UDCID AND UDCUDCGID = 17 AND RTRIM(UDCCode) = 'TAS3'
			LEFT JOIN tas.syJDE_F060116 e ON a.PermitCreatedBy = CAST(e.YAAN8 AS INT)
			LEFT JOIN tas.syJDE_F060116 f ON a.PermitModifiedBy = CAST(f.YAAN8 AS INT)
		WHERE 
			(a.PermitEmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.PermitCostCenter) = @costCenter OR @costCenter IS NULL)
		ORDER BY a.PermitEmpNo, a.PermitCostCenter
    END 

GO 

/*	Debug:

PARAMETERS:
	@loadType		TINYINT,	--(Note: 0 = Summary; 1 = Cost Center List)
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = ''

	EXEC tas.Pr_GetCostCenterPermission 0
	EXEC tas.Pr_GetCostCenterPermission 1, 10003632			--By Emp. No.
	EXEC tas.Pr_GetCostCenterPermission 0, 0, '7500'		--By Cost Center

*/