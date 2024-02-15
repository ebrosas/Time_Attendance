/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfEntitledtoDayoff
*	Description: This functions is used to check the employee is entitled for the specified day-off
*
*	Date:			Author:		Rev.#:		Comments:
*	21/08/2020		Ervin		1.0			Created
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfEntitledtoDayoff
(
	@empNo				INT,
	@attendanceDate		DATETIME	
)
RETURNS  @rtnTable TABLE  
(   
	EmpNo			INT,  
	DayOffArray		VARCHAR(200),
	LeaveType		VARCHAR(10) 
) 
AS
BEGIN

	DECLARE @isDayOff			BIT				= 0,
			@countWeekDays		INT				= 0,
			@countAbsent		INT				= 0,
			@lastDayoffDate		DATETIME		= NULL,
			@lastLeaveType		VARCHAR(10)		= '',
			@dayOffArray		VARCHAR(200)	= ''

	--Determine if the specified date is a day-off
	IF EXISTS	
	(
		SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.IsLastRow = 1
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND a.EmpNo = @empNo
			AND a.DT = @attendanceDate
	)
	SET @isDayOff = 1

	IF @isDayOff = 1
	BEGIN 

		--Get the last day-off
		SELECT TOP 1 @lastDayoffDate = a.DT
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.IsLastRow = 1
			AND RTRIM(b.Effective_ShiftCode) = 'O'
			AND a.EmpNo = @empNo
			AND (a.DT < @attendanceDate AND a.DT <> DATEADD(DAY, -1, @attendanceDate))
		ORDER BY a.DT DESC 

		--Get the last leave type
		SELECT TOP 1 @lastLeaveType = RTRIM(ISNULL(a.LeaveType, a.AbsenceReasonCode))
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
		WHERE a.IsLastRow = 1
			AND a.EmpNo = @empNo
			AND RTRIM(b.Effective_ShiftCode) <> 'O'
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND a.DT BETWEEN @lastDayoffDate AND DATEADD(DAY, -1, @attendanceDate)
		ORDER BY a.DT DESC 

		SELECT @countWeekDays = COUNT(*) FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.DT BETWEEN DATEADD(DAY, 1, @lastDayoffDate) AND DATEADD(DAY, -1, @attendanceDate)
			AND a.IsLastRow = 1
			AND ISNULL(a.RemarkCode, '') <> 'A'

		SELECT @countAbsent = COUNT(*) FROM tas.Tran_Timesheet a WITH (NOLOCK)
		WHERE a.EmpNo = @empNo
			AND a.DT BETWEEN DATEADD(DAY, 1, @lastDayoffDate) AND DATEADD(DAY, -1, @attendanceDate)
			AND a.IsLastRow = 1
			AND a.Duration_Worked_Cumulative = 0
			AND ISNULL(a.RemarkCode, '') <> 'A'

		IF @countWeekDays = @countAbsent
			AND @lastLeaveType <> 'SL'		--Note: Proceed only if the last leave type is not Special Leave
		BEGIN
				
			--Check if employee did not come to work during the day-off
			IF EXISTS
			(
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
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
	END 

	--Set the return table
	INSERT INTO @rtnTable
	SELECT @empNo, @dayOffArray, @lastLeaveType

	RETURN 

END 

/*	Debug:

	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 1, 10001710  , '200716', 'AL'			--Remove dayoff 
	EXEC tas.Pr_AutoMarkLeaveRemoveDayOff_V2 2, 10001710 , '200716', 'AL'			--Undo removal of dayoff

	SELECT * FROM tas.fnCheckIfEntitledtoDayoff(10001766, '06/29/2020')

PARAMETERS:
	@empNo				INT,
	@attendanceDate		DATETIME	

*/