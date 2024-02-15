/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnDetermineShiftCode
*	Description: This function is used to identify the correct shift code of a given employee on specific date
*
*	Date:			Author:		Rev.#:		Comments:
*	05/02/2020		Ervin		1.0			Created
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnDetermineShiftCode
(
	@empNo		INT,
	@dt			DATETIME	
)
RETURNS VARCHAR(10)
AS
BEGIN

	DECLARE @shiftCodeToUse		VARCHAR(10)	= '',
			@shiftPatCode		VARCHAR(2) = '',
			@shiftCode			VARCHAR(10) = '',		 	
			@actualShiftCode	VARCHAR(10) = '',
			@arrivalFrom		TIME = NULL,
			@departTo			TIME = NULL,
			@timeInMG			DATETIME = NULL 

	--Get the schedule shift pattern info
	SELECT	@shiftPatCode = RTRIM(a.Effective_ShiftPatCode),
			@shiftCode = RTRIM(a.Effective_ShiftCode),
			@shiftCodeToUse = RTRIM(a.Effective_ShiftCode)		 	  
	FROM tas.Tran_ShiftPatternUpdates a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND a.DateX = @dt

	--Get the actual shift info
	SELECT	TOP 1 @actualShiftCode = RTRIM(a.Actual_ShiftCode)
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND a.DT = @dt
	ORDER BY a.AutoID

	--Get the Main Gate time-in from the Timesheet table
	SELECT TOP 1 @timeInMG = a.dtIN
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND a.DT = @dt
	ORDER BY a.AutoID ASC 

	IF ISNULL(@timeInMG, '') = '' OR @timeInMG = CAST('' AS DATETIME)
	BEGIN
    
		--Get the Main Gate time-in from the Access System database
		SELECT TOP 1 @timeInMG = a.TimeDate
		FROM tas.sy_EvnLog a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
		WHERE CAST(a.FName AS INT) + 10000000 = @empNo
			AND CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) = @dt
			AND UPPER(RTRIM(b.Direction)) = 'I'
		ORDER BY a.TimeDate DESC 
	END 

	IF NOT (ISNULL(@timeInMG, '') = '' OR @timeInMG = CAST('' AS DATETIME))
	BEGIN 

		--Check if the Main Gate time-in falls within the scheduled timing
		SELECT	@arrivalFrom = CAST(a.ArrivalFrom AS TIME), 
				@departTo = CAST(a.DepartFrom AS TIME)
		FROM tas.Master_ShiftTimes a WITH (NOLOCK)
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode 
			AND RTRIM(a.ShiftCode) = @shiftCode

		IF CAST(@timeInMG AS TIME) >= @arrivalFrom AND CAST(@timeInMG AS TIME) < @departTo
			SET @shiftCodeToUse = @shiftCode
	
		ELSE
		BEGIN

			--Check if the Main Gate time-in falls within the actual timing
			SELECT	@arrivalFrom = CAST(a.ArrivalFrom AS TIME), 
					@departTo = CAST(a.DepartFrom AS TIME)
			FROM tas.Master_ShiftTimes a WITH (NOLOCK)
			WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode 
				AND RTRIM(a.ShiftCode) = @actualShiftCode

			IF @actualShiftCode = 'N'
			BEGIN
            
				IF CAST(@timeInMG AS TIME) >= @arrivalFrom 
					SET @shiftCodeToUse = @actualShiftCode
			END

			ELSE
            BEGIN

				IF CAST(@timeInMG AS TIME) >= @arrivalFrom AND CAST(@timeInMG AS TIME) < @departTo
					SET @shiftCodeToUse = @actualShiftCode
            END 
		END
	END 

	RETURN RTRIM(@shiftCodeToUse) 

END

/*	Debug:

PARAMETERS:
	@empNo		INT,
	@dt			DATETIME

	SELECT tas.fnDetermineShiftCode(10003368, '02/04/2020')
	SELECT tas.fnDetermineShiftCode(10003211, '02/04/2020')
	SELECT tas.fnDetermineShiftCode(10003469, '02/04/2020')


	SELECT a.ArrivalFrom, a.DepartTo, * 
	FROM tas.Master_ShiftTimes a
	WHERE RTRIM(a.ShiftPatCode) = 'I' 

	SELECT a.* 
	FROM tas.sy_EvnLog a
		INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.Loc = b.LocationCode AND a.Dev = b.ReaderNo
	WHERE CAST(a.FName AS INT) + 10000000 = 10003368
		AND CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) = '02/04/2020'
		AND UPPER(RTRIM(b.Direction)) = 'I'

*/

