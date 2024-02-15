/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpShiftPatternInfo
*	Description: Get the shift pattern information of an employee
*
*	Date			Author		Rev. #		Comments:
*	03/11/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmpShiftPatternInfo
(   
	@autoID				INT = 0,
	@empNo				INT = 0,
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

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Master_EmployeeAdditional a
								INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = CAST(b.YAAN8 AS INT) '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						LTRIM(RTRIM(b.YAALPH)) AS EmpName,
						LTRIM(RTRIM(ISNULL(e.JMDL01, ''''))) AS Position,
						a.ShiftPatCode,
						a.ShiftPointer,
						a.WorkingBusinessUnit,
						LTRIM(RTRIM(f.BUname)) AS WorkingBusinessUnitName, 
						CASE WHEN LTRIM(RTRIM(c.ABAT1)) = ''E'' THEN LTRIM(RTRIM(b.YAHMCU))
							WHEN LTRIM(RTRIM(c.ABAT1)) = ''UG'' THEN LTRIM(RTRIM(c.ABMCU)) 
						END AS ParentCostCenter,
						a.SpecialJobCatg,
						a.LastUpdateUser,
						a.LastUpdateTime 
				FROM tas.Master_EmployeeAdditional a
					INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = CAST(b.YAAN8 AS INT) 
					LEFT JOIN tas.syJDE_F0101 c ON b.YAAN8 = c.ABAN8
					LEFT JOIN tas.syJDE_F00092 d ON b.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = ''WH'' AND LTRIM(RTRIM(d.T3SDB)) = ''E''
					LEFT JOIN tas.syJDE_F08001 e on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
					LEFT JOIN tas.Master_BusinessUnit_JDE_view f ON RTRIM(a.WorkingBusinessUnit) = RTRIM(f.BU) '

	--Add Auto ID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) + ')'

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add @costCenter filter
	IF @costCenter IS NOT NULL
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (tas.fnGetEmployeeCostCenter(a.EmpNo) = ' + @chrQuote + RTRIM(@costCenter) + @chrQuote + ')'   

		--Add filter to return only active employees
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (ISNUMERIC(CASE WHEN (b.YAPAST IN (''R'', ''T'', ''E'', ''X'') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = ''I'') THEN ''0'' ELSE b.YAPAST END) = 1)'
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
	@autoID				INT = 0,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = '',
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	EXEC tas.Pr_GetEmpShiftPatternInfo 1840
	EXEC tas.Pr_GetEmpShiftPatternInfo 0, 0, '', 1, 10 
	EXEC tas.Pr_GetEmpShiftPatternInfo 0, 10003632
	EXEC tas.Pr_GetEmpShiftPatternInfo 0, 0, '7600'

*/


