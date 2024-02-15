/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetShiftPointerBasedOnDate
*	Description: This function is used to calculate the shift pointer of an employee based on specific date
*
*	Date:			Author:		Rev.#:		Comments:
*	04/06/2018		Ervin		1.0			Created
*	16/06/2018		Ervin		1.1			Fixed bbug related to Divide by zero
*	25/04/2021		Ervin		1.2			Fixed the bug reported by HR wherein the shift pointer is wrong for temporary shift pattern
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetShiftPointerBasedOnDate
(
	@empNo	INT,
	@date	SMALLDATETIME
)
RETURNS INT 
AS
BEGIN

	DECLARE	@empShiftPatternPointer			INT,
			@shiftPatCode					VARCHAR(2),			
			@empShiftMaxShiftPointer		INT,
			@remShiftPatternCycle			INT,
			@currentShiftPointer			INT,
			@shiftDateDiff					INT,
			@lastShiftDate					SMALLDATETIME

	--Initialize variable
	SELECT	@empShiftPatternPointer			= 0,
			@shiftPatCode					= '',			
			@empShiftMaxShiftPointer		= 0,
			@remShiftPatternCycle			= 0,
			@currentShiftPointer			= 0,
			@shiftDateDiff					= 0,
			@lastShiftDate					= NULL 

	--Get the employee's shift pattern information
	SELECT	@shiftPatCode = RTRIM(a.ShiftPatCode),
			@currentShiftPointer = a.ShiftPointer
	FROM tas.Master_EmployeeAdditional a
	WHERE a.EmpNo = @empNo

	--Get the last time the SPU service completed successfully
	SELECT @lastShiftDate = a.SPU_Date 
	FROM tas.System_Filters a

	--Checks the difference between the last shift processed date and the starting date
	SELECT @shiftDateDiff = DATEDIFF(dd, @lastShiftDate, @date)

	--Get the maximum shift pointer sequence
	SELECT	@empShiftMaxShiftPointer = MAX(a.ShiftPointer)
	FROM tas.Master_ShiftPattern a
	WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode

	--Start date is equal to last shift date or future date
	IF @shiftDateDiff > 0
	BEGIN

		--Compute the remaining shift pattern cycle
		SELECT @remShiftPatternCycle = @shiftDateDiff % @empShiftMaxShiftPointer

		--Retrieve the future shift pointer and pattern code of the employee
		IF @remShiftPatternCycle = @empShiftMaxShiftPointer
			SELECT @empShiftPatternPointer = @currentShiftPointer

		ELSE
		BEGIN

			SELECT @empShiftPatternPointer = (@currentShiftPointer + @remShiftPatternCycle) % @empShiftMaxShiftPointer
			
			--Checks if remainder is zero
			IF @empShiftPatternPointer = 0
				SELECT @empShiftPatternPointer = @empShiftMaxShiftPointer
		END
	END 

	ELSE IF @shiftDateDiff = 0
	BEGIN
	
		--Get the current day-of-week
		SET @currentShiftPointer = DATEPART(WEEKDAY, @lastShiftDate)

		--Calculate the effective shift pointer based on current DOW and max shift pointer
		SELECT @empShiftPatternPointer = @currentShiftPointer % @empShiftMaxShiftPointer
	END 

	ELSE
    BEGIN

		--Set to absolute value
		SELECT @shiftDateDiff = @shiftDateDiff * -1

		--Compute the remaining shift pattern cycle
		SELECT @remShiftPatternCycle = @shiftDateDiff % @empShiftMaxShiftPointer

		--Set the current shift pointer
		SELECT @empShiftPatternPointer = @currentShiftPointer - @remShiftPatternCycle
		
		IF @empShiftPatternPointer <= 0
			SELECT @empShiftPatternPointer = @empShiftMaxShiftPointer + @empShiftPatternPointer
    END 

	RETURN @empShiftPatternPointer	

END 


/*	Debug:

PARAMETERS:
	@empNo	INT,
	@date	SMALLDATETIME
	
	SELECT tas.fnGetShiftPointerBasedOnDate(10003632, '10/06/2018')		--Test database
	SELECT tas.fnGetShiftPointerBasedOnDate(10003662, '05/16/2021')		--Live database

	SELECT * FROM tas.Master_EmployeeAdditional a
	WHERE a.EmpNo = 10003632

	--Correct the current shift pointer
	BEGIN TRAN T1

	UPDATE tas.Master_EmployeeAdditional 
	SET ShiftPointer = 2
	WHERE RTRIM(ShiftPatCode) = 'D'

	COMMIT TRAN T1

*/
