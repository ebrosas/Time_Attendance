/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetUDCList
*	Description: This function is used to retrieve the user-defined codes based on "DRSY" and "DRRT" parameters
*
*	Date:			Author:		Rev.#:		Comments:
*	05/01/2018		Ervin		1.0			Created
**************************************************************************************************************************************************************/

CREATE FUNCTION tas.fnGetUDCList
(
	@DRSY	VARCHAR(4),
	@DRRT	VARCHAR(2)
)
RETURNS  @rtnTable TABLE  
(     
	UDCCode			VARCHAR(10),
	UDCDescription	VARCHAR(30),
	UDCDesc2		VARCHAR(30)
) 
AS 
BEGIN 

	INSERT INTO @rtnTable  
	SELECT	LTRIM(RTRIM(a.DRKY)), 
			LTRIM(RTRIM(a.DRDL01)), 
			LTRIM(RTRIM(a.DRDL02))
	FROM tas.syJDE_F0005 a
	WHERE LTRIM(RTRIM(a.DRSY)) = @DRSY
		AND LTRIM(RTRIM(a.DRRT)) = @DRRT
	ORDER BY a.DRDL01
	
	RETURN
END

/*	Debugging:

	SELECT * FROM tas.fnGetUDCList('55', 'RA')

*/
	
