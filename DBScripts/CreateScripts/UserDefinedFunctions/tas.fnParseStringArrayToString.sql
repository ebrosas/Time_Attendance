/*********************************************************************************
*	Revision History
*
*	Name: tas.fnParseStringArrayToString
*	Description: Converts array of string into a table 
*
*	Date:			Author:		Rev. #:		Comments:
*	04/02/2018		Ervin		1.0			Created
**********************************************************************************/

CREATE FUNCTION tas.fnParseStringArrayToString
(
	@delimString	VARCHAR(1000), 
	@delim			CHAR(1)
)
RETURNS @paramtable 
TABLE (GenericStringField VARCHAR(50)) 
AS 

BEGIN

    DECLARE @len		INT,
            @index		INT,
            @nextindex	INT

    SET @len = DATALENGTH(@delimString)
    SET @index = 0
    SET @nextindex = 0

    WHILE (@len > @index )
    BEGIN

		SET @nextindex = CHARINDEX(@delim, @delimString, @index)

		IF (@nextindex = 0) SET @nextindex = @len + 2

		INSERT @paramtable
		SELECT LTRIM(RTRIM(SUBSTRING(@delimString, @index, @nextindex - @index)))

		SET @index = @nextindex + 1
	END
    
	RETURN
END


/*	Debug:

PARAMETERS:
	@delimString	VARCHAR(1000), 
	@delim			CHAR(1)

	SELECT * FROM tas.fnParseStringArrayToString('D, M, E, N', ',')

*/
