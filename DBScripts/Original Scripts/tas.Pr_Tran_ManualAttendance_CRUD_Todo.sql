/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Tran_ManualAttendance_CRUD
*	Description: Performs insert, update, and delete operations against "Tran_ManualAttendance" table
*
*	Date			Author		Revision No.	Comments:
*	29/01/2017		Ervin		1.0				Created
*	05/03/2017		Ervin		1.1				Fixed bug that updates the Timesheet record on previous day if scheduled shift is Night Shift
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_Tran_ManualAttendance_CRUD
(	
	@actionType			INT,	
	@autoID				INT, 	
	@empNo				INT,
	@dtIN				DATETIME,
	@timeIN				DATETIME,
	@dtOUT				DATETIME,
	@timeOUT			DATETIME,
	@userID				VARCHAR(30)
)
AS
	
	DECLARE @newID					INT,
			@rowsAffected			INT,
			@ts_AutoID				INT,
			@shiftPatCode			VARCHAR(2),	
			@shiftCode				VARCHAR(10),
			@combinedTimeIn			DATETIME,
			@combinedTimeOut		DATETIME,
			@hasMultipleSwipe		BIT,
			@autoIDFirstSwipe		INT,
			@autoIDLastSwipe		INT   

	--Initialize variables
	SELECT	@newID					= 0,
			@rowsAffected			= 0,
			@ts_AutoID				= 0,
			@shiftPatCode			= '',	
			@shiftCode				= '',
			@combinedTimeIn			= NULL,
			@combinedTimeOut		= NULL,
			@hasMultipleSwipe		= 0,
			@autoIDFirstSwipe		= 0,
			@autoIDLastSwipe		= 0

	IF @actionType = 1		--Insert new record
	BEGIN
		
		INSERT INTO tas.Tran_ManualAttendance
		(
			EmpNo,
			dtIN,
			timeIN,
			dtOUT,
			[timeOUT],
			CreatedUser,
			CreatedTime
		)
		VALUES
		(
			@empNo,
			@dtIN,
			tas.fmtHHmm(@timeIN),
			@dtOUT,
			tas.fmtHHmm(@timeOUT),
			@userID,
			GETDATE()	
		)
		
		--Get the new ID
		SET @newID = @@identity

		IF @newID > 0
		BEGIN

			--Update the Timesheet record
			IF	@dtIN IS NOT NULL 
				AND @dtOUT IS NOT NULL				
			BEGIN							

				SELECT	@combinedTimeIn = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeIN)), @dtIN),	
						@combinedTimeOut = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeOUT)), @dtOUT)	

				IF @dtIN < @dtOUT
				BEGIN
                
					--Process Night shift
					SELECT	@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) 
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtOUT
						AND IsLastRow = 1

					UPDATE tas.Tran_Timesheet 
					SET dtIN = @combinedTimeIn,
						dtOUT = @combinedTimeOut,
						NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, @combinedTimeOut),
						Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode),
						Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
					WHERE EmpNo = @empNo
						AND DT = @dtOUT
						AND IsLastRow = 1

					--Calculate the total work duration
					UPDATE tas.Tran_Timesheet 
					SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
						Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
					WHERE EmpNo = @empNo
						AND DT = @dtOUT
						AND IsLastRow = 1
				END 

				ELSE IF @dtIN = @dtOUT
				BEGIN

					--Process Morning, Evening, or Day shifts
					IF @hasMultipleSwipe = 1
					BEGIN

						--Process multiple swipe record in the Timesheet
						SELECT	@shiftPatCode = RTRIM(a.ShiftPatCode),
								@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))
						FROM tas.Tran_Timesheet a
						WHERE a.EmpNo = @empNo
							AND a.DT = @dtOUT
							AND a.AutoID = @ts_AutoID

						UPDATE tas.Tran_Timesheet 
						SET dtIN = @combinedTimeIn,
							dtOUT = @combinedTimeOut,
							NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, @combinedTimeOut),
							Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode),
							Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1
                    END
                    
					ELSE
                    BEGIN
                    
						--Process single swipe record in the Timesheet
						SELECT	@ts_AutoID = a.AutoID,
								@shiftPatCode = RTRIM(a.ShiftPatCode),
								@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))
						FROM tas.Tran_Timesheet a
						WHERE a.EmpNo = @empNo
							AND a.DT = @dtOUT
							AND IsLastRow = 1

						UPDATE tas.Tran_Timesheet 
						SET dtIN = @combinedTimeIn,
							dtOUT = @combinedTimeOut,
							NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, @combinedTimeOut),
							Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode),
							Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1

						--Calculate the total work duration
						UPDATE tas.Tran_Timesheet 
						SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
							Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1
					END 
                END 
			END

			ELSE
            BEGIN

				IF	@dtIN IS NULL 
					AND @dtOUT IS NOT NULL
                BEGIN

					--Determine if there are multiple swipes
					SELECT @hasMultipleSwipe = tas.fnCheckIfMutipleGateEntry(@empNo, @dtOUT) 

					--Get the Auto ID of the first record where dtOUT is NULL
					SELECT @ts_AutoID = a.AutoID
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtOUT
						AND a.dtOUT IS NULL
					ORDER BY a.AutoID 

					--Manual entry for Time-out
					SELECT @combinedTimeOut = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeOUT)), @dtOUT)

					SELECT	@combinedTimeIn = a.dtIN,
							@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) 
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtOUT
						AND IsLastRow = 1

					IF	@combinedTimeIn IS NOT NULL 
						AND @combinedTimeOut IS NOT NULL
					BEGIN

						
						UPDATE tas.Tran_Timesheet 
						SET dtOUT = @combinedTimeOut,
							NetMinutes = DATEDIFF(MINUTE, dtIN, @combinedTimeOut),
							Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1

						--Calculate the total work duration
						UPDATE tas.Tran_Timesheet 
						SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
							Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1
                    END 
                END 

				ELSE IF @dtIN IS NOT NULL 
					AND @dtOUT IS NULL
                BEGIN

					--Manual entry for Time-in
					SELECT @combinedTimeIn = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeIN)), @dtIN)

					SELECT	@combinedTimeOut = a.dtOUT,
							@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) 
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtIN
						AND IsLastRow = 1

					IF @combinedTimeIn IS NOT NULL AND @combinedTimeOut IS NOT NULL
					BEGIN

						UPDATE tas.Tran_Timesheet 
						SET dtIN = @combinedTimeIn,
							NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, dtOUT),
							Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode)
						WHERE EmpNo = @empNo
							AND DT = @dtIN
							AND IsLastRow = 1

						--Calculate the total work duration
						UPDATE tas.Tran_Timesheet 
						SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
							Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
						WHERE EmpNo = @empNo
							AND DT = @dtIN
							AND IsLastRow = 1
                    END 
                END 
            END 
        END 
	END

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		UPDATE tas.Tran_ManualAttendance
		SET	dtIN = @dtIN,
            dtOUT = @dtOUT,
            timeIN = tas.fmtHHmm(@timeIN),
            [timeOUT] = tas.fmtHHmm(@timeOUT),
            LastUpdateUser = @userID,
			LastUpdateTime = GETDATE()
		WHERE AutoID = @autoID

		SELECT @rowsAffected = @@rowcount 

		IF @rowsAffected > 0
		BEGIN

			--Update the Timesheet record
			IF	@dtIN IS NOT NULL 
				AND @dtOUT IS NOT NULL				
			BEGIN

				SELECT	@combinedTimeIn = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeIN)), @dtIN),	
						@combinedTimeOut = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeOUT)), @dtOUT)	

				IF @dtIN < @dtOUT
				BEGIN
                
					--Process Night shift
					SELECT	@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) 
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtOUT
						AND IsLastRow = 1

					UPDATE tas.Tran_Timesheet 
					SET dtIN = @combinedTimeIn,
						dtOUT = @combinedTimeOut,
						NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, @combinedTimeOut),
						Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode),
						Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
					WHERE EmpNo = @empNo
						AND DT = @dtOUT
						AND IsLastRow = 1

					--Calculate the total work duration
					UPDATE tas.Tran_Timesheet 
					SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
						Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
					WHERE EmpNo = @empNo
						AND DT = @dtOUT
						AND IsLastRow = 1
				END 

				ELSE IF @dtIN = @dtOUT
				BEGIN

					--Process Morning, Evening, or Day shifts
					SELECT	@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtOUT
						AND IsLastRow = 1

					UPDATE tas.Tran_Timesheet 
					SET dtIN = @combinedTimeIn,
						dtOUT = @combinedTimeOut,
						NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, @combinedTimeOut),
						Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode),
						Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
					WHERE EmpNo = @empNo
						AND DT = @dtOUT
						AND IsLastRow = 1

					--Calculate the total work duration
					UPDATE tas.Tran_Timesheet 
					SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
						Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
					WHERE EmpNo = @empNo
						AND DT = @dtOUT
						AND IsLastRow = 1
                END 
			END

			ELSE
            BEGIN

				IF	@dtIN IS NULL 
					AND @dtOUT IS NOT NULL
                BEGIN

					--Manual entry for Time-out
					SELECT @combinedTimeOut = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeOUT)), @dtOUT)

					SELECT	@combinedTimeIn = a.dtIN,
							@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) 
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtOUT
						AND IsLastRow = 1

					IF	@combinedTimeIn IS NOT NULL 
						AND @combinedTimeOut IS NOT NULL
					BEGIN

						UPDATE tas.Tran_Timesheet 
						SET dtOUT = @combinedTimeOut,
							NetMinutes = DATEDIFF(MINUTE, dtIN, @combinedTimeOut),
							Shaved_OUT = tas.fnGetShavingTime(1, @combinedTimeOut, @shiftPatCode, @shiftCode)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1

						--Calculate the total work duration
						UPDATE tas.Tran_Timesheet 
						SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
							Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
						WHERE EmpNo = @empNo
							AND DT = @dtOUT
							AND IsLastRow = 1
                    END 
                END 

				ELSE IF @dtIN IS NOT NULL 
					AND @dtOUT IS NULL
                BEGIN

					--Manual entry for Time-in
					SELECT @combinedTimeIn = DATEADD(MINUTE, tas.fmtHHmm_Min(tas.fmtHHmm(@timeIN)), @dtIN)

					SELECT	@combinedTimeOut = a.dtOUT,
							@ts_AutoID = a.AutoID,
							@shiftPatCode = RTRIM(a.ShiftPatCode),
							@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) 
					FROM tas.Tran_Timesheet a
					WHERE a.EmpNo = @empNo
						AND a.DT = @dtIN
						AND IsLastRow = 1

					IF @combinedTimeIn IS NOT NULL AND @combinedTimeOut IS NOT NULL
					BEGIN

						UPDATE tas.Tran_Timesheet 
						SET dtIN = @combinedTimeIn,
							NetMinutes = DATEDIFF(MINUTE, @combinedTimeIn, dtOUT),
							Shaved_IN = tas.fnGetShavingTime(0, @combinedTimeIn, @shiftPatCode, @shiftCode)
						WHERE EmpNo = @empNo
							AND DT = @dtIN
							AND IsLastRow = 1

						--Calculate the total work duration
						UPDATE tas.Tran_Timesheet 
						SET Duration_Worked_Cumulative = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT),
							Duration_Worked = DATEDIFF(MINUTE, Shaved_IN, Shaved_OUT)
						WHERE EmpNo = @empNo
							AND DT = @dtIN
							AND IsLastRow = 1
                    END 
                END 
            END 
        END 
	END

	ELSE IF @actionType = 3		--Delete existing record 
	BEGIN

		DELETE FROM tas.Tran_ManualAttendance
		WHERE AutoID = @autoID

		--Undo manual entry changes in the Timesheet
		IF	@dtIN IS NOT NULL 
			AND @dtOUT IS NOT NULL				
		BEGIN

			UPDATE tas.Tran_Timesheet 
			SET dtIN = NULL,
				dtOUT = NULL,
				Shaved_IN = NULL,
				Shaved_OUT = NULL,
				NetMinutes = 0,
				Duration_Worked_Cumulative = 0,
				Duration_Worked = 0
			WHERE EmpNo = @empNo
				AND DT = @dtOUT
				AND IsLastRow = 1
		END 

		ELSE
        BEGIN

			IF	@dtIN IS NULL 
				AND @dtOUT IS NOT NULL
            BEGIN

				UPDATE tas.Tran_Timesheet 
				SET dtOUT = NULL,
					Shaved_OUT = NULL,
					NetMinutes = 0,
					Duration_Worked_Cumulative = 0,
					Duration_Worked = 0						
				WHERE EmpNo = @empNo
					AND DT = @dtOUT
					AND IsLastRow = 1
            END 

			ELSE IF @dtIN IS NOT NULL 
				AND @dtOUT IS NULL
            BEGIN

				UPDATE tas.Tran_Timesheet 
				SET dtIN = NULL,
					Shaved_IN = NULL,
					NetMinutes = 0,
					Duration_Worked_Cumulative = 0,
					Duration_Worked = 0		
				WHERE EmpNo = @empNo
					AND DT = @dtIN
					AND IsLastRow = 1
            END 
        END 		
	END

	ELSE IF (@actionType = 4)  --Delete record by Emp. No.
	BEGIN

		DELETE FROM tas.Tran_ManualAttendance
		WHERE EmpNo = @empNo

		SELECT @rowsAffected = @@rowcount
	END

	--Return the variables as resultset
	SELECT	@newID AS NewIdentityID,
			@rowsAffected AS RowsAffected


/*	Debugging:

PARAMETERS:
	@autoID					INT, 	
	@otReason				VARCHAR(10),	
	@comment				VARCHAR(1000),
	@userID					VARCHAR(30), 
	@otApproved				VARCHAR(1) = '0', 
	@mealVoucherEligibilityCode	VARCHAR(10) = NULL,
	@otDuration				INT = 0

	EXEC tas.Pr_Tran_ManualAttendance_CRUD

*/

