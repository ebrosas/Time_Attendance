USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorAttendance_V2]    Script Date: 15/07/2018 10:42:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ContractorAttendance_V2
*	Description: Get the attendance records of all Contractors
*
*	Date:			Author:		Rev. #:		Comments:
*	13/10/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_ContractorAttendance_V2]
AS
	
	SELECT	
		CONVERT(INT, a.EmpNo) AS EmpNo,
		RTRIM(a.FName) + ' ' + RTRIM(a.LName) AS EmpName,
		b.CostCenter,
		ISNULL(c.BusinessUnitName, a.Department) AS CostCenterName,
		RTRIM(a.CPR) AS CPRNo,
		a.JobTitle,
		RTRIM(a.ContractCompany) AS EmployerName,
		
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= a.DateStop AND a.DateStop IS NOT NULL
			THEN 0
			ELSE 1
		END AS StatusID,
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= a.DateStop AND a.DateStop IS NOT NULL
			THEN 'Inactive'
			ELSE 'Active'
		END AS StatusDesc,
		NULL AS ContractorTypeID,
		NULL AS ContractorTypeDesc,
		a.DateStart AS IDStartDate,
		a.DateStop AS IDEndDate,
		a.DateStart AS ContractStartDate,
		a.DateStop AS ContractEndDate,
		CONVERT(FLOAT, ISNULL(a.Nhrs, 0)) * 60 AS RequiredWorkDuration,
		a.PrintDate AS CreatedDate,
		NULL AS CreatedByNo,
		NULL AS CreatedByName			
	FROM tas.sy_PrintedCards a
		LEFT JOIN tas.AccessSystemCostCenterMapping b ON CONVERT(SMALLINT, a.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.CostCenter)) = LTRIM(RTRIM(c.BusinessUnit))
		

/*	Debugging:

	SELECT * FROM tas.Vw_ContractorAttendance_V2 a
	WHERE a.EmpNo IN (58803, 57773, 50573, 55943)

*/
GO


