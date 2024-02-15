/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractorAttendanceAll
*	Description: Get the attendance records of Contractors
*
*	Date			Author		Revision No.	Comments:
*	13/09/2016		Ervin		1.0				Created
*	14/10/2016		Ervin		1.1				Used the "Vw_ContractorSwipe" view to fetch the Contractor's swipe from the Main gate
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContractorAttendanceAll
(   
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@contractorNo		INT = 0,
	@contractorName		VARCHAR(100) = '',
	@costCenter			VARCHAR(12) = ''
)
AS

	--Validate parameters
	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@contractorNo, 0) = 0
		SET @contractorNo = NULL

	IF ISNULL(@contractorName, '') = ''
		SET @contractorName = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	SELECT	a.EmpNo,
			a.EmpName,
			a.CostCenter,
			a.CostCenterName,
			a.CPRNo,
			a.JobTitle,
			a.EmployerName,
			a.StatusID,
			a.StatusDesc,
			a.ContractorTypeID,
			a.ContractorTypeDesc,
			a.IDStartDate,
			a.IDEndDate,
			a.ContractStartDate,
			a.ContractEndDate,
			a.RequiredWorkDuration,
			a.CreatedDate,
			a.CreatedByNo,
			a.CreatedByName,						
			b.SwipeDate,
			--b.SwipeTime,
			--b.SwipeType,
			b.LocationName,
			b.ReaderName
	FROM tas.Vw_ContractorAttendance_V2 a
		INNER JOIN tas.Vw_ContractorSwipe b ON a.EmpNo = b.EmpNo
	WHERE 
		a.EmpNo < 10000000 
		AND a.EmpNo > 50000 
		AND (a.EmpNo = @contractorNo OR @contractorNo IS NULL)
		AND (UPPER(RTRIM(a.EmpName)) LIKE '%' + UPPER(RTRIM(@contractorName)) + '%' OR @contractorName IS NULL)
		AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND 
		(
			b.SwipeDate BETWEEN @startDate AND @endDate
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
	GROUP BY 
		a.EmpNo, 
		a.EmpName,
		a.CostCenter,
		a.CostCenterName,
		a.CPRNo,
		a.JobTitle,
		a.EmployerName,
		a.StatusID,
		a.StatusDesc,
		a.ContractorTypeID,
		a.ContractorTypeDesc,
		a.ContractStartDate,
		a.IDStartDate,
		a.IDEndDate,
		a.ContractEndDate,
		a.RequiredWorkDuration,
		a.CreatedDate,
		a.CreatedByNo,
		a.CreatedByName,	
		b.SwipeDate,
		b.LocationName,
		b.ReaderName 
	ORDER BY b.SwipeDate DESC	

GO 

/*	Debugging:

PARAMETERS:
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@contractorNo		INT = 0,
	@contractorName		VARCHAR(100) = '',
	@costCenter			VARCHAR(12) = ''

	EXEC tas.Pr_GetContractorAttendanceAll '09/16/2016', '10/15/2016', 0, '', '3470'

*/


