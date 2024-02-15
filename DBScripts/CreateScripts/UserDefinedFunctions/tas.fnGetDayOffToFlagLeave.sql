/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetDayOffToFlagLeave
*	Description: This functions gets all the day-off dates during the entire period of unplanned leave
*
*	Date:			Author:		Rev.#:		Comments:
*	28/04/2019		Ervin		1.0			Created
*	15/05/2019		Ervin		1.1			Implemented logic for the following levae types: Injury Leave, Sick Leave Paid     
*	16/06/2019		Ervin		1.2			Fixed the bug reported by Hamad Enad regarding the wrong sick leave for emp. #10003608     
*	01/07/2019		Ervin		1.3			Added condition that checks if employee come to work on previous days prior to the last dayoff      
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetDayOffToFlagLeave
(
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME
)
RETURNS  @rtnTable TABLE  
(     
	IsDayOffBeforeStartDate		BIT,
	IsDayOffAfterEndDate		BIT,
	DayOffArray					VARCHAR(200),
	LeaveType					VARCHAR(10)
) 
AS
BEGIN

	DECLARE @isDayOffBeforeStartDate	BIT,
			@isDayOffAfterEndDate		BIT,
			@dayOffArray				VARCHAR(200),
			@leaveType					VARCHAR(10),
			@dayOffDate					DATETIME = NULL,
			@counter					INT = 0,
			@lastDayoffDate				DATETIME = NULL 

	SELECT	@isDayOffBeforeStartDate	= 0,
			@isDayOffAfterEndDate		= 0,
			@dayOffArray				= '',
			@leaveType					= ''

	--Get the leave type of the day prior to the start date
	SELECT @leaveType = a.LeaveType
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND a.DT = @startDate
		AND a.IsLastRow = 1

	--Check if the day prior to the start date of the absent period is a day-off
	IF 
	(
		SELECT RTRIM(a.ShiftCode) 
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.DT = DATEADD(DAY, -1, @startDate)
			AND a.IsLastRow = 1
	) = 'O'
	SET @isDayOffBeforeStartDate = 1

	--Check if the day after the end date of the absent period is a day-off
	IF 
	(
		SELECT RTRIM(a.ShiftCode) 
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.DT = DATEADD(DAY, 1, @endDate)
			AND a.IsLastRow = 1
	) = 'O'
	SET @isDayOffAfterEndDate = 1    			
		
	IF	@isDayOffBeforeStartDate = 1
		AND @isDayOffAfterEndDate = 0	--Rev. #1.2
	BEGIN			

		--Get all the day-off between start and end date
		DECLARE AttendanceCursor CURSOR READ_ONLY FOR
		SELECT a.DT 
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND ISNULL(a.RemarkCode, '') <> 'A'
			AND ISNULL(a.LeaveType, '') = ''	
			AND a.IsLastRow = 1
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND a.DT BETWEEN @startDate AND @endDate
			AND EXISTS	
			(
				SELECT 1 FROM tas.Tran_Timesheet
				WHERE EmpNo = @empNo
					AND DT = DATEADD(DAY, -1, a.DT) 
					AND RTRIM(LeaveType) IN ('UL', 'SLU', 'ILU', 'SLP', 'IL', 'AL')
			)	

		OPEN AttendanceCursor
		FETCH NEXT FROM AttendanceCursor
		INTO @dayOffDate

		WHILE @@FETCH_STATUS = 0
		BEGIN
	
			IF @counter = 0
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

			--Rev. #1.3
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
					INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				WHERE a.EmpNo = @empNo
					AND RTRIM(b.Effective_ShiftCode) = 'O'
					AND ISNULL(a.RemarkCode, '') <> 'A'
					AND ISNULL(a.LeaveType, '') = ''	
					AND a.IsLastRow = 1
					AND ISNULL(a.IsPublicHoliday, 0) = 0
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

		--Get all the day-off between start and end date
		DECLARE AttendanceCursor CURSOR READ_ONLY FOR
		SELECT a.DT 
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @empNo
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND ISNULL(a.RemarkCode, '') <> 'A'
			AND ISNULL(a.LeaveType, '') = ''	
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
				AND ISNULL(a.IsPublicHoliday, 0) = 0
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
			@dayOffArray AS DayOffArray,
			@leaveType AS LeaveType

	RETURN 

END


/*	Debugging:
	
PARAMETERS:
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME

	SELECT * FROM tas.fnGetDayOffToFlagLeave(10006032, '02/21/2016', '03/01/2016')		--Unpaid Leave
	SELECT * FROM tas.fnGetDayOffToFlagLeave(10003584, '11/18/2015', '11/20/2015')		--Unpaid Injury Leave
	
	--Live database
	SELECT * FROM tas.fnGetDayOffToFlagLeave(10006119, '05/24/2019', '06/15/2019')		--Sick Leave Paid (based on start and end date of the long period of leave)
	SELECT * FROM tas.fnGetDayOffToFlagLeave(10003157, '02/16/2019', '03/15/2019')		--Sick Leave Paid (based on payroll cutoff period)

	SELECT * FROM tas.fnGetDayOffToFlagLeave(10003809, '04/22/2019', '05/05/2019')		--Annual Leave (based on payroll cutoff period)

*/
