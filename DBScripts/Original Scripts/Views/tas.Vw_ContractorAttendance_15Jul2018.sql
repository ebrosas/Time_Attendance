USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorAttendance]    Script Date: 15/07/2018 10:51:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ContractorAttendance
*	Description: Get the attendance records of all Contractors
*
*	Date:			Author:		Rev. #:		Comments:
*	07/09/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_ContractorAttendance]
AS
	
	SELECT	
		CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
		END AS EmpNo,
		RTRIM(a.LName) AS EmpName,
		c.CostCenter,
		ISNULL(d.BusinessUnitName, b.Department) AS CostCenterName,
		RTRIM(b.CPR) AS CPRNo,
		b.JobTitle,
		RTRIM(b.ContractCompany) AS EmployerName,
		
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= b.DateStop AND b.DateStop IS NOT NULL
			THEN 0
			ELSE 1
		END AS StatusID,
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= b.DateStop AND b.DateStop IS NOT NULL
			THEN 'Inactive'
			ELSE 'Active'
		END AS StatusDesc,
		NULL AS ContractorTypeID,
		NULL AS ContractorTypeDesc,
		b.DateStart AS IDStartDate,
		b.DateStop AS IDEndDate,
		b.DateStart AS ContractStartDate,
		b.DateStop AS ContractEndDate,
		CONVERT(FLOAT, ISNULL(b.Nhrs, 0)) * 60 AS RequiredWorkDuration,
		b.PrintDate AS CreatedDate,
		NULL AS CreatedByNo,
		NULL AS CreatedByName			
	FROM tas.sy_NAMES a
		INNER JOIN tas.sy_PrintedCards b ON CONVERT(INT, a.FName) = b.EmpNo
		LEFT JOIN tas.AccessSystemCostCenterMapping c ON CONVERT(SMALLINT, b.CostCode) = c.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE d ON LTRIM(RTRIM(c.CostCenter)) = LTRIM(RTRIM(d.BusinessUnit))
		

/*	Debugging:

	SELECT * FROM tas.Vw_ContractorAttendance a
	WHERE a.EmpNo IN (58803, 57773, 50573, 55943)

*/
GO


