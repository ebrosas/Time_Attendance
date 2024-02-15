/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDutyROTA
*	Description: Get the duty ROTA records
*
*	Date			Author		Rev. #		Comments:
*	02/11/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDutyROTA
(   
	@autoID				INT = 0,
	@empNo				INT = 0,
	@effectiveDate		DATETIME = NULL,
	@endingDate			DATETIME = NULL,
	@dutyType			VARCHAR(1) = '',
	@pageNumber			INT = 1,
	@pageSize			INT = 10	
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
			@ORDERBY					= ' ORDER BY EffectiveDate DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@autoID, 0) = 0
		SET @autoID = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@effectiveDate, '') = ''
		SET @effectiveDate = NULL

	IF ISNULL(@endingDate, '') = ''
		SET @endingDate = NULL

	IF ISNULL(@dutyType, '') = ''
		SET @dutyType = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_DutyRota a
								INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						b.EmpName,
						b.Position,
						b.BusinessUnit,
						c.BusinessUnitName,
						a.EffectiveDate,
						a.EndingDate,
						a.DutyType,
						d.[Description] AS DutyDescription,
						d.DutyAllowance,
						a.LastUpdateUser,
						a.LastUpdateTime 
				FROM tas.Tran_DutyRota a
					INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit) 
					LEFT JOIN tas.Master_DutyType d ON a.DutyType = d.DutyType '

	--Add Auto ID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) + ')'

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	IF (@effectiveDate IS NOT NULL AND @endingDate IS NULL)
	BEGIN

		--Add @effectiveDate filter
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.EffectiveDate, 12) = ' + @chrQuote + CONVERT(varchar, @effectiveDate, 12) + @chrQuote 
	END		
	ELSE IF (@effectiveDate IS NULL AND @endingDate IS NOT NULL)
	BEGIN

		--Add @endingDate filter
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.EndingDate, 12) = ' + @chrQuote + CONVERT(varchar, @endingDate, 12) + @chrQuote 
	END
	ELSE IF (@effectiveDate IS NOT NULL AND @endingDate IS NOT NULL)
	BEGIN

		--Add @effectiveDate and @endingDate filter
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.EffectiveDate, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @effectiveDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endingDate, 12) + @chrQuote 
	END

	--Add @dutyType filter
	IF @dutyType IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.DutyType) = '+ @chrQuote + RTRIM(@dutyType) + @chrQuote   

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY EffectiveDate DESC) as RowNumber 
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
	@autoID				INT = 0,
	@empNo				INT = 0,
	@effectiveDate		DATETIME = NULL,
	@endingDate			DATETIME = NULL,
	@dutyType			VARCHAR(1) = '',
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	EXEC tas.Pr_GetDutyROTA 

*/


