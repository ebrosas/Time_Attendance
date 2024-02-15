/*********************************************************************************
*	Revision History
*
*	Name: tas.fnGetUDCDescription
*	Description: This function is used to fetch the UDC description
*
*	Date:				Author:			Comments:
*	27/11/2023			Ervin			Created
**********************************************************************************/

CREATE FUNCTION tas.fnGetUDCDescription
(
    @DRSY	VARCHAR(4),
	@DRRT	VARCHAR(2),
	@DRKY	VARCHAR(10)
)
RETURNS VARCHAR(30)
AS
BEGIN

	DECLARE	@udcDesc VARCHAR(30) = ''

	SELECT TOP 1 @udcDesc = LTRIM(RTRIM(a.DRDL01)) 
	FROM tas.syJDE_F0005 a WITH (NOLOCK)
	WHERE UPPER(LTRIM(RTRIM(a.DRSY))) = UPPER(LTRIM(RTRIM(@DRSY)))
		AND UPPER(LTRIM(RTRIM(a.DRRT))) = UPPER(LTRIM(RTRIM(@DRRT)))
		AND UPPER(LTRIM(RTRIM(a.DRKY))) = UPPER(LTRIM(RTRIM(@DRKY)))

	RETURN ISNULL(@udcDesc, '')

END 

/*	Test:

	SELECT tas.fnGetUDCDescription('55', 'T0', 'acs')

*/