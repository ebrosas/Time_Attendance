/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetUnentitledDayoffArray
*	Description: Get the date array of all unentitled day-offs during the specified duration
*
*	Date			Author		Rev. #		Comments:
*	19/08/2020		Ervin		1.0			Created
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetUnentitledDayoffArray
(
	@empNo		INT,
	@startDate	DATETIME,
	@endDate	DATETIME
)
RETURNS VARCHAR(200)
AS
BEGIN      

	DECLARE @attendanceDate		DATETIME		= NULL,
			@filterEmpNo		INT				= 0,
			@countWeekDays		INT				= 0,
			@countAbsent		INT				= 0,
			@lastDayoffDate		DATETIME		= NULL,
			@dayOffArray		VARCHAR(200)	= ''

	--Create temporary table, get all weekends
	DECLARE AttendanceCursor CURSOR READ_ONLY FOR 
	SELECT a.EmpNo, a.DT
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
	WHERE a.IsLastRow = 1
		AND RTRIM(b.Effective_ShiftCode) = 'O'
		AND a.EmpNo = @empNo
		AND a.DT BETWEEN @startDate AND @endDate
	ORDER BY a.DT

	--Open the cursor and fetch the data
	OPEN AttendanceCursor
	FETCH NEXT FROM AttendanceCursor
	INTO @filterEmpNo, @attendanceDate

	--Loop through each record to determine the unentitled dayoffs
	WHILE @@FETCH_STATUS = 0 
	BEGIN			

		--Get the last day-off
		SELECT TOP 1 @lastDayoffDate = a.DT
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.EmpNo = @filterEmpNo
			AND a.IsLastRow = 1
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND (a.DT < @attendanceDate AND a.DT <> DATEADD(DAY, -1, @attendanceDate))
		ORDER BY a.DT DESC 

		SELECT @countWeekDays = COUNT(*) FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @filterEmpNo
			AND a.DT BETWEEN DATEADD(DAY, 1, @lastDayoffDate) AND DATEADD(DAY, -1, @attendanceDate)
			AND a.IsLastRow = 1
			AND ISNULL(a.RemarkCode, '') <> 'A'

		SELECT @countAbsent = COUNT(*) FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @filterEmpNo
			AND a.DT BETWEEN DATEADD(DAY, 1, @lastDayoffDate) AND DATEADD(DAY, -1, @attendanceDate)
			AND a.IsLastRow = 1
			AND a.Duration_Worked_Cumulative = 0
			AND ISNULL(a.RemarkCode, '') <> 'A'

		IF @countWeekDays = @countAbsent
		BEGIN
			
			--Check if employee did not come to work during the day-off
			IF EXISTS
            (
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @filterEmpNo
					AND a.DT = @attendanceDate
					AND a.IsLastRow = 1
					AND a.Duration_Worked_Cumulative = 0
					AND ISNULL(a.IsPublicHoliday, 0) = 0
			)
			BEGIN 

				IF LEN(@dayOffArray) = 0
					SELECT @dayOffArray = CONVERT(VARCHAR, @attendanceDate, 12)
				ELSE
					SELECT @dayOffArray = @dayOffArray + ',' + CONVERT(VARCHAR, @attendanceDate, 12)
			END 
		END 

		--Reset variables
		SET @countWeekDays = 0
		SET @countAbsent = 0

		--Fetch next record
		FETCH NEXT FROM AttendanceCursor
		INTO @filterEmpNo, @attendanceDate
    END 

	--Close and deallocate
	CLOSE AttendanceCursor
	DEALLOCATE AttendanceCursor

	RETURN @dayOffArray

END


/*	Debugging:

	SELECT tas.fnGetUnentitledDayoffArray(10001766, '06/16/2020', '07/15/2020')

	SELECT * FROM tas.fnParseDateArrayToDateTime('200807,200808', ',')
	SELECT * FROM tas.fnParseDateArrayToDateTime(tas.fnGetUnentitledDayoffArray(10003266, '08/05/2020', '08/09/2020'), ',')

*/
