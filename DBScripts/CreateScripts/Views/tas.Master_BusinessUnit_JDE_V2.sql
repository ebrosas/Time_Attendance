/************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Master_BusinessUnit_JDE_V2
*	Description: Retrieves cost center information from JDE
*
*	Date:			Author:		Rev. #:		Comments:
*	05/01/2017		Ervin		1.0			Created
*	27/07/2017		Ervin		1.1			Added "GroupCode" field
*	09/11/2020		Ervin		1.2			Removed Foil Mill cost centers
*********************************************************************************************************************************/

ALTER VIEW tas.Master_BusinessUnit_JDE_V2
AS

	SELECT     
		LTRIM(RTRIM(a.MCCO)) AS CompanyCode,
		LTRIM(RTRIM(a.MCMCU)) AS BusinessUnit, 
		LTRIM(RTRIM(a.MCDC)) AS BusinessUnitName, 
		LTRIM(RTRIM(a.MCRP21)) AS ParentBU, 
		LTRIM(RTRIM(a.MCRP22)) AS StopOT, 
		tas.ConvertFromJulian(MCD2J) AS StopOTFrom, 
		tas.ConvertFromJulian(MCD4J) AS StopOTTo, 
		LTRIM(RTRIM(a.MCRP23)) AS TSCorrectionSet,
		a.MCANPA AS CostCenterManager,
		a.MCAN8 AS Superintendent,
		LTRIM(RTRIM(a.MCRP02)) AS GroupCode
	FROM tas.syJDE_F0006 a
	WHERE   
		LTRIM(RTRIM(a.MCSTYL)) IN ('*', '', 'BP', 'DA') 
		AND LTRIM(RTRIM(a.MCCO)) IN ('00000', '00100', '00850', '00333', '00777')
		AND ISNUMERIC(MCMCU) = 1
GO

/*	Debug:

	SELECT * FROM tas.Master_BusinessUnit_JDE_V2 a
	ORDER BY a.CompanyCode, a.BusinessUnit

*/