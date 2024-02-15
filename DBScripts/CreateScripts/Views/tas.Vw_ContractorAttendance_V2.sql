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
*	25/11/2021		Ervin		1.6			Fetch contractor information registered through the new Contractor Management System
*	07/12/2021		Ervin		1.7			Added filter to return contractors only whose employee no. is between 5000 to 69999
********************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ContractorAttendance_V2
AS
	
	--Get contractors details from the old Access system
	SELECT	
		CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END AS EmpNo,
		ISNULL(e.FName, '') + ' ' + ISNULL(e.LName, '') AS EmpName,
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
			ORDER BY PrintDate DESC
		) e
		LEFT JOIN tas.AccessSystemCostCenterMapping b WITH (NOLOCK) ON CONVERT(SMALLINT, e.CostCode) = b.CompanyID
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON LTRIM(RTRIM(b.CostCenter)) = RTRIM(LTRIM(c.BU))	
	WHERE CASE WHEN ISNUMERIC(a.EmpNo) = 1 THEN CONVERT(INT, a.EmpNo) ELSE 0 END BETWEEN 50000 AND 69999	--Rev. #1.7

	UNION
    
	--Get contractor details from the new Contractor Management Syste
	SELECT	a.ContractorNo AS EmpNo,
			UPPER(RTRIM(a.FirstName)) + ' ' + UPPER(RTRIM(a.LastName)) AS EmpName,
			a.VisitedCostCenter AS CostCenter,
			RTRIM(c.BUname) AS CostCenterName,
			a.IDNumber AS CPRNo,
			d.JobTitleDesc AS JobTitle,
			a.CompanyName AS EmployerName,
			CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR(10), GETDATE(), 12)) BETWEEN a.ContractStartDate AND a.ContractEndDate THEN 1 ELSE 0 END AS StatusID,
			CASE WHEN CONVERT(DATETIME, CONVERT(VARCHAR(10), GETDATE(), 12)) BETWEEN a.ContractStartDate AND a.ContractEndDate THEN 'Active' ELSE 'Inactive' END AS StatusDesc,
			NULL AS ContractorTypeID,
			NULL AS ContractorTypeDesc,
			a.ContractStartDate AS IDStartDate,
			a.ContractEndDate AS IDEndDate,
			a.ContractStartDate,
			a.ContractEndDate,
			CONVERT(FLOAT, (a.WorkDurationHours * 60) + a.WorkDurationMins) AS RequiredWorkDuration,
			a.CreatedDate,
			LTRIM(RTRIM(b.YAALPH)) AS CreatedByName,
			a.CreatedByEmpNo AS CreatedByNo
	FROM tas.ContractorRegistry a WITH (NOLOCK)
		LEFT JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.CreatedByEmpNo = CAST(b.YAAN8 AS INT)
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON RTRIM(a.VisitedCostCenter) = RTRIM(c.BU)
		OUTER APPLY
		(
			SELECT	RTRIM(UDCCode) AS JobTitleCode,
					RTRIM(UDCDesc1) AS JobTitleDesc
			FROM tas.sy_UserDefinedCode WITH (NOLOCK)
			WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'CONTJOBTLE')
				AND RTRIM(a.JobTitle) = RTRIM(UDCCode)
		) d	

GO


/*	Debugging:

	SELECT * FROM tas.Vw_ContractorAttendance_V2 a
	WHERE a.EmpNo IN (61012)

	SELECT * FROM tas.Vw_ContractorAttendance_V2 a
	WHERE a.RequiredWorkDuration > 0

*/