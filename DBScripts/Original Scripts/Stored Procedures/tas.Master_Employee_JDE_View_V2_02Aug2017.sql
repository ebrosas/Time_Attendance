USE [tas2]
GO

/****** Object:  View [tas].[Master_Employee_JDE_View_V2]    Script Date: 02/08/2017 09:47:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Master_Employee_JDE_View_V2
*	Description: Get the employee information from the Employee Master Table
*
*	Date:			Author:		Rev. #:		Comments:
*	04/04/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Master_Employee_JDE_View_V2]
AS

	SELECT     
		CAST(a.YAAN8 AS INT) AS EmpNo, 
		LTRIM(RTRIM(a.YAALPH)) AS EmpName, 
		LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS Position,
		LTRIM(RTRIM(a.YAEEOM)) AS ReligionCode, 
		LTRIM(RTRIM(a.YAEEOJ)) AS JobCategoryCode, 
		LTRIM(RTRIM(a.YASEX)) AS SexCode, 
		CASE WHEN ISNULL(b.WorkingBusinessUnit, '') <> ''
			THEN LTRIM(RTRIM(b.WorkingBusinessUnit))
			ELSE
				CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
					WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
				END
		END AS BusinessUnit,	
		LTRIM(RTRIM(a.YAHMCO)) AS Company, 
		CASE WHEN ISNUMERIC(ISNULL(a.YAPGRD, '0')) = 1 
			THEN CONVERT(INT, LTRIM(RTRIM(ISNULL(a.YAPGRD, '0')))) 
			ELSE 0 
		END AS GradeCode,
		CASE WHEN ISNULL(d.T3EFT, 0) = 0 
			THEN tas.ConvertFromJulian(ISNULL(a.YADST, 0)) 
			ELSE tas.ConvertFromJulian(d.T3EFT) 
		END AS DateJoined,
		tas.ConvertFromJulian(a.YADT) AS DateResigned,
		--a.YAPAST AS PayStatus,
		CASE WHEN (a.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(a.YADT)  OR UPPER(LTRIM(RTRIM(a.YAPAST))) = 'I') THEN '0' ELSE a.YAPAST END AS PayStatus,
		tas.ConvertFromJulian(a.YADOB) AS DateOfBirth,
		CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(a.YAHMCU))
			WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
		END AS ActualCostCenter,
		ROUND
		(
			CONVERT(FLOAT,
			DATEDIFF
			(
				MONTH, 
				CASE WHEN ISNULL(d.T3EFT, 0) = 0 
					THEN tas.ConvertFromJulian(ISNULL(a.YADST, 0)) 
					ELSE tas.ConvertFromJulian(d.T3EFT) 
				END,
				GETDATE() 
			)) 
		/ 12, 2) AS YearsOfService
	FROM tas.syJDE_F060116 a
		LEFT JOIN tas.Master_EmployeeAdditional b ON CAST(a.YAAN8 AS INT) = b.EmpNo
		LEFT JOIN tas.syJDE_F0101 c ON a.YAAN8 = c.ABAN8
		LEFT JOIN tas.syJDE_F00092 d ON a.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = 'WH' AND LTRIM(RTRIM(d.T3SDB)) = 'E'
		LEFT JOIN tas.syJDE_F08001 e ON LTRIM(RTRIM(a.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
	WHERE a.YAAN8 > 10000000


GO


