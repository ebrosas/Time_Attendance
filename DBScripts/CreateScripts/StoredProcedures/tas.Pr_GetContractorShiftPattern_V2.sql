/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractorShiftPattern_V2
*	Description: Get the contractor's shift pattern information
*
*	Date			Author		Rev. #		Comments:
*	04/01/2017		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContractorShiftPattern_V2
(   
	@autoID					INT = 0,
	@empNo					INT = 0,
	@empName				VARCHAR(40) = '',
	@dateJoinedStart		DATETIME = NULL,
	@dateJoinedEnd			DATETIME = NULL,
	@dateResignedStart		DATETIME = NULL,
	@dateResignedEnd		DATETIME = NULL,
	@pageNumber				INT = 1,
	@pageSize				INT = 10	
)
AS

	--Declare table variable
	DECLARE @tempTable TABLE (totalRows int)

	--Declare variables
	DECLARE	@userCostCenter				VARCHAR(12),
			@CMDMaster					NVARCHAR(MAX), 
			@CMDMaster2					NVARCHAR(MAX), 
			@CMD						VARCHAR(MAX), 
			@CMDTotalRecords			VARCHAR(MAX),
			@WHERE						VARCHAR(MAX),		
			@ORDERBY					VARCHAR(MAX),	
			@chrQuote					CHAR(1),
			@startIndex					INT,
			@endIndex					INT,
			@totalRecords				INT,
			@recordCount				INT

	SELECT	@userCostCenter				= '',
			@WHERE						= 'WHERE',
			@ORDERBY					= ' ORDER BY LastUpdateTime DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

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

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Master_ContractEmployee a '

	SET @CMD = 'SELECT	a.AutoID,
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
					LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.ReligionCode) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) = ''06'' AND LTRIM(RTRIM(b.DRRT)) = ''M''
					LEFT JOIN  tas.syJDE_F0101 c ON RTRIM(a.ContractorNumber) = LTRIM(RTRIM(c.ABAN8)) AND LTRIM(RTRIM(c.ABAT1)) = ''V''
					LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.GroupCode) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) = ''55'' AND LTRIM(RTRIM(d.DRRT)) = ''CG'' '

	--Add Auto ID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) + ')'

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add @empName filter
	IF @empName IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' UPPER(RTRIM(a.ContractorEmpName)) LIKE ' + @chrQuote + '%' + RTRIM(@empName) + '%' + @chrQuote 

	--Add @dateJoinedStart and @dateJoinedEnd filter
	IF (@dateJoinedStart IS NOT NULL AND @dateJoinedEnd IS NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DateJoined, 12) = ' + @chrQuote + CONVERT(varchar, @dateJoinedStart, 12) + @chrQuote 
	END		
	
	ELSE IF (@dateJoinedStart IS NOT NULL AND @dateJoinedEnd IS NOT NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DateJoined, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @dateJoinedStart, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @dateJoinedEnd, 12) + @chrQuote 
	END

	--Add @dateResignedStart and @dateResignedEnd filter
	IF (@dateResignedStart IS NOT NULL AND @dateResignedEnd IS NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DateResigned, 12) = ' + @chrQuote + CONVERT(varchar, @dateResignedStart, 12) + @chrQuote 
	END		
	
	ELSE IF (@dateResignedStart IS NOT NULL AND @dateResignedEnd IS NOT NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DateResigned, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @dateResignedStart, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @dateResignedEnd, 12) + @chrQuote 
	END


	--Get the total records
	SELECT @CMDMaster2 = @CMDTotalRecords + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END

	INSERT INTO @tempTable
	EXEC sp_executesql @CMDMaster2 
	SELECT @totalRecords = totalRows FROM @tempTable

	--Build the final query
	SELECT @CMDMaster = 
		'SELECT * FROM 
		(
			SELECT *, ' + CONVERT(VARCHAR(10), @totalRecords) + 
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY LastUpdateTime DESC) as RowNumber 
			FROM (' + @CMD + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END + ') as innerTable 
		) as outerTable WHERE RowNumber BETWEEN ' + CONVERT(VARCHAR(10), @startIndex) + ' AND ' + CONVERT(VARCHAR(10), @endIndex)		
		+ @ORDERBY

	--Check if total records to return is greater than 200
	IF @recordCount > 100
		SELECT @CMDMaster = REPLACE(@CMDMaster, 'TOP 100', 'TOP ' + CONVERT(VARCHAR(10), @recordCount));

	PRINT @CMDMaster
	EXEC sp_executesql @CMDMaster

GO 

/*	Debugging:

PARAMETERS:
	@autoID					INT = 0,
	@empNo					INT = 0,
	@empName				VARCHAR(40) = '',
	@dateJoinedStart		DATETIME = NULL,
	@dateJoinedEnd			DATETIME = NULL,
	@dateResignedStart		DATETIME = NULL,
	@dateResignedEnd		DATETIME = NULL,
	@pageNumber				INT = 1,
	@pageSize				INT = 10	

	EXEC tas.Pr_GetContractorShiftPattern_V2 
	EXEC tas.Pr_GetContractorShiftPattern_V2 78682								--By AutoID
	EXEC tas.Pr_GetContractorShiftPattern_V2 0, 54214							--By Emp. No.
	EXEC tas.Pr_GetContractorShiftPattern_V2 0, 0, 'katriane' 					--By Emp. Name
	EXEC tas.Pr_GetContractorShiftPattern_V2 0, 0, '', '05/07/2005'					--By Date Joined Start
	EXEC tas.Pr_GetContractorShiftPattern_V2 0, 0, '', '', '18/03/2006'			--By Date Resigned Start

*/


