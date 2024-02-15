/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Master_UDCValues
*	Description: Get the employee information from the Employee Master Table
*
*	Date:			Author:		Rev. #:		Comments:
*	20/06/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Master_UDCValues
AS
	
	SELECT	LTRIM(RTRIM(a.DRSY)) + '-' + UPPER(LTRIM(RTRIM(a.DRRT))) AS UDCKey,
			LTRIM(RTRIM(a.DRKY)) AS Code, 
			LTRIM(RTRIM(a.DRDL01)) AS [Description], 
			LTRIM(RTRIM(a.DRSPHD)) AS FieldRef, 
			LTRIM(RTRIM(a.DRDL02)) AS Description2
	FROM tas.syJDE_F0005 a
	WHERE a.DRSY + '-' + a.DRRT IN (SELECT UDCKey FROM tas.Master_UDCKeys)

GO


/*	Debugging:

	SELECT * FROM tas.Master_UDCValues a
	WHERE a.UDCKey = '55-SJ' 

*/
		
