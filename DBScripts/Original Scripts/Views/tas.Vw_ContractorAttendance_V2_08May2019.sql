USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorAttendance_V2]    Script Date: 08/05/2019 12:56:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*******************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ContractorAttendance_V2
*	Description: Get the attendance records of all Contractors
*
*	Date:			Author:		Rev. #:		Comments:
*	13/10/2016		Ervin		1.0			Created
*	15/07/2018		Ervin		1.1			Modified the logic in fetching the start and end dates of the card. Added join to "tas.sy_CardDates"
********************************************************************************************************************************************************************/

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
		
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= d.StopDate AND d.StopDate IS NOT NULL
			THEN 0
			ELSE 1
		END AS StatusID,
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= d.StopDate AND d.StopDate IS NOT NULL
			THEN 'Inactive'
			ELSE 'Active'
		END AS StatusDesc,
		NULL AS ContractorTypeID,
		NULL AS ContractorTypeDesc,

		d.StartDate AS IDStartDate,
		d.StopDate AS IDEndDate,
		d.StartDate AS ContractStartDate,
		d.StopDate AS ContractEndDate,		
		CONVERT(FLOAT, ISNULL(a.Nhrs, 0)) * 60 AS RequiredWorkDuration,
		a.PrintDate AS CreatedDate,
		NULL AS CreatedByNo,
		NULL AS CreatedByName			
	FROM tas.sy_PrintedCards a
		LEFT JOIN tas.AccessSystemCostCenterMapping b ON CONVERT(SMALLINT, a.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.CostCenter)) = LTRIM(RTRIM(c.BusinessUnit))
		LEFT JOIN tas.sy_CardDates d ON CAST(a.EmpNo AS INT) = CAST(d.FName AS INT)		--Rev. #1.1
		

/*	Debugging:

	SELECT * FROM tas.Vw_ContractorAttendance_V2 a
	WHERE a.EmpNo IN (58803, 57773, 50573, 55943)

*/
GO


