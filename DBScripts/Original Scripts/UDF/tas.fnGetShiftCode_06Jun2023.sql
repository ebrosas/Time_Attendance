USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnGetShiftCode]    Script Date: 06/06/2023 10:46:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetShiftCode
*	Description: This functions gets all man-hour count breakdown based on specific period
*
*	Date:			Author:		Rev.#:		Comments:
*	30/01/2020		Ervin		1.0			Created
*	12/04/2021		Ervin		1.1			Fixed bug related to converting the employee number
*	08/05/2023		Ervin		1.2			Added code to check if Time-in is within the shift start time plus 4 hours
*
**************************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetShiftCode]
(
	@empNo		INT,
	@dt			DATETIME	
)
RETURNS  @rtnTable TABLE  
(   
	EmpNo			INT,  
	DT				DATETIME,  
	ShiftCode		VARCHAR(10),
	TimeInMG		DATETIME
) 
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
		WHERE (CASE WHEN ISNUMERIC(a.FName) = 1 THEN CAST(a.FName AS INT) + 10000000 ELSE 0 END) = @empNo	--Rev. #1.1
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

		IF CAST(@timeInMG AS TIME) >= @arrivalFrom AND CAST(@timeInMG AS TIME) < @departTo AND CAST(@timeInMG AS TIME) <= DATEADD(HOUR, 4, @arrivalFrom)	--Rev. #1.2
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

	INSERT INTO @rtnTable 
	SELECT @empNo, @dt, RTRIM(@shiftCodeToUse), @timeInMG		

	RETURN 

END


/*	Debugging:
	
PARAMETERS:
	@empNo		INT,
	@dt			DATETIME	

	SELECT * FROM tas.fnGetShiftCode(10003368, '02/04/2020')
	SELECT * FROM tas.fnGetShiftCode(10003211, '02/04/2020')
	SELECT * FROM tas.fnGetShiftCode(10003469, '02/04/2020')

	SELECT * FROM tas.fnGetShiftCode(10006072, '02/05/2020')
	

*/
