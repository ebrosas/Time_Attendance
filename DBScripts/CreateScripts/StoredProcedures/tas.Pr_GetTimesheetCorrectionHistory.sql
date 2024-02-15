/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetCorrectionHistory
*	Description: Get the employee's attendance records by date period
*
*	Date			Author		Rev. #		Comments:
*	19/07/2016		Ervin		1.0			Created
*	29/12/2016		Ervin		1.1			Added "CorrectionDesc" in the query results
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetCorrectionHistory
(   
	@autoID			INT,
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
			@ORDERBY					= ' ORDER BY Autoid',
			@chrQuote					= '''',
			@CMDMaster2					= '',
			@totalRecords				= 0,
			@recordCount				= 0 

	-- Set the starting and ending index
	SET @startIndex	= (@pageNumber * @pageSize) - @pageSize + 1
	SET @endIndex	= (@startIndex + @pageSize) - 1 
	SET @recordCount = @pageNumber * @pageSize

	SET @CMDTotalRecords = 'SELECT COUNT(*) 
							FROM tas.AUDIT_Tran_Timesheet a
								INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID '

	SET @CMD = 'SELECT	a.Autoid,
					a.XID_AutoID,
					a.CorrectionCode,
					LTRIM(RTRIM(e.DRDL01)) AS CorrectionDesc, 
					a.LastUpdateUser,
					a.EmpNo,
					c.EmpName,
					b.BusinessUnit,
					d.BusinessUnitName,
					a.DT,
					a.dtIN,
					a.dtOUT,
					CASE WHEN a.ShiftAllowance = 1 THEN ''Y'' ELSE '''' END AS ShiftAllowance,
					a.OTType,
					a.OTStartTime,
					a.OTEndTime,
					a.NoPayHours,
					a.AbsenceReasonCode,
					a.LeaveType,
					a.RemarkCode,
					a.DIL_Entitlement,
					b.Processed,
					a.action_time,
					a.action_machine,
					a.action_type
			FROM tas.AUDIT_Tran_Timesheet a
				INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID
				INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo
				LEFT JOIN tas.Master_BusinessUnit_JDE d ON RTRIM(a.BusinessUnit) = RTRIM(d.BusinessUnit)
				LEFT JOIN tas.syJDE_F0005 e ON LTRIM(RTRIM(e.DRSY)) = ''55'' AND LTRIM(RTRIM(e.DRRT)) = ''T0'' AND RTRIM(a.CorrectionCode) = LTRIM(RTRIM(e.DRKY)) '
	
	--Add @autoID filter
	IF @autoID IS NOT NULL
		SELECT @WHERE = @WHERE + CASE WHEN (@WHERE <> 'WHERE') THEN ' AND ' ELSE ' ' END + ' XID_AutoID = ' + RTRIM(CAST(@autoID AS VARCHAR(10))) 

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
			' AS TotalRecords, ROW_NUMBER() OVER (ORDER BY Autoid ASC) as RowNumber 
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

	SELECT TOP 100 * FROM tas.AUDIT_Tran_Timesheet a
	WHERE a.Processed = 1
	ORDER BY a.LastUpdateTime DESC

PARAMETERS:
	@autoID				INT,
	@pageNumber			INT = 1,
	@pageSize			INT = 10	

	EXEC tas.Pr_GetTimesheetCorrectionHistory 4619954

*/


