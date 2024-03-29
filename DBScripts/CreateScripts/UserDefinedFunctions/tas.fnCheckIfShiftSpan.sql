/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfShiftSpan
*	Description: This function is used to determine if shift span is enabled based on the supplied dates
*
*	Date:				Author:		Rev.#:		Comments:
*	17/08/2015			Ervin		1.0			Created
*	23/12/2020			Ervin		1.1			Refactored the code to enhance performance
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfShiftSpan
(
	@DT_SwipeLastProcessed	datetime,	
	@DT_SwipeNewProcess		datetime,
	@empNo					int	
)
RETURNS BIT
AS

BEGIN

	DECLARE	@isShiftSpan		bit,
			@dtMIN				datetime,
			@dtMAX				datetime,
			@arrivalFrom		datetime,
			@arrivalTo			datetime,
			@departFrom			datetime,
			@departTo			datetime,
			@durationRequired	int,
			@workDuration		int,
			@shiftPatCode		varchar(2),	
			@shiftCode			varchar(10)

	SELECT	@isShiftSpan		= 0,
			@dtMIN				= null,
			@dtMAX				= null,
			@arrivalFrom		= null,
			@arrivalTo			= null,
			@departFrom			= null,
			@departTo			= null,
			@durationRequired	= 0,
			@workDuration		= 0,
			@shiftPatCode		= '',	
			@shiftCode			= ''

	IF @empNo > 0
	BEGIN

		--Get the shift pattern info
		SELECT TOP 1
			 @shiftPatCode = RTRIM(Effective_ShiftPatCode),
			 @shiftCode = RTRIM(Effective_ShiftCode)
		FROM tas.Tran_ShiftPatternUpdates WITH (NOLOCK)
		WHERE EmpNo = @empNo
			AND DateX = CONVERT(DATETIME, CONVERT(VARCHAR, @DT_SwipeNewProcess, 12)) 
	END

	IF ISNULL(@shiftPatCode, '') <> '' AND ISNULL(@shiftCode, '') <> ''
	BEGIN

		--Get the shift timing info
		SELECT	@arrivalFrom = a.ArrivalFrom,
				@arrivalTo = a.ArrivalTo,
				@departFrom = a.DepartFrom,
				@departTo = a.DepartTo,
				@durationRequired = DATEDIFF(n, a.ArrivalTo, a.DepartFrom)
		FROM tas.Master_ShiftTimes a WITH (NOLOCK)
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
			AND RTRIM(a.ShiftCode) = @shiftCode
	END

	--Get the maximum and minimum swipe time
	SELECT	@dtMIN = MIN(DT),
			@dtMAX = MAX(DT)
	FROM tas.fnGetEmployeeSwipeRawData(@DT_SwipeLastProcessed, @DT_SwipeNewProcess, @empNo) 

	--Set the work duration
	SET @workDuration = DATEDIFF(n, @dtMIN, @dtMAX)
		
	IF 
	(	@durationRequired > 0 
		AND @workDuration >= @durationRequired * 2
		OR @shiftCode = 'O'
	)
	BEGIN

		SET @isShiftSpan = 1
	END
	
	RETURN @isShiftSpan
END


/*	Debugging:

Parameters:
	@DT_SwipeLastProcessed	datetime,	
	@DT_SwipeNewProcess		datetime,
	@empNo					int

	SELECT tas.fnCheckIfShiftSpan('2020-12-22 09:00:00.000', '2020-12-23 09:00:00.000', 10003631)
	SELECT tas.fnCheckIfShiftSpan('2014-31-08 09:00:00.000', '2014-01-09 09:00:00.000', 10003157)
	SELECT tas.fnCheckIfShiftSpan('2014-31-08 09:00:00.000', '2014-01-09 09:00:00.000', 10001127)
	SELECT tas.fnCheckIfShiftSpan('2014-31-08 09:00:00.000', '2014-01-09 09:00:00.000', 10001881)

*/
