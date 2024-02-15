/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetRequiredSwipeTime
*	Description: Get the required Time in/out based on Shift Pattern Code and Shift Code
*
*	Date			Author		Rev. #		Comments:
*	24/07/2018		Ervin		1.0			Created
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetRequiredSwipeTime
(
    @shiftPatCode	VARCHAR(2),
	@shiftCode		VARCHAR(10),
	@isRamadan		BIT,
	@swipeType		TINYINT 
)
RETURNS DATETIME
AS
BEGIN      

	IF ISNULL(@swipeType, 0) = 0
		SET @swipeType = 0

	DECLARE	@requiredSwipeTime	DATETIME	
	SET @requiredSwipeTime = NULL

	IF @swipeType = 0	--Get the required Time-in
	BEGIN
	 
		SELECT	@requiredSwipeTime = CASE WHEN @isRamadan = 1 THEN a.RArrivalTo ELSE a.ArrivalTo END 
		FROM tas.Master_ShiftTimes a
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
			AND RTRIM(a.ShiftCode) = @shiftCode
	END 

	ELSE	--Get the required Time-out
    BEGIN
		
		SELECT	@requiredSwipeTime = CASE WHEN @isRamadan = 1 THEN a.RDepartFrom ELSE a.DepartFrom END 
		FROM tas.Master_ShiftTimes a
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
			AND RTRIM(a.ShiftCode) = @shiftCode
    END 

	--Return the data
	RETURN @requiredSwipeTime           
END

/*	Debugging:

PARAMTERS:
	@shiftPatCode	VARCHAR(2),
	@shiftCode		VARCHAR(10),
	@isRamadan		BIT,
	@swipeType		TINYINT = 0	

	SELECT tas.fnGetRequiredSwipeTime('I', 'E', 0, 0)
	SELECT tas.fnGetRequiredSwipeTime('I', 'M', 0, 1)

*/
