USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetCostCenter_V2]    Script Date: 16/07/2020 13:37:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetCostCenter_V2
*	Description: Get all cost centers 
*
*	Date			Author		Revision No.	Comments:
*	05/01/2017		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetCostCenter_V2]
(   
	@loadType	TINYINT = 0
)
AS
	
	IF @loadType = 0	--Get all cost center where Manager is defined
	BEGIN
    
		SELECT	a.CompanyCode,
				a.BusinessUnit, 
				a.BusinessUnitName,
				a.ParentBU,
				a.Superintendent,
				a.CostCenterManager 
		FROM tas.Master_BusinessUnit_JDE_V2 a
		WHERE 
			ISNULL(a.CostCenterManager, 0) > 0
		ORDER BY a.CompanyCode, a.BusinessUnit
	END
    
	ELSE IF @loadType = 1	--Get cost centers with matching record in "Master_Employee_JDE" view
	BEGIN
    
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
		ORDER BY a.CompanyCode, a.BusinessUnit
	END

