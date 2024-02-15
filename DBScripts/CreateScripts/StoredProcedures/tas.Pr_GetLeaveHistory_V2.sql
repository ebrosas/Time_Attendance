/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetLeaveHistory_V2
*	Description: Get leave history records 
*
*	Date			Author		Rev. #		Comments:
*	04/08/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetLeaveHistory_V2
(   
	@empNo			INT,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@pageNumber		INT = 1,
	@pageSize		INT = 10	
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
			@ORDERBY					= ' ORDER BY LeaveStartDate DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.syJDE_LeaveRequisition a '

	SET @CMD = 'SELECT	a.EmpNo,			
						a.RequisitionNo AS LeaveNo,
						a.LeaveStartDate, 
						a.LeaveEndDate, 
						a.RequisitionDate,
						a.LeaveType, 
						RTRIM(b.DRDL01) AS LeaveTypeDesc, 
						a.LeaveDuration
				FROM tas.syJDE_LeaveRequisition a
					LEFT JOIN tas.syJDE_F0005 b ON RTRIM(a.LeaveType) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(b.DRSY)) = ''58'' AND LTRIM(RTRIM(b.DRRT)) = ''VC'' '

	--Add Employee No. filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	--Add date range filter
	IF @startDate IS NOT NULL AND @endDate IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.LeaveStartDate, 12) >= ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote 
			+ ' AND CONVERT(VARCHAR, a.LeaveEndDate, 12) <= ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 

	--Add additional filter 
	SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.ApprovalFlag) NOT IN (''C'', ''R'', ''D'') ' 

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY LeaveStartDate DESC) as RowNumber 
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
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME,
	@pageNumber		INT = 1,
	@pageSize		INT = 10			

	EXEC tas.Pr_GetLeaveHistory_V2 10003632
	EXEC tas.Pr_GetLeaveHistory_V2 10003632, '01/01/2014', '31/03/2016'
		
*/


