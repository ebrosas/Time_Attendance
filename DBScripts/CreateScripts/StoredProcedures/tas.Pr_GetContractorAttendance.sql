/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractorAttendance
*	Description: Get the attendance records of Contractors
*
*	Date			Author		Revision No.	Comments:
*	16/10/2016		Ervin		1.0				Created
*	27/06/2021		Ervin		1.1				Refactored code to enhance performance
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetContractorAttendance
(   
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@contractorNo		INT = 0,
	@contractorName		VARCHAR(100) = '',
	@costCenter			VARCHAR(12) = '',	
	@pageNumber			INT = 1,
	@pageSize			INT = 10	
)
AS
BEGIN
	
	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

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
			@WHERE						= 'WHERE a.EmpNo < 10000000 AND a.EmpNo > 50000',
			@ORDERBY					= ' ORDER BY SwipeDate DESC, EmpNo',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

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

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	/*
	SET @CMDTotalRecords = 'SELECT a.EmpNo 
							FROM tas.Vw_ContractorAttendance_V2 a
								INNER JOIN tas.Vw_ContractorSwipe b ON a.EmpNo = b.EmpNo '

	
	SET @CMD = 'SELECT	a.EmpNo,
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
						b.SwipeTime,
						b.SwipeType,
						b.LocationName,
						b.ReaderName
				FROM tas.Vw_ContractorAttendance_V2 a WITH (NOLOCK)
					INNER JOIN tas.Vw_ContractorSwipe b WITH (NOLOCK) ON a.EmpNo = b.EmpNo '
	*/									

	SET @CMDTotalRecords = 'SELECT a.EmpNo 
							FROM tas.Vw_ContractorAttendance_V2 a
								CROSS APPLY	
								(
									SELECT DISTINCT SwipeDate, LocationName, ReaderName
									FROM tas.Vw_ContractorSwipe WITH (NOLOCK)
									WHERE EmpNo = a.EmpNo
								) b '

	SET @CMD = 'SELECT	a.EmpNo,
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
				FROM tas.Vw_ContractorAttendance_V2 a WITH (NOLOCK)
					CROSS APPLY	
					(
						SELECT DISTINCT SwipeDate, LocationName, ReaderName
						FROM tas.Vw_ContractorSwipe WITH (NOLOCK)
						WHERE EmpNo = a.EmpNo
					) b '

	--Add @startDate and @endDate parameters
	IF @startDate IS NOT NULL AND @endDate IS NOT NULL
    BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, b.SwipeDate, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 
    END

	--Add @contractorNo filter    
	IF @contractorNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@contractorNo AS VARCHAR(10))) + ')'

	--Add @contractorName filter
	IF @contractorName IS NOT NULL
	BEGIN
		--Rev. #1.3
		IF (SELECT COUNT(*) FROM tas.fn_SplitIn2Rows(LOWER(TRIM(@contractorName)), ';')) > 1
			SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (LOWER(LTRIM(RTRIM((a.EmpName)))) IN (' + 'SELECT ItemValue FROM tas.fn_SplitIn2Rows(' + @chrQuote + LOWER(TRIM(@contractorName)) + @chrQuote + ', '';''))'  +  ' OR LOWER(LTRIM(RTRIM((a.EmployerName)))) IN (' + 'SELECT ItemValue FROM tas.fn_SplitIn2Rows(' + @chrQuote + LOWER(TRIM(@contractorName)) + @chrQuote + ', '';''))' + ')'			
		ELSE	
			SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (LOWER(LTRIM(RTRIM((a.EmpName)))) LIKE ' + @chrQuote + '%' + LOWER(TRIM(@contractorName)) + '%' + @chrQuote  +  ' OR LOWER(LTRIM(RTRIM((a.EmployerName)))) LIKE ' + @chrQuote + '%' + LOWER(TRIM(@contractorName)) + '%' + @chrQuote + ')'
	END 

	--Add @costCenter filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.CostCenter) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    

	--Get the total records
	SELECT @CMDMaster2 = @CMDTotalRecords + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END
	SELECT @CMDMaster2 = 'SELECT COUNT(*) FROM (' + @CMDMaster2 + ') A'

	INSERT INTO @tempTable
	EXEC sp_executesql @CMDMaster2 
	SELECT @totalRecords = totalRows FROM @tempTable

	--Build the final query
	SELECT @CMDMaster = 
		'SELECT * FROM 
		(
			SELECT *, ' + CONVERT(VARCHAR(10), @totalRecords) + 
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY SwipeDate DESC, EmpNo) as RowNumber 
			FROM (' + @CMD + CASE WHEN @WHERE = 'WHERE' THEN '' ELSE @WHERE END + ') as innerTable 
		) as outerTable WHERE RowNumber BETWEEN ' + CONVERT(VARCHAR(10), @startIndex) + ' AND ' + CONVERT(VARCHAR(10), @endIndex)			
		+ @ORDERBY

	--Check if total records to return is greater than 200
	--IF @recordCount > 100
	--	SELECT @CMDMaster = REPLACE(@CMDMaster, 'TOP 100', 'TOP ' + CONVERT(VARCHAR(10), @recordCount));

	PRINT @CMDMaster
	EXEC sp_executesql @CMDMaster

END 


/*	Debug:

PARAMETERS:
	@startDate			DATETIME = NULL,
	@endDate			DATETIME = NULL,
	@contractorNo		INT = 0,
	@contractorName		VARCHAR(100) = '',
	@costCenter			VARCHAR(12) = '',	
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	EXEC tas.Pr_GetContractorAttendance '06/01/2021', '06/30/2021', 61048, '', '', 1, 100
	EXEC tas.Pr_GetContractorAttendance '06/01/2021', '06/30/2021', 0, 'excellence', '', 1, 100

*/