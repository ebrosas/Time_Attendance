/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetCostCenterManager
*	Description: Retrieves the Superintendent and Cost Center Managers for the specified department
*
*	Date			Author		Revision No.	Comments:
*	08/06/2016		Ervin		1.0				Created
*	02/01/2017		Ervin		1.1				Modified the filter clause to exclude in the query results those cost centers without assigned Manager
*	05/01/2017		Ervin		1.2				Added cost centers that belong to '00333' '00777' companies
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetCostCenterManager
(   
	@costCenter		VARCHAR(12) = NULL,
	@companyCode	VARCHAR(10) = NULL
)
AS

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@companyCode, '') = ''
		SET @companyCode = NULL

	SELECT	LTRIM(RTRIM(a.MCCO)) AS CompanyCode,
			CASE WHEN LTRIM(RTRIM(a.MCCO)) = '00100' 
				THEN 'GARMCO' 
				WHEN LTRIM(RTRIM(a.MCCO)) = '00600' 
				THEN 'Foil Mill' 
				ELSE '' 
			END AS CompanyName,
			a.MCMCU AS CostCenter,
			a.MCDL01 AS CostCenterName,
			a.MCAN8 AS SuperintendentEmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS SuperintendentEmpName,
			a.MCANPA AS ManagerEmpNo,
			LTRIM(RTRIM(c.YAALPH)) AS ManagerEmpName
	FROM tas.syJDE_F0006 a
		LEFT JOIN tas.syJDE_F060116 b ON a.MCAN8 = b.YAAN8
		LEFT JOIN tas.syJDE_F060116 c ON a.MCANPA = c.YAAN8
	WHERE 
		a.MCSTYL IN ('*', ' ', 'BP') 
		AND 
		(
			(LTRIM(RTRIM(a.MCCO)) = @companyCode AND @companyCode IS NOT NULL)
			OR
            (LTRIM(RTRIM(a.MCCO))IN ('00000', '00100', '00600', '00333', '00777') AND @companyCode IS NULL)
		)
		AND (LTRIM(RTRIM(a.MCMCU)) = @costCenter OR @costCenter IS NULL)
		AND ISNULL(a.MCANPA, 0) > 0
	ORDER BY LTRIM(RTRIM(a.MCCO)), LTRIM(RTRIM(a.MCMCU))

GO 

/*	Debugging:

PARAMETERS:
	@costCenter		VARCHAR(12) = NULL,
	@companyCode	VARCHAR(10) = NULL

	EXEC tas.Pr_GetCostCenterManager
	EXEC tas.Pr_GetCostCenterManager '5300'  
	EXEC tas.Pr_GetCostCenterManager '', '00100'  

*/


