/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfSwipedOut
*	Description: This function is used to determine if the employee has swiped out from the Main Gate
*
*	Date:				Author:		Rev.#:		Comments:
*	18/04/2022			Ervin		1.0			Created
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfSwipedOut
(
	@empNo		INT,
	@DT			DATETIME
)
RETURNS BIT 
AS
BEGIN

	DECLARE	@swipeType		VARCHAR(5)	= '',
			@isSwipedOut	BIT = 0

	SELECT TOP 1 @swipeType = UPPER(RTRIM(a.SwipeType)) 
	FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK) 
	WHERE a.EmpNo = @empNo
		AND a.SwipeDate = @DT
	ORDER BY SwipeTime DESC

	IF @swipeType = 'OUT'
		SET @isSwipedOut = 1

	RETURN @isSwipedOut 

END 


/*	Debug:

	SELECT tas.fnCheckIfSwipedOut(10003726, '04/18/2022')
	SELECT tas.fnCheckIfSwipedOut(10003836, '04/18/2022')	

*/

