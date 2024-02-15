USE [tas2]
GO

/****** Object:  View [tas].[Vw_SeniorSupervisor]    Script Date: 25/08/2020 11:40:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_SeniorSupervisor
*	Description: Get the list of all Senior Supervisors and Specialists
*
*	Date:			Author:		Rev. #:		Comments:
*	27/12/2017		Ervin		1.0			Created
*	21/11/2018		Ervin		1.1			Modified the WHERE clause for Specialist. Set comparison operator to ">=".
*	13/10/2019		Ervin		1.2			Added link to "F0005" in JDE to fetch the employee's job title
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_SeniorSupervisor]
AS		

	SELECT DISTINCT * FROM 
	(
		SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
				LTRIM(RTRIM(a.YAALPH)) AS EmpName,
				LTRIM(RTRIM(b.DRDL01)) + RTRIM(b.DRDL02) AS Position,
				CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
					WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
				END AS CostCenter,
				CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END AS PayStatus
		FROM tas.syJDE_F060116 a WITH (NOLOCK)
			LEFT JOIN tas.syJDE_F0005 b WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(b.DRKY)) AND RTRIM(LTRIM(b.DRSY)) = '06' AND RTRIM(LTRIM(b.DRRT)) = 'G'	
			LEFT JOIN tas.syJDE_F0101 c WITH (NOLOCK) ON a.YAAN8 = c.ABAN8			
		WHERE 
			UPPER(LTRIM(RTRIM(b.DRDL01)) + ' ' + LTRIM(RTRIM(b.DRDL02))) LIKE '%' + 'SUPERVISOR' + '%'
			AND ISNUMERIC(CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END) = 1
			AND CASE WHEN ISNUMERIC(ISNULL(a.YAPGRD, '0')) = 1 
				THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(a.YAPGRD, '0')))) 
				ELSE 0 
			END = 10

		UNION
    
		SELECT	CAST(a.YAAN8 AS INT) AS EmpNo, 
				LTRIM(RTRIM(a.YAALPH)) AS EmpName,
				LTRIM(RTRIM(b.DRDL01)) + ' ' + LTRIM(RTRIM(b.DRDL02)) AS Position,
				CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
					WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
				END AS CostCenter,
				CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END AS PayStatus
		FROM tas.syJDE_F060116 a WITH (NOLOCK)
			LEFT JOIN tas.syJDE_F0005 b WITH (NOLOCK) ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(b.DRKY)) AND RTRIM(LTRIM(b.DRSY)) = '06' AND RTRIM(LTRIM(b.DRRT)) = 'G'	
			LEFT JOIN tas.syJDE_F0101 c WITH (NOLOCK) ON a.YAAN8 = c.ABAN8
		WHERE 
			UPPER(LTRIM(RTRIM(b.DRDL01)) + ' ' + LTRIM(RTRIM(b.DRDL02))) LIKE '%' + 'SPECIALIST' + '%'
			AND ISNUMERIC(CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END) = 1
			AND CASE WHEN ISNUMERIC(ISNULL(a.YAPGRD, '0')) = 1 
					THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(a.YAPGRD, '0')))) 
					ELSE 0 
				END >= 10
	) a

GO


