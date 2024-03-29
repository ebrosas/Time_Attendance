USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetMainGateSwipe]    Script Date: 11/08/2022 16:10:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetMainGateSwipe
*	Description: Retrieve the the swipe time in/out based on specific date
*
*	Date			Author		Rev. #		Comments:
*	23/10/2016		Ervin		1.0			Created
*	02/07/2020		Ervin		1.1			Modified the filter condition to return data for the curren date only
*	08/08/2020		Ervin		1.2			Implemented logic for Car Park #5 reader nos. 11 (IN) and 12 (OUT)
*	15/08/2021		Ervin		1.3			Implemented logic for the new Foil Mill turnstile and barrier readers
*	31/08/2021		Ervin		1.4			Implemented logic for the new Main Mill turnstile and barrier readers
*	06/04/2022		Ervin		1.5			Refactored the logic in fetching the swipe data based on whether the workplace reader is enabled for the employee
*	10/04/2022		Ervin		1.6			Added union to "Vw_AdminBldgReaderSwipe" view
*	12/04/2022		Ervin		1.7			Implemented logic to return the first in and last out from the Admin Bldg. reader swipe data
*	18/04/2022		Ervin		1.8			Added condition to set the valeu of "@isWorkplaceEnabled" flag depending on the effectivity date of the workplace reader
******************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetMainGateSwipe]
(     
	@empNo	INT,           
    @date	DATETIME = NULL   
)
As
BEGIN

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

    DECLARE	@fromDate				DATETIME,
			@toDate					DATETIME,
			@shiftCode				VARCHAR(10),
			@isWorkplaceEnabled		BIT = 0,
			@isAdminBldgEnabled		BIT = 0,
			@showFinalAdminSwipe	BIT = 1,		--(Notes: Set value to 1 to enable single swipe pair to be shown in the grid)
			@hasSwipedOutMainGate	BIT = 0,
			@effectiveDate			DATETIME = NULL,
			@costCenter				VARCHAR(12) = NULL 

	--Initialize variables
	SELECT	@fromDate = CONVERT(DATETIME, CONVERT(VARCHAR, @date, 12)),
			@toDate = CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, 1, @date)))

	--Determine if workplace swipe is enabled for the employee
	SELECT	@isWorkplaceEnabled = a.IsWorkplaceEnabled,
			@isAdminBldgEnabled = IsAdminBldgEnabled  
	FROM tas.fnCheckWorkplaceEnabled(@empNo) a

	--Start of Rev. #1.8
	IF @isWorkplaceEnabled = 1
	BEGIN

		--Get the employee's cost center
		SELECT @costCenter = RTRIM(a.BusinessUnit) 
		FROM tas.Master_Employee_JDE a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo

		--Get the workplace reader effectivity date
		SELECT TOP 1 @effectiveDate = a.EffectiveDate
		FROM tas.WorkplaceReaderSetting a WITH (NOLOCK) 
		WHERE a.IsActive = 1	
			AND RTRIM(a.CostCenter) = @costCenter
			
		--Set @isWorkplaceEnabled to false if @date is less than Effective Date
		IF @effectiveDate IS NOT NULL AND @date < @effectiveDate
			SET @isWorkplaceEnabled = 0
    END 
	--End of Rev. #1.8

	--Set the flag to identify whether the employee has swiped out in the main gate
	IF EXISTS
    (
		SELECT TOP 1 * FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK) 
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @date
			AND RTRIM(a.SwipeType) = 'OUT'
		ORDER BY a.SwipeTime DESC
	)
	SET @hasSwipedOutMainGate = 1
      
	--Get the shift pattern information
    SELECT @shiftCode = RTRIM(a.Effective_ShiftCode)
    FROM tas.Tran_ShiftPatternUpdates a WITH (NOLOCK)
    WHERE a.EmpNo = @empNo
		AND a.DateX = @fromDate

    IF @shiftCode = 'N'
		SET @fromDate = DATEADD(DAY, -1, @fromDate)

	IF @isWorkplaceEnabled = 1
	BEGIN
		
		IF @showFinalAdminSwipe = 1 AND @isAdminBldgEnabled = 1
		BEGIN
        
			SELECT a.DT, a.SwipeType, a.SwipeLocation
			FROM
			(
				SELECT	a.EmpNo, 
						a.DT, 
						a.LocationCode, 
						a.ReaderNo, 
						a.EventCode, 
						a.[Source],
						a.SwipeType, 
						a.SwipeLocation 
				FROM tas.frGetAdminReaderSwipe(@empNo, @date) a

				UNION

				SELECT	EmpNo, 
						DATEADD(MINUTE,CONVERT(INT, SUBSTRING(timeIN,3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING(timeIN,1,2)), dtIN)) AS DT, 
						-1 AS LocationCode, 
						-1 AS ReaderNo, 
						0 AS EventCode, 
						'' AS 'Source',
						'' AS SwipeType,
						'Manual' AS SwipeLocation
				FROM tas.Tran_ManualAttendance WITH (NOLOCK)

				UNION

				SELECT	EmpNo, 
						DATEADD(MINUTE,CONVERT(INT, SUBSTRING([timeOUT],3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING([timeOUT],1,2)), dtOut)) AS DT, 
						-1 AS LocationCode, 
						-2 AS ReaderNo, 
						0 AS EventCode, 
						'' AS 'Source',
						'' AS SwipeType,
						'Manual' AS SwipeLocation
				FROM tas.Tran_ManualAttendance WITH (NOLOCK)
			) a
			WHERE a.EmpNo = @empNo
				AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) = @date
			ORDER BY a.DT
		END 

		ELSE BEGIN

			SELECT a.EmpNo, a.SwipeDate, a.SwipeDateTime AS DT, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode AS ShiftCode
			INTO #AttendaceTable
			FROM tas.Vw_WorkplaceReaderSwipe a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DateX
			WHERE a.EmpNo = @empNo
				AND a.SwipeDate = @date
			GROUP BY a.EmpNo, a.SwipeDate, a.SwipeDateTime, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode

			UNION
        
			SELECT a.EmpNo, a.SwipeDate, a.SwipeDateTime AS DT, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode AS ShiftCode
			FROM tas.Vw_AdminBldgReaderSwipe a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DateX
			WHERE a.EmpNo = @empNo
				AND a.SwipeDate = @date
			GROUP BY a.EmpNo, a.SwipeDate, a.SwipeDateTime, a.LocationCode, a.ReaderNo, a.EventCode, b.Effective_ShiftCode
		

			SELECT a.EmpNo, a.SwipeDate, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, 'A' AS 'Source', 
				CASE WHEN a.ID%2 = 0 THEN --'OUT' 
					CASE WHEN @hasSwipedOutMainGate = 0 AND a.ID = a.SwipeCount THEN 'IN' ELSE 'OUT' END 
					ELSE 
						CASE WHEN a.SwipeCount = 1 AND RTRIM(a.ShiftCode) = 'N' THEN 'OUT' 
						ELSE --'IN' 
							CASE WHEN a.ID = a.SwipeCount AND a.SwipeCount > 1 AND @hasSwipedOutMainGate = 1 THEN 'OUT' ELSE 'IN' END 
						END 
				END AS SwipeType,
				a.ShiftCode, a.ID, a.SwipeCount
			INTO #AttendaceTable2
			FROM
			(
				SELECT ROW_NUMBER() OVER (PARTITION BY EmpNo, SwipeDate ORDER BY DT) AS ID, 		
					COUNT(*) OVER (PARTITION BY EmpNo, SwipeDate) AS SwipeCount, 
				* FROM #AttendaceTable	a	
				GROUP BY a.EmpNo, a.SwipeDate, a.DT,  a.LocationCode, a.ReaderNo, a.EventCode, a.ShiftCode
			) a

			SELECT a.EmpNo, a.DT, a.LocationCode, a.ReaderNo, a.EventCode, a.[Source], a.SwipeType, a.ShiftCode, a.ID, a.SwipeCount 
			INTO #WorkplaceTable
			FROM #AttendaceTable2 a

			SELECT a.DT, a.SwipeType, a.SwipeLocation
			FROM
			(
				SELECT	a.EmpNo, 
						a.DT, 
						a.LocationCode, 
						a.ReaderNo, 
						a.EventCode, 
						a.[Source],
						a.SwipeType, 
						'GARMCO' SwipeLocation 
				FROM #WorkplaceTable a

				UNION

				SELECT	EmpNo, 
						DATEADD(MINUTE,CONVERT(INT, SUBSTRING(timeIN,3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING(timeIN,1,2)), dtIN)) AS DT, 
						-1 AS LocationCode, 
						-1 AS ReaderNo, 
						0 AS EventCode, 
						'' AS 'Source',
						'' AS SwipeType,
						'Manual' AS SwipeLocation
				FROM tas.Tran_ManualAttendance WITH (NOLOCK)

				UNION

				SELECT	EmpNo, 
						DATEADD(MINUTE,CONVERT(INT, SUBSTRING([timeOUT],3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING([timeOUT],1,2)), dtOut)) AS DT, 
						-1 AS LocationCode, 
						-2 AS ReaderNo, 
						0 AS EventCode, 
						'' AS 'Source',
						'' AS SwipeType,
						'Manual' AS SwipeLocation
				FROM tas.Tran_ManualAttendance WITH (NOLOCK)
			) a
			WHERE a.EmpNo = @empNo
				AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) = @date
			ORDER BY a.DT

			-- Dropping temporary tables
			DROP TABLE #AttendaceTable
			DROP TABLE #AttendaceTable2
			DROP TABLE #WorkplaceTable
		END 
    END

	ELSE BEGIN
    
		SELECT  a.DT,
				CASE WHEN a.LocationCode IN (-1, 1) AND a.ReaderNo IN (-1, 4, 6, 8, 11, 13, 15) THEN 'IN'		--Rev. #1.4
					WHEN a.LocationCode IN (-1, 1) AND a.ReaderNo IN (-2, 5, 7, 9, 12, 14, 16) THEN 'OUT'		--Rev. #1.4
					WHEN a.LocationCode = 2 AND a.ReaderNo IN (0, 3, 31, 33) THEN 'IN'							--Rev. #1.3
					WHEN locationcode = 2 AND a.ReaderNo IN (1, 2, 32, 34) THEN 'OUT'							--Rev. #1.3
					ELSE '' 
				END AS SwipeType,
				CASE WHEN a.LocationCode = 1 THEN 'GARMCO' 
					WHEN a.LocationCode = 2 THEN 'FOILMIL' 
					ELSE '' 
				END AS SwipeLocation
		FROM tas.vuEmployeeAttendance a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.LocationCode IN (1, 2)
			AND CONVERT(DATETIME, CONVERT(VARCHAR, a.DT, 12)) = @date
		ORDER BY a.DT
    END 

END 

/*	Debugging:

	exec tas.Pr_GetMainGateSwipe 10001988, '04/14/2022'
	exec tas.Pr_GetMainGateSwipe 10003632, '04/13/2022'
	exec tas.Pr_GetMainGateSwipe 10003605, '04/11/2022'	
	exec tas.Pr_GetMainGateSwipe 10003726, '04/13/2022'	

*/