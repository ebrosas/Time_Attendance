USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_SyncAdminBldgSwipeToTimesheet]    Script Date: 29/08/2022 08:59:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_SyncAdminBldgSwipeToTimesheet
*	Description: This stored procedure is used to synchronize the valid swipe data from the Admin building readers into the Timesheet. It will replace the main gate first time in and last time out
*
*	Date:			Author:		Rev.#:		Comments:
*	15/04/2022		Ervin		1.0			Created
*	23/04/2022		Ervin		1.1			Check if Swipe Date is greater than or equal to the Effective Date set in "WorkplaceReaderSetting" table 
*	18/08/2022		Ervin		1.2			Added condition to remove NPH for Salary Staff employees only for Day Shift
***************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_SyncAdminBldgSwipeToTimesheet]
(
	@actionTypeID		TINYINT = 0,		--(Notes: 0 -> Synchronize valid swipes; 1 -> Synchronize missing swipes)
	@tsRowsAffected		INT OUTPUT, 
	@startDate			DATETIME,
	@endDate			DATETIME,	
	@costCenter			VARCHAR(12) = NULL,
	@empNo				INT = NULL	
)
AS	
BEGIN

	--Initialize parameters
	IF ISNULL(@costCenter, '') = '' OR RTRIM(@costCenter) = '0'
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL
			
	DECLARE	@tsRowCount					INT = 0,
			@shiftPatCode				VARCHAR(2),
			@shiftCode					VARCHAR(10),
			@filterEmpNo				INT = 0,
			@DT							DATETIME = NULL,
			@timeInWP					DATETIME = NULL,
			@timeOutWP					DATETIME = NULL,
			@shavedIN					DATETIME = NULL,
			@shavedOUT					DATETIME = NULL,
			@noPayHours					INT = 0,
			@netMinutes					INT = 0,
			@durationWorked				INT = 0,
			@durationWorkedCumulative	INT = 0,
			@durationRequired			INT = 0,
			@gracePeriod				INT = 0,
			@leaveType					VARCHAR(10) = '',
			@absenceReasonCode			VARCHAR(10) = '',
			@absenceReasonColumn		VARCHAR(10) = '',
			@isPublicHoliday			BIT = 0,
			@isSalStaff					BIT = 0,
			@isDILdayWorker				BIT = 0,
			@isDriver					BIT = 0,
			@isLiasonOfficer			BIT = 0,
			@isDayWorker_OR_Shifter		BIT = 0,
			@isDayShift					BIT = 1,
			@rowsAffected				INT = 0

	--Get the grade period
	SELECT @gracePeriod = ISNULL(a.Minutes_GracePeriod , 0)
	FROM tas.System_Values a WITH (NOLOCK)
			 
	IF @actionTypeID = 0
	BEGIN 

		DECLARE WorkplaceCursor CURSOR READ_ONLY FOR
		SELECT a.EmpNo, a.DT, b.TimeInWP, b.TimeOutWP
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_WorkplaceSwipe b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.SwipeDate
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) c
			CROSS APPLY
			(
				SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
				WHERE IsActive = 1 
					AND RTRIM(CostCenter) = RTRIM(b.CostCenter)
			) d		--Rev. #1.1
		WHERE 
			a.DT BETWEEN @startDate AND @endDate
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)							
			AND a.IsLastRow = 1							
			AND	
			(
				b.TimeInWP IS NOT NULL
				AND b.TimeOutWP IS NOT NULL
			)
			AND (c.IsWorkplaceEnabled = 1 AND c.IsAdminBldgEnabled = 1 AND c.IsSyncTimesheet = 1)
			AND (b.SwipeDate >= d.EffectiveDate AND d.EffectiveDate IS NOT NULL)	--Rev. #1.1

		OPEN WorkplaceCursor
		FETCH NEXT FROM WorkplaceCursor
		INTO @filterEmpNo, @DT, @timeInWP, @timeOutWP
	
		WHILE @@FETCH_STATUS = 0
		BEGIN

			--Get the total timesheet record count
			SELECT @tsRowCount = COUNT(*)
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @filterEmpNo
				AND a.DT = @DT

			--Get other attendance related information
			SELECT	@shiftPatCode = RTRIM(a.ShiftPatCode),
					@shiftCode = RTRIM(b.Effective_ShiftCode),
					@durationRequired = a.Duration_Required,
					@leaveType = RTRIM(a.LeaveType),
					@absenceReasonCode = RTRIM(a.AbsenceReasonCode),
					@absenceReasonColumn = RTRIM(a.AbsenceReasonColumn),
					@isPublicHoliday = a.IsPublicHoliday,
					@isSalStaff	= a.IsSalStaff,
					@isDILdayWorker = a.IsDILdayWorker,
					@isDriver = a.IsDriver,
					@isLiasonOfficer = a.IsLiasonOfficer,
					@isDayWorker_OR_Shifter = a.IsDayWorker_OR_Shifter,
					@isDayShift = c.IsDayShift		--Rev. #1.2
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				INNER JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(b.Effective_ShiftPatCode) = RTRIM(c.ShiftPatCode)		--Rev. #1.2
			WHERE a.EmpNo = @filterEmpNo
				AND a.DT = @DT
				AND a.IsLastRow = 1

			IF @tsRowCount = 1
			BEGIN
		
				--PRINT 'Goes here 1 timesheet record'

				/***************************************************************************************
					Update Timesheet having single record
				****************************************************************************************/
				--Calculate the shaving time
				SELECT	@shavedIN = tas.fnGetShavingTime(0, @timeInWP, @shiftPatCode, @shiftCode),
						@shavedOUT = tas.fnGetShavingTime(1, @timeOutWP, @shiftPatCode, @shiftCode)

				--Calculate the work duration
				SELECT	@netMinutes = DATEDIFF(MINUTE, @timeInWP, @timeOutWP),
						@durationWorked = DATEDIFF(MINUTE, @shavedIN, @shavedOUT),
						@durationWorkedCumulative = DATEDIFF(MINUTE, @shavedIN, @shavedOUT)

				--Calculate NPH
				IF	(@isSalStaff = 1 AND @isDayShift = 1)		--Rev. #1.2
					OR (@isDILdayWorker = 1 AND @isDayWorker_OR_Shifter = 1)
					OR (@isSalStaff = 0 AND @isDayWorker_OR_Shifter = 1)
					OR @isDriver = 1 
					OR @isLiasonOfficer = 1 
					OR @isPublicHoliday = 1		
					OR RTRIM(@shiftCode) = 'O'	
					OR ISNULL(@leaveType, '') <> ''	
				BEGIN

					--Set No Pay Hour to zero 
					SET @noPayHours = 0
				END

				ELSE
				BEGIN 

					IF @durationRequired > 0 AND @durationWorkedCumulative > 0
					BEGIN
			
						SET @noPayHours = @durationRequired - @durationWorkedCumulative
						IF @noPayHours < 0 OR @noPayHours <= @gracePeriod
							SET @noPayHours = 0
					END

					ELSE IF @durationRequired > 0 AND @durationWorkedCumulative = 0	
					BEGIN

						SET @noPayHours = @durationRequired
					END 

					ELSE
						SET @noPayHours = 0
				END 

				UPDATE tas.Tran_Timesheet 
				SET dtIN = @timeInWP,
					dtOUT = @timeOutWP,
					Shaved_IN = @shavedIN,
					Shaved_OUT = @shavedOUT,
					NoPayHours = @noPayHours,
					Duration_Worked = @durationWorked,
					Duration_Worked_Cumulative = @durationWorkedCumulative,
					NetMinutes = @netMinutes,
					LastUpdateUser = 'System Admin',
					LastUpdateTime = GETDATE()
				WHERE EmpNo = @filterEmpNo
					AND dt = @DT
					AND IsLastRow = 1

				--Get the number of affected rows
				SELECT @rowsAffected = @@ROWCOUNT
			END 

			ELSE IF @tsRowCount > 1
			BEGIN
		
				PRINT 'Goes here multiple timesheet record'

				/***************************************************************************************
					Update Timesheet having multiple records
				****************************************************************************************/
				--Get the first swipe record
				SELECT TOP 1 a.AutoID, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.Shaved_IN, a.Shaved_OUT, a.NoPayHours, a.Duration_Worked, a.Duration_Worked_Cumulative, a.NetMinutes, a.LastUpdateUser, a.LastUpdateTime 
				INTO #AttendanceTable1
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @filterEmpNo
					AND a.DT = @DT
				ORDER BY a.DT

				--Update the first swipe record
				UPDATE tas.Tran_Timesheet 
				SET tas.Tran_Timesheet.dtIN = @timeInWP,
					tas.Tran_Timesheet.Shaved_IN = c.Shaved_IN,
					tas.Tran_Timesheet.Duration_Worked = DATEDIFF(MINUTE, c.Shaved_IN, a.Shaved_OUT),
					tas.Tran_Timesheet.Duration_Worked_Cumulative = DATEDIFF(MINUTE, c.Shaved_IN, a.Shaved_OUT),
					tas.Tran_Timesheet.NetMinutes = DATEDIFF(MINUTE, @timeInWP, a.dtOUT),
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM #AttendanceTable1 a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.AutoID = b.AutoID AND a.EmpNo = b.EmpNo AND a.DT = b.DT
					OUTER APPLY
					(
						SELECT tas.fnGetShavingTime(0, @timeInWP, @shiftPatCode, @shiftCode) AS Shaved_IN
					) c

				--Get the last swipe record
				SELECT a.AutoID, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.Shaved_IN, a.Shaved_OUT, a.NoPayHours, a.Duration_Worked, a.Duration_Worked_Cumulative, a.NetMinutes, a.LastUpdateUser, a.LastUpdateTime 
				INTO #AttendanceTable2
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @filterEmpNo
					AND a.DT = @DT
					AND a.IsLastRow = 1

				--Update the last swipe record
				UPDATE tas.Tran_Timesheet 
				SET tas.Tran_Timesheet.dtOUT = @timeOutWP,
					tas.Tran_Timesheet.Shaved_OUT = c.Shaved_OUT,
					tas.Tran_Timesheet.Duration_Worked = DATEDIFF(MINUTE, a.Shaved_IN, c.Shaved_OUT),
					tas.Tran_Timesheet.Duration_Worked_Cumulative = DATEDIFF(MINUTE, a.Shaved_IN, c.Shaved_OUT),
					tas.Tran_Timesheet.NetMinutes = DATEDIFF(MINUTE, a.dtIN, @timeOutWP),
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM #AttendanceTable2 a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.AutoID = b.AutoID AND a.EmpNo = b.EmpNo AND a.DT = b.DT
					OUTER APPLY
					(
						SELECT tas.fnGetShavingTime(1, @timeOutWP, @shiftPatCode, @shiftCode) AS Shaved_OUT
					) c

				--Get the number of affected rows
				SELECT @rowsAffected = @@ROWCOUNT

				--Delete temporary tables
				DROP TABLE #AttendanceTable1
				DROP TABLE #AttendanceTable2
			END 
		
			--Get the number of affected rows
			SELECT @tsRowsAffected = @tsRowsAffected + @rowsAffected

			-- Retrieve next record
			FETCH NEXT FROM WorkplaceCursor
			INTO @filterEmpNo, @DT, @timeInWP, @timeOutWP
		END

		--Close and deallocate
		CLOSE WorkplaceCursor
		DEALLOCATE WorkplaceCursor
	END 

	ELSE IF @actionTypeID = 1
	BEGIN 

		DECLARE WorkplaceCursor CURSOR READ_ONLY FOR
		SELECT a.EmpNo, a.DT, b.TimeInWP, b.TimeOutWP
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_WorkplaceSwipe b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.SwipeDate
			OUTER APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, b.TimeInWP, b.TimeOutWP, 0) c
			CROSS APPLY tas.fnCheckWorkplaceEnabled(a.EmpNo) d
			CROSS APPLY
			(
				SELECT TOP 1 EffectiveDate FROM tas.WorkplaceReaderSetting WITH (NOLOCK)  
				WHERE IsActive = 1 
					AND RTRIM(CostCenter) = RTRIM(b.CostCenter)
			) e		--Rev. #1.1
		WHERE 
			a.DT BETWEEN @startDate AND @endDate
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)							
			AND a.IsLastRow = 1							
			AND (b.TimeInWP IS NULL OR b.TimeOutWP IS NULL)
			--AND ISNULL(a.CorrectionCode, '') = ''	
			AND NOT (RTRIM(a.ShiftPatCode) = 'SX' AND RTRIM(a.ShiftCode) = 'N')		
			AND (d.IsWorkplaceEnabled = 1 AND d.IsAdminBldgEnabled = 1 AND d.IsSyncTimesheet = 1)
			AND (b.SwipeDate >= e.EffectiveDate AND e.EffectiveDate IS NOT NULL)	--Rev. #1.1

		OPEN WorkplaceCursor
		FETCH NEXT FROM WorkplaceCursor
		INTO @filterEmpNo, @DT, @timeInWP, @timeOutWP
	
		WHILE @@FETCH_STATUS = 0
		BEGIN
	
			--Get the total timesheet record count
			SELECT @tsRowCount = COUNT(*)
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @filterEmpNo
				AND a.DT = @DT

			--Get other attendance related information
			SELECT	@shiftPatCode = RTRIM(a.ShiftPatCode),
					@shiftCode = RTRIM(b.Effective_ShiftCode),
					@durationRequired = a.Duration_Required,
					@leaveType = RTRIM(a.LeaveType),
					@absenceReasonCode = RTRIM(a.AbsenceReasonCode),
					@absenceReasonColumn = RTRIM(a.AbsenceReasonColumn),
					@isPublicHoliday = a.IsPublicHoliday,
					@isSalStaff	= a.IsSalStaff,
					@isDILdayWorker = a.IsDILdayWorker,
					@isDriver = a.IsDriver,
					@isLiasonOfficer = a.IsLiasonOfficer,
					@isDayWorker_OR_Shifter = a.IsDayWorker_OR_Shifter,
					@isDayShift = c.IsDayShift		--Rev. #1.2
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				INNER JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(b.Effective_ShiftPatCode) = RTRIM(c.ShiftPatCode)		--Rev. #1.2
			WHERE a.EmpNo = @filterEmpNo
				AND a.DT = @DT
				AND a.IsLastRow = 1

			IF @tsRowCount = 1
			BEGIN
		
				/***************************************************************************************
					Update Timesheet having single record
				****************************************************************************************/
				--Calculate the shaving time
				SELECT	@shavedIN = tas.fnGetShavingTime(0, @timeInWP, @shiftPatCode, @shiftCode),
						@shavedOUT = tas.fnGetShavingTime(1, @timeOutWP, @shiftPatCode, @shiftCode)

				--Calculate the work duration
				SELECT	@netMinutes = DATEDIFF(MINUTE, @timeInWP, @timeOutWP),
						@durationWorked = DATEDIFF(MINUTE, @shavedIN, @shavedOUT),
						@durationWorkedCumulative = DATEDIFF(MINUTE, @shavedIN, @shavedOUT)

				--Calculate NPH
				IF	(@isSalStaff = 1 AND @isDayShift = 1)		--Rev. #1.2
					OR (@isDILdayWorker = 1 AND @isDayWorker_OR_Shifter = 1)
					OR (@isSalStaff = 0 AND @isDayWorker_OR_Shifter = 1)
					OR @isDriver = 1 
					OR @isLiasonOfficer = 1 
					OR @isPublicHoliday = 1		
					OR RTRIM(@shiftCode) = 'O'	
					OR ISNULL(@leaveType, '') <> ''	
				BEGIN

					--Set No Pay Hour to zero 
					SET @noPayHours = 0
				END

				ELSE
				BEGIN 

					IF @durationRequired > 0 AND @durationWorkedCumulative > 0
					BEGIN
			
						SET @noPayHours = @durationRequired - @durationWorkedCumulative
						IF @noPayHours < 0 OR @noPayHours <= @gracePeriod
							SET @noPayHours = 0
					END

					ELSE IF @durationRequired > 0 AND @durationWorkedCumulative = 0	
					BEGIN

						SET @noPayHours = @durationRequired
					END 

					ELSE
						SET @noPayHours = 0
				END 

				UPDATE tas.Tran_Timesheet 
				SET dtIN = @timeInWP,
					dtOUT = @timeOutWP,
					Shaved_IN = @shavedIN,
					Shaved_OUT = @shavedOUT,
					NoPayHours = @noPayHours,
					Duration_Worked = @durationWorked,
					Duration_Worked_Cumulative = @durationWorkedCumulative,
					NetMinutes = @netMinutes,
					LastUpdateUser = 'System Admin',
					LastUpdateTime = GETDATE()
				WHERE EmpNo = @filterEmpNo
					AND dt = @DT
					AND IsLastRow = 1

				--Get the number of affected rows
				SELECT @rowsAffected = @@ROWCOUNT
			END 

			ELSE IF @tsRowCount > 1
			BEGIN

				/***************************************************************************************
					Update Timesheet having multiple records
				****************************************************************************************/
				--Get the first swipe record
				SELECT TOP 1 a.AutoID, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.Shaved_IN, a.Shaved_OUT, a.NoPayHours, a.Duration_Worked, a.Duration_Worked_Cumulative, a.NetMinutes, a.LastUpdateUser, a.LastUpdateTime 
				INTO #AttendanceTable11
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @filterEmpNo
					AND a.DT = @DT
				ORDER BY a.DT

				--Update the first swipe record
				UPDATE tas.Tran_Timesheet 
				SET tas.Tran_Timesheet.dtIN = @timeInWP,
					tas.Tran_Timesheet.Shaved_IN = c.Shaved_IN,
					tas.Tran_Timesheet.Duration_Worked = DATEDIFF(MINUTE, c.Shaved_IN, a.Shaved_OUT),
					tas.Tran_Timesheet.Duration_Worked_Cumulative = DATEDIFF(MINUTE, c.Shaved_IN, a.Shaved_OUT),
					tas.Tran_Timesheet.NetMinutes = DATEDIFF(MINUTE, @timeInWP, a.dtOUT),
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM #AttendanceTable11 a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.AutoID = b.AutoID AND a.EmpNo = b.EmpNo AND a.DT = b.DT
					OUTER APPLY
					(
						SELECT tas.fnGetShavingTime(0, @timeInWP, @shiftPatCode, @shiftCode) AS Shaved_IN
					) c
			

				--Get the last swipe record
				SELECT a.AutoID, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.Shaved_IN, a.Shaved_OUT, a.NoPayHours, a.Duration_Worked, a.Duration_Worked_Cumulative, a.NetMinutes, a.LastUpdateUser, a.LastUpdateTime 
				INTO #AttendanceTable12
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @filterEmpNo
					AND a.DT = @DT
					AND a.IsLastRow = 1

				--Update the last swipe record
				UPDATE tas.Tran_Timesheet 
				SET tas.Tran_Timesheet.dtOUT = @timeOutWP,
					tas.Tran_Timesheet.Shaved_OUT = c.Shaved_OUT,
					tas.Tran_Timesheet.Duration_Worked = DATEDIFF(MINUTE, a.Shaved_IN, c.Shaved_OUT),
					tas.Tran_Timesheet.Duration_Worked_Cumulative = DATEDIFF(MINUTE, a.Shaved_IN, c.Shaved_OUT),
					tas.Tran_Timesheet.NetMinutes = DATEDIFF(MINUTE, a.dtIN, @timeOutWP),
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM #AttendanceTable12 a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.AutoID = b.AutoID AND a.EmpNo = b.EmpNo AND a.DT = b.DT
					OUTER APPLY
					(
						SELECT tas.fnGetShavingTime(1, @timeOutWP, @shiftPatCode, @shiftCode) AS Shaved_OUT
					) c

				--Get the number of affected rows
				SELECT @rowsAffected = @@ROWCOUNT

				--Delete temporary tables
				DROP TABLE #AttendanceTable11
				DROP TABLE #AttendanceTable12
			END 

			--Get the number of affected rows
			SELECT @tsRowsAffected = @tsRowsAffected + @rowsAffected

			-- Retrieve next record
			FETCH NEXT FROM WorkplaceCursor
			INTO @filterEmpNo, @DT, @timeInWP, @timeOutWP
		END

		--Close and deallocate
		CLOSE WorkplaceCursor
		DEALLOCATE WorkplaceCursor
	END 
END 

/*	Debug:

	DECLARE	@return_value			INT,
			@validSwipeRows			INT = 0,
			@swipeDate				DATETIME = '04/12/2022',
			@costCenter				VARCHAR(12) = '7600',
			@empNo					INT = 10003589	

	EXEC	@return_value = tas.Pr_SyncAdminBldgSwipeToTimesheet
			@actionTypeID = 0,
			@tsRowsAffected = @validSwipeRows OUTPUT,
			@startDate = @swipeDate,
			@endDate = @swipeDate,
			@costCenter = @costCenter,
			@empNo = @empNo

	SELECT @validSwipeRows AS RowsAffected

*/

