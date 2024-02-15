USE [tas2]
GO

/****** Object:  View [tas].[Vw_ContractorAttendance_V2]    Script Date: 25/11/2021 11:37:19 ******/
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
*	16/06/2021		Ervin		1.4			Added join to "Master_BusinessUnit_JDE_view" view
*	22/06/2021		Ervin		1.5			Modified the "sy_Vw_LIC_PrintedCards" view from the backend to fetch the card start and expiry date information
********************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_ContractorAttendance_V2]
AS
	
	SELECT	
		CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END AS EmpNo,
		RTRIM(e.FName) + ' ' + RTRIM(e.LName) AS EmpName,
		b.CostCenter,
		ISNULL(c.BUname, e.Department) AS CostCenterName,
		RTRIM(e.CPR) AS CPRNo,
		e.JobTitle,
		RTRIM(e.ContractCompany) AS EmployerName,
		
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= e.DateStop AND e.DateStop IS NOT NULL
			THEN 0
			ELSE 1
		END AS StatusID,
		CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)) >= e.DateStop AND e.DateStop IS NOT NULL
			THEN 'Inactive'
			ELSE 'Active'
		END AS StatusDesc,
		NULL AS ContractorTypeID,
		NULL AS ContractorTypeDesc,
		e.DateStart AS IDStartDate,
		e.DateStop AS IDEndDate,
		e.DateStart AS ContractStartDate,
		e.DateStop AS ContractEndDate,
		CONVERT(FLOAT, ISNULL(e.Nhrs, 0)) * 60 AS RequiredWorkDuration,
		e.PrintDate AS CreatedDate,
		NULL AS CreatedByName,
		NULL AS CreatedByNo	
	FROM 
		(
			SELECT DISTINCT EmpNo	
			FROM tas.sy_PrintedCards WITH (NOLOCK)
		) a
		CROSS APPLY	
		(
			SELECT TOP 1 FName, LName, CostCode, PrintDate, DateStart, DateStop, Department, ContractCompany, CPR, JobTitle, Nhrs 
			FROM tas.sy_PrintedCards WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				--AND (DateStop IS NOT NULL AND DateStop > CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12)))
			ORDER BY PrintDate DESC
		) e
		LEFT JOIN tas.AccessSystemCostCenterMapping b WITH (NOLOCK) ON CONVERT(SMALLINT, e.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON LTRIM(RTRIM(b.CostCenter)) = RTRIM(LTRIM(c.BU))	

GO


