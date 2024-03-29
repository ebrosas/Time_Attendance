USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetEmployeeSwipeInfo]    Script Date: 29/11/2020 10:50:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
*	31/10/2016		Ervin		1.3			Added filter condition to exclude records wherein the value of Dev is either 8 or 9
*	07/07/2020		Ervin		1.4			Refactored the code to enhance performance
*	08/08/2020		Ervin		1.5			Added union to "Vw_CarParkSwipeData" view, and returned the top 5000 records 
*	24/09/2020		Ervin		1.6			Implemented the new workplace reader from "unis_tenter" database
*	11/11/2020		Ervin		1.7			Added union to "Vw_NewReaderSwipeData" view
******************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetEmployeeSwipeInfo]
(
	@startDate			DATETIME,
	@endDate			DATETIME,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12)	= '',
	@locationName		VARCHAR(40)	= '',
	@readerName			VARCHAR(40)	= ''
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL
		
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@locationName, '') = ''
		SET @locationName = NULL

	IF ISNULL(@readerName, '') = ''
		SET @readerName = NULL
	
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
		b.Position,
		RTRIM(b.BusinessUnit) AS CostCenter,
		RTRIM(c.BusinessUnitName) AS CostCenterName,
		d.Effective_ShiftPatCode AS ShiftPatCode,
		d.Effective_ShiftPointer AS ShiftPointer,
		d.Effective_ShiftCode AS ShiftCode,
		CAST(b.SupervisorNo AS FLOAT) AS SupervisorNo,
		LTRIM(RTRIM(f.YAALPH)) AS SupervisorName,
		c.CostCenterManager AS ManagerNo,		
		LTRIM(RTRIM(g.YAALPH)) AS ManagerName,
		CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 THEN 1 ELSE 0 END AS IsContractor					
	FROM
	(	 
		--Start of Rev. #1.5
		SELECT	a.EmpNo,  
				CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) as SwipeDate,
				CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 126)) as SwipeTime,
				RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
				CASE WHEN UPPER(RTRIM(b.Direction)) = 'I' THEN 'In' 
					WHEN UPPER(RTRIM(b.Direction)) = 'O' THEN 'Out' 
					ELSE '' 
				END AS SwipeType
		FROM
        (
			SELECT	TOP 5000
				CASE WHEN ISNUMERIC(a.FName) = 1 
					THEN 
						CASE WHEN CONVERT(INT, a.FName) <= 9999 
							THEN CONVERT(INT, a.FName) + 10000000
							ELSE CONVERT(INT, a.FName) 
						END
					ELSE 0 
				END AS EmpNo,
				a.TimeDate AS DT,
				a.Loc AS LocationCode,
				a.Dev AS ReaderNo,
				a.[Event] AS EventCode,
				'A' 'Source'
			FROM tas.External_DSX_evnlog a WITH (NOLOCK)
			WHERE ISNULL(a.FName, '') <> ''				
			ORDER BY a.TimeDate DESC

			UNION ALL  

			--Get swipe data from the Car Park #5
			SELECT	TOP 5000 
					a.EmpNo, 
					a.SwipeDateTime AS DT,
					a.LocationCode,
					a.ReaderNo,
					CASE WHEN a.EventCode = 0 THEN 8 ELSE a.EventCode END AS EventCode,
					a.Source
			FROM tas.Vw_CarParkSwipeData a WITH (NOLOCK)
			WHERE a.EmpNo > 0
			ORDER BY a.SwipeDateTime DESC 

			UNION ALL
            
			--Get swipe data from the new readers that use "UNIS_TENTER" database (Rev. #1.7)
			SELECT	TOP 10000
					a.EmpNo, 
					a.SwipeDateTime AS DT,
					a.LocationCode,
					a.ReaderNo,
					CASE WHEN a.EventCode = 0 THEN 8 ELSE a.EventCode END AS EventCode,
					a.SOURCE
			FROM tas.Vw_NewReaderSwipeData a WITH (NOLOCK)
			WHERE a.EmpNo > 0
			ORDER BY a.SwipeDateTime DESC
		) a
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
		WHERE a.ReaderNo NOT IN (8, 9)	--Rev. #1.3 (Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)    
			AND a.EventCode = 8	--(Note: 8 means successful swipe)
			AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) BETWEEN @startDate AND @endDate
			AND (UPPER(RTRIM(b.LocationName)) = UPPER(RTRIM(@locationName)) OR @locationName IS NULL)
			AND (UPPER(RTRIM(b.ReaderName)) = UPPER(RTRIM(@readerName)) OR @readerName IS NULL)
		--End of Rev. #1.5

		--SELECT 
		--	CASE WHEN ISNUMERIC(a.FName) = 1 
		--	THEN 
		--		CASE WHEN (CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000
		--		THEN 
		--			CONVERT(INT, a.FName)
		--		ELSE 
		--			CONVERT(INT, a.FName) + 10000000 
		--		END
		--	ELSE 0 
		--	END AS EmpNo,
		--	CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) as SwipeDate,
		--	CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) as SwipeTime,
		--	RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
		--	(
		--		CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'In' 
		--				WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'Out' 
		--				ELSE '' END
		--	) AS SwipeType
		--FROM tas.sy_EvnLog a WITH (NOLOCK)
		--	INNER JOIN tas.Master_AccessReaders c WITH (NOLOCK) ON a.Loc = c.LocationCode AND a.Dev = c.ReaderNo
		--WHERE 
		--	CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) BETWEEN @startDate AND @endDate
		--	AND a.[Event] = 8	--(Note: 8 means successful swipe)
		--	AND (UPPER(RTRIM(LocationName)) = UPPER(RTRIM(@locationName)) OR @locationName IS NULL)
		--	AND (UPPER(RTRIM(ReaderName)) = UPPER(RTRIM(@readerName)) OR @readerName IS NULL)
		--	AND a.Dev NOT IN (8, 9)	--Rev. #1.3 (Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)        
	) a
	INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
	LEFT JOIN tas.Master_BusinessUnit_JDE_V2 c WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit) 	
	LEFT JOIN tas.Tran_ShiftPatternUpdates d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND d.DateX = a.SwipeDate
	LEFT JOIN tas.syJDE_F060116 f WITH (NOLOCK) ON B.SupervisorNo = CAST(F.YAAN8 AS INT)
	LEFT JOIN tas.syJDE_F060116 g WITH (NOLOCK) ON C.CostCenterManager = CAST(g.YAAN8 AS INT)
	LEFT JOIN tas.Master_ContractEmployee i WITH (NOLOCK) ON A.EmpNo = i.EmpNo
	WHERE A.EmpNo > 0
		AND (A.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(B.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)

	UNION
	
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
		b.Position,
		RTRIM(b.BusinessUnit) AS CostCenter,
		RTRIM(c.BusinessUnitName) AS CostCenterName,
		d.Effective_ShiftPatCode AS ShiftPatCode,
		d.Effective_ShiftPointer AS ShiftPointer,
		d.Effective_ShiftCode AS ShiftCode,
		CAST(b.SupervisorNo AS FLOAT) AS SupervisorNo,
		LTRIM(RTRIM(f.YAALPH)) AS SupervisorName,
		c.CostCenterManager AS ManagerNo,		
		LTRIM(RTRIM(g.YAALPH)) AS ManagerName,
		CASE WHEN (a.EmpNo >= 10000 OR a.EmpNo >= 50000) AND a.EmpNo < 10000000 THEN 1 ELSE 0 END AS IsContractor	
	FROM
	(	 
		--Get workplace swipe records
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
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) AS SwipeDate,
			CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) AS SwipeTime,
			RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
			'In/Out' AS SwipeType
		FROM tas.sy_ExtLog a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders c WITH (NOLOCK) ON a.Loc = c.LocationCode AND a.Dev = c.ReaderNo
		WHERE CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) BETWEEN @startDate AND @endDate
			AND a.[Event] = 8	--(Note: 8 means successful swipe)
			AND (UPPER(RTRIM(LocationName)) = UPPER(RTRIM(@locationName)) OR @locationName IS NULL)
			AND (UPPER(RTRIM(ReaderName)) = UPPER(RTRIM(@readerName)) OR @readerName IS NULL)

		UNION
        
		--Get Workplace swipes (Note: New reader where data is fetch from "unis_tenter" database)	Rev. #1.6
		SELECT	a.EmpNo,
				a.SwipeDate,
				a.SwipeDateTime AS SwipeTime,
				RTRIM(b.LocationName)  + ' - ' + RTRIM(b.ReaderName) AS SwipeLocation,
				'In/Out' AS SwipeType
		FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
		WHERE a.EventCode = 8	--(Note: 8 means successful swipe)

		/*************************************************************************************************************
			Rev. #1.1 - Get manual swipe attendance records at the workplace which is already processed in Timesheet
		*************************************************************************************************************/
		UNION

		SELECT a.EmpNo,
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
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType IN (1, 2)
			AND a.SwipeDate BETWEEN @startDate AND @endDate		--Rev. #1.2

		UNION

		SELECT	a.EmpNo,
				a.SwipeDate,
				a.TimeInWP AS SwipeTime,
				'Workplace Manual Swipe' AS SwipeLocation,
				'In' AS SwipeType
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType = 3
			AND a.SwipeDate BETWEEN @startDate AND @endDate		--Rev. #1.2

		UNION 
	
		SELECT	a.EmpNo,
				a.SwipeDate,
				a.TimeOutWP AS SwipeTime,
				'Workplace Manual Swipe' AS SwipeLocation,
				'Out' AS SwipeType
		FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
		WHERE a.IsProcessedByTimesheet = 1
			AND a.CorrectionType = 3
			AND a.SwipeDate BETWEEN @startDate AND @endDate	
	) a
	INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
	LEFT JOIN tas.Master_BusinessUnit_JDE_V2 c WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)	
	LEFT JOIN tas.Tran_ShiftPatternUpdates d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND d.DateX = a.SwipeDate
	LEFT JOIN tas.syJDE_F060116 f WITH (NOLOCK) ON b.SupervisorNo = CAST(f.YAAN8 AS INT)
	LEFT JOIN tas.syJDE_F060116 g WITH (NOLOCK) ON c.CostCenterManager = CAST(g.YAAN8 AS INT)
	LEFT JOIN tas.Master_ContractEmployee i WITH (NOLOCK) ON a.EmpNo = i.EmpNo
	WHERE a.EmpNo > 0
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
	ORDER BY SwipeDate, IsContractor, CostCenter, EmpNo, SwipeTime DESC 

/*	Debug:

	EXEC tas.Pr_GetEmployeeSwipeInfo '11/01/2020', '11/30/2020', 10003632, '', '', ''

PARAMETERS:
	@startDate			datetime,
	@endDate			datetime,
	@empNo				int = 0,
	@costCenter			varchar(12)	= '',
	@locationName		varchar(40)	= '',
	@readerName			varchar(40)	= ''

*/