/*****************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeSwipeInfo
*	Description: Retrieves the employee swipe history data
*
*	Date:			Author:		Rev.#:		Comments:
*	10/08/2014		Ervin		1.0			Created
*	24/07/2015		Ervin		1.1			Fetch the manual swipe records at the workplace which is already processed in the Timesheet
*	20/04/2016		Ervin		1.2			Added filter condition to compare the swipe date between the supplied date duration 
*	31/10/2016		EBrosas		1.3			Added filter condition to exclude records wherein the value of Dev is either 8 or 9
******************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeSwipeInfo
(
	@startDate			datetime,
	@endDate			datetime,
	@empNo				int = 0,
	@costCenter			varchar(12)	= '',
	@locationName		varchar(40)	= '',
	@readerName			varchar(40)	= ''
)

AS

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL
		
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@locationName, '') = ''
		SET @locationName = NULL

	IF ISNULL(@readerName, '') = ''
		SET @readerName = NULL
	
	--Get main gate swipe records
	SELECT DISTINCT
		a.SwipeDate,
		a.SwipeTime,
		a.SwipeLocation,
		a.SwipeType,
		a.EmpNo,
		CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 
			THEN RTRIM(i.ContractorEmpName)
			ELSE RTRIM(b.EmpName) 
			END AS EmpName, 
		LTRIM(RTRIM(ISNULL(h.JMDL01, ''))) AS Position,
		RTRIM(b.BusinessUnit) AS CostCenter,
		RTRIM(c.BusinessUnitName) AS CostCenterName,
		d.Effective_ShiftPatCode AS ShiftPatCode,
		d.Effective_ShiftPointer AS ShiftPointer,
		d.Effective_ShiftCode AS ShiftCode,
		e.YAANPA as SupervisorNo,
		RTRIM(f.EmpName) as SupervisorName,
		c.CostCenterManager AS ManagerNo,		
		RTRIM(g.EmpName) AS ManagerName,
		CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 THEN 1 ELSE 0 END AS IsContractor					
	FROM
	(	 
		SELECT 
			CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN (CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
			END AS EmpNo,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) as SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) as SwipeTime,
			RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
			(
				CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'In' 
						WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'Out' 
						ELSE '' END
			) AS SwipeType
		FROM tas.sy_EvnLog a
			INNER JOIN tas.Master_AccessReaders c ON a.Loc = c.LocationCode AND a.Dev = c.ReaderNo
		WHERE 
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) BETWEEN @startDate AND @endDate
			AND a.[Event] = 8	--(Note: 8 means successful swipe)
			AND (UPPER(RTRIM(LocationName)) = UPPER(RTRIM(@locationName)) OR @locationName IS NULL)
			AND (UPPER(RTRIM(ReaderName)) = UPPER(RTRIM(@readerName)) OR @readerName IS NULL)
			AND a.Dev NOT IN (8, 9)	--Rev. #1.3 (Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)        
	) a
	LEFT JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
	LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)	
	LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND d.DateX = a.SwipeDate
	LEFT JOIN tas.syJDE_F060116 e ON a.EmpNo = e.YAAN8
	LEFT JOIN tas.Master_Employee_JDE_View f on e.YAANPA = f.EmpNo
	LEFT JOIN tas.Master_Employee_JDE_View g on c.CostCenterManager = g.EmpNo
	LEFT JOIN tas.syJDE_F08001 h ON LTRIM(RTRIM(e.YAJBCD)) = LTRIM(RTRIM(h.JMJBCD))
	LEFT JOIN tas.Master_ContractEmployee i ON a.EmpNo = i.EmpNo
	WHERE
		a.EmpNo > 0
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)

	UNION

	--Get workplace swipe records
	SELECT DISTINCT
		a.SwipeDate,
		a.SwipeTime,
		a.SwipeLocation,
		a.SwipeType,
		a.EmpNo,
		CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 
			THEN RTRIM(i.ContractorEmpName)
			ELSE RTRIM(b.EmpName) 
			END AS EmpName, 
		LTRIM(RTRIM(ISNULL(h.JMDL01, ''))) AS Position,
		RTRIM(b.BusinessUnit) AS CostCenter,
		RTRIM(c.BusinessUnitName) AS CostCenterName,
		d.Effective_ShiftPatCode AS ShiftPatCode,
		d.Effective_ShiftPointer AS ShiftPointer,
		d.Effective_ShiftCode AS ShiftCode,
		e.YAANPA AS SupervisorNo,
		f.EmpName AS SupervisorName,
		c.CostCenterManager AS ManagerNo,		
		RTRIM(g.EmpName) AS ManagerName,
		CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 THEN 1 ELSE 0 END AS IsContractor	
	FROM
	(	 
		SELECT 
			CASE WHEN ISNUMERIC(a.FName) = 1 
			THEN 
				CASE WHEN (CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000
				THEN 
					CONVERT(INT, a.FName)
				ELSE 
					CONVERT(INT, a.FName) + 10000000 
				END
			ELSE 0 
			END AS EmpNo,			
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) as SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) as SwipeTime,
			RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
			
			'In/Out' AS SwipeType
			--CASE WHEN CONVERT(VARCHAR, a.TimeDate, 126) = (SELECT TOP 1 CONVERT(VARCHAR, TimeDate, 126) FROM tas.sy_ExtLog WHERE CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) BETWEEN @startDate AND @endDate AND RTRIM(FName) = RTRIM(a.FName) ORDER BY TimeDate DESC) 
			--	 THEN 'Out' 
			--	 ELSE 'In' 
			--	 END
			-- AS SwipeType
			--CASE WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'In' 
			--	 WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'Out' 
			--	 ELSE '' 
			--	 END
			-- AS SwipeType
		FROM tas.sy_ExtLog a
			INNER JOIN tas.Master_AccessReaders c ON a.Loc = c.LocationCode AND a.Dev = c.ReaderNo
		WHERE 
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) BETWEEN @startDate AND @endDate
			AND a.[Event] = 8	--(Note: 8 means successful swipe)
			AND (UPPER(RTRIM(LocationName)) = UPPER(RTRIM(@locationName)) OR @locationName IS NULL)
			AND (UPPER(RTRIM(ReaderName)) = UPPER(RTRIM(@readerName)) OR @readerName IS NULL)

		/*************************************************************************************************************
			Rev. #1.1 - Get manual swipe attendance records at the workplace which is already processed in Timesheet
		*************************************************************************************************************/
		UNION

		SELECT 
			a.EmpNo,
			a.SwipeDate,
			CASE WHEN CorrectionType = 1 THEN TimeInWP
				WHEN CorrectionType = 2 THEN TimeOutWP
				ELSE NULL
				END AS SwipeTime,
			'Workplace Manual Swipe' AS SwipeLocation,
			CASE WHEN CorrectionType = 1 THEN 'In'
				WHEN CorrectionType = 2 THEN 'Out'
				ELSE ''
				END AS SwipeType
		FROM tas.Tran_WorkplaceSwipe a
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType IN (1, 2)
			AND a.SwipeDate BETWEEN @startDate AND @endDate		--Rev. #1.2

		UNION

		SELECT 
			a.EmpNo,
			a.SwipeDate,
			a.TimeInWP AS SwipeTime,
			'Workplace Manual Swipe' AS SwipeLocation,
			'In' AS SwipeType
		FROM tas.Tran_WorkplaceSwipe a
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType = 3
			AND a.SwipeDate BETWEEN @startDate AND @endDate		--Rev. #1.2

		UNION 
	
		SELECT 
			a.EmpNo,
			a.SwipeDate,
			a.TimeOutWP AS SwipeTime,
			'Workplace Manual Swipe' AS SwipeLocation,
			'Out' AS SwipeType
		FROM tas.Tran_WorkplaceSwipe a
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType = 3
			AND a.SwipeDate BETWEEN @startDate AND @endDate	--Rev. #1.2
		/**************************************** End of Rev. #1.1 *******************************************************/
	) a
	LEFT JOIN tas.Master_Employee_JDE_View b ON a.EmpNo = b.EmpNo
	LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)	
	LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND d.DateX = a.SwipeDate
	LEFT JOIN tas.syJDE_F060116 e ON a.EmpNo = e.YAAN8
	LEFT JOIN tas.Master_Employee_JDE_View f on e.YAANPA = f.EmpNo
	LEFT JOIN tas.Master_Employee_JDE_View g on c.CostCenterManager = g.EmpNo
	LEFT JOIN tas.syJDE_F08001 h ON LTRIM(RTRIM(e.YAJBCD)) = LTRIM(RTRIM(h.JMJBCD))
	LEFT JOIN tas.Master_ContractEmployee i ON a.EmpNo = i.EmpNo
	WHERE
		a.EmpNo > 0
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
	--ORDER BY SwipeDate, IsContractor, CostCenter, EmpNo, SwipeTime
	ORDER BY SwipeDate, IsContractor, CostCenter, EmpNo, SwipeTime DESC 

GO


/*	Debugging:

Parameters:
	@startDate			datetime,
	@endDate			datetime,
	@empNo				int = 0,
	@costCenter			varchar(12)	= '',
	@locationName		varchar(40)	= '',
	@readerName			varchar(40)	= ''

	EXEC tas.Pr_GetEmployeeSwipeInfo  '08/16/2016', '09/15/2016', 10003632, '7600'
	EXEC tas.Pr_GetEmployeeSwipeInfo '8/21/2014', '8/21/2014', 0, '', '', 'Main gate Turnstile'
	EXEC tas.Pr_GetEmployeeSwipeInfo '8/18/2014', '8/18/2014', 10001404, ''
	EXEC tas.Pr_GetEmployeeSwipeInfo '8/16/2014', '8/17/2014', 10001516, ''
	EXEC tas.Pr_GetEmployeeSwipeInfo '8/18/2014', '8/18/2014', 0, '', 'GARMCO', 'HM Pulpit'	

	EXEC tas.Pr_GetEmployeeSwipeInfo '8/1/2014', '8/17/2014', 0, '3240'
	EXEC tas.Pr_GetEmployeeSwipeInfo '8/6/2014', '8/6/2014', 10001434, '3240'
	SELECT * FROM tas.sy_ExtLog WHERE LTRIM(RTRIM(FName)) LIKE '%1434%' and CONVERT(DATETIME, CONVERT(VARCHAR, TimeDate, 12)) = '8/6/2014' 
	SELECT * FROM tas.sy_EvnLog WHERE LTRIM(RTRIM(FName)) LIKE '%1434%' and CONVERT(DATETIME, CONVERT(VARCHAR, TimeDate, 12)) = '8/6/2014' 

	SELECT * FROM tas.sy_ExtLog ORDER BY TimeDate, FName
	
	SELECT * FROM tas.Master_AccessReaders
	
	SELECT TOP 10 * FROM tas.sy_EvnLog 
	WHERE CONVERT(VARCHAR, TimeDate, 12) = CONVERT(VARCHAR, GETDATE(), 12)
		AND CONVERT(INT, FName) = 3632 

	SELECT YAAN8, YAANPA FROM tas.syJDE_F060116

	SELECT CONVERT(VARCHAR, GETDATE(), 12)
	SELECT CONVERT(VARCHAR, GETDATE(), 126)
	SELECT CONVERT(VARCHAR, GETDATE(), 108)

*/