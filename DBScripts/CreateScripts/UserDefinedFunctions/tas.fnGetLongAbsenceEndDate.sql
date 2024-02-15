/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetLongAbsenceEndDate
*	Description: This function is used to determine end date from long absence period
*
*	Date:				Author:		Rev.#:		Comments:
*	13/06/2022			Ervin		1.0			Created
*
**************************************************************************************************************************************************************/

CREATE FUNCTION tas.fnGetLongAbsenceEndDate
(
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME 
)
RETURNS DATETIME
AS
BEGIN

	DECLARE	@longAbsenceEndDate				DATETIME = NULL,
			@DT								DATETIME = NULL,
			@durationWorkedCumulative		INT = 0,
			@remarkCode						VARCHAR(10) = NULL,
			@leaveType						VARCHAR(10) = NULL,
			@absenceReasonCode				VARCHAR(10) = NULL,
			@isPublicHoliday				BIT = NULL 

	--Create temporary table
	DECLARE TimesheetCursor CURSOR READ_ONLY FOR 
	SELECT a.DT, a.Duration_Worked_Cumulative, RTRIM(a.RemarkCode), RTRIM(a.LeaveType), RTRIM(a.AbsenceReasonCode), a.IsPublicHoliday
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
	WHERE a.DT BETWEEN @startDate AND @endDate
		AND a.EmpNo = @empNo
		AND a.IsLastRow = 1

	--Open the cursor and fetch the data
	OPEN TimesheetCursor
	FETCH NEXT FROM TimesheetCursor
	INTO @DT, @durationWorkedCumulative, @remarkCode, @leaveType, @absenceReasonCode, @isPublicHoliday

	--Loop through each record to determing the end date
	WHILE @@FETCH_STATUS = 0 
	BEGIN			
						
		IF	@remarkCode = 'A' 
			OR @leaveType IN ('SLP', 'SLU', 'ILU', 'IL', 'UL', 'AL') 
			OR @absenceReasonCode = 'DD' 
			OR @isPublicHoliday = 1
		BEGIN

			SET @longAbsenceEndDate = @DT
		END 

		IF @durationWorkedCumulative > 0
			BREAK 

		--Fetch next record
		FETCH NEXT FROM TimesheetCursor
		INTO @DT, @durationWorkedCumulative, @remarkCode, @leaveType, @absenceReasonCode, @isPublicHoliday
	END 

	--Close and deallocate
	CLOSE TimesheetCursor
	DEALLOCATE TimesheetCursor

	RETURN @longAbsenceEndDate
END


/*	Debugging:

Parameters:
	@empNo			INT,
	@startDate		DATETIME,
	@endDate		DATETIME 

	SELECT tas.fnGetLongAbsenceEndDate(10003662, '05/17/2022', '06/04/2022') as LongAbsenceEndDate

*/
