USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnGetCorrectedMainGateWorkplaceSwipe]    Script Date: 28/01/2021 13:46:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetCorrectedMainGateWorkplaceSwipe
*	Description: This table-view function is used to get the corrected main gate and workplace swipe data
*
*	Date:			Author:		Rev.#:		Comments:
*	29/10/2015		Ervin		1.0			Created
*	09/11/2015		Ervin		1.1			Set @timeInWP and @timeOutWP to null if no swipe record is found at the workplace based on the shift timing
*	10/12/2015		Ervin		1.2			Check if the date difference between @timeInWP and @timeOutWP is less or equal to 300 seconds. If true, then set @timeOutWP to NULL
*	10/12/2015		Ervin		1.3			Set the workplace Time-in to null if it is less than the Main Gate Time-in. 
											Set the workplace Time-out to null if it is greated than the Main Gate Time-out
*	29/12/2015		Ervin		1.4			Commented code changes applied in Rev. #1.3 since the clock time in the reader devices is 3 hours late
*	29/12/2015		Ervin		1.5			Check Swipe-in time at the workplace that should be put into the Swipe-out time
*	17/01/2016		Ervin		1.6			Set the workplace Time-in to null if it is less than the Main Gate Time-in. SET the workplace Time-out to null if it is greater than the Main Gate Time-out
*	20/01/2016		Ervin		1.7			Added extra condition in calculating the valued for @timeInWP and @timeOutWP variables
*	20/01/2016		Ervin		1.8			Check if Main Gate Swipe In/Out is equal to the Timesheet
*	22/01/2016		Ervin		1.9			Recalculated the value of "@timeInWP" and "@timeOutWP" variables for Night Shift workers and worked duration greater than duration required
*	25/01/2016		Ervin		2.0			Check if time diffecrence between @timeInMG and @timeInWP is greater than 480 mins. If true, then get the first time-in at the workplace
*	03/02/2016		Ervin		2.1			Added condition that gets the workplace Time-in from previous day if @durationShift > 0 and @durationWorked = 0
*	13/04/2016		Ervin		2.2			Added condition that check if the difference between @timeOutWP and @timeOutMG is greater then 4 hours
*	23/06/2016		Ervin		2.3			Refactored logic in fetching the Shift Code and calculation of main gate swipes
*	25/06/2016		Ervin		2.4			Added extra checking to get the correct shift timing 
*	26/06/2016		Ervin		2.5			Modified the logic in calculating the value for "@timeInWP"
*	02/07/2016		Ervin		2.6			Refactored the logic in calculating the value for @dtIN and @dtOUT variables
*	05/01/2021		Ervin		2.7			Refactored the code to enhance data retrieval performance
******************************************************************************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetCorrectedMainGateWorkplaceSwipe]
(
	@empNo		INT,
	@dt			DATETIME
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo			INT,
	SwipeDate		DATETIME,
	TimeInMG		DATETIME,
	TimeOutMG		DATETIME,
	TimeInWP		DATETIME,
	TimeOutWP		DATETIME,
	NetMinutesMG	FLOAT,
	NetMinutesWP	FLOAT,
	ShiftPatCode	VARCHAR(2),
	ShiftCode		VARCHAR(10),
	ShiftSpan		BIT,
	ArrivalFrom		DATETIME,
	ArrivalTo		DATETIME,
	DepartFrom		DATETIME,
	DepartTo		DATETIME
)
AS
BEGIN

	DECLARE @myTable TABLE 
	(
		EmpNo			INT,
		SwipeDate		DATETIME,
		TimeInMG		DATETIME,
		TimeOutMG		DATETIME,
		TimeInWP		DATETIME,
		TimeOutWP		DATETIME,
		NetMinutesMG	FLOAT,
		NetMinutesWP	FLOAT,
		ShiftPatCode	VARCHAR(2),
		ShiftCode		VARCHAR(10),
		ShiftSpan		BIT,
		ArrivalFrom		DATETIME,
		ArrivalTo		DATETIME,
		DepartFrom		DATETIME,
		DepartTo		DATETIME
	)

	--Declare field variables
	DECLARE	@timeInMG DATETIME,
			@timeOutMG DATETIME,
			@timeInWP DATETIME,
			@timeOutWP DATETIME,
			@netMinutesMG FLOAT,
			@netMinutesWP FLOAT

	--Initialize field variables
	SELECT	@timeInMG		= NULL,
			@timeOutMG		= NULL,
			@timeInWP		= NULL,
			@timeOutWP		= NULL,
			@netMinutesMG	= 0,
			@netMinutesWP	= 0

	--Declare business logic variables
	DECLARE	@dtIN				DATETIME,
			@dtOUT				DATETIME,
			@shiftPatCode		VARCHAR(2),
			@shiftCode			VARCHAR(10),			
			@shiftSpan			BIT,
			@arrivalFrom		DATETIME,
			@arrivalTo			DATETIME,
			@departFrom			DATETIME,
			@departTo			DATETIME,
			@durationShift		INT,
			@durationWorked		INT,
			@actualShiftCode	VARCHAR(10),
			@shiftCodeTemp		VARCHAR(10),
			@durationRequired	INT   

	--Get the time in and out data from the Timesheet
	SELECT	@dtIN = CASE WHEN CONVERT(TIME, a.dtIN) = CONVERT(TIME, '23:00:00.000')		--Rev. #2.6
						THEN a.dtIN
						ELSE b.TimeInMG
					END,
			@dtOUT = CASE WHEN CONVERT(TIME, a.dtOUT) = CONVERT(TIME, '23:00:00.000')	--Rev. #2.6
						THEN a.dtOUT
						ELSE b.TimeOutMG
					END,
			@shiftPatCode = RTRIM(a.ShiftPatCode),
			@shiftCode = RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)),					
			@shiftSpan = ISNULL(a.ShiftSpan, 0),
			@durationShift = ISNULL(a.Duration_Shift, 0),
			@durationWorked = ISNULL(a.Duration_Worked, 0),
			@actualShiftCode = a.Actual_ShiftCode,
			@shiftCodeTemp = a.ShiftCode
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		LEFT JOIN tas.Tran_WorkplaceSwipe b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.SwipeDate	--Rev. #2.3
	WHERE a.EmpNo = @empNo
		AND a.DT = @dt
		AND a.IsLastRow = 1

	--Get the Shift Timing
	SELECT	@arrivalFrom = a.ArrivalFrom,
			@arrivalTo = a.ArrivalTo,
			@departFrom = a.DepartFrom,
			@departTo = a.DepartTo
	FROM tas.Master_ShiftTimes a WITH (NOLOCK)
	WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
		AND RTRIM(a.ShiftCode) = @shiftCode

	--Start of Rev. #2.3
	--Validate if shift timing is correct
	IF	@dtIN IS NOT NULL	
		AND 
		(
			(ISNULL(@actualShiftCode, '') <> '' AND ISNULL(@shiftCodeTemp, '') <> '')
			AND
            @actualShiftCode <> @shiftCodeTemp
		)
		AND NOT CONVERT(TIME, @dtIN) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @arrivalTo)
	BEGIN

		--Get the Shift timing info based on @shiftCodeTemp
		SELECT	@arrivalFrom = a.ArrivalFrom,
				@arrivalTo = a.ArrivalTo,
				@departFrom = a.DepartFrom,
				@departTo = a.DepartTo
		FROM tas.Master_ShiftTimes a WITH (NOLOCK)
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
			AND RTRIM(a.ShiftCode) = @shiftCodeTemp

		--Start of Rev. #2.4
		IF CONVERT(TIME, @dtIN) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @arrivalTo)
		BEGIN

			--Set the correct shift code
			SET @shiftCode = @shiftCodeTemp
		END 

		ELSE
        BEGIN

			--Find the correct shift timing
			SELECT	@arrivalFrom = a.ArrivalFrom,
					@arrivalTo = a.ArrivalTo,
					@departFrom = a.DepartFrom,
					@departTo = a.DepartTo,
					@shiftCode = RTRIM(a.ShiftCode)
			FROM tas.Master_ShiftTimes a WITH (NOLOCK)
			WHERE CONVERT(TIME, @dtIN) BETWEEN CONVERT(TIME, a.ArrivalFrom)	AND CONVERT(TIME, a.ArrivalTo)
				AND 
				(
					CONVERT(TIME, @dtIN) BETWEEN CONVERT(TIME, a.DepartFrom)	AND CONVERT(TIME, a.DepartTo)
					OR
					CONVERT(TIME, @dtIN) > CONVERT(TIME, a.DepartTo)
				)
				AND RTRIM(a.ShiftPatCode) = @shiftPatCode

			IF NOT CONVERT(TIME, @dtIN) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @arrivalTo)
			BEGIN

				--Set the correct shift code
				SET @shiftCode = @shiftCodeTemp
			END 
        END 
		--End of Rev. #2.4
    END	
	--End of rev. #2.3

	--Calculate the duration required
	SET @durationRequired = DATEDIFF(n, @arrivalTo, @departFrom)
	IF @durationRequired < 0
		SET @durationRequired = DATEDIFF(n, @arrivalTo, @departFrom) + (24 * 60)

	IF @shiftCode <> 'N'
	BEGIN

		--Get the first swipe-in at the Main Gate
		SELECT TOP 1 @timeInMG = a.SwipeTime
		FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @dt
			AND RTRIM(a.SwipeType) = 'IN'
		ORDER BY a.SwipeTime

		--Get the last swipe-out at the Main Gate
		SELECT TOP 1 @timeOutMG = a.SwipeTime
		FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @dt
			AND RTRIM(a.SwipeType) = 'OUT'
		ORDER BY a.SwipeTime --DESC

		IF @shiftSpan = 0
		BEGIN

			--Get the first swipe-in at the Workplace
			SELECT TOP 1 @timeInWP = a.SwipeTime
			FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.SwipeDate = @dt
				AND 
				(
					(@arrivalFrom IS NOT NULL AND @departFrom IS NOT NULL)
					AND
					(
						CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
						OR 
						(
							CONVERT(TIME, a.SwipeTime) > CONVERT(TIME, @departFrom)
							AND
							DATEDIFF(N, CONVERT(TIME, @departFrom), CONVERT(TIME, a.SwipeTime)) < 480
						)
						--Start of Rev. #1.7
						OR
						(
							NOT CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departTo)
							AND CONVERT(TIME, a.SwipeTime) < CONVERT(TIME, @arrivalFrom)
						)
						--End of Rev. #1.7
					)
				)
			ORDER BY a.SwipeTime
		END

		ELSE 
		BEGIN
			
			--Get the last swipe-in at the Workplace
			SELECT TOP 1 @timeInWP = a.SwipeTime
			FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.SwipeDate = @dt
				AND 
				(
					(@arrivalFrom IS NOT NULL AND @departFrom IS NOT NULL)
					AND
					(
						CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
						OR 
						(
							CONVERT(TIME, a.SwipeTime) > CONVERT(TIME, @departFrom)
							AND
							DATEDIFF(N, CONVERT(TIME, @departFrom), CONVERT(TIME, a.SwipeTime)) < 480
						)
						--Start of Rev. #1.7
						OR
						(
							NOT CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departTo)
							AND CONVERT(TIME, a.SwipeTime) < CONVERT(TIME, @arrivalFrom)
						)
						--End of Rev. #1.7
					)
				)
			ORDER BY a.SwipeTime DESC

			--Start of Rev. #2.0
			IF	CONVERT(TIME, @timeInWP) > CONVERT(TIME, @timeInMG)
				AND CONVERT(VARCHAR, DATEDIFF(n, @timeInMG, @timeInWP)) > 480
			BEGIN

				--Get the first swipe-in at the Workplace
				SELECT TOP 1 @timeInWP = a.SwipeTime
				FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.SwipeDate = @dt
					AND 
					(
						(@arrivalFrom IS NOT NULL AND @departFrom IS NOT NULL)
						AND
						(
							CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
							OR 
							(
								CONVERT(TIME, a.SwipeTime) > CONVERT(TIME, @departFrom)
								AND
								DATEDIFF(N, CONVERT(TIME, @departFrom), CONVERT(TIME, a.SwipeTime)) < 480
							)
							OR
							(
								NOT CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departTo)
								AND CONVERT(TIME, a.SwipeTime) < CONVERT(TIME, @arrivalFrom)
							)
						)
					)
				ORDER BY a.SwipeTime 
			END
			--End of Rev. #2.0
		END

		--Get the last swipe-out at the Workplace
		SELECT TOP 1 @timeOutWP = a.SwipeTime
		FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @dt
			AND
			(
				(@arrivalTo IS NOT NULL AND @departTo IS NOT NULL)
				AND
				(
					CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
					OR 
					(
						CONVERT(TIME, a.SwipeTime) > CONVERT(TIME, @departTo)
						AND
						DATEDIFF(N, CONVERT(TIME, @departTo), CONVERT(TIME, a.SwipeTime)) < 480
					)
					--Start of Rev. #1.7
					OR
					(
						NOT CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
						AND CONVERT(TIME, a.SwipeTime) > CONVERT(TIME, @departTo)
					)
					--End of Rev. #1.7
				)
			)

		ORDER BY a.SwipeTime DESC
	END

	ELSE 
	BEGIN

		--Get the last swipe-in at the Main Gate on the previous day
		SELECT TOP 1 @timeInMG = a.SwipeTime
		FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = DATEADD(d, -1, @dt)
			AND RTRIM(a.SwipeType) = 'IN'
		ORDER BY a.SwipeTime DESC

		--Get the first swipe-out at the Main Gate on current day
		SELECT TOP 1 @timeOutMG = a.SwipeTime
		FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @dt
			AND RTRIM(a.SwipeType) = 'OUT'
		ORDER BY a.SwipeTime 

		--Get the last swipe-in at the Workplace on previous day
		SELECT TOP 1 @timeInWP = a.SwipeTime
		FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = DATEADD(d, -1, @dt)			
			--Start of Rev. #2.5
			AND 
			(
				(@arrivalFrom IS NOT NULL AND @departFrom IS NOT NULL)
				AND
				(
					a.SwipeTime BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR(10), DATEADD(d, -1, @dt), 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @arrivalFrom), 108)) AND CONVERT(DATETIME, CONVERT(VARCHAR(10), @dt, 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @departFrom), 108))
					OR
					(					
						(
							a.SwipeTime < CONVERT(DATETIME, CONVERT(VARCHAR(10), DATEADD(d, -1, @dt), 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @arrivalFrom), 108)) 											
							AND a.SwipeTime <= CONVERT(DATETIME, CONVERT(VARCHAR(10), @dt, 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @departFrom), 108))
						)
						AND DATEDIFF(n, a.SwipeTime, CONVERT(DATETIME, CONVERT(VARCHAR(10), DATEADD(d, -1, @dt), 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @arrivalFrom), 108))) < (@durationRequired / 2)
					)					
				)
			)
			--End of Rev. #2.5
		ORDER BY a.SwipeTime

		--Start of Rev. #1.9
		IF @timeInWP IS NULL
		BEGIN

			IF	@durationShift > 0
				AND @durationWorked > 0				
			BEGIN
            
				IF @durationWorked > @durationShift 
				BEGIN

					SELECT TOP 1 @timeInWP = a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
					ORDER BY a.SwipeTime
				END

				ELSE
				BEGIN
					SELECT TOP 1 @timeInWP = a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = DATEADD(d, -1, @dt)
					ORDER BY a.SwipeTime
				END
			END

			--Start of Rev. #2.1
			ELSE IF @durationShift > 0
				AND @durationWorked = 0
			BEGIN

				SELECT TOP 1 @timeInWP = a.SwipeTime
				FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.SwipeDate = DATEADD(d, -1, @dt)
				ORDER BY a.SwipeTime
			END
			--End of Rev. #2.1
		END
		--End of Rev. #1.9
		
		--Get the first swipe-out at the Workplace on current day
		SELECT TOP 1 @timeOutWP = a.SwipeTime
		FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.SwipeDate = @dt
			AND
			(
				(@arrivalTo IS NOT NULL AND @departTo IS NOT NULL)
				AND
				--CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
				a.SwipeTime 
					BETWEEN 
					CONVERT(DATETIME, CONVERT(VARCHAR(10), DATEADD(d, -1, @dt), 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @arrivalTo), 108))
					AND 
					CONVERT(DATETIME, CONVERT(VARCHAR(10), @dt, 101) + ' ' + CONVERT(VARCHAR, CONVERT(TIME, @departTo), 108))
				
			)
		ORDER BY a.SwipeTime 

		--Start of Rev. #1.9
		IF	
		(
			(
				@timeInWP = @timeOutWP
				AND @durationShift > 0
				AND @durationWorked > 0
				AND @durationWorked > @durationShift 
			)
			OR
			(
				@durationShift > 0 AND DATEDIFF(N, CONVERT(TIME, @timeOutWP), CONVERT(TIME, @timeOutMG)) > (@durationShift / 2)
			)
		)
		BEGIN

			SELECT TOP 1 @timeOutWP = a.SwipeTime
			FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.SwipeDate = @dt
			ORDER BY a.SwipeTime DESC
		END
		--End of Rev. #1.9

		--Start of Rev. #2.2
		ELSE IF DATEDIFF(MINUTE, CONVERT(TIME, @timeOutWP), CONVERT(TIME, @timeOutMG)) > 240 
		BEGIN

			--Get the last swipe-out at the Workplace on current day
			SELECT TOP 1 @timeOutWP = a.SwipeTime
			FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.SwipeDate = @dt
			ORDER BY a.SwipeTime DESC 
        END        
		--End of Rev. #2.2
	END

	--Start of Rev. #1.8
	--Validate Main Gate Swipe against the Timesheet swipe
	IF CONVERT(TIME, @timeInMG) <> CONVERT(TIME, @dtIN) 
		SET @timeInMG = @dtIN

	IF CONVERT(TIME, @timeOutMG) <> CONVERT(TIME, @dtOUT) 
		SET @timeOutMG = @dtOUT
	--End of Rev. #1.8

	--Check if the Timesheet time-in equals to 23:00:00
	IF CONVERT(TIME, @dtIN) = CONVERT(TIME, '23:00:00')
	BEGIN

		--Set the Main Gate and Workplace time-in equal to Timesheet time-in
		SELECT	@timeInMG = @dtIN,
				@timeInWP = @dtIN
	END

	--Check if the Timesheet time-out equals to 23:00:00
	IF CONVERT(TIME, @dtOUT) = CONVERT(TIME, '23:00:00')
	BEGIN

		--Set the Main Gate and Workplace time-out equal to Timesheet time-out
		SELECT	@timeOutMG = @dtOUT,
				@timeOutWP = @dtOUT
	END

	--Check if Workplace time-in is less than Main Gate time-in
	IF @timeInWP < @timeInMG
	BEGIN

		--Get time-in based on @arrivalFrom and @departFrom
		IF ISNULL(@arrivalFrom, '') <> '' AND ISNULL(@departFrom, '') <> ''
		BEGIN

			IF @shiftCode <> 'N'
			BEGIN

				IF EXISTS
				(
					SELECT TOP 1 a.SwipeTime 
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
				)
				BEGIN

					SELECT TOP 1 @timeInWP = a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
					ORDER BY a.SwipeTime DESC
				END
				ELSE
					SET @timeInWP = NULL	--Rev. #1.1
			END

			ELSE
			BEGIN

				IF EXISTS
				(
					SELECT TOP 1 a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = DATEADD(d, -1, @dt)
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
				)
				BEGIN

					SELECT TOP 1 @timeInWP = a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = DATEADD(d, -1, @dt)
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @departFrom)
					ORDER BY a.SwipeTime DESC
				END
				ELSE
					SET @timeInWP = NULL	--Rev. #1.1
			END
		END
		ELSE
			SET @timeInWP = NULL
	END

	--Check if Workplace time-out is greater than Main Gate time-out
	IF @timeOutWP > @timeOutMG
	BEGIN

		--Get time-out based on @arrivalTo and @departTo
		IF ISNULL(@arrivalTo, '') <> '' AND ISNULL(@departTo, '') <> ''
		BEGIN

			IF @shiftCode <> 'N'
			BEGIN

				IF EXISTS
				(
					SELECT TOP 1 a.SwipeTime 
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
				)
				BEGIN

					SELECT TOP 1 @timeOutWP = a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
					ORDER BY a.SwipeTime ASC
				END
				ELSE
					SET @timeOutWP = NULL	--Rev. #1.1
			END

			ELSE
			BEGIN

				IF EXISTS
				(
					SELECT TOP 1 a.SwipeTime 
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
				)
				BEGIN

					SELECT TOP 1 @timeOutWP = a.SwipeTime
					FROM tas.Vw_WorplaceSwipeRawData a WITH (NOLOCK)
					WHERE a.EmpNo = @empNo
						AND a.SwipeDate = @dt
						AND CONVERT(TIME, a.SwipeTime) BETWEEN CONVERT(TIME, @arrivalTo) AND CONVERT(TIME, @departTo)
					ORDER BY a.SwipeTime ASC
				END
				ELSE
					SET @timeOutWP = NULL	--Rev. #1.1
			END
		END
		ELSE
			SET @timeOutWP = NULL
	END

	--Check if Workplace time-in equals to Workplace time-out
	IF @timeInWP = @timeOutWP
	BEGIN

		DECLARE @timeInDiffMGToWP int,
				@timeOutDiffMgToWP int,
				@timeInGap int,
				@timeOutGap int

		SELECT	@timeInDiffMGToWP = DATEDIFF(n, @timeInMG, @timeInWP),
				@timeOutDiffMgToWP = DATEDIFF(n, @timeOutWP, @timeOutMG)

		SELECT	@timeInGap = 480 - @timeInDiffMGToWP,
				@timeOutGap = 480 - @timeOutDiffMgToWP

		IF @timeInGap > @timeOutGap
			SET @timeOutWP = NULL
		ELSE
			SET @timeInWP = NULL
	END		

	/************************************ Start of Rev. #1.2 ****************************************************/
	--Check if the date difference between @timeInWP and @timeOutWP is less or equal to 300 seconds. If true, then set @timeOutWP to NULL
	IF ISNULL(@timeInWP, '') <> '' AND ISNULL(@timeOutWP, '') <> ''
	BEGIN 

		IF DATEDIFF(ss, @timeInWP, @timeOutWP) <= 300
			SET @timeOutWP = NULL
	END
	/************************************ End of Rev. #1.2 ****************************************************/


	/************************************ Start of Rev. #1.5 ****************************************************/
	--Check Swipe-in time at the workplace that should be put into the Swipe-out time
	IF	(@timeInMG IS NOT NULL AND @timeOutMG IS NOT NULL)
		AND (@timeInWP IS NOT NULL AND @timeOutWP IS NULL)
		AND (DATEDIFF(n, @timeInMG, @timeInWP) > 180)
	BEGIN

		SET @timeOutWP = @timeInWP
		SET @timeInWP = NULL
	END
	/************************************ End of Rev. #1.5 ****************************************************/


	/************************************ Start of Rev. #1.6 ****************************************************/
	--Set the Workplace Time-in to null if it is less than the Main Gate Time-in. 	
	IF @timeInMG IS NOT NULL AND @timeInWP IS NOT NULL 
	BEGIN 

		IF CONVERT(TIME, @timeInMG) > CONVERT(TIME, @timeInWP)
			SET @timeInWP = NULL
	END

	--SET the Workplace Time-out to null if it is greater than the Main Gate Time-out
	IF @timeOutMG IS NOT NULL AND @timeOutWP IS NOT NULL
	BEGIN 

		IF CONVERT(TIME, @timeOutMG) < CONVERT(TIME, @timeOutWP)
			SET @timeOutWP = NULL
	END
	/************************************ End of Rev. #1.6 ****************************************************/

	--Calculate the Main Gate swipe duration
	IF ISNULL(@timeInMG, '') <> '' AND ISNULL(@timeOutMG, '') <> ''
	BEGIN
    
		SELECT @netMinutesMG = DATEDIFF(n, @timeInMG, @timeOutMG)
		IF @netMinutesMG < 0
			SELECT @netMinutesMG = DATEDIFF(n, @timeInMG, @timeOutMG) + (24 * 60)	--Rev. #2.3
	END 

	--Calculate the Workplace swipe duration
	IF ISNULL(@timeInWP, '') <> '' AND ISNULL(@timeOutWP, '') <> ''
	BEGIN
    
		SELECT @netMinutesWP = DATEDIFF(n, @timeInWP, @timeOutWP)
		IF @netMinutesWP < 0
			SELECT @netMinutesWP = DATEDIFF(n, @timeInWP, @timeOutWP) + (24 * 60)	--Rev. #2.3
	END 

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@empNo,
			@DT,
			@timeInMG,
			@timeOutMG,
			@timeInWP,
			@timeOutWP,
			@netMinutesMG,
			@netMinutesWP,
			@shiftPatCode,
			@shiftCode,
			@shiftSpan,
			@arrivalFrom,
			@arrivalTo,
			@departFrom,
			@departTo
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 

END


/*	Testing:

PARAMETERS:
	@empNo		INT,
	@dt			DATETIME

	SELECT * FROM tas.fnGetCorrectedMainGateWorkplaceSwipe(10003631, '01/04/2021')

*/
