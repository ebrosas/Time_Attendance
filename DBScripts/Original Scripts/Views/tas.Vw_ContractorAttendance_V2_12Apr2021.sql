USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorAttendance_V2]    Script Date: 12/04/2021 11:54:21 ******/
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
*	08/05/2019		Ervin		1.2			Refactored the code to enhance data retrieval performance
*	16/07/2019		Ervin		1.3			Added join to "tas.sy_Vw_LIC_PrintedCards" view
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
		ISNULL(e.IssueDate, d.StartDate) AS IDStartDate,
		ISNULL(e.ExpiryDate, d.StopDate) AS IDEndDate,
		ISNULL(e.IssueDate, d.StartDate) AS ContractStartDate,
		ISNULL(e.ExpiryDate, d.StopDate) AS ContractEndDate,		
		CONVERT(FLOAT, ISNULL(a.Nhrs, 0)) * 60 AS RequiredWorkDuration,
		ISNULL(e.PrintDate, a.PrintDate) AS CreatedDate,
		RTRIM(e.IssuedBy) AS CreatedByName,		
		NULL AS CreatedByNo	
	FROM tas.sy_PrintedCards a WITH (NOLOCK)
		LEFT JOIN tas.AccessSystemCostCenterMapping b WITH (NOLOCK) ON CONVERT(SMALLINT, a.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE c WITH (NOLOCK) ON LTRIM(RTRIM(b.CostCenter)) = LTRIM(RTRIM(c.BusinessUnit))
		LEFT JOIN tas.sy_CardDates d WITH (NOLOCK) ON CAST(a.EmpNo AS INT) = CAST(d.FName AS INT)		--Rev. #1.1
		LEFT JOIN tas.sy_Vw_LIC_PrintedCards e WITH (NOLOCK) ON a.EmpNo = e.EmpNo	--Rev. #1.3
		

/*	Debugging:

	SELECT * FROM tas.Vw_ContractorAttendance_V2 a
	WHERE a.EmpNo IN (56527)

*/
GO

