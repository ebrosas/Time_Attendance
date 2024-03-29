USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_SyncWorkplaceSwipeToTimesheet_V3]    Script Date: 08/04/2023 07:22:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_SyncWorkplaceSwipeToTimesheet_V3
*	Description: This stored procedure is used to update the timesheet and overtime records based on the swipes at the workplace readers. 
				 This stored procedure should be hosted in a scheduled service. The service should be triggered to run right after the execution of the Timesheet Processing Service 
*
*	Date:			Author:		Rev.#:		Comments:
*	11/09/2015		Ervin		1.0			Created
*	13/09/2015		Ervin		1.1			Used the BusinessUnit from "Master_Employee_JDE_View" and not from "Tran_Timesheet" in the join tables
*	25/09/2015		Ervin		1.2			Added "@syncWorkplaceToTimesheet" flag that is used to determine if the workplace swipe will be synchronized to Timesheet
*	10/10/2015		Ervin		1.3			Refactored the code in updating the Timesheet and overtime records based on the value of "@syncWorkplaceToTimesheet" flag
*	12/10/2015		Ervin		1.4			Added "@logRecordsProcessed" variable that is used to return number of log records processed
*	16/10/2015		Ervin		1.5			Refactored logic in fetching the shaving time. Removed the cost center join field condition to Tran_Timesheet
*	18/10/2015		Ervin		1.6			Added condition that check if Timesheet has single record without multiple swipes
*	29/10/2015		Ervin		1.7			Used the function "fnGetCorrectedMainGateWorkplaceSwipe" to correct invalid records in "Tran_WorkplaceSwipe" table
*	31/10/2015		Ervin		1.8			Refactored calculation of Overtime, NoPayHours, NetMinutes, and Duration_Worked_Cummulative if ShiftSpan = 1
*	09/11/2015		Ervin		1.9			Moved the code that correct the Tran_WorkplaceSwipe table before inserting or updating records in "SyncWorkplaceSwipeToTimesheetLog" table
*	13/12/2015		Ervin		2.0			Added condition that will update swipe records in "Tran_WorkplaceSwipe" table if Time-in MG/WP not equal 23:00:00, or Time-out MG/WP not equal 23:00:00  
*	13/12/2015		Ervin		2.1			Refactored the join condition between "Tran_TempSwipeData" and "Tran_Timesheet" tables
*	29/12/2015		Ervin		2.2			Commented condition that checks if log record already exists
*	17/01/2016		Ervin		2.3			Filter Timesheet and Overtime records to update based on Missing Swipes and Valid Swipes
*	20/01/2016		Ervin		2.4			Refactored logic in fetching the value for "ShavedIn_New" and "ShavedOut_New" fields
*	20/01/2016		Ervin		2.5			Added condition that will allow Timesheet update only when the value of "CorrectionCode" field is null
*	21/01/2016		Ervin		2.6			Added condition that check if DT equals to todays date
*	22/01/2016		Ervin		2.7			Added "@timeSheetRecordsProcessedMS" and "@overtimeRecordsProcessedMS" variables which are used to store Timesheet and OT records processed for Missing Swipes
*	08/02/2016		Ervin		2.8			Added filter condition to exclude employees that exist in "WorkplaceSwipeExclusion" table
*	11/02/2016		Ervin		2.9			Refactored code changes applied in Rev. #2.8
*	20/02/2016		Ervin		3.0			Modified the logic in getting the value for "dtIN_New", "dtOUT_New", "ShavedIn_New", "ShavedOut_New", "OTStartTime_New", and "OTEndTime_New" for missing swipes
*	09/03/2016		Ervin		3.1			Added code that check and correct the value of "Duration_Worked_Cumulative" and "NetMinutes" fields in Tran_Timesheet and SyncWorkplaceSwipeToTimesheetLog tables
*	22/03/2016		Ervin		3.2			Update the "OTStartTime" and "OTEndTime" in the Timesheet table if auto approve OT is activated
*	28/03/2016		Ervin		3.3			Added condition to update overtime in Tran_Timesheet_Extra table if auto approval is not activated
*	25/04/2016		Ervin		3.4			Modified the date filter in processing missing workplace swipes
*	16/06/2016		Ervin		3.5			Commented code applied in Rev. #2.5 when @actionTypeID = 1
*	22/06/2016		Ervin		3.6			Refactored the filter condition that checks the date in Tran_TempSwipeData table
*	30/01/2017		Ervin		3.7			Set the "NoPayHours" to the entire shift duration if employee has missing swipe at the workplace 
*	26/02/2017		Ervin		3.8			Added condition that checks the value of "LeaveType" field. If it is not null, then set NoPayHours to zero
*	09/03/2017		Ervin		3.9			Added condition that checks the value of "ShiftCode" field. If it is equal to 'O', then set NoPayHours to zero
*	09/03/2017		Ervin		4.0			Set the "Processed" to zero whenever the service is rerun
*	29/06/2017		Ervin		4.1			Modified the logic in calculating the overtime by fetching values from "fnGetProcessedWorkplaceDataToSync" UDF
*	20/11/2018		Ervin		4.2			Refactored the code to enhance performance
*	28/03/2020		Ervin		4.3			Added filter condition in updating the work duration in "Tran_Timesheet" table if it contains a single record only
*	01/02/2021		Ervin		4.4			Fixed the bug reported by HR through Helpdesk No. 122294. Added restriction not to update timesheet record if employee shift pattern is "SX" and shift code is "N"
*	15/04/2022		Ervin		4.5			Implemented synchronization of Admin Bldg. reader swipe data into the Timesheet
*	07/11/2022		Ervin		4.6			Added filter to exclude work duration greater than 900 hours or less than zero
*	29/01/2023		Ervin		4.7			Set the "Processed" to zero whenever executing the ff action: Undo workplace swipe sync to Timesheet
*
***************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_SyncWorkplaceSwipeToTimesheet_V3]
(
	@startDate		DATETIME,
	@endDate		DATETIME,
	@actionTypeID	INT = 0,		--(Note: 0 -> Update Timesheet; 1 -> Undo Timesheet Updates)
	@costCenter		VARCHAR(12) = NULL,
	@empNo			INT = NULL	
)
AS	
BEGIN

	--Tell SQL Engine not to return the row-count information
	--SET NOCOUNT ON 

	--Define constants
	DECLARE @CONST_RETURN_OK			int,
			@CONST_RETURN_ERROR			int

	--Initialize constants
	SELECT	@CONST_RETURN_OK			= 0,
			@CONST_RETURN_ERROR			= -1

	--Define variables
	DECLARE @syncWorkplaceToTimesheet		BIT,	--This flag determines whether the workplace swipe will be processed in the Timesheet
			@timeSheetRecordsProcessed		INT,
			@timeSheetRecordsProcessedMS	INT,
			@overtimeRecordsProcessed		INT,
			@overtimeRecordsProcessedMS		INT,
			@logRecordsProcessed			INT,
			@hasError						BIT,
			@retError						INT,
			@retErrorDesc					VARCHAR(200),				
			@logID							BIGINT,
			@empNoTemp						INT,
			@dtTemp							DATETIME

	--Initialize variables
	SELECT	@syncWorkplaceToTimesheet		= 0,
			@timeSheetRecordsProcessed		= 0,
			@timeSheetRecordsProcessedMS	= 0,
			@overtimeRecordsProcessed		= 0,
			@overtimeRecordsProcessedMS		= 0,
			@logRecordsProcessed			= 0,
			@hasError						= 0,
			@retError						= @CONST_RETURN_OK,
			@retErrorDesc					= '',
			@logID							= 0,
			@empNoTemp						= 0,
			@dtTemp							= NULL

	--Start a transaction
	BEGIN TRANSACTION

	BEGIN TRY

		--Initialize parameters
		IF ISNULL(@costCenter, '') = '' OR RTRIM(@costCenter) = '0'
			SET @costCenter = NULL

		IF ISNULL(@empNo, 0) = 0
			SET @empNo = NULL

		--Check the flag that determine whether the workplace swipe will be processed in the Timesheet
		SELECT TOP 1 @syncWorkplaceToTimesheet = ISNULL(SyncWorkplaceToTimesheet, 0)
		FROM tas.WorkplaceTimesheetSetting WITH (NOLOCK)
		WHERE IsActive = 1

		IF @actionTypeID = 0
		BEGIN
			
			/*************************************************************************
				Synchronize missing workplace swipes into the Timesheet. 
				Check if records exist in the processed swipe temporary table
			**************************************************************************/
			IF EXISTS
			(
				SELECT DISTINCT b.AutoID
				FROM tas.Tran_TempSwipeData a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) 
					LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
					LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON d.EmpNo = b.EmpNo AND d.SwipeDate = b.DT AND RTRIM(d.CostCenter) = RTRIM(b.BusinessUnit)
					INNER JOIN  tas.Master_Employee_JDE_View e WITH (NOLOCK) ON a.EmpNo = e.EmpNo
				WHERE 
					a.DT IS NULL
					AND b.IsLastRow = 1
					AND 
					(	
						(a.Direction = 'O' AND b.dtOUT IS NOT NULL)
						OR a.Direction = 'I' AND b.dtIN IS NOT NULL
					)	
					
					--Start of Rev. #3.6
					AND 
					(
						(b.DT BETWEEN @startDate AND @endDate AND @startDate < @endDate)
						OR 
						(b.DT = @startDate AND @startDate = @endDate)
					)	
					--End of Rev. #3.6		
					--AND 
					--(
					--	(
					--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) >= @startDate 
					--		AND 
					--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12)) <= @endDate
					--		AND 
					--		@endDate > @startDate
					--	)
					--	--Start of Rev. #3.4
					--	OR
					--	(
					--		b.DT = CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12))
					--		AND
					--		@startDate = @endDate
					--	)
					--	--End of Rev. #3.4
					--)

					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(e.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)						
					AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
					(
						SELECT 1	--LogID 
						FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
						WHERE AutoID = b.AutoID
							AND EmpNo = a.EmpNo 
							AND DT = b.DT 
							AND IsActive = 1
					)
					AND NOT	--(Note: Exclude morning and evening shifts where time-out is null)
					(
						(CASE WHEN ISNULL(b.Actual_ShiftCode, '') <> '' THEN b.Actual_ShiftCode ELSE b.ShiftCode END) IN ('M', 'E') 
						AND dtOUT IS NULL
						AND b.DT = CONVERT(DATETIME, GETDATE(), 101)	--Rev. #2.6
					) 		
					AND RTRIM(e.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
					AND 
					(
						(a.Direction = 'I' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
						OR (a.Direction = 'O' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
					)					
			)
			BEGIN

				/*****************************************************************************
					Rev. #1.7 - Correct invalid records in the "Tran_WorkplaceSwipe" table
				******************************************************************************/					
				DECLARE TimesheetCursor CURSOR READ_ONLY FOR
				SELECT b.EmpNo, b.DT
				FROM tas.Tran_TempSwipeData a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) 
					LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
					LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON b.EmpNo = d.EmpNo AND b.DT = d.SwipeDate AND RTRIM(b.BusinessUnit) = RTRIM(d.CostCenter)
					INNER JOIN  tas.Master_Employee_JDE_View e WITH (NOLOCK) ON a.EmpNo = e.EmpNo
				WHERE 
					a.DT IS NULL
					AND b.IsLastRow = 1
					AND 
					(	
						(a.Direction = 'O' AND b.dtOUT IS NOT NULL)
						OR a.Direction = 'I' AND b.dtIN IS NOT NULL
					)	
					
					--Start of Rev. #3.6
					AND 
					(
						(b.DT BETWEEN @startDate AND @endDate AND @startDate < @endDate)
						OR 
						(b.DT = @startDate AND @startDate = @endDate)
					)		
					--End of Rev. #3.6		
					--AND 
					--(
					--	(
					--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) >= @startDate 
					--		AND 
					--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12)) <= @endDate
					--		AND 
					--		@endDate > @startDate
					--	)						
					--	--Start of Rev. #3.4
					--	OR
					--	(
					--		b.DT = CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12))
					--		AND
					--		@startDate = @endDate
					--	)
					--	--End of Rev. #3.4
					--)

					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(e.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	

					--End of Rev. #2.2
					--AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
					--(
					--	SELECT LogID FROM tas.SyncWorkplaceSwipeToTimesheetLog
					--	WHERE AutoID = b.AutoID
					--		AND EmpNo = a.EmpNo 
					--		AND DT = b.DT 
					--		AND IsActive = 1
					--)
					--Start of Rev. #2.2

					AND NOT	--(Note: Exclude morning and evening shifts where time-out is null)
					(
						(CASE WHEN ISNULL(b.Actual_ShiftCode, '') <> '' THEN b.Actual_ShiftCode ELSE b.ShiftCode END) IN ('M', 'E') 
						AND dtOUT IS NULL
						AND b.DT = CONVERT(DATETIME, GETDATE(), 101)	--Rev. #2.6
					) 	
					AND RTRIM(e.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
					AND 
					(
						(a.Direction = 'I' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
						OR (a.Direction = 'O' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
					)

				OPEN TimesheetCursor
				FETCH NEXT FROM TimesheetCursor
				INTO @empNoTemp, @dtTemp

				WHILE @@FETCH_STATUS = 0
				BEGIN

					IF ISNULL(@empNoTemp, 0) > 0 AND ISNULL(@dtTemp, '') <> ''
					BEGIN

						UPDATE tas.Tran_WorkplaceSwipe
						SET tas.Tran_WorkplaceSwipe.TimeInMG = b.TimeInMG,
							tas.Tran_WorkplaceSwipe.TimeOutMG = b.TimeOutMG,
							tas.Tran_WorkplaceSwipe.TimeInWP = b.TimeInWP,
							tas.Tran_WorkplaceSwipe.TimeOutWP = b.TimeOutWP,
							tas.Tran_WorkplaceSwipe.NetMinutesMG = b.NetMinutesMG,
							tas.Tran_WorkplaceSwipe.NetMinutesWP = b.NetMinutesWP
						FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
							CROSS APPLY tas.fnGetCorrectedMainGateWorkplaceSwipe(a.EmpNo, a.SwipeDate) b								
						WHERE a.SwipeDate = @dtTemp
							AND a.EmpNo = @empNoTemp
							AND 
							(
								(CONVERT(TIME, a.TimeInMG) <> CONVERT(TIME, '23:00:00') AND CONVERT(TIME, a.TimeInWP) <> CONVERT(TIME, '23:00:00'))
								OR
								(CONVERT(TIME, a.TimeOutMG) <> CONVERT(TIME, '23:00:00') AND CONVERT(TIME, a.TimeOutWP) <> CONVERT(TIME, '23:00:00'))
							)
					END

					-- Retrieve next record
					FETCH NEXT FROM TimesheetCursor
					INTO @empNoTemp, @dtTemp
				END

				-- Close and deallocate
				CLOSE TimesheetCursor
				DEALLOCATE TimesheetCursor
				/********************************* End **************************************/

				--Insert transaction log records
				IF NOT EXISTS
				(
					SELECT 1	--LogID 
					FROM tas.SyncWorkplaceSwipeToTimesheetLog a WITH (NOLOCK)
					WHERE AutoID IN
					(
						SELECT DISTINCT b.AutoID
						FROM tas.Tran_TempSwipeData a WITH (NOLOCK)
							INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) 
							LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
							LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON d.EmpNo = b.EmpNo AND d.SwipeDate = b.DT AND RTRIM(d.CostCenter) = RTRIM(b.BusinessUnit)
							INNER JOIN  tas.Master_Employee_JDE_View e WITH (NOLOCK) ON a.EmpNo = e.EmpNo
						WHERE a.DT IS NULL
							AND b.IsLastRow = 1
							AND 
							(	
								(a.Direction = 'O' AND b.dtOUT IS NOT NULL)
								OR a.Direction = 'I' AND b.dtIN IS NOT NULL
							)	
							
							--Start of Rev. #3.6
							AND 
							(
								(b.DT BETWEEN @startDate AND @endDate AND @startDate < @endDate)
								OR 
								(b.DT = @startDate AND @startDate = @endDate)
							)	
							--End of Rev. #3.6				
							--AND 
							--(
							--	(
							--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) >= @startDate 
							--		AND 
							--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12)) <= @endDate
							--		AND 
							--		@endDate > @startDate
							--	)								
							--	--Start of Rev. #3.4
							--	OR
							--	(
							--		b.DT = CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12))
							--		AND
							--		@startDate = @endDate
							--	)
							--	--End of Rev. #3.4
							--)

							AND (a.EmpNo = @empNo OR @empNo IS NULL)
							AND (RTRIM(e.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)										
							AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
							(
								SELECT 1	--LogID 
								FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
								WHERE AutoID = b.AutoID
									AND EmpNo = a.EmpNo 
									AND DT = b.DT 
									AND IsActive = 1
							)
							AND NOT	--(Note: Exclude morning and evening shifts where time-out is null)
							(
								(CASE WHEN ISNULL(b.Actual_ShiftCode, '') <> '' THEN b.Actual_ShiftCode ELSE b.ShiftCode END) IN ('M', 'E') 
								AND dtOUT IS NULL
								AND b.DT = CONVERT(DATETIME, GETDATE(), 101)	--Rev. #2.6
							) 	
							AND RTRIM(e.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
							AND 
							(
								(a.Direction = 'I' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
								OR (a.Direction = 'O' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
							)							
					)
				)
				BEGIN									

					--Insert new log records
					INSERT INTO tas.SyncWorkplaceSwipeToTimesheetLog
					(
						LogDate,
						AutoID,
						DT,
						CostCenter,
						EmpNo,

						dtIN_Old,
						dtIN_New,
						dtOUT_Old,
						dtOUT_New,

						ShavedIn_Old,
						ShavedIn_New,
						ShavedOut_Old,
						ShavedOut_New,

						OTStartTime_Old,
						OTStartTime_New,
						OTEndTime_Old,
						OTEndTime_New,

						NoPayHours_Old,
						NoPayHours_New,
						DurationWorkedCumulative_Old,
						DurationWorkedCumulative_New,
						NetMinutes_Old,
						NetMinutes_New,
						LastUpdateUser,
						LastUpdateTime
					)
					SELECT	DISTINCT
							GETDATE() AS LogDate, 
							b.AutoID,
							b.DT,
							e.BusinessUnit AS CostCenter,
							b.EmpNo,

							b.dtIN AS dtIN_Old,
							CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL) 	--(Note: No workplace swipe-out but has swipe-in time)			
									OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)	--Rev. #3.0
								THEN d.TimeInWP		
								ELSE NULL 
							END AS dtIN_New,

							b.dtOUT AS dtOUT_Old,
							CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL) 	--(Note: No workplace swipe-in but has swipe-out time)
									OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)	--Rev. #3.0
								THEN d.TimeOutWP		
								ELSE NULL 
							END AS dtOUT_New,

							b.Shaved_IN AS ShavedIn_Old,
							CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL)	--(Note: No workplace swipe-out but has swipe-in time)
									OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)	--Rev. #3.0
								THEN 
									--Start of Rev. #2.4
									tas.fnGetShavingTime
									(
										0, 
										d.TimeInWP, 
										RTRIM(b.ShiftPatCode), 
										CASE WHEN RTRIM(b.ShiftCode) = 'O' 
											THEN RTRIM(b.Actual_ShiftCode) 
											ELSE 
												CASE WHEN ISNULL(b.ShiftCode, '') = '' 
													THEN RTRIM(b.Actual_ShiftCode) 
													ELSE RTRIM(b.ShiftCode) 
												END
										END
									) 
									--End of Rev. #2.4
								ELSE NULL 
							END AS ShavedIn_New,

							b.Shaved_OUT AS ShavedOut_Old,
							CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL)	--(Note: No workplace swipe-in but has swipe-out time)
									OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)	--Rev. #3.0
								THEN 
									--Start of Rev. #2.4
									tas.fnGetShavingTime
									(
										1, 
										d.TimeOutWP, 
										RTRIM(b.ShiftPatCode), 
										CASE WHEN RTRIM(b.ShiftCode) = 'O' 
											THEN RTRIM(b.Actual_ShiftCode) 
											ELSE 
												CASE WHEN ISNULL(b.ShiftCode, '') = '' 
													THEN RTRIM(b.Actual_ShiftCode) 
													ELSE RTRIM(b.ShiftCode) 
												END
										END
									)
									--End of Rev. #2.4
								ELSE NULL 
							END AS ShavedOut_New,

							CASE WHEN b.OTStartTime IS NOT NULL 
								THEN b.OTStartTime 
								ELSE c.OTstartTime 
							END AS OTStartTime_Old,

							CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL)	--(Note: No workplace swipe-out but has swipe-in time)
									OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)	--Rev. #3.0
								THEN 
									CASE WHEN b.OTStartTime IS NOT NULL 
										THEN b.OTStartTime 
										ELSE c.OTstartTime 
									END
								ELSE NULL 
							END AS OTStartTime_New,

							CASE WHEN b.OTEndTime IS NOT NULL 
								THEN b.OTEndTime 
								ELSE c.OTendTime 
							END AS OTEndTime_Old,

							CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL)	--(Note: No workplace swipe-in but has swipe-out time)
									OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)	--Rev. #3.0
								THEN 
									CASE WHEN b.OTEndTime IS NOT NULL 
										THEN b.OTEndTime 
										ELSE c.OTendTime 
									END
								ELSE NULL 
							END AS OTEndTime_New,

							b.NoPayHours AS NoPayHours_Old,
							CASE WHEN	--Rev. #3.7
								(CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL) 	
										OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)	
									THEN d.TimeInWP		
									ELSE NULL 
								END) IS NULL 
								OR 
								(CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL) 	
										OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)	
									THEN d.TimeOutWP		
									ELSE NULL 
								END) IS NULL
								THEN tas.fnGetRequiredWorkDuration(RTRIM(b.ShiftPatCode), RTRIM(ISNULL(b.Actual_ShiftCode, b.ShiftCode)))
								ELSE b.NoPayHours 
							END AS NoPayHours_New,
							--CASE WHEN ISNULL(b.NoPayHours, 0) > 0 
							--	THEN tas.fnGetRequiredWorkDuration(RTRIM(b.ShiftPatCode), RTRIM(ISNULL(b.Actual_ShiftCode, b.ShiftCode)))	--Rev. #3.7
							--	ELSE b.NoPayHours 
							--END AS NoPayHours_New,

							b.Duration_Worked_Cumulative AS DurationWorkedCumulative_Old,
							CASE WHEN b.Duration_Worked_Cumulative > 0 THEN 0 ELSE b.Duration_Worked_Cumulative END AS DurationWorkedCumulative_New,
							b.NetMinutes AS NetMinutes_Old,
							CASE WHEN b.NetMinutes > 0 THEN 0 ELSE b.NetMinutes END AS NetMinutes_New,
							'System Admin' AS LastUpdateUser,
							GETDATE() AS LastUpdateTime
					FROM tas.Tran_TempSwipeData a WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) 
						LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
						LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON b.EmpNo = d.EmpNo AND b.DT = d.SwipeDate AND RTRIM(b.BusinessUnit) = RTRIM(d.CostCenter)
						INNER JOIN  tas.Master_Employee_JDE_View e WITH (NOLOCK) ON a.EmpNo = e.EmpNo
					WHERE a.DT IS NULL
						AND b.IsLastRow = 1
						AND 
						(	
							(a.Direction = 'O' AND b.dtOUT IS NOT NULL)
							OR a.Direction = 'I' AND b.dtIN IS NOT NULL
						)	
						
						--Start of Rev. #3.6
						AND 
						(
							(b.DT BETWEEN @startDate AND @endDate AND @startDate < @endDate)
							OR 
							(b.DT = @startDate AND @startDate = @endDate)
						)	
						--End of Rev. #3.6				
						--AND 
						--(
						--	(
						--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) >= @startDate 
						--		AND 
						--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12)) <= @endDate
						--		AND 
						--		@endDate > @startDate
						--	)							
						--	--Start of Rev. #3.4
						--	OR
						--	(
						--		b.DT = CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12))
						--		AND
						--		@startDate = @endDate
						--	)
						--	--End of Rev. #3.4
						--)

						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND (RTRIM(e.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	
						AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
						(
							SELECT 1	--LogID 
							FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
							WHERE AutoID = b.AutoID
								AND EmpNo = a.EmpNo 
								AND DT = b.DT 
								AND IsActive = 1
						)
						AND NOT	--(Note: Exclude morning and evening shifts where time-out is null)
						(
							(CASE WHEN ISNULL(b.Actual_ShiftCode, '') <> '' THEN b.Actual_ShiftCode ELSE b.ShiftCode END) IN ('M', 'E') 
							AND dtOUT IS NULL
							AND b.DT = CONVERT(DATETIME, GETDATE(), 101)	--Rev. #2.6
						) 	
						AND RTRIM(e.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
						AND 
						(
							(a.Direction = 'I' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
							OR (a.Direction = 'O' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
						)
						--Start of Rev. #2.8
						AND NOT EXISTS
						(
							SELECT 1	--AutoID 
							FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
							WHERE EmpNo = a.EmpNo 
								AND RTRIM(CostCenter) = RTRIM(b.BusinessUnit)
								AND IsActive = 1
								AND @startDate >= EffectiveDate 
						)
						--End of Rev. #2.8

					--Get the number of log records processed
					SELECT @logRecordsProcessed = @@ROWCOUNT
				END

				ELSE
				BEGIN																																																																																								BEGIN
				
					--Update existing records
					UPDATE tas.SyncWorkplaceSwipeToTimesheetLog
					SET tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_Old = b.dtIN,
						tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_New = CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL)		--(Note: No workplace swipe-out but has swipe-in time)
																				OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)		--Rev. #3.0
																			THEN d.TimeInWP 
																			ELSE NULL 
																		END,

						tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_Old = b.dtOUT,
						tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_New = CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL)		--(Note: No workplace swipe-in but has swipe-out time)
																				OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)		--Rev. #3.0
																			THEN d.TimeOutWP 
																			ELSE NULL 
																		 END,

						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_Old = b.Shaved_IN,
						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_New = CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL)	--(Note: No workplace swipe-out but has swipe-in time)
																					OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)	--Rev. #3.0
																				THEN 
																					--Start of Rev. #2.4
																					tas.fnGetShavingTime
																					(
																						0, 
																						d.TimeInWP, 
																						RTRIM(b.ShiftPatCode), 
																						CASE WHEN RTRIM(b.ShiftCode) = 'O' 
																							THEN RTRIM(b.Actual_ShiftCode) 
																							ELSE 
																								CASE WHEN ISNULL(b.ShiftCode, '') = '' 
																									THEN RTRIM(b.Actual_ShiftCode) 
																									ELSE RTRIM(b.ShiftCode) 
																								END
																						END
																					)
																					--End of Rev. #2.4
																				ELSE NULL 
																			END,

						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_Old = b.Shaved_OUT,
						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_New = CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL)		--(Note: No workplace swipe-in but has swipe-out time)
																					OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)		--Rev. #3.0
																				THEN 
																					--Start of Rev. #2.4
																					tas.fnGetShavingTime
																					(
																						1, 
																						d.TimeOutWP, 
																						RTRIM(b.ShiftPatCode), 
																						CASE WHEN RTRIM(b.ShiftCode) = 'O' 
																							THEN RTRIM(b.Actual_ShiftCode) 
																							ELSE 
																								CASE WHEN ISNULL(b.ShiftCode, '') = '' 
																									THEN RTRIM(b.Actual_ShiftCode) 
																									ELSE RTRIM(b.ShiftCode) 
																								END
																						END
																					)
																					--End of Rev. #2.4
																				ELSE NULL 
																			 END,

						tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_Old =	CASE WHEN b.OTStartTime IS NOT NULL 
																					THEN b.OTStartTime 
																					ELSE c.OTstartTime 
																				END,
						tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_New =	CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL)	--(Note: No workplace swipe-out but has swipe-in time)
																						OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)	--Rev. #3.0
																					THEN 
																						CASE WHEN b.OTStartTime IS NOT NULL 
																							THEN b.OTStartTime 
																							ELSE c.OTstartTime 
																						END
																					ELSE NULL 
																				END,

						tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_Old = CASE WHEN b.OTEndTime IS NOT NULL 
																				THEN b.OTEndTime 
																				ELSE c.OTendTime 
																			 END,
						tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_New = CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL)		--(Note: No workplace swipe-in but has swipe-out time)
																					OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)		--Rev. #3.0
																				THEN 
																					CASE WHEN b.OTEndTime IS NOT NULL 
																						THEN b.OTEndTime 
																						ELSE c.OTendTime 
																					END
																				ELSE NULL 
																			 END,

						tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_Old = b.NoPayHours,
						tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_New =	CASE WHEN	--Rev. #3.7 
																					(CASE WHEN (a.Direction = 'O' AND d.TimeInWP IS NOT NULL)		
																							OR (a.Direction = 'I' AND d.TimeInWP IS NOT NULL)		
																						THEN d.TimeInWP 
																						ELSE NULL 
																					END) IS NULL
																					OR
																					(CASE WHEN (a.Direction = 'I' AND d.TimeOutWP IS NOT NULL)		
																							OR (a.Direction = 'O' AND d.TimeOutWP IS NOT NULL)		
																						THEN d.TimeOutWP 
																						ELSE NULL 
																					 END) IS NULL
																					THEN tas.fnGetRequiredWorkDuration(RTRIM(b.ShiftPatCode), RTRIM(ISNULL(b.Actual_ShiftCode, b.ShiftCode)))
																					ELSE b.NoPayHours
																				END,
						--tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_New =	CASE WHEN ISNULL(b.NoPayHours, 0) > 0 
						--														THEN tas.fnGetRequiredWorkDuration(RTRIM(b.ShiftPatCode), RTRIM(ISNULL(b.Actual_ShiftCode, b.ShiftCode)))	--Rev. #3.7  
						--														ELSE b.NoPayHours 
						--														END,

						tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_Old = b.Duration_Worked_Cumulative,
						tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_New = CASE WHEN b.Duration_Worked_Cumulative > 0 THEN 0 ELSE b.Duration_Worked_Cumulative END,

						tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_Old = b.NetMinutes,
						tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_New = CASE WHEN b.NetMinutes > 0 THEN 0 ELSE b.NetMinutes END,

						tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateUser = 'System Admin',
						tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateTime = GETDATE()
					FROM tas.Tran_TempSwipeData a WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) 
						LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
						LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON b.EmpNo = d.EmpNo AND b.DT = d.SwipeDate AND RTRIM(b.BusinessUnit) = RTRIM(d.CostCenter)
						INNER JOIN  tas.Master_Employee_JDE_View e WITH (NOLOCK) ON a.EmpNo = e.EmpNo
					WHERE 
						a.DT IS NULL
						AND b.IsLastRow = 1
						AND 
						(	
							(a.Direction = 'O' AND b.dtOUT IS NOT NULL)
							OR a.Direction = 'I' AND b.dtIN IS NOT NULL
						)	
						
						--Start of Rev. #3.6
						AND 
						(
							(b.DT BETWEEN @startDate AND @endDate AND @startDate < @endDate)
							OR 
							(b.DT = @startDate AND @startDate = @endDate)
						)	
						--End of Rev. #3.6				
						--AND 
						--(
						--	(
						--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeLastProcessed, 12)) >= @startDate 
						--		AND 
						--		CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12)) <= @endDate
						--		AND 
						--		@endDate > @startDate
						--	)							
						--	--Start of Rev. #3.4
						--	OR
						--	(
						--		b.DT = CONVERT(DATETIME, CONVERT(VARCHAR, a.DTSwipeNewProcess, 12))
						--		AND
						--		@startDate = @endDate
						--	)
						--	--End of Rev. #3.4
						--)

						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND (RTRIM(e.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	
						AND EXISTS	--(Note: Include records which already exists in the log table)
						(
							SELECT 1	--LogID 
							FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
							WHERE AutoID = b.AutoID
								AND EmpNo = a.EmpNo 
								AND DT = b.DT 
								AND IsActive = 1
						)
						AND NOT	--(Note: Exclude morning and evening shifts where time-out is null)
						(
							(CASE WHEN ISNULL(b.Actual_ShiftCode, '') <> '' THEN b.Actual_ShiftCode ELSE b.ShiftCode END) IN ('M', 'E') 
							AND dtOUT IS NULL
							AND b.DT = CONVERT(DATETIME, GETDATE(), 101)	--Rev. #2.6
						) 	
						AND RTRIM(e.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1)
						AND 
						(
							(a.Direction = 'I' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
							OR (a.Direction = 'O' AND EXISTS (SELECT SwipeID FROM tas.Tran_WorkplaceSwipe WITH (NOLOCK) WHERE EmpNo = b.EmpNo AND SwipeDate = b.DT AND RTRIM(CostCenter) = RTRIM(e.BusinessUnit)))
						)	
						--Start of Rev. #2.8
						AND NOT EXISTS
						(
							SELECT 1	--AutoID 
							FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
							WHERE EmpNo = a.EmpNo 
								AND RTRIM(CostCenter) = RTRIM(b.BusinessUnit)
								AND IsActive = 1
								AND @startDate >= EffectiveDate 
						)
						--End of Rev. #2.8

					--Get the number of log records processed
					SELECT @logRecordsProcessed = @@ROWCOUNT
				END
			END

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Check if workplace swipe should be synchronized to Timesheet
					IF @syncWorkplaceToTimesheet = 1
					BEGIN

						--Update Timesheet records with data in the workplace swipes
						UPDATE tas.Tran_Timesheet
						SET tas.Tran_Timesheet.dtIN = b.dtIN_New,
							tas.Tran_Timesheet.dtOUT = b.dtOUT_New,
							tas.Tran_Timesheet.Shaved_IN = b.ShavedIn_New,
							tas.Tran_Timesheet.Shaved_OUT = b.ShavedOut_New,
							tas.Tran_Timesheet.NoPayHours = CASE WHEN ISNULL(a.LeaveType, '') = '' AND RTRIM(a.ShiftCode) <> 'O' THEN b.NoPayHours_New ELSE 0 END,	--Rev. #3.8, #3.9
							tas.Tran_Timesheet.Duration_Worked_Cumulative = b.DurationWorkedCumulative_New,
							tas.Tran_Timesheet.NetMinutes = b.NetMinutes_New,
							tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
							tas.Tran_Timesheet.LastUpdateTime = GETDATE(),

							--Start of Rev. #3.2
							--tas.Tran_Timesheet.OTStartTime = CASE WHEN a.OTStartTime IS NOT NULL THEN b.OTStartTime_New ELSE a.OTStartTime END,	
							--tas.Tran_Timesheet.OTEndTime = CASE WHEN a.OTEndTime IS NOT NULL THEN b.OTEndTime_New ELSE a.OTEndTime END, 
							--End of Rev. #3.2

							tas.Tran_Timesheet.Processed = 0	--Rev. #4.0
						FROM tas.Tran_Timesheet a WITH (NOLOCK)
							INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
							CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) c	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	--Rev. #4.5	
						WHERE a.DT BETWEEN @startDate AND @endDate
							AND (a.EmpNo = @empNo OR @empNo IS NULL)							
							AND (RTRIM(b.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
							AND a.IsLastRow = 1							
							AND b.IsActive = 1	
							AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
							(
								SELECT COUNT(AutoID) FROM tas.Tran_Timesheet WITH (NOLOCK)
								WHERE EmpNo = a.EmpNo 
									AND DT = a.DT
									--AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
							) = 1

							--Start of Rev. #2.3
							AND	
							(
								b.dtIN_New IS NULL OR b.dtOUT_New IS NULL
							)
							--End of Rev. #2.3

							AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

							--Start of Rev. #2.8
							AND NOT EXISTS
							(
								SELECT 1	--AutoID 
								FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
								WHERE EmpNo = a.EmpNo 
									AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
									AND IsActive = 1
									AND @startDate >= EffectiveDate 
							)
							--End of Rev. #2.8
							AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
							AND c.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)		--Rev. #4.5	
							AND c.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)				--Rev. #4.5	

						--Get the number of timesheet records processed
						SELECT @timeSheetRecordsProcessedMS = @@ROWCOUNT	--Rev. #2.7

						--Checks for error
						IF @@ERROR <> @CONST_RETURN_OK
						BEGIN
				
							SELECT	@retError = @CONST_RETURN_ERROR,
									@hasError = 1
						END

						-- Checks if there's no error
						IF @retError = @CONST_RETURN_OK
						BEGIN

							--Update overtime records in "Tran_Timesheet_Extra" table
							IF EXISTS
							(
								SELECT 1	--a.AutoID
								FROM tas.Tran_Timesheet a WITH (NOLOCK)
									INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
									INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog c WITH (NOLOCK) ON a.AutoID = c.AutoID
									CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) d	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	--Rev. #4.5	
								WHERE 
									a.DT BETWEEN @startDate AND @endDate
									AND (a.EmpNo = @empNo OR @empNo IS NULL)									
									AND (RTRIM(c.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
									AND a.IsLastRow = 1									
									AND c.IsActive = 1	
									AND b.OTStartTime IS NOT NULL 	
									AND b.OTendTime IS NOT NULL 	
									--AND ISNULL(a.ShiftSpan, 0) <> 1

									AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
									(
										SELECT COUNT(AutoID) FROM tas.Tran_Timesheet WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND DT = a.DT
											--AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
									) = 1

									--Start of Rev. #2.3
									AND 
									(
										c.OTStartTime_New IS NULL OR c.OTEndTime_New IS NULL
									)
									--End of Rev. #2.3

									AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

									--Start of Rev. #2.8
									AND NOT EXISTS
									(
										SELECT 1	--AutoID 
										FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
											AND IsActive = 1
											AND @startDate >= EffectiveDate
									)
									--End of Rev. #2.8

									--Start of Rev. #3.3
									AND RTRIM(a.BusinessUnit) NOT IN
									(
										SELECT RTRIM(CostCenter) FROM tas.OTApprovalSetting WITH (NOLOCK) 
										WHERE CONVERT(VARCHAR, GETDATE(), 12) BETWEEN CONVERT(VARCHAR, EffectiveStartDate, 12) AND CONVERT(VARCHAR, EffectiveEndDate, 12)  
											AND IsActive = 1
									)
									--End of Rev. #3.3
									AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
									AND d.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)		--Rev. #4.5	
									AND d.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)				--Rev. #4.5	
							)
							BEGIN

								UPDATE tas.Tran_Timesheet_Extra
								SET tas.Tran_Timesheet_Extra.OTstartTime = c.OTStartTime_New,
									tas.Tran_Timesheet_Extra.OTendTime = c.OTEndTime_New,
									tas.Tran_Timesheet_Extra.LastUpdateUser = 'System Admin',
									tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
								FROM tas.Tran_Timesheet a WITH (NOLOCK)
									INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
									INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog c WITH (NOLOCK) ON a.AutoID = c.AutoID
									CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) d	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	--Rev. #4.5	
								WHERE 
									a.DT BETWEEN @startDate AND @endDate
									AND (a.EmpNo = @empNo OR @empNo IS NULL)									
									AND (RTRIM(c.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
									AND a.IsLastRow = 1									
									AND c.IsActive = 1	
									AND b.OTStartTime IS NOT NULL 	
									AND b.OTendTime IS NOT NULL 
									--AND ISNULL(a.ShiftSpan, 0) <> 1

									AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
									(
										SELECT COUNT(AutoID) FROM tas.Tran_Timesheet WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND DT = a.DT
											--AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
									) = 1		

									--Start of Rev. #2.3
									AND 
									(
										c.OTStartTime_New IS NULL OR c.OTEndTime_New IS NULL
									) 
									--End of Rev. #2.3

									AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

									--Start of Rev. #2.8
									AND NOT EXISTS
									(
										SELECT 1	--AutoID 
										FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
											AND IsActive = 1
											AND @startDate >= EffectiveDate
									)
									--End of Rev. #2.8

									--Start of Rev. #3.3
									AND RTRIM(a.BusinessUnit) NOT IN
									(
										SELECT RTRIM(CostCenter) FROM tas.OTApprovalSetting WITH (NOLOCK) 
										WHERE CONVERT(VARCHAR, GETDATE(), 12) BETWEEN CONVERT(VARCHAR, EffectiveStartDate, 12) AND CONVERT(VARCHAR, EffectiveEndDate, 12)  
											AND IsActive = 1
									)
									--End of Rev. #3.3
									AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
									AND d.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)		--Rev. #4.5	
									AND d.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)				--Rev. #4.5	

								--Get the number of overtime records processed
								SELECT @overtimeRecordsProcessedMS = @@ROWCOUNT	--Rev. #2.7

								--Checks for error
								IF @@ERROR <> @CONST_RETURN_OK
								BEGIN
				
									SELECT	@retError = @CONST_RETURN_ERROR,
											@hasError = 1
								END
							END
						END
					END
				END						
			END

			/*************************************************************************
				Processed valid workplace swipes into the timeshet
			**************************************************************************/
			IF EXISTS
			(
				SELECT 1	--a.SwipeID
				FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DT --AND RTRIM(a.CostCenter) = RTRIM(b.Businessunit)
					LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
				WHERE 
					a.SwipeDate BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
					AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
					AND (b.dtIN IS NOT NULL AND b.dtOUT IS NOT NULL)
					AND b.IsLastRow = 1
					AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
					(
						SELECT 1	--LogID 
						FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
						WHERE AutoID = b.AutoID
							AND EmpNo = a.EmpNo 
							AND DT = a.SwipeDate 
							AND IsActive = 1
					)
			)
			BEGIN

				/*****************************************************************************
					Rev. #1.7 - Correct valid records in the "Tran_WorkplaceSwipe" table
				******************************************************************************/					
				DECLARE TimesheetCursor CURSOR READ_ONLY FOR
				SELECT b.EmpNo, b.DT
				FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DT 
					LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
					CROSS APPLY tas.fnGetProcessedTimesheetData
					(
						b.AutoID, 
						CASE WHEN b.OTType IS NOT NULL 
							THEN b.OTType 
							ELSE c.OTType 
						END , 
						a.TimeInWP, 
						a.TimeOutWP
					) d
				WHERE 
					a.SwipeDate BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
					AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
					AND (b.dtIN IS NOT NULL AND b.dtOUT IS NOT NULL)
					AND b.IsLastRow = 1
					--Start of Rev. #2.2
					--AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
					--(
					--	SELECT LogID FROM tas.SyncWorkplaceSwipeToTimesheetLog
					--	WHERE AutoID = b.AutoID
					--		AND EmpNo = a.EmpNo 
					--		AND DT = a.SwipeDate 
					--		AND IsActive = 1
					--)
					--End of Rev. #2.2

				OPEN TimesheetCursor
				FETCH NEXT FROM TimesheetCursor
				INTO @empNoTemp, @dtTemp

				WHILE @@FETCH_STATUS = 0
				BEGIN

					IF ISNULL(@empNoTemp, 0) > 0 AND ISNULL(@dtTemp, '') <> ''
					BEGIN

						UPDATE tas.Tran_WorkplaceSwipe
						SET tas.Tran_WorkplaceSwipe.TimeInMG = b.TimeInMG,
							tas.Tran_WorkplaceSwipe.TimeOutMG = b.TimeOutMG,
							tas.Tran_WorkplaceSwipe.TimeInWP = b.TimeInWP,
							tas.Tran_WorkplaceSwipe.TimeOutWP = b.TimeOutWP,
							tas.Tran_WorkplaceSwipe.NetMinutesMG = b.NetMinutesMG,
							tas.Tran_WorkplaceSwipe.NetMinutesWP = b.NetMinutesWP
						FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
							CROSS APPLY tas.fnGetCorrectedMainGateWorkplaceSwipe(a.EmpNo, a.SwipeDate) b								
						WHERE a.SwipeDate = @dtTemp
							AND a.EmpNo = @empNoTemp
							AND 
							(
								(CONVERT(TIME, a.TimeInMG) <> CONVERT(TIME, '23:00:00') AND CONVERT(TIME, a.TimeInWP) <> CONVERT(TIME, '23:00:00'))
								OR
								(CONVERT(TIME, a.TimeOutMG) <> CONVERT(TIME, '23:00:00') AND CONVERT(TIME, a.TimeOutWP) <> CONVERT(TIME, '23:00:00'))
							)
					END

					-- Retrieve next record
					FETCH NEXT FROM TimesheetCursor
					INTO @empNoTemp, @dtTemp
				END

				-- Close and deallocate
				CLOSE TimesheetCursor
				DEALLOCATE TimesheetCursor
				/********************************* End **************************************/

				--Insert transaction log records
				IF NOT EXISTS
				(
					SELECT 1	--LogID 
					FROM tas.SyncWorkplaceSwipeToTimesheetLog a WITH (NOLOCK)
					WHERE AutoID IN
					(
						SELECT b.AutoID
						FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
							INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DT --AND RTRIM(a.CostCenter) = RTRIM(b.Businessunit)
							LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
						WHERE 
							a.SwipeDate BETWEEN @startDate AND @endDate
							AND (a.EmpNo = @empNo OR @empNo IS NULL)
							AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
							AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
							AND (b.dtIN IS NOT NULL AND b.dtOUT IS NOT NULL)
							AND b.IsLastRow = 1
							AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
							(
								SELECT 1	--LogID 
								FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
								WHERE AutoID = b.AutoID
									AND EmpNo = a.EmpNo 
									AND DT = a.SwipeDate 
									AND IsActive = 1
							)
					)
				)
				BEGIN									

					--Insert new records
					INSERT INTO tas.SyncWorkplaceSwipeToTimesheetLog
					(
						LogDate,
						AutoID,
						DT,
						CostCenter,
						EmpNo,
						dtIN_Old,
						dtIN_New,
						dtOUT_Old,
						dtOUT_New,
						ShavedIn_Old,
						ShavedIn_New,
						ShavedOut_Old,
						ShavedOut_New,
						OTStartTime_Old,
						OTStartTime_New,
						OTEndTime_Old,
						OTEndTime_New,
						NoPayHours_Old,
						NoPayHours_New,
						DurationWorkedCumulative_Old,
						DurationWorkedCumulative_New,
						NetMinutes_Old,
						NetMinutes_New,
						LastUpdateUser,
						LastUpdateTime
					)
					SELECT	DISTINCT
							GETDATE() AS LogDate, 
							b.AutoID,
							b.DT,
							a.CostCenter, 
							b.EmpNo,

							/********************** Official Time In/Out Calculation *********************/
							b.dtIN AS dtIN_Old,
							CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
								THEN b.dtIN
								ELSE a.TimeInWP
							END AS dtIN_New,
							b.dtOUT AS dtOUT_Old,
							CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
								THEN b.dtOUT
								ELSE a.TimeOutWP 
							END AS dtOUT_New,
							/********************** End of Official Time In/Out Calculation *********************/

							/**************** Calculate Shaving Time *************************************/
							b.Shaved_IN AS ShavedIn_Old,
							CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
								THEN b.Shaved_IN
								ELSE 
									tas.fnGetShavingTime
									(
										0, 
										a.TimeInWP, 
										RTRIM(b.ShiftPatCode), 
										CASE WHEN RTRIM(b.ShiftCode) = 'O' 
											THEN RTRIM(b.Actual_ShiftCode) 
											ELSE 
												CASE WHEN ISNULL(b.ShiftCode, '') = '' 
													THEN RTRIM(b.Actual_ShiftCode) 
													ELSE RTRIM(b.ShiftCode) 
												END
										END
									)
							END AS ShavedIn_New,
							b.Shaved_OUT AS ShavedOut_Old,
							CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
								THEN b.Shaved_OUT
								ELSE 
									tas.fnGetShavingTime
									(
										1, 
										a.TimeOutWP, 
										RTRIM(b.ShiftPatCode), 
										CASE WHEN RTRIM(b.ShiftCode) = 'O' 
											THEN RTRIM(b.Actual_ShiftCode) 
											ELSE 
												CASE WHEN ISNULL(b.ShiftCode, '') = '' 
													THEN RTRIM(b.Actual_ShiftCode) 
													ELSE RTRIM(b.ShiftCode) 
												END
										END
									)
							END AS ShavedOut_New,
							/**************** End of Shaving Time Calculation *************************************/

							/*************************** Start of Overtime calculation ****************************/
							CASE WHEN b.OTStartTime IS NOT NULL 
								THEN b.OTStartTime 
								ELSE c.OTstartTime 
							END AS OTStartTime_Old,
							CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
								THEN 
									CASE WHEN b.OTStartTime IS NOT NULL 
										THEN b.OTStartTime 
										ELSE c.OTstartTime 
									END
								ELSE 
									CASE WHEN b.ShiftSpan = 1	
										THEN 
											CASE WHEN (CASE WHEN b.OTStartTime IS NOT NULL THEN b.OTStartTime ELSE c.OTstartTime END) IS NOT NULL
												THEN d.OTStartTime 
												ELSE NULL
											END
										ELSE
											CASE WHEN (CASE WHEN b.OTStartTime IS NOT NULL THEN b.OTStartTime ELSE c.OTstartTime END) IS NOT NULL
												THEN d.OTStartTime 
												ELSE NULL
											END
									END
							END AS OTStartTime_New,

							CASE WHEN b.OTEndTime IS NOT NULL 
								THEN b.OTEndTime 
								ELSE c.OTendTime 
							END AS OTEndTime_Old,
							CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
								THEN 
									CASE WHEN b.OTEndTime IS NOT NULL 
										THEN b.OTEndTime 
										ELSE c.OTendTime 
									END
								ELSE 
									CASE WHEN b.ShiftSpan = 1	
										THEN 
											CASE WHEN (CASE WHEN b.OTEndTime IS NOT NULL THEN b.OTEndTime ELSE c.OTEndTime END) IS NOT NULL
												THEN d.OTEndTime 
												ELSE NULL
											END 
										ELSE
											CASE WHEN (CASE WHEN b.OTEndTime IS NOT NULL THEN b.OTEndTime ELSE c.OTEndTime END) IS NOT NULL
												THEN d.OTEndTime 
												ELSE NULL
											END 
									END
							END AS OTEndTime_New,
							/*************************** End of Overtime calculation ****************************/

							--No Pay Hours
							b.NoPayHours AS NoPayHours_Old,
							CASE WHEN b.ShiftSpan = 1	--Rev. #1.8	
								THEN b.NoPayHours 
								ELSE d.NoPayHours
							END AS NoPayHours_New,

							--Total work duration (shaved time)
							b.Duration_Worked_Cumulative AS DurationWorkedCumulative_Old,
							CASE WHEN b.ShiftSpan = 1	--Rev. #1.8	
								THEN b.Duration_Worked_Cumulative
								ELSE d.Duration_Worked_Cumulative 
							END AS DurationWorkedCumulative_New,

							--Net work duration
							b.NetMinutes AS NetMinutes_Old,
							CASE WHEN b.ShiftSpan = 1	--Rev. #1.8	
								THEN b.NetMinutes
								ELSE d.NetMinutes 
							END AS NetMinutes_New,

							'System Admin' AS LastUpdateUser,
							GETDATE() AS LastUpdateTime
					FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DT --AND RTRIM(a.CostCenter) = RTRIM(b.Businessunit)
						LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
						CROSS APPLY tas.fnGetProcessedTimesheetData
						(
							b.AutoID, 
							CASE WHEN b.OTType IS NOT NULL 
								THEN b.OTType 
								ELSE c.OTType 
							END , 

							--a.TimeInWP, 
							CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
								THEN b.dtIN
								ELSE a.TimeInWP
							END,

							--a.TimeOutWP
							CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
								THEN b.dtOUT
								ELSE a.TimeOutWP 
							END
						) d
					WHERE 
						a.SwipeDate BETWEEN @startDate AND @endDate
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
						AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
						AND (b.dtIN IS NOT NULL AND b.dtOUT IS NOT NULL)
						AND b.IsLastRow = 1
						AND NOT EXISTS	--(Note: Exclude records which already exists in the log table)
						(
							SELECT 1	--LogID 
							FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
							WHERE AutoID = b.AutoID
								AND EmpNo = a.EmpNo 
								AND DT = a.SwipeDate 
								AND IsActive = 1
						)
						--Start of Rev. #2.8
						AND NOT EXISTS
						(
							SELECT 1	--AutoID 
							FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
							WHERE EmpNo = a.EmpNo 
								AND RTRIM(CostCenter) = RTRIM(b.BusinessUnit)
								AND IsActive = 1
								AND @startDate >= EffectiveDate
						)
						--End of Rev. #2.8

					--Get the number of log records processed
					SELECT @logRecordsProcessed = @logRecordsProcessed + @@ROWCOUNT
				END

				ELSE
				BEGIN

					--Update existing log records
					UPDATE tas.SyncWorkplaceSwipeToTimesheetLog
					SET tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_Old = b.dtIN,
						tas.SyncWorkplaceSwipeToTimesheetLog.dtIN_New = --a.TimeInWP,
																		CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
																			THEN b.dtIN
																			ELSE a.TimeInWP
																		END,

						tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_Old = b.dtOUT,
						tas.SyncWorkplaceSwipeToTimesheetLog.dtOUT_New = --a.TimeOutWP,
																		CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
																			THEN b.dtOUT
																			ELSE a.TimeOutWP 
																		END,

						--Shaving Time
						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_Old = b.Shaved_IN,
						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedIn_New =	CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
																				THEN b.Shaved_IN
																				ELSE 
																					tas.fnGetShavingTime
																					(
																						0, 
																						a.TimeInWP, 
																						RTRIM(b.ShiftPatCode), 
																						CASE WHEN RTRIM(b.ShiftCode) = 'O' 
																							THEN RTRIM(b.Actual_ShiftCode) 
																							ELSE 
																								CASE WHEN ISNULL(b.ShiftCode, '') = '' 
																									THEN RTRIM(b.Actual_ShiftCode) 
																									ELSE RTRIM(b.ShiftCode) 
																								END
																						END
																					)
																			END,

						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_Old = b.Shaved_OUT,
						tas.SyncWorkplaceSwipeToTimesheetLog.ShavedOut_New = CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
																				THEN b.Shaved_OUT
																				ELSE 
																					tas.fnGetShavingTime
																					(
																						1, 
																						a.TimeOutWP, 
																						RTRIM(b.ShiftPatCode), 
																						CASE WHEN RTRIM(b.ShiftCode) = 'O' 
																							THEN RTRIM(b.Actual_ShiftCode) 
																							ELSE 
																								CASE WHEN ISNULL(b.ShiftCode, '') = '' 
																									THEN RTRIM(b.Actual_ShiftCode) 
																									ELSE RTRIM(b.ShiftCode) 
																								END
																						END
																					)
																			END,

						--Overtime
						tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_Old =	CASE WHEN b.OTStartTime IS NOT NULL 
																					THEN b.OTStartTime 
																					ELSE c.OTstartTime 
																				END,
						tas.SyncWorkplaceSwipeToTimesheetLog.OTStartTime_New =	CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
																					THEN 
																						CASE WHEN b.OTStartTime IS NOT NULL 
																							THEN b.OTStartTime 
																							ELSE c.OTstartTime 
																						END
																					ELSE 
																						CASE WHEN b.ShiftSpan = 1	
																							THEN 
																								CASE WHEN (CASE WHEN b.OTStartTime IS NOT NULL THEN b.OTStartTime ELSE c.OTstartTime END) IS NOT NULL
																									THEN d.OTStartTime 
																									ELSE NULL
																								END
																							ELSE
																								CASE WHEN (CASE WHEN b.OTStartTime IS NOT NULL THEN b.OTStartTime ELSE c.OTstartTime END) IS NOT NULL
																									THEN d.OTStartTime 
																									ELSE NULL
																								END
																						END
																				END,
						tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_Old =	CASE WHEN b.OTEndTime IS NOT NULL 
																					THEN b.OTEndTime 
																					ELSE c.OTendTime 
																				END,
						tas.SyncWorkplaceSwipeToTimesheetLog.OTEndTime_New =	CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
																					THEN 
																						CASE WHEN b.OTEndTime IS NOT NULL 
																							THEN b.OTEndTime 
																							ELSE c.OTendTime 
																						END
																					ELSE 
																						CASE WHEN b.ShiftSpan = 1	
																							THEN 
																								CASE WHEN (CASE WHEN b.OTEndTime IS NOT NULL THEN b.OTEndTime ELSE c.OTEndTime END) IS NOT NULL
																									THEN d.OTEndTime 
																									ELSE NULL
																								END 
																							ELSE
																								CASE WHEN (CASE WHEN b.OTEndTime IS NOT NULL THEN b.OTEndTime ELSE c.OTEndTime END) IS NOT NULL
																									THEN d.OTEndTime 
																									ELSE NULL
																								END 
																						END
																				END,
						
						--No Pay Hours
						tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_Old = b.NoPayHours,
						tas.SyncWorkplaceSwipeToTimesheetLog.NoPayHours_New =	CASE WHEN b.ShiftSpan = 1	--Rev. #1.8	
																					THEN b.NoPayHours 
																					ELSE d.NoPayHours
																				END,
						--Total work duration (shaved time)
						tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_Old = b.Duration_Worked_Cumulative,
						tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_New = CASE WHEN b.ShiftSpan = 1	--Rev. #1.8	
																								THEN b.Duration_Worked_Cumulative
																								ELSE d.Duration_Worked_Cumulative 
																							END,
						--Net work duration
						tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_Old = b.NetMinutes,
						tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_New =	CASE WHEN b.ShiftSpan = 1	--Rev. #1.8	
																					THEN b.NetMinutes
																					ELSE d.NetMinutes 
																				END,

						tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateUser = 'System Admin',
						tas.SyncWorkplaceSwipeToTimesheetLog.LastUpdateTime = GETDATE()
					FROM tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.SwipeDate = b.DT --AND RTRIM(a.CostCenter) = RTRIM(b.Businessunit)
						LEFT JOIN tas.Tran_Timesheet_Extra c WITH (NOLOCK) ON b.AutoID = c.XID_AutoID	
						CROSS APPLY tas.fnGetProcessedTimesheetData
						(
							b.AutoID, 
							CASE WHEN b.OTType IS NOT NULL 
								THEN b.OTType 
								ELSE c.OTType 
							END , 

							--a.TimeInWP, 
							CASE WHEN CONVERT(TIME, b.dtIN) = CONVERT(TIME, '23:00:00.000')
								THEN b.dtIN
								ELSE a.TimeInWP
							END,

							--a.TimeOutWP
							CASE WHEN CONVERT(TIME, b.dtOUT) = CONVERT(TIME, '23:00:00.000')
								THEN b.dtOUT
								ELSE a.TimeOutWP 
							END
						) d
					WHERE 
						a.SwipeDate BETWEEN @startDate AND @endDate
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
						AND (a.TimeInWP IS NOT NULL AND a.TimeOutWP IS NOT NULL)
						AND (b.dtIN IS NOT NULL AND b.dtOUT IS NOT NULL)
						AND b.IsLastRow = 1
						AND EXISTS	--(Note: Update records which already exists in the log table)
						(
							SELECT 1	--LogID 
							FROM tas.SyncWorkplaceSwipeToTimesheetLog WITH (NOLOCK)
							WHERE AutoID = b.AutoID
								AND EmpNo = a.EmpNo 
								AND DT = a.SwipeDate 
								AND IsActive = 1
						)
						--Start of Rev. #2.8
						AND NOT EXISTS
						(
							SELECT 1	--AutoID 
							FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
							WHERE EmpNo = a.EmpNo 
								AND RTRIM(CostCenter) = RTRIM(b.BusinessUnit)
								AND IsActive = 1
								AND @startDate >= EffectiveDate
						)
						--End of Rev. #2.8

					--Get the number of log records processed
					SELECT @logRecordsProcessed = @logRecordsProcessed + @@ROWCOUNT
				END

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Check if workplace swipe should be synchronized to Timesheet
					IF @syncWorkplaceToTimesheet = 1
					BEGIN

						--Update Timesheet record for employees who do not swipe in the Admin Bldg readers
						UPDATE tas.Tran_Timesheet
						SET tas.Tran_Timesheet.dtIN = b.dtIN_New,
							tas.Tran_Timesheet.dtOUT = b.dtOUT_New,
							tas.Tran_Timesheet.Shaved_IN = b.ShavedIn_New,
							tas.Tran_Timesheet.Shaved_OUT = b.ShavedOut_New,
							tas.Tran_Timesheet.NoPayHours = b.NoPayHours_New,
							tas.Tran_Timesheet.Duration_Worked_Cumulative = b.DurationWorkedCumulative_New,
							tas.Tran_Timesheet.NetMinutes = b.NetMinutes_New,
							tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
							tas.Tran_Timesheet.LastUpdateTime = GETDATE(),
							tas.Tran_Timesheet.Processed = 0	
							--tas.Tran_Timesheet.OTStartTime = CASE WHEN a.OTStartTime IS NOT NULL THEN c.OTStartTime ELSE a.OTStartTime END,
							--tas.Tran_Timesheet.OTEndTime = CASE WHEN a.OTEndTime IS NOT NULL THEN c.OTEndTime ELSE a.OTEndTime END  
						FROM tas.Tran_Timesheet a WITH (NOLOCK)
							INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
							OUTER APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, b.dtIN_New, b.dtOUT_New, 0) c
							CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) d	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)		--Rev. #4.5	
						WHERE 
							a.DT BETWEEN @startDate AND @endDate
							AND (a.EmpNo = @empNo OR @empNo IS NULL)
							AND (RTRIM(b.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)							
							AND a.IsLastRow = 1							
							AND b.IsActive = 1		
							--AND ISNULL(a.ShiftSpan, 0) <> 1 

							AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
							(
								SELECT COUNT(AutoID) 
								FROM tas.Tran_Timesheet WITH (NOLOCK)
								WHERE EmpNo = a.EmpNo 
									AND DT = a.DT
									--AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
							) = 1

							--Start of Rev. #2.3
							AND	
							(
								b.dtIN_New IS NOT NULL
								AND b.dtOUT_New IS NOT NULL
								AND b.ShavedIn_New IS NOT NULL
								AND b.ShavedOut_New IS NOT NULL
							)
							--End of Rev. #2.3

							AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

							--Start of Rev. #2.8
							AND NOT EXISTS
							(
								SELECT 1	--AutoID 
								FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
								WHERE EmpNo = a.EmpNo 
									AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
									AND IsActive = 1
									AND @startDate >= EffectiveDate
							)
							--End of Rev. #2.8

							AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
							AND d.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)			--Rev. #4.5	
							AND d.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)					--Rev. #4.5	
							AND NOT (DATEDIFF(MINUTE, b.dtIN_New, b.dtOUT_New) > 900 OR DATEDIFF(MINUTE, b.dtIN_New, b.dtOUT_New) <= 0)		--Rev. #4.6

						--Get the number of timesheet records processed
						SELECT @timeSheetRecordsProcessed = @@ROWCOUNT

						--Checks for error
						IF @@ERROR <> @CONST_RETURN_OK
						BEGIN
				
							SELECT	@retError = @CONST_RETURN_ERROR,
									@hasError = 1
						END

						-- Checks if there's no error
						IF @retError = @CONST_RETURN_OK
						BEGIN

							--Update overtime records in "Tran_Timesheet_Extra" table
							IF EXISTS
							(
								SELECT 1	--a.AutoID
								FROM tas.Tran_Timesheet a WITH (NOLOCK)
									INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
									INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog c WITH (NOLOCK) ON a.AutoID = c.AutoID
									CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) d	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	--Rev. #4.5	
								WHERE 
									a.DT BETWEEN @startDate AND @endDate
									AND (a.EmpNo = @empNo OR @empNo IS NULL)
									AND (RTRIM(c.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)										
									AND a.IsLastRow = 1
									AND c.IsActive = 1	
									AND b.OTStartTime IS NOT NULL
									AND b.OTendTime IS NOT NULL
									--AND ISNULL(a.ShiftSpan, 0) <> 1 

									AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
									(
										SELECT COUNT(AutoID) 
										FROM tas.Tran_Timesheet WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND DT = a.DT
											--AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
									) = 1	

									--Start of Rev. #2.3
									AND		
									(
										c.OTStartTime_New IS NOT NULL AND c.OTEndTime_New IS NOT NULL
									)
									--End of Rev. #2.3

									AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

									--Start of Rev. #2.8
									AND NOT EXISTS
									(
										SELECT 1	--AutoID 
										FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
											AND IsActive = 1
											AND @startDate >= EffectiveDate
									)
									--End of Rev. #2.8

									--Start of Rev. #3.3
									AND RTRIM(a.BusinessUnit) NOT IN
									(
										SELECT RTRIM(CostCenter) 
										FROM tas.OTApprovalSetting WITH (NOLOCK)
										WHERE CONVERT(VARCHAR, GETDATE(), 12) BETWEEN CONVERT(VARCHAR, EffectiveStartDate, 12) AND CONVERT(VARCHAR, EffectiveEndDate, 12)  
											AND IsActive = 1
									)
									--End of Rev. #3.3
									AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
									AND d.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)			--Rev. #4.5	
									AND d.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)					--Rev. #4.5	
									AND NOT (DATEDIFF(MINUTE, c.dtIN_New, c.dtOUT_New) > 900 OR DATEDIFF(MINUTE, c.dtIN_New, c.dtOUT_New) <= 0)		--Rev. #4.6
							)
							BEGIN

								UPDATE tas.Tran_Timesheet_Extra
								SET tas.Tran_Timesheet_Extra.OTstartTime = d.OTStartTime,
									tas.Tran_Timesheet_Extra.OTendTime = d.OTEndTime,
									tas.Tran_Timesheet_Extra.LastUpdateUser = 'System Admin',
									tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
								FROM tas.Tran_Timesheet a WITH (NOLOCK)
									INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
									INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog c WITH (NOLOCK) ON a.AutoID = c.AutoID
									OUTER APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, c.dtIN_New, c.dtOUT_New, 0) d
									CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) e		--(Note: Join to this function to return employees who do not belong to Admin Bldg.)	--Rev. #4.5	
								WHERE 
									a.DT BETWEEN @startDate AND @endDate
									AND (a.EmpNo = @empNo OR @empNo IS NULL)
									AND (RTRIM(c.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
									AND a.IsLastRow = 1
									AND c.IsActive = 1	
									AND b.OTStartTime IS NOT NULL
									AND b.OTendTime IS NOT NULL
									--AND ISNULL(a.ShiftSpan, 0) <> 1 

									AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
									(
										SELECT COUNT(AutoID) 
										FROM tas.Tran_Timesheet WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND DT = a.DT
											--AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
									) = 1		

									--Start of Rev. #2.3
									AND	
									(
										c.OTStartTime_New IS NOT NULL AND c.OTEndTime_New IS NOT NULL
									) 
									--End of Rev. #2.3

									AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

									--Start of Rev. #2.8
									AND NOT EXISTS
									(
										SELECT 1	--AutoID 
										FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
										WHERE EmpNo = a.EmpNo 
											AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
											AND IsActive = 1
											AND @startDate >= EffectiveDate
									)
									--End of Rev. #2.8

									--Start of Rev. #3.3
									AND RTRIM(a.BusinessUnit) NOT IN
									(
										SELECT RTRIM(CostCenter) FROM tas.OTApprovalSetting WITH (NOLOCK) 
										WHERE CONVERT(VARCHAR, GETDATE(), 12) BETWEEN CONVERT(VARCHAR, EffectiveStartDate, 12) AND CONVERT(VARCHAR, EffectiveEndDate, 12)  
											AND IsActive = 1
									)
									--End of Rev. #3.3
									AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
									AND e.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)			--Rev. #4.5	
									AND e.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)					--Rev. #4.5	
									AND NOT (DATEDIFF(MINUTE, c.dtIN_New, c.dtOUT_New) > 900 OR DATEDIFF(MINUTE, c.dtIN_New, c.dtOUT_New) <= 0)		--Rev. #4.6

								--Get the number of overtime records processed
								SELECT @overtimeRecordsProcessed = @@ROWCOUNT

								--Checks for error
								IF @@ERROR <> @CONST_RETURN_OK
								BEGIN
				
									SELECT	@retError = @CONST_RETURN_ERROR,
											@hasError = 1
								END
							END
						END
					END
				END	
			END

			/**************************************************************************************************
				Start of Rev. #3.1 - Update Timesheet and SyncWorkplaceSwipeToTimesheetLog tables 
									Correct the value of "Duration_Worked_Cumulative" and "NetMinutes" fields
			****************************************************************************************************/
			--Correct log records with invalid "DurationWorkedCumulative_New" and "NetMinutes_New"
			IF EXISTS
			(
				SELECT 1	--a.EmpNo
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
				WHERE 
					(b.DurationWorkedCumulative_New = 0 AND b.DurationWorkedCumulative_Old > 0)
					AND (b.ShavedIn_New IS NOT NULL AND b.ShavedOut_New IS NOT NULL)
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	
			)
			BEGIN
				
				UPDATE tas.SyncWorkplaceSwipeToTimesheetLog
				SET tas.SyncWorkplaceSwipeToTimesheetLog.DurationWorkedCumulative_New = DATEDIFF(n, b.ShavedIn_New, b.ShavedOut_New),
					tas.SyncWorkplaceSwipeToTimesheetLog.NetMinutes_New = DATEDIFF(n, b.dtIN_New, b.dtOUT_New)
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
				WHERE 
					(b.DurationWorkedCumulative_New = 0 AND b.DurationWorkedCumulative_Old > 0)
					AND (b.ShavedIn_New IS NOT NULL AND b.ShavedOut_New IS NOT NULL)
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
			END 

			--Correct Timesheet records with invalid "Duration_Worked_Cumulative" and "NetMinutes"
			IF EXISTS
			(
				SELECT 1 --a.EmpNo
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
					CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) c	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)		--Rev. #4.5	
				WHERE 
					a.Duration_Worked_Cumulative = 0
					AND (a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL)
					AND (a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL)
					AND a.IsLastRow = 1
					AND a.EmpNo > 10000000
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	
					AND c.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)		--Rev. #4.5	
					AND c.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)				--Rev. #4.5	
			)
			BEGIN
				
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.Duration_Worked_Cumulative = CASE WHEN (b.ShavedIn_New IS NOT NULL AND b.ShavedOut_New IS NOT NULL)
																	THEN DATEDIFF(n, b.ShavedIn_New, b.ShavedOut_New)
																	ELSE DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)
																	END, 
					tas.Tran_Timesheet.NetMinutes = CASE WHEN (b.dtIN_New IS NOT NULL AND b.dtOUT_New IS NOT NULL) 
													THEN DATEDIFF(n, b.dtIN_New, b.dtOUT_New)
													ELSE DATEDIFF(n, a.dtIN, a.dtOUT)
                                                    END 
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
					CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) c	--(Note: Join to this function to return employees who do not belong to Admin Bldg.)		--Rev. #4.5	
				WHERE 
					a.Duration_Worked_Cumulative = 0
					AND (a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL)
					AND (a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL)
					AND a.IsLastRow = 1
					AND a.EmpNo > 10000000
					AND		--Note: Update Timesheet with one swipe record only (Rev. #4.3)
					(
						SELECT COUNT(AutoID) 
						FROM tas.Tran_Timesheet WITH (NOLOCK)
						WHERE EmpNo = a.EmpNo 
							AND DT = a.DT
					) = 1
					AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		--Rev. #4.4	
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)	
					AND c.IsAdminBldgEnabled = 0	--(Notes: Filter records by employees who do not belong to Admin Bldg.)			--Rev. #4.5	
					AND c.IsSyncTimesheet = 1		--(Notes: Filter records wherein sync to Timesheet is enabled)					--Rev. #4.5	
					AND NOT (DATEDIFF(MINUTE, b.dtIN_New, b.dtOUT_New) > 900 OR DATEDIFF(MINUTE, b.dtIN_New, b.dtOUT_New) <= 0)		--Rev. #4.6

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
			END 
			--End of Rev. #3.1

			/***********************************************************************************************************
				Automate the process of synchronizing Admin Bldg. readers valid swipe data into the Timesheet
			*************************************************************************************************************/
			DECLARE	@return_value			INT,
					@validSwipeRows			INT = 0,
					@missingSwipeRows		INT = 0

			EXEC	@return_value = tas.Pr_SyncAdminBldgSwipeToTimesheet
					@actionTypeID = 0,
					@tsRowsAffected = @validSwipeRows OUTPUT,
					@startDate = @startDate,
					@endDate = @endDate,
					@costCenter = @costCenter,
					@empNo = @empNo

			--Add row count to the counter
			SELECT @timeSheetRecordsProcessed = ISNULL(@timeSheetRecordsProcessed, 0) + @validSwipeRows

			/***********************************************************************************************************
				Automate the process of synchronizing Admin Bldg. readers missing swipe data into the Timesheet
			*************************************************************************************************************/
			EXEC	@return_value = tas.Pr_SyncAdminBldgSwipeToTimesheet
					@actionTypeID = 1,
					@tsRowsAffected = @missingSwipeRows OUTPUT,
					@startDate = @startDate,
					@endDate = @endDate,
					@costCenter = @costCenter,
					@empNo = @empNo

			--Add row count to the counter
			SELECT @timeSheetRecordsProcessedMS = ISNULL(@timeSheetRecordsProcessedMS, 0) + @missingSwipeRows
		END
		
		ELSE IF @actionTypeID = 1
		BEGIN

			/**********************************************************************
				Undo corrections done in the Timesheet and Overtime records
				Delete transaction logs.
			***********************************************************************/

			EXEC tas.Pr_CorrectTimesheetAndSwipeLog @empNo, @startDate, 'System Admin'

			--Check if workplace swipe should be synchronized to Timesheet
			IF @syncWorkplaceToTimesheet = 1
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.dtIN = b.dtIN_Old,
					tas.Tran_Timesheet.dtOUT = b.dtOUT_Old,
					tas.Tran_Timesheet.Shaved_IN = b.ShavedIn_Old,
					tas.Tran_Timesheet.Shaved_OUT = b.ShavedOut_Old,
					tas.Tran_Timesheet.NoPayHours = b.NoPayHours_Old,
					tas.Tran_Timesheet.Duration_Worked_Cumulative = b.DurationWorkedCumulative_Old,
					tas.Tran_Timesheet.NetMinutes = b.NetMinutes_Old,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE(),
					tas.Tran_Timesheet.Processed = 0,		--Rev. #4.7
					tas.Tran_Timesheet.OTStartTime = CASE WHEN a.IsPublicHoliday = 1 OR a.isRamadan = 1			--Rev. #4.1
														THEN c.OTStartTime
														ELSE a.OTStartTime
													 END,
					tas.Tran_Timesheet.OTEndTime = CASE WHEN a.IsPublicHoliday = 1 OR a.isRamadan = 1			--Rev. #4.1
														THEN c.OTEndTime
														ELSE a.OTEndTime
													END
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog b WITH (NOLOCK) ON a.AutoID = b.AutoID
					OUTER APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, b.dtIN_Old, b.dtOUT_Old, 0) c
				WHERE 
					a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(b.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
					AND a.IsLastRow = 1
					AND b.IsActive = 1	
					--AND ISNULL(a.ShiftSpan, 0) <> 1 
					--AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
					--(
					--	SELECT COUNT(AutoID) FROM tas.Tran_Timesheet
					--	WHERE EmpNo = a.EmpNo AND DT = a.DT
					--		AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
					--) = 1	
					--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

					--Start of Rev. #2.8
					AND NOT EXISTS
					(
						SELECT 1	--AutoID 
						FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
						WHERE EmpNo = a.EmpNo 
							AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
							AND IsActive = 1
							AND @startDate >= EffectiveDate
					)
					--End of Rev. #2.8

				--Get the number of timesheet records processed
				SELECT @timeSheetRecordsProcessed = @@ROWCOUNT

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Undo updates in the overtime records
					UPDATE tas.Tran_Timesheet_Extra
					SET --tas.Tran_Timesheet_Extra.OTstartTime = c.OTStartTime_Old,
						--tas.Tran_Timesheet_Extra.OTendTime = c.OTEndTime_Old,

						--Start of Rev. #4.1
						tas.Tran_Timesheet_Extra.OTstartTime = d.OTStartTime,
						tas.Tran_Timesheet_Extra.OTendTime = d.OTEndTime,
						--End of Rev. #4.1

						tas.Tran_Timesheet_Extra.LastUpdateUser = 'System Admin',
						tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
					FROM tas.Tran_Timesheet a WITH (NOLOCK)
						INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
						INNER JOIN tas.SyncWorkplaceSwipeToTimesheetLog c WITH (NOLOCK) ON a.AutoID = c.AutoID
						OUTER APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, c.dtIN_Old, c.dtOUT_Old, 0) d
					WHERE 
						a.DT BETWEEN @startDate AND @endDate
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND (RTRIM(c.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	
						AND a.IsLastRow = 1
						AND c.IsActive = 1	

						--AND ISNULL(a.ShiftSpan, 0) <> 1 
						--AND	--Note: Update Timesheet with one swipe record only (Rev. #1.6)
						--(
						--	SELECT COUNT(AutoID) FROM tas.Tran_Timesheet
						--	WHERE EmpNo = a.EmpNo AND DT = a.DT
						--		AND (dtIN IS NOT NULL AND dtOUT IS NOT NULL)
						--) = 1	
						--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #2.5

						--Start of Rev. #2.3
						--AND 
						--(
						--	c.OTStartTime_New IS NULL OR c.OTEndTime_New IS NULL
						--) 
						--End of Rev. #2.3

						--Start of Rev. #2.8
						AND NOT EXISTS
						(
							SELECT 1	--AutoID 
							FROM tas.WorkplaceSwipeExclusion WITH (NOLOCK)
							WHERE EmpNo = a.EmpNo 
								AND RTRIM(CostCenter) = RTRIM(a.BusinessUnit)
								AND IsActive = 1
								AND @startDate >= EffectiveDate
						)
						--End of Rev. #2.8

						--Start of Rev. #3.3
						AND RTRIM(a.BusinessUnit) NOT IN
						(
							SELECT RTRIM(CostCenter) FROM tas.OTApprovalSetting WITH (NOLOCK) 
							WHERE CONVERT(VARCHAR, GETDATE(), 12) BETWEEN CONVERT(VARCHAR, EffectiveStartDate, 12) AND CONVERT(VARCHAR, EffectiveEndDate, 12)  
								AND IsActive = 1
						)
						--End of Rev. #3.3

					--Get the number of overtime records processed
					SELECT @overtimeRecordsProcessed = @@ROWCOUNT

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END
				END
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Delete transaction logs created by the "Workplace Swipes to Timesheet Synchronization Service"
				DELETE tas.SyncWorkplaceSwipeToTimesheetLog 
				WHERE DT BETWEEN @startDate AND @endDate
					AND (EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)	

				--Get the number of log records processed
				SELECT @logRecordsProcessed = @@ROWCOUNT

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				--IF @retError = @CONST_RETURN_OK
				--BEGIN

				--	--Call the stored procedure to undo timesheet corrections made through the "Workplace Swipe Correction Form"
				--	--EXEC tas.Pr_ProcessWorkplaceSwipeCorrection @startDate, @endDate, @costCenter, @empNo, 1									
				--END
			END
		END

		ELSE IF @actionTypeID = 2
		BEGIN

			--Update Timesheet records (For troubleshooting purpose)
			UPDATE tas.Tran_Timesheet
			SET dtIN = '2016-01-25 08:58:10.000',
				dtOUT = '2016-01-25 19:03:17.000',
				Shaved_IN = '2016-01-25 08:58:10.000',
				Shaved_OUT = '2016-01-25 19:03:17.000',
				Duration_Worked = DATEDIFF(mi, '2016-01-25 08:58:10.000', '2016-01-25 19:03:17.000'),
				Duration_Worked_Cumulative = Duration_Worked_Cumulative + (DATEDIFF(mi, '2016-01-25 08:58:10.000', '2016-01-25 19:03:17.000')),
				NetMinutes = DATEDIFF(mi, '2016-01-25 08:58:10.000', '2016-01-25 19:03:17.000'),
				NoPayHours = 0
			WHERE AutoID = 	0
				AND EmpNo = 0
		END

	END TRY
	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

EXIT_POINT:

	IF @retError = @CONST_RETURN_OK
	BEGIN

		IF @@TRANCOUNT > 0
			COMMIT TRANSACTION;		
	END

	ELSE
	BEGIN

		IF @actionTypeID = 0
		BEGIN

			--Delete transaction logs
			DELETE FROM tas.SyncWorkplaceSwipeToTimesheetLog
			WHERE AutoID IN
			(
				SELECT DISTINCT b.AutoID
				FROM tas.Tran_TempSwipeData a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) = b.DT
					INNER JOIN tas.Master_Employee_JDE_View c WITH (NOLOCK) ON a.EmpNo = c.EmpNo
				WHERE 
					a.DT IS NULL
					AND CONVERT(DATETIME, CONVERT(VARCHAR, DTSwipeNewProcess, 12)) BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(c.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			)
		END

		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION
	END

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@timeSheetRecordsProcessed AS TimesheetRecordProcessed,
			@overtimeRecordsProcessed AS OvertimeRecordProcessed,			
			@timeSheetRecordsProcessedMS AS TimesheetRecordProcessedMS,
			@overtimeRecordsProcessedMS AS OvertimeRecordProcessedMS,
			@logRecordsProcessed AS LogRecordProcessed
END 


/*	Debug:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@actionTypeID	INT = 0,		--(Note: 0 -> Update Timesheet; 1 -> Undo Timesheet Updates)
	@costCenter		VARCHAR(12) = NULL,
	@empNo			INT = NULL	

	EXEC tas.Pr_SyncWorkplaceSwipeToTimesheet_V3 '03/30/2016', '03/31/2016'
*/