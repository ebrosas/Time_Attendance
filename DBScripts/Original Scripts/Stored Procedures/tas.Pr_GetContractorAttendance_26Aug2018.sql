USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetContractorAttendance]    Script Date: 26/08/2018 12:21:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetContractorAttendance
*	Description: Get the attendance records of Contractors
*
*	Date			Author		Revision No.	Comments:
*	07/09/2016		Ervin		1.0				Created
*	14/10/2016		Ervin		1.1				Used the "Vw_ContractorSwipe" view to fetch the Contractor's swipe from the Main gate
******************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetContractorAttendance]
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

	--Declare table variable
	DECLARE @tempTable TABLE (totalRows int)

	--Declare variables
	DECLARE	@userCostCenter				VARCHAR(12),
			@CMDMaster					NVARCHAR(MAX), 
			@CMDMaster2					NVARCHAR(MAX), 
			@CMD						VARCHAR(MAX), 
			@CMDTotalRecords			VARCHAR(MAX),
			@WHERE						VARCHAR(MAX),		
			@WHERE_GROUP				VARCHAR(MAX),		
			@ORDERBY					VARCHAR(MAX),	
			@chrQuote					CHAR(1),
			@startIndex					INT,
			@endIndex					INT,
			@totalRecords				INT,
			@recordCount				INT

	SELECT	@userCostCenter				= '',
			@WHERE						= 'WHERE a.EmpNo < 10000000 AND a.EmpNo > 50000',
			@WHERE_GROUP				= 'WHERE a.EmpNo < 10000000 AND a.EmpNo > 50000',
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
						b.LocationName,
						b.ReaderName
				FROM tas.Vw_ContractorAttendance_V2 a
					INNER JOIN tas.Vw_ContractorSwipe b ON a.EmpNo = b.EmpNo '

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
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' UPPER(RTRIM(a.EmpName)) LIKE ' + @chrQuote + '%' + UPPER(RTRIM(@contractorName)) + '%' + @chrQuote 

	--Add @costCenter filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.CostCenter) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    

	--Add Group by clause
	SELECT @WHERE_GROUP = @WHERE + ' GROUP BY 
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
								b.ReaderName '

	--Get the total records
	SELECT @CMDMaster2 = @CMDTotalRecords + CASE WHEN @WHERE_GROUP = 'WHERE' THEN '' ELSE @WHERE_GROUP END
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
			FROM (' + @CMD + CASE WHEN @WHERE_GROUP = 'WHERE' THEN '' ELSE @WHERE_GROUP END + ') as innerTable 
		) as outerTable WHERE RowNumber BETWEEN ' + CONVERT(VARCHAR(10), @startIndex) + ' AND ' + CONVERT(VARCHAR(10), @endIndex)		
		+ @ORDERBY

	--Check if total records to return is greater than 200
	--IF @recordCount > 100
	--	SELECT @CMDMaster = REPLACE(@CMDMaster, 'TOP 100', 'TOP ' + CONVERT(VARCHAR(10), @recordCount));

	PRINT @CMDMaster
	--PRINT @CMDMaster2
	EXEC sp_executesql @CMDMaster

