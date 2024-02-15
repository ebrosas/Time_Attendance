/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetLeaveHistory
*	Description: Used to fetch the leave requisition records of an employee on specific date period
*
*	Date			Author		Rev. #		Comments:
*	27/07/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetLeaveHistory
(   
	@empNo			INT,
	@DT				DATETIME,
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
			@ORDERBY					= ' ORDER BY FromDate DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) FROM tas.Vw_LeaveHistory '
	SET @CMD = 'SELECT * FROM tas.Vw_LeaveHistory a '
	
	--Add @empNo filter
	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) 

	----Add @DT filter	
	IF @DT IS NOT NULL 
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + @chrQuote + CONVERT(varchar, @DT, 12) + @chrQuote + ' BETWEEN CONVERT(VARCHAR, FromDate, 12) AND CONVERT(VARCHAR, ToDate, 12) '

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY FromDate DESC) as RowNumber 
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

	--Retrieve all leave type codes
	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'VC'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve all Absent Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-RA'
	ORDER BY LTRIM(RTRIM(DRKY))

PARAMETERS:
	@empNo			INT,
	@DT				DATETIME,
	@pageNumber		INT = 1,
	@pageSize		INT = 10	

	EXEC tas.Pr_GetTimesheetLeaveHistory 10001168, '16/03/2016'		
	EXEC tas.Pr_GetTimesheetLeaveHistory 10003632, '18/01/2016'		

*/


