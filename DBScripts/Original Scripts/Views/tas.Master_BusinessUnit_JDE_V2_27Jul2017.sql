USE [tas2]
GO

/****** Object:  View [tas].[Master_BusinessUnit_JDE_V2]    Script Date: 27/07/2017 09:32:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Master_BusinessUnit_JDE_V2
*	Description: Retrieves cost center information from JDE
*
*	Date:			Author:		Rev. #:		Comments:
*	05/01/2017		Ervin		1.0				Created
*********************************************************************************************************************************/

ALTER VIEW [tas].[Master_BusinessUnit_JDE_V2]
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
		a.MCAN8 AS Superintendent
	FROM tas.syJDE_F0006 a
	WHERE   
		LTRIM(RTRIM(a.MCSTYL)) IN ('*', '', 'BP', 'DA') 
		AND LTRIM(RTRIM(a.MCCO)) IN ('00000', '00100', '00600', '00850', '00333', '00777')
		AND ISNUMERIC(MCMCU) = 1

GO


