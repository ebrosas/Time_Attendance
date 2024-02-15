/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetRequiredTimeOut
*	Description: Get the required time out
*
*	Date			Author		Rev. #		Comments:
*	06/04/2016		Ervin		1.0			Created
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetRequiredTimeOut
(
    @empNo			INT,
	@processDate	DATETIME
)
RETURNS DATETIME
AS
BEGIN      

	DECLARE	@requiredTimeOut	DATETIME,
			@arrivalTime		DATETIME,
			@shiftCode			VARCHAR(10),
			@shiftPatCode		VARCHAR(2),	
			@durationRequired	INT		

	--Validate parameters
	IF @processDate IS NULL OR @processDate = CONVERT(DATETIME, '') 
		SET @processDate = GETDATE()

	--Initialize variables
	SELECT	@requiredTimeOut	= NULL,
			@arrivalTime		= NULL,
			@shiftCode			= '',
			@shiftPatCode		= '',	
			@durationRequired	= 0

	--Get the shift pattern information
	SELECT	@shiftPatCode = RTRIM(Effective_ShiftPatCode),
			@shiftCode = RTRIM(Effective_ShiftCode)
	FROM  tas.Tran_ShiftPatternUpdates a
	WHERE a.DateX = CONVERT(DATETIME, CONVERT(VARCHAR, @processDate, 12))
		AND a.EmpNo = @empNo

	--Get the shift timing information
	IF ISNULL(@shiftPatCode, '') <> ''
		AND ISNULL(@shiftCode, '') <> ''
	BEGIN
    
		SELECT @durationRequired= DATEDIFF(MINUTE, a.ArrivalTo, a.DepartFrom)
		FROM tas.Master_ShiftTimes a
		WHERE RTRIM(ShiftPatCode) = @shiftPatCode 
			AND RTRIM(ShiftCode) = @shiftCode
	END 

	--Get the arrival time
	SELECT @arrivalTime = MIN(AttendanceDate)
	FROM tas.Master_EmployeeAttendance a
	WHERE a.EmployeeNo = @empNo		

	--Calculate the required time-out
	IF @durationRequired > 0 AND @arrivalTime IS NOT NULL
		SELECT @requiredTimeOut = DATEADD(MINUTE, @durationRequired, @arrivalTime)

	--Return the data
	RETURN @requiredTimeOut           
END

/*	Debugging:

	SELECT tas.fnGetRequiredTimeOut(10003632, '02/03/2016')

*/
