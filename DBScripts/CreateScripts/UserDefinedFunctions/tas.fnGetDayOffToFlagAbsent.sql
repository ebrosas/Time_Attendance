/********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetDayOffToFlagAbsent
*	Description: This functions gets all the day-off dates during the entire absences period 
*
*	Date:			Author:		Rev.#:		Comments:
*	24/03/2019		Ervin		1.0			Created
*	10/04/2019		Ervin		1.1			Added filter condition to return day-off dates wherein the previous day is flagged as absent
*	25/04/2019		Ervin		1.2			Added filter condition to check is LeaveType is null
*	28/04/2019		Ervin		1.3			Added join to "Tran_ShiftPatternUpdates" table to determine the correct shift code
*	16/05/2019		Ervin		1.4			Modified the logic in fetching the attendance records with long period of absences
*	16/06/2019		Ervin		1.5			Fixed the bug reported by Hamad Enad regarding the wrong sick leave for emp. #10003608     
*	01/07/2019		Ervin		1.6			Added condition that checks if employee come to work on previous days prior to the last dayoff    
*	02/12/2019		Ervin		1.7			Added additional checking to determine the dayoff days to be flagged as absent       
*	04/12/2019		Ervin		1.8			Added validation that check if "CorrectionCode" is null
*	13/06/2022		Ervin		1.9			Disabled the filter for "CorrectionCode" if @isDayOffBeforeStartDate = 0 and @isDayOffAfterEndDate = 0
*	21/06/2022		Ervin		2.0			Added condition to check if the employee came to work
*	24/07/2022		Ervin		2.1			Added filter to check if Duration_Worked_Cumulative is zero for day-off employees
********************************************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetDayOffToFlagAbsent
(
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME
)
RETURNS  @rtnTable TABLE  
(     
	IsDayOffBeforeStartDate		BIT,
	IsDayOffAfterEndDate		BIT,
	DayOffArray					VARCHAR(200)
) 
AS
BEGIN

	DECLARE @isDayOffBeforeStartDate	BIT,
			@isDayOffAfterEndDate		BIT,
			@dayOffArray				VARCHAR(200),
			@lastDayoffDate				DATETIME = NULL 

	SELECT	@isDayOffBeforeStartDate	= 0,
			@isDayOffAfterEndDate		= 0,
			@dayOffArray				= ''

	--Check if the day prior to the start date of the absent period is a day-off
	IF 
	(
		SELECT RTRIM(b.Effective_ShiftCode)  
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND a.DT = DATEADD(DAY, -1, @startDate)
			AND a.IsLastRow = 1
	) = 'O'
	SET @isDayOffBeforeStartDate = 1

	--Check if the day after the end date of the absent period is a day-off
	IF 
	(
		SELECT RTRIM(b.Effective_ShiftCode) 
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND a.DT = DATEADD(DAY, 1, @endDate)
			AND a.IsLastRow = 1
	) = 'O'
	SET @isDayOffAfterEndDate = 1    	
		
	IF	@isDayOffBeforeStartDate = 1
		AND @isDayOffAfterEndDate = 0	--Rev. #1.5
	BEGIN

		--Get all the day-off between start and end date
		DECLARE	@dayOffDate	DATETIME = NULL,
				@counter	INT = 0

		DECLARE AttendanceCursor CURSOR READ_ONLY FOR
		SELECT a.DT 
		FROM tas.Tran_Timesheet a WITH	(NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND ISNULL(a.RemarkCode, '') <> 'A'
			AND ISNULL(a.LeaveType, '') = ''			--Rev. #1.2
			AND ISNULL(a.CorrectionCode, '') = ''		--Rev. #1.9
			AND a.IsLastRow = 1
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND ISNULL(a.Duration_Worked_Cumulative, 0) = 0
			AND a.DT BETWEEN @startDate AND @endDate
			--AND EXISTS	--Rev. #1.1
			--(
			--	SELECT 1 FROM tas.Tran_Timesheet
			--	WHERE EmpNo = @empNo
			--		AND DT = DATEADD(DAY, -1, a.DT) 
			--		AND RTRIM(RemarkCode) = 'A'
			--)						

		OPEN AttendanceCursor
		FETCH NEXT FROM AttendanceCursor
		INTO @dayOffDate

		--Get the last day-off
		SELECT TOP 1 @lastDayoffDate = a.DT
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND a.IsLastRow = 1
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND a.DT < @dayOffDate
		ORDER BY a.DT DESC 

		WHILE @@FETCH_STATUS = 0
		BEGIN
	
			IF NOT EXISTS	--Rev. #2.0
            (
				--Check if employee come to work on previous days prior to the last dayoff 
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT BETWEEN @lastDayoffDate AND DATEADD(DAY, -1, @dayOffDate)
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative > 0
			)
			BEGIN

				IF @counter = 0
					SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
				ELSE
					SELECT @dayOffArray = @dayOffArray + ',' + CONVERT(VARCHAR, @dayOffDate, 12)
			END 

			--Increment the counter
			SET @counter = @counter + 1

			-- Retrieve next record
			FETCH NEXT FROM AttendanceCursor
			INTO @dayOffDate
		END

		-- Close and deallocate
		CLOSE AttendanceCursor
		DEALLOCATE AttendanceCursor
    END 

	ELSE IF	@isDayOffBeforeStartDate = 1
		AND @isDayOffAfterEndDate = 1
	BEGIN

		--Get all the day-off after the end date
		DECLARE	@firstDutyDate	DATETIME

		SELECT	TOP 1 @firstDutyDate = DT
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.DT > @endDate
			AND ISNULL(a.ShiftCode, '') <> 'O'
		ORDER BY a.DT

		IF @firstDutyDate IS NOT NULL
		BEGIN

			--Get the last day-off
			SELECT TOP 1 @lastDayoffDate = a.DT
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
			WHERE a.EmpNo = @empNo
				AND a.IsLastRow = 1
				AND RTRIM(b.Effective_ShiftCode) = 'O'
				AND a.DT < @endDate
			ORDER BY a.DT DESC 

			--Rev. #1.6
			IF NOT EXISTS
            (
				--Check if employee come to work on previous days prior to the last dayoff 
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT BETWEEN @lastDayoffDate AND DATEADD(DAY, -1, @endDate)
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative > 0
			)
			BEGIN 

				--Reset counter
				SET @counter = 0

				DECLARE AttendanceCursor CURSOR READ_ONLY FOR
				SELECT a.DT 
				FROM tas.Tran_Timesheet a WITH	(NOLOCK)
					INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				WHERE a.EmpNo = @empNo
					AND RTRIM(b.Effective_ShiftCode) = 'O'
					AND ISNULL(a.RemarkCode, '') <> 'A'
					AND ISNULL(a.LeaveType, '') = ''			--Rev. #1.2
					AND ISNULL(a.CorrectionCode, '') = ''		--Rev. #1.9
					AND a.IsLastRow = 1
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.Duration_Worked_Cumulative, 0) = 0
					AND a.DT BETWEEN @endDate AND @firstDutyDate

				OPEN AttendanceCursor
				FETCH NEXT FROM AttendanceCursor
				INTO @dayOffDate

				WHILE @@FETCH_STATUS = 0
				BEGIN
	
					IF LEN(@dayOffArray) = 0
						SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
					ELSE
						SELECT @dayOffArray = @dayOffArray + ',' + CONVERT(VARCHAR, @dayOffDate, 12)

					--Increment the counter
					SET @counter = @counter + 1

					-- Retrieve next record
					FETCH NEXT FROM AttendanceCursor
					INTO @dayOffDate
				END

				-- Close and deallocate
				CLOSE AttendanceCursor
				DEALLOCATE AttendanceCursor
			END 
		END 
    END 

	ELSE
    BEGIN	

		--Initialize variable
		SET @lastDayoffDate = NULL 

		DECLARE AttendanceCursor CURSOR READ_ONLY FOR
		SELECT a.DT 
		FROM tas.Tran_Timesheet a WITH	(NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND ISNULL(a.RemarkCode, '') <> 'A'
			AND ISNULL(a.LeaveType, '') = ''			
			--AND ISNULL(a.CorrectionCode, '') = ''		--Rev. #1.9
			AND a.IsLastRow = 1
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND a.DT BETWEEN @startDate AND @endDate

		OPEN AttendanceCursor
		FETCH NEXT FROM AttendanceCursor
		INTO @dayOffDate

		WHILE @@FETCH_STATUS = 0
		BEGIN
	
			--Get the last day-off
			SELECT TOP 1 @lastDayoffDate = a.DT
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
			WHERE a.EmpNo = @empNo
				AND a.IsLastRow = 1
				AND RTRIM(b.Effective_ShiftCode) = 'O'
				AND a.DT < @dayOffDate
			ORDER BY a.DT DESC 

			IF NOT EXISTS
            (
				--Check if employee come to work on previous days prior to the last dayoff 
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT BETWEEN @lastDayoffDate AND DATEADD(DAY, -1, @dayOffDate)
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative > 0
			)
			AND NOT EXISTS
            (
				--Check if the employee come to work on the next day 
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT = DATEADD(DAY, 1, @dayOffDate)
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative > 0
			)
			BEGIN 
			
				--Check if previous day is day-off
				IF
				(
					SELECT b.Effective_ShiftCode
					FROM tas.Tran_Timesheet a WITH (NOLOCK)
						INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
					WHERE a.EmpNo = @empNo
						AND a.IsLastRow = 1
						AND a.DT = DATEADD(DAY, -1, @dayOffDate)
				) = 'O'
				BEGIN
				
					IF CHARINDEX(CONVERT(VARCHAR, DATEADD(DAY, -1, @dayOffDate), 12), @dayOffArray) > 0
					BEGIN 
					
						IF @counter = 0
							SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
						ELSE
						BEGIN

							IF ISNULL(@dayOffArray, '') = ''
								SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
							ELSE
								SELECT @dayOffArray = @dayOffArray + ',' + CONVERT(VARCHAR, @dayOffDate, 12)
						END 
					END 
                END
                
				ELSE
                BEGIN

					IF @counter = 0
						SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
					ELSE
					BEGIN

						IF ISNULL(@dayOffArray, '') = ''
							SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
						ELSE
							SELECT @dayOffArray = @dayOffArray + ',' + CONVERT(VARCHAR, @dayOffDate, 12)
					END 
				END 
			END 

			--Start of Rev. #1.7
			ELSE IF NOT EXISTS
            (
				--Check if employee come to work on previous days prior to the last dayoff 
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT BETWEEN @lastDayoffDate AND DATEADD(DAY, -1, @dayOffDate)
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative > 0
			)
			AND EXISTS
            (
				--Check if the employee come to work on the next day 
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT = DATEADD(DAY, 1, @dayOffDate)
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative > 0
			)
			BEGIN 

				IF @counter = 0
					SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
				ELSE
				BEGIN

					IF ISNULL(@dayOffArray, '') = ''
						SELECT @dayOffArray = CONVERT(VARCHAR, @dayOffDate, 12)
					ELSE
						SELECT @dayOffArray = @dayOffArray + ',' + CONVERT(VARCHAR, @dayOffDate, 12)
				END 
			END 
			--End of Rev. #1.7

			--Increment the counter
			SET @counter = @counter + 1

			-- Retrieve next record
			FETCH NEXT FROM AttendanceCursor
			INTO @dayOffDate
		END

		-- Close and deallocate
		CLOSE AttendanceCursor
		DEALLOCATE AttendanceCursor
	END 


	INSERT INTO @rtnTable 
	SELECT	@isDayOffBeforeStartDate AS IsDayOffBeforeStartDate,
			@isDayOffAfterEndDate AS IsDayOffAfterEndDate,
			@dayOffArray AS DayOffArray


	RETURN 

END


/*	Debugging:
	
PARAMETERS:
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME

	SELECT * FROM tas.fnGetDayOffToFlagAbsent(10003703, '05/17/2022', '05/29/2022')

*/
