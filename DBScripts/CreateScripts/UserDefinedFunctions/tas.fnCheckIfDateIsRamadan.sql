/***********************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfDateIsRamadan
*	Description: This function is used to check if the input date is Ramadan
*
*	Date			Author		Rev. #		Comments:
*	04/06/2018		Ervin		1.0			Created
***********************************************************************************************************************/

CREATE FUNCTION tas.fnCheckIfDateIsRamadan 
(
	@inputDate	DATETIME 
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result BIT
	SET @result = 0
    
	IF EXISTS
    (
		SELECT a.HOHDT
		FROM tas.syJDE_F55HOLID a
		WHERE UPPER(LTRIM(RTRIM(a.HOHLCD))) = 'R'
			AND tas.ConvertFromJulian(a.HOHDT) = @inputDate
	)
	SET @result = 1

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnCheckIfDateIsRamadan('06/04/2018') 

*/