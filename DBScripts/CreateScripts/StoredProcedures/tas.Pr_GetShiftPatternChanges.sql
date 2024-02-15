/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftPatternChanges
*	Description: Get the list of employees with changes in the Shift Pattern
*
*	Date			Author		Revision No.	Comments:
*	09/06/2016		Ervin		1.0				Created
*	29/01/2017		Ervin		1.1				Added "SupervisorEmpNo" and "SupervisorEmpName" fields
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftPatternChanges
(   
	@autoID			INT = 0,
	@loadType		TINYINT = 0,	--(Note: 0 = All; 1 = Employee; 2 = Fire Team Member)
	@empNo			INT = 0,
	@changeType		VARCHAR(10) = NULL,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
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

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@changeType, '') = ''
		SET @changeType = NULL

	IF ISNULL(@startDate, '') = ''
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_ShiftPatternChanges a
								INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8 '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						LTRIM(RTRIM(b.YAALPH)) AS EmpName,
						LTRIM(RTRIM(ISNULL(c.JMDL01, ''''))) AS Position,
			
						CASE WHEN ISNULL(e.WorkingBusinessUnit, '''') <> ''''
							THEN LTRIM(RTRIM(e.WorkingBusinessUnit))
							ELSE
								CASE WHEN LTRIM(RTRIM(d.ABAT1)) = ''E'' THEN LTRIM(RTRIM(b.YAHMCU))
									WHEN LTRIM(RTRIM(d.ABAT1)) = ''UG'' THEN LTRIM(RTRIM(d.ABMCU)) 
								END
						END AS BusinessUnit,
						LTRIM(RTRIM(f.MCDC)) AS BusinessUnitName,

						a.EffectiveDate,
						a.EndingDate,
						a.ShiftPatCode,
						a.ShiftPointer,
						a.ChangeType,
						CASE WHEN RTRIM(a.ChangeType) = ''D''
							THEN ''Permanent''
							ELSE ''Temporary''
						END  AS ChangeTypeDesc,
						a.LastUpdateUser,
						a.LastUpdateTime,
						CAST(b.YAANPA AS INT) AS SupervisorEmpNo,
						LTRIM(RTRIM(g.YAALPH)) AS SupervisorEmpName  
				FROM tas.Tran_ShiftPatternChanges a
					INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
					LEFT JOIN tas.syJDE_F08001 c on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(c.JMJBCD))
					LEFT JOIN tas.syJDE_F0101 d ON b.YAAN8 = d.ABAN8
					LEFT JOIN tas.Master_EmployeeAdditional e ON CAST(b.YAAN8 AS INT) = e.EmpNo
					LEFT JOIN tas.syJDE_F0006 f ON 
						(
							CASE WHEN ISNULL(e.WorkingBusinessUnit, '''') <> ''''
								THEN LTRIM(RTRIM(e.WorkingBusinessUnit))
								ELSE
									CASE WHEN LTRIM(RTRIM(d.ABAT1)) = ''E'' THEN LTRIM(RTRIM(b.YAHMCU))
										WHEN LTRIM(RTRIM(d.ABAT1)) = ''UG'' THEN LTRIM(RTRIM(d.ABMCU)) 
									END
							END
						) = LTRIM(RTRIM(f.MCMCU))
					LEFT JOIN tas.syJDE_F060116 g ON b.YAANPA = g.YAAN8 '

	--Add Auto ID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) + ')'

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add Change Type filter
	IF @changeType IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.ChangeType) = '+ @chrQuote + RTRIM(@changeType) + @chrQuote    

	--Add Effective Date filter
	IF (@startDate IS NOT NULL AND @endDate IS NOT NULL)
	BEGIN

		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.EffectiveDate, 12) BETWEEN ' + @chrQuote + CONVERT(varchar, @startDate, 12) + @chrQuote 
			+ ' AND ' + @chrQuote + CONVERT(varchar, @endDate, 12) + @chrQuote      
	END

	IF @loadType = 1	--Employee
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.EmpNo NOT IN (SELECT EmpNo FROM tas.Master_FireteamMember) ' 
	ELSE IF @loadType = 2	--Fire Team Member
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' a.EmpNo IN (SELECT EmpNo FROM tas.Master_FireteamMember) ' 

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
	@autoID			INT = 0,
	@loadType		TINYINT = 0,	--(Note: 0 = All; 1 = Employee; 2 = Fire Team Member)
	@empNo			INT = 0,
	@changeType		VARCHAR(10) = NULL,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@pageNumber		INT = 1,
	@pageSize		INT = 10	

	EXEC tas.Pr_GetShiftPatternChanges 
	EXEC tas.Pr_GetShiftPatternChanges 19030			--AutoID
	EXEC tas.Pr_GetShiftPatternChanges 0, 1				--Employee
	EXEC tas.Pr_GetShiftPatternChanges 0, 2, 10003412, '', '01/01/2016', '12/31/2016' 		--Fire Team Member

	SELECT * FROM tas.Tran_ShiftPatternChanges a
	ORDER BY a.LastUpdateTime DESC

*/


