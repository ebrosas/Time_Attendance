/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetCostCenter
*	Description: Get all cost centers 
*
*	Date			Author		Revision No.	Comments:
*	20/09/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetCostCenter
(   
	@loadType	TINYINT = 0
)
AS
	
	IF @loadType = 0	--Get all cost center where Manager is defined
	BEGIN
    
		SELECT	a.BusinessUnit, 
				a.BusinessUnitName,
				a.ParentBU,
				a.Superintendent,
				a.CostCenterManager 
		FROM tas.Master_BusinessUnit_JDE a
		WHERE 
			ISNULL(a.CostCenterManager, 0) > 0
		ORDER BY a.BusinessUnit
	END
    
	ELSE IF @loadType = 1	--Get cost centers with matching record in "Master_Employee_JDE" view
	BEGIN
    
		SELECT	a.BusinessUnit, 
				a.BusinessUnitName,
				a.ParentBU,
				a.Superintendent,
				a.CostCenterManager
		FROM tas.Master_BusinessUnit_JDE a
		WHERE 
			LTRIM(RTRIM(a.BusinessUnit)) IN 
			(
				SELECT DISTINCT LTRIM(RTRIM(BusinessUnit)) 
				FROM tas.Master_Employee_JDE
			) 
		ORDER BY a.BusinessUnit
	END

GO

/*	Debug:

	EXEC tas.Pr_GetCostCenter
	EXEC tas.Pr_GetCostCenter 1

*/