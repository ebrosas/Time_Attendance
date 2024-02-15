/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Retrieve_VisitorPassLog_V2
*	Description: Retrieves visitor's log records
*
*	Date			Author		Rev. #		Comments:
*	15/08/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_Retrieve_VisitorPassLog_V2
(   
	@logID					BIGINT = 0,
	@visitorName			VARCHAR(100) = '',
	@idNumber				VARCHAR(50) = '',
	@visitorCardNo			INT = 0,
	@visitEmpNo				INT = 0,
	@visitCostCenter		VARCHAR(12) = '',
	@startDate				DATETIME = NULL,
	@endDate				DATETIME = NULL,
	@blockOption			TINYINT = 0,
	@userEmpNo				INT = 0,
	@createdByOtherEmpNo	INT = 0,
	@createdByTypeID		TINYINT = 0,	--(Note: 0 = All; 1 = Me; 2 = Others) 
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
			@recordCount				INT,
			@isBlock					BIT

	SELECT	@userCostCenter				= '',
			@WHERE						= 'WHERE',
			@ORDERBY					= ' ORDER BY VisitDate DESC, CreatedDate DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@logID, 0) = 0
		SET @logID = NULL

	IF ISNULL(@visitorName, '') = ''
		SET @visitorName = NULL

	IF ISNULL(@idNumber, '') = ''
		SET @idNumber = NULL

	IF ISNULL(@visitorCardNo, 0) = 0
		SET @visitorCardNo = NULL
	
	IF ISNULL(@visitEmpNo, 0) = 0
		SET @visitEmpNo = NULL

	IF ISNULL(@visitCostCenter, '') = ''
		SET @visitCostCenter = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@createdByTypeID, 0) = 0
		SET @createdByTypeID = NULL

	SELECT @isBlock = CASE	WHEN @blockOption = 1 THEN 1	--Yes
							WHEN @blockOption = 2 THEN 0	--No
							ELSE NULL						--All
					  END 

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.VisitorPassLog a
								LEFT JOIN tas.Master_Employee_JDE_View b ON a.VisitEmpNo = b.EmpNo
								LEFT JOIN tas.syJDE_F060116 c on b.EmpNo = c.YAAN8
								LEFT JOIN tas.Master_Employee_JDE_View d on c.YAANPA = d.EmpNo
								LEFT JOIN tas.External_JDE_F0006 e on ltrim(rtrim(b.BusinessUnit)) = ltrim(rtrim(e.MCMCU))
								LEFT JOIN tas.Master_Employee_JDE_View f on e.MCANPA = f.EmpNo
								LEFT JOIN tas.Master_BusinessUnit_JDE g on ltrim(rtrim(b.BusinessUnit)) = ltrim(rtrim(g.BusinessUnit))
								LEFT JOIN tas.syJDE_F08001 h on LTRIM(RTRIM(c.YAJBCD)) = LTRIM(RTRIM(h.JMJBCD))
								LEFT JOIN tas.syJDE_F0115 i on b.EmpNo = i.WPAN8 AND upper(ltrim(rtrim(i.WPPHTP))) = ''EXT'' '

	SET @CMD = 'SELECT	a.LogID,
						a.VisitorName,
						a.IDNumber,
						a.VisitorCardNo,
						a.VisitEmpNo,
						b.EmpName AS VisitEmpName,
						LTRIM(RTRIM(ISNULL(h.JMDL01, ''''))) AS VisitEmpPosition,
						ISNULL(CONVERT(VARCHAR(20), i.WPPH1), '''') AS VisitEmpExtension,
						b.BusinessUnit AS VisitEmpCostCenter,
						g.BusinessUnitName AS VisitEmpCostCenterName,
						c.YAANPA as VisitEmpSupervisorNo,
						d.EmpName as VisitEmpSupervisorName,
						e.MCANPA as VisitEmpManagerNo, 
						f.EmpName as VisitEmpManagerName,
						a.VisitDate,
						a.VisitTimeIn,
						a.VisitTimeOut,
						a.Remarks,
						a.IsBlock,
						a.CreatedDate,
						a.CreatedByEmpNo,
						a.CreatedByUserID,
						a.CreatedByEmpName,
						a.CreatedByEmpEmail,
						a.LastUpdateTime,
						a.LastUpdateEmpNo,
						a.LastUpdateUserID,
						a.LastUpdateEmpName,
						a.LastUpdateEmpEmail
				FROM tas.VisitorPassLog a
					LEFT JOIN tas.Master_Employee_JDE_View b ON a.VisitEmpNo = b.EmpNo
					LEFT JOIN tas.syJDE_F060116 c on b.EmpNo = c.YAAN8
					LEFT JOIN tas.Master_Employee_JDE_View d on c.YAANPA = d.EmpNo
					LEFT JOIN tas.External_JDE_F0006 e on ltrim(rtrim(b.BusinessUnit)) = ltrim(rtrim(e.MCMCU))
					LEFT JOIN tas.Master_Employee_JDE_View f on e.MCANPA = f.EmpNo
					LEFT JOIN tas.Master_BusinessUnit_JDE g on ltrim(rtrim(b.BusinessUnit)) = ltrim(rtrim(g.BusinessUnit))
					LEFT JOIN tas.syJDE_F08001 h on LTRIM(RTRIM(c.YAJBCD)) = LTRIM(RTRIM(h.JMJBCD))
					LEFT JOIN tas.syJDE_F0115 i on b.EmpNo = i.WPAN8 AND upper(ltrim(rtrim(i.WPPHTP))) = ''EXT'' '

	--Add "LogID" filter
	IF @logID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.LogID = ' + RTRIM(CAST(@logID AS VARCHAR(20))) 

	--Add "VisitorName" filter
	IF @visitorName IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' UPPER(RTRIM(a.VisitorName)) LIKE ' + @chrQuote + '%' + UPPER(RTRIM(@visitorName)) + '%' + @chrQuote
		--SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' UPPER(RTRIM(a.VisitorName)) LIKE ' + @chrQuote + RTRIM(@visitorName) + @chrQuote 

	--Add "IDNumber" filter
	IF @idNumber IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.IDNumber) = ' + @chrQuote + RTRIM(@idNumber) + @chrQuote 

	--Add "VisitorCardNo" filter
	IF @visitorCardNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.VisitorCardNo = ' + RTRIM(CAST(@visitorCardNo AS VARCHAR(10)))

	--Add "VisitEmpNo" filter
	IF @visitEmpNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.VisitEmpNo = ' + RTRIM(CAST(@visitEmpNo AS VARCHAR(10))) 

	--Add "BusinessUnit" filter
	IF @visitCostCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(b.BusinessUnit) = '+ @chrQuote + RTRIM(@visitCostCenter) + @chrQuote 

	--Add "VisitDate" filter
	IF @startDate IS NOT NULL AND @endDate IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.VisitDate, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote 

	--Add "IsBlock" filter
	IF @isBlock = 1
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.IsBlock = 1' 
	ELSE IF @isBlock = 0
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.IsBlock = 0' 

	--Add "CreatedByEmpNo" filter
	IF (@createdByTypeID = 1 AND @userEmpNo > 0)	--Created by Me
	BEGIN

		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.CreatedByEmpNo = ' + CAST(@userEmpNo AS VARCHAR(10)) + ')'
	END 

	ELSE IF @createdByTypeID = 2	--Created by Others
	BEGIN

		IF @createdByOtherEmpNo > 0
			SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.CreatedByEmpNo = ' + CAST(@createdByOtherEmpNo AS VARCHAR(10)) + ')'
		
		ELSE
		BEGIN

			IF @userEmpNo > 0
				SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.CreatedByEmpNo <> ' + CAST(@userEmpNo AS VARCHAR(10)) + ')'
		END
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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY VisitDate DESC, CreatedDate DESC) as RowNumber 
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
	@logID					BIGINT = 0,
	@visitorName			VARCHAR(100) = '',
	@idNumber				VARCHAR(50) = '',
	@visitorCardNo			INT = 0,
	@visitEmpNo				INT = 0,
	@visitCostCenter		VARCHAR(12) = '',
	@startDate				DATETIME = NULL,
	@endDate				DATETIME = NULL,
	@blockOption			TINYINT = 0,
	@userEmpNo				INT = 0,
	@createdByOtherEmpNo	INT = 0,
	@createdByTypeID		TINYINT = 0,	--(Note: 0 = All; 1 = Me; 2 = Others) 
	@pageNumber				INT = 1,
	@pageSize				INT = 10				

	EXEC tas.Pr_Retrieve_VisitorPassLog_V2 
	EXEC tas.Pr_Retrieve_VisitorPassLog_V2 16
	EXEC tas.Pr_Retrieve_VisitorPassLog_V2 0, 'lim'
	EXEC tas.Pr_Retrieve_VisitorPassLog_V2 0, '', '987456321'
	EXEC tas.Pr_Retrieve_VisitorPassLog_V2 0, '', '', 10003011
		
*/


