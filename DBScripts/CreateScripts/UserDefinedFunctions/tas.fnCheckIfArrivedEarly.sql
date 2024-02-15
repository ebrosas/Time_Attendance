/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfArrivedEarly
*	Description: This function is used to check if an employee came to work earlier than the shaving time allowance
*
*	Date			Author		Rev. #		Comments:
*	10/09/2017		Ervin		1.0			Created
*	12/09/2017		Ervin		1.1			Refactored the logic in identifying the shift code to used
*	18/09/2017		Ervin		1.2			Added new logic to identify the shift code to use
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfArrivedEarly 
(
	@empNo		INT,
	@dt			DATETIME 
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result				BIT,
			@dtIn				DATETIME,
			@shiftPatCode		VARCHAR(2),	
			@shiftCode			VARCHAR(10),
			@isRamadan			BIT,
			@isPublicHoliday	BIT,
			@arrivalFrom		DATETIME

	SELECT	@result				= 0,
			@dtIn				= NULL,
			@shiftPatCode		= NULL,	
			@shiftCode			= NULL,
			@isRamadan			= 0,
			@isPublicHoliday	= 0,
			@arrivalFrom		= NULL
    
	--Get the timesheet details
	SELECT	@dtIn = dtIN,
			@shiftPatCode = RTRIM(a.ShiftPatCode),	
			@shiftCode = CASE WHEN b.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O'		--Rev. #1.2 
						 THEN a.ShiftCode
						 ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, d.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
						 END, 
			@isRamadan = a.isRamadan,
			@isPublicHoliday = a.IsPublicHoliday
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_ShiftPatternTitles b ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode)
		INNER JOIN tas.Master_ShiftTimes c ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(c.ShiftCode)
		INNER JOIN tas.Master_ShiftTimes d ON RTRIM(a.ShiftPatCode) = RTRIM(d.ShiftPatCode) 
			AND 
			(
				CASE WHEN b.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, c.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END
				END
			) = RTRIM(d.ShiftCode)
	WHERE a.EmpNo = @empNo
		AND a.DT = @dt
		AND a.IsLastRow = 1

	--Get the shaving arrival time allowance
	SELECT	@arrivalFrom = CASE WHEN @isRamadan = 1 THEN a.RArrivalFrom ELSE a.ArrivalFrom END 
	FROM tas.Master_ShiftTimes a
	WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
		AND RTRIM(a.ShiftCode) = @shiftCode

	IF	@dtIn IS NOT NULL
		AND @arrivalFrom IS NOT NULL
		AND @shiftCode <> 'O'
		AND ISNULL(@isPublicHoliday, 0) = 0
	BEGIN

		IF CONVERT(TIME, @dtIn) < CONVERT(TIME, @arrivalFrom)
			SET @result = 1
    END 

	RETURN @result
END


/*	Debugging:

PARAMETERS:
	@empNo		INT,
	@dt			DATETIME 

	SELECT tas.fnCheckIfArrivedEarly(10001766, '26/03/2016') 

*/