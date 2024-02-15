/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_VisitorSwipeHistory
*	Description: Get the visitor's swipe records
*
*	Date			Author		Rev. #		Comments:
*	08/08/2016		Ervin		1.0			Created
*	16/01/2017		Ervin		1.1			Modified the filter condition to add EmpNo < 10000000
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_VisitorSwipeHistory
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
			@WHERE						= 'WHERE EmpNo < 10000000',
			@ORDERBY					= ' ORDER BY SwipeDate DESC, SwipeTime ASC',
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

	IF ISNULL(@pageNumber, 0) = 0
		SET @pageNumber = 1

	IF ISNULL(@pageSize, 0) = 0
		SET @pageSize = 10

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Vw_VisitorSwipe a '

	SET @CMD = 'SELECT * FROM tas.Vw_VisitorSwipe a '

	--Add Employee No. filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	--Add date range filter
	IF @startDate IS NOT NULL AND @endDate IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.SwipeDate, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY SwipeDate DESC, SwipeTime ASC) as RowNumber 
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

	SELECT * FROM tas.VisitorSwipeLog a 
	ORDER BY a.SwipeID DESC

PARAMETERS:
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME,
	@pageNumber		INT = 1,
	@pageSize		INT = 10			

	EXEC tas.Pr_VisitorSwipeHistory 11101, '01/15/2017', '01/15/2017'
	EXEC tas.Pr_VisitorSwipeHistory 10003632, '01/15/2017', '01/15/2017'
	EXEC tas.Pr_VisitorSwipeHistory 50456, '14/08/2016', '14/08/2016'
	EXEC tas.Pr_VisitorSwipeHistory 10003011, '27/01/2016', '27/01/2016'
		
*/


