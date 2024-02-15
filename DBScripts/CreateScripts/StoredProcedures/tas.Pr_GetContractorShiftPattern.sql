/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractorShiftPattern
*	Description: Get the contractor's shift pattern information
*
*	Date			Author		Revision No.	Comments:
*	08/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContractorShiftPattern
(   
	@autoID					INT = 0,
	@empNo					INT = 0,
	@empName				VARCHAR(40) = '',
	@dateJoinedStart		DATETIME = NULL,
	@dateJoinedEnd			DATETIME = NULL,
	@dateResignedStart		DATETIME = NULL,
	@dateResignedEnd		DATETIME = NULL
)
AS

	--Validate parameters
	IF ISNULL(@autoID, 0) = 0
		SET @autoID = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@empName, '') = ''
		SET @empName = NULL

	IF ISNULL(@dateJoinedStart, '') = '' OR CONVERT(DATETIME, '') = CONVERT(DATETIME, @dateJoinedStart)
		SET @dateJoinedStart = NULL

	IF ISNULL(@dateJoinedEnd, '') = '' OR CONVERT(DATETIME, '') = CONVERT(DATETIME, @dateJoinedEnd) 
		SET @dateJoinedEnd = NULL

	IF ISNULL(@dateResignedStart, '') = '' OR CONVERT(DATETIME, '') = CONVERT(DATETIME, @dateResignedStart) 
		SET @dateResignedStart = NULL

	IF ISNULL(@dateResignedEnd, '') = '' OR CONVERT(DATETIME, '') = CONVERT(DATETIME, @dateResignedEnd) 
		SET @dateResignedEnd = NULL

	SELECT	a.AutoID,
			a.EmpNo,
			a.ContractorEmpName,
			a.GroupCode,
			LTRIM(RTRIM(d.DRDL01)) AS GroupDesc,
			a.ContractorNumber AS SupplierNo,
			LTRIM(RTRIM(c.ABALPH)) AS SupplierName,
			a.DateJoined,
			a.DateResigned,
			a.ShiftPatCode,
			a.ShiftPointer,
			a.ReligionCode,
			LTRIM(RTRIM(b.DRDL01)) AS ReligionDesc,
			a.LastUpdateUser,
			a.LastUpdateTime
	FROM tas.Master_ContractEmployee a
		LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.ReligionCode) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) = '06' AND LTRIM(RTRIM(b.DRRT)) = 'M'
		LEFT JOIN  tas.syJDE_F0101 c ON RTRIM(a.ContractorNumber) = LTRIM(RTRIM(c.ABAN8)) AND LTRIM(RTRIM(c.ABAT1)) = 'V'
		LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.GroupCode) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) = '55' AND LTRIM(RTRIM(d.DRRT)) = 'CG'
	WHERE
		(a.AutoID = @autoID OR @autoID IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (UPPER(RTRIM(a.ContractorEmpName)) LIKE '%' + UPPER(RTRIM(@empName)) + '%' OR @empName IS NULL)
		AND 
		(
			(a.DateJoined BETWEEN @dateJoinedStart AND @dateJoinedEnd AND @dateJoinedStart IS NOT NULL AND @dateJoinedEnd IS NOT NULL)
			OR
            (a.DateJoined = @dateJoinedStart AND @dateJoinedStart IS NOT NULL AND @dateJoinedEnd IS NULL)
			OR
            (@dateJoinedStart IS NULL AND @dateJoinedEnd IS NULL)
		)
		AND 
		(
			(a.DateResigned BETWEEN @dateResignedStart AND @dateResignedEnd AND @dateResignedStart IS NOT NULL AND @dateResignedEnd IS NOT NULL)
			OR
            (a.DateResigned = @dateResignedStart AND @dateResignedStart IS NOT NULL AND @dateResignedEnd IS NULL)
			OR
            (@dateResignedStart IS NULL AND @dateResignedEnd IS NULL)
		)
	ORDER BY a.LastUpdateTime DESC

GO 

/*	Debugging:

PARAMETERS:
	@autoID					INT = 0,
	@empNo					INT = 0,
	@empName				VARCHAR(40),
	@dateJoinedStart		DATETIME = NULL,
	@dateJoinedEnd			DATETIME = NULL,
	@dateResignedStart		DATETIME = NULL,
	@dateResignedEnd		DATETIME = NULL

	EXEC tas.Pr_GetContractorShiftPattern
	EXEC tas.Pr_GetContractorShiftPattern 78682								--By AutoID
	EXEC tas.Pr_GetContractorShiftPattern 0, 52724							--By Emp. No.
	EXEC tas.Pr_GetContractorShiftPattern 0, 0, 'katriane' 					--By Emp. Name
	EXEC tas.Pr_GetContractorShiftPattern 0, 0, '05/07/2005'				--By Date Joined Start
	EXEC tas.Pr_GetContractorShiftPattern 0, 0, '', '', '18/03/2006'		--By Date Resigned Start

*/


