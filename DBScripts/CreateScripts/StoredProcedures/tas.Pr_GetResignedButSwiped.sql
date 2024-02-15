/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetResignedButSwiped
*	Description: Get the list of employees with changes in the Shift Pattern
*
*	Date			Author		Revision No.	Comments:
*	28/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetResignedButSwiped
(   
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= '',
	@pageNumber		INT = 1,
	@pageSize		INT = 10	
)
AS

	--Declare table variable
	DECLARE @tempTable					TABLE (totalRows int)

	--Declare variables
	DECLARE	@CONST_GRMTRAIN				VARCHAR(10),
			@userCostCenter				VARCHAR(12),
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

	SELECT	@CONST_GRMTRAIN				= 'GRMTRAIN',
			@userCostCenter				= '',
			@WHERE						=' WHERE 
											(
												ISNUMERIC(CASE WHEN (b.YAPAST IN (''R'', ''T'', ''E'', ''X'') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = ''I'') THEN ''0'' ELSE b.YAPAST END) = 0
												AND 
												CASE WHEN (b.YAPAST IN (''R'', ''T'', ''E'', ''X'') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = ''I'') THEN ''0'' ELSE b.YAPAST END <> ''T''
											)
											AND (a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL) ',
			@ORDERBY					= ' ORDER BY BusinessUnit, EmpNo, DT DESC',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDate, '') = CONVERT(DATETIME, '')
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = CONVERT(DATETIME, '')
		SET @endDate = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_Timesheet a
								INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
								LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
								LEFT JOIN tas.syJDE_F08001 d on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(d.JMJBCD)) '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						LTRIM(RTRIM(b.YAALPH)) AS EmpName,
						LTRIM(RTRIM(ISNULL(d.JMDL01, ''''))) AS Position,
						a.BusinessUnit,
						c.BusinessUnitName,
						a.DT,
						a.dtIN,
						a.dtOUT,
						CASE WHEN DATEDIFF(n, a.dtIN, a.dtOUT) < 0
							THEN DATEDIFF(n, a.dtIN, a.dtOUT) + (24 * 60)
							ELSE DATEDIFF(n, a.dtIN, a.dtOUT)
						END AS Duration,
						tas.ConvertFromJulian(b.YADT) AS DateResigned,
						CASE WHEN (b.YAPAST IN (''R'', ''T'', ''E'', ''X'') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = ''I'') THEN ''0'' ELSE b.YAPAST END AS PayStatus						
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
					LEFT JOIN tas.syJDE_F08001 d on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(d.JMJBCD)) '

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add Cost Center filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.BusinessUnit) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    

	--Add Date Period filter
	IF (@startDate IS NOT NULL AND @endDate IS NOT NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.DT, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote 
			+ ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote    
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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY BusinessUnit, EmpNo, DT DESC) as RowNumber 
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
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= '',
	@pageNumber		INT = 1,
	@pageSize		INT = 10	

	EXEC tas.Pr_GetResignedButSwiped '', '', 0, '', 2, 10 
	EXEC tas.Pr_GetResignedButSwiped '', '', 10001757, '', 1, 10
	EXEC tas.Pr_GetResignedButSwiped '01/01/2010', '31/12/2015', 10001757, '7600', 1, 10 
	EXEC tas.Pr_GetResignedButSwiped '01/01/2015', '31/12/2015', 0, '', 1, 10 

*/


