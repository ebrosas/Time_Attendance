/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractorAttendanceAll
*	Description: Get the attendance records of Contractors
*
*	Date			Author		Revision No.	Comments:
*	13/09/2016		Ervin		1.0				Created

******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContractorAttendance_Excel
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
			a.ContractStartDate,
			a.ContractEndDate,
			a.RequiredWorkDuration,
			a.CreatedDate,
			a.CreatedByNo,
			a.CreatedByName,						
			b.SwipeDate,
			b.SwipeTime,
			b.SwipeType,
			b.LocationName,
			b.ReaderName
	FROM tas.Vw_ContractorAttendance_V2 a
		LEFT JOIN tas.Vw_ContractorSwipe b ON a.EmpNo = b.EmpNo
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
	ORDER BY b.SwipeDate DESC, a.EmpNo, b.SwipeTime	

/*	Debugging:

PARAMETERS:
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@contractorNo		INT = 0,
	@contractorName		VARCHAR(100) = '',
	@costCenter			VARCHAR(12) = ''

	EXEC tas.Pr_GetContractorAttendance_Excel '10/19/2016', '10/19/2016'
	EXEC tas.Pr_GetContractorAttendance_Excel '10/19/2016', '10/19/2016', 54107

*/
