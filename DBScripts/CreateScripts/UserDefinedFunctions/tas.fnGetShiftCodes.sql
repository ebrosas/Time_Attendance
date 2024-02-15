/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetShiftCodes
*	Description: Get the shift codes
*
*	Date			Author		Rev. #		Comments:
*	07/06/2016		Ervin		1.0			Created
*	29/01/2017		Ervin		1.1			Fixed Where filter clause to query by the value of "@shiftPatCode" parameter
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetShiftCodes
(
	@shiftPatCode	VARCHAR(2)
)
RETURNS VARCHAR(MAX)
AS
BEGIN      

	DECLARE @shiftCode VARCHAR(max) 

	SELECT @shiftCode = COALESCE(@shiftCode + ' ', '') + RTRIM(ShiftCode)
	FROM tas.Master_ShiftPattern 
	WHERE RTRIM(ShiftPatCode) = RTRIM(@shiftPatCode)

	RETURN ' | ' + @shiftCode         
END


/*	Debugging:

	SELECT tas.fnGetShiftCodes('D')

*/
