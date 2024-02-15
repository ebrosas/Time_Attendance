/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetReasonOfAbsence
*	Description: Fetch the reason of absences entry
*
*	Date			Author		Rev. #		Comments:
*	17/07/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetReasonOfAbsence
(   
	@autoID				INT = 0,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = '',
	@effectiveDate		DATETIME = NULL,
	@endingDate			DATETIME = NULL,
	@absenceReasonCode	VARCHAR(10) = '',
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

	IF ISNULL(@effectiveDate, '') = ''
		SET @effectiveDate = NULL

	IF ISNULL(@endingDate, '') = ''
		SET @endingDate = NULL

	IF ISNULL(@absenceReasonCode, '') = ''
		SET @absenceReasonCode = NULL

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.Tran_Absence a
								INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
								LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit) '

	SET @CMD = 'SELECT	a.AutoID,
						a.EmpNo,
						b.EmpName,
						b.Position,
						b.BusinessUnit,
						c.BusinessUnitName,
						a.EffectiveDate,
						a.EndingDate,
						CASE WHEN a.StartTime IS NOT NULL AND ISDATE(LEFT(a.StartTime, 2) + '':'' + RIGHT(a.StartTime, 2) + '':00'') = 1
							THEN CONVERT(TIME, LEFT(a.StartTime, 2) + '':'' + RIGHT(a.StartTime, 2) + '':00'')
							ELSE NULL
						END AS StartTime,
						CASE WHEN a.EndTime IS NOT NULL AND ISDATE(LEFT(a.EndTime, 2) + '':'' + RIGHT(a.EndTime, 2) + '':00'') = 1
							THEN CONVERT(TIME, LEFT(a.EndTime, 2) + '':'' + RIGHT(a.EndTime, 2) + '':00'')
							ELSE NULL
						END AS EndTime,
						a.[DayOfWeek],
						a.AbsenceReasonCode,
						LTRIM(RTRIM(d.DRDL01)) AS AbsenceReasonDesc,
						a.XID_TS_DIL_ENT,
						a.XID_TS_DIL_USD,
						a.LastUpdateUser,
						a.LastUpdateTime,
						a.DIL_ENT_CODE
				FROM tas.Tran_Absence a
					INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
					LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)
					LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.AbsenceReasonCode) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) + ''-'' + LTRIM(RTRIM(d.DRRT)) = ''55-RA'' '

	--Add Auto ID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) + ')'

	IF @empNo IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' (a.EmpNo = ' + RTRIM(CAST(@empNo AS VARCHAR(10))) + ')'

	--Add @costCenter filter
	IF @costCenter IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(b.BusinessUnit) = '+ @chrQuote + RTRIM(@costCenter) + @chrQuote    
			
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
			+ ' AND CONVERT(VARCHAR, a.EndingDate, 12) BETWEEN  ' + @chrQuote + CONVERT(varchar, @effectiveDate, 12) + @chrQuote + ' AND ' + @chrQuote + CONVERT(varchar, @endingDate, 12) + @chrQuote 

		--SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' CONVERT(VARCHAR, a.EffectiveDate, 12) >= ' + @chrQuote + CONVERT(varchar, @effectiveDate, 12) + @chrQuote 
		--	+ ' AND CONVERT(VARCHAR, a.EndingDate, 12) <= ' + @chrQuote + CONVERT(varchar, @endingDate, 12) + @chrQuote 
	END

	--Add @absenceReasonCode filter
	IF @absenceReasonCode IS NOT NULL
		SELECT @WHERE =  @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' RTRIM(a.AbsenceReasonCode) = '+ @chrQuote + RTRIM(@absenceReasonCode) + @chrQuote   

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

	--Retrieve all Absent Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-RA'
	ORDER BY LTRIM(RTRIM(DRKY))

	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRKY)) = 'DD'

	SELECT * FROM tas.Tran_Absence a 
	ORDER BY a.LastUpdateTime DESC

	SELECT * FROM tas.Tran_Absence a 
	WHERE ISNULL(DIL_ENT_CODE, '') <> ''
	ORDER BY a.EmpNo

	SELECT * FROM tas.Tran_Absence a 
	WHERE ISNULL(DayOfWeek, '') <> ''
	ORDER BY a.EmpNo

	SELECT * FROM tas.Tran_Absence a 
	WHERE a.EmpNo < 10000000
	ORDER BY a.EmpNo

PARAMETERS:
	@autoID				INT = 0,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = '',
	@effectiveDate		DATETIME = NULL,
	@endingDate			DATETIME = NULL,
	@absenceReasonCode	VARCHAR(10) = '',
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	EXEC tas.Pr_GetReasonOfAbsence 
	EXEC tas.Pr_GetReasonOfAbsence 153457							--By AutoID
	EXEC tas.Pr_GetReasonOfAbsence 0, 10003693						--By Employee No.
	EXEC tas.Pr_GetReasonOfAbsence 0, 0, '7600'						--By Cost Center
	EXEC tas.Pr_GetReasonOfAbsence 0, 0, '', '13/03/2016'			--By Effective Date 
	EXEC tas.Pr_GetReasonOfAbsence 0, 0, '', '', '12/04/2016'		--By Ending Date
	EXEC tas.Pr_GetReasonOfAbsence 0, 0, '', '01/01/2016', '31/01/2016'		--By Effective Date and Ending Date
	EXEC tas.Pr_GetReasonOfAbsence 0, 0, '', '', '', 'DD'			--By AbsenceReasonCode

*/


