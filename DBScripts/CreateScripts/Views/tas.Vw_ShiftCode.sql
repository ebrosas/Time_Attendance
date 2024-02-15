/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ShiftCode
*	Description: Get the list of all shift codes
*
*	Date:			Author:		Rev. #:		Comments:
*	11/06/2018		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ShiftCode
AS		
	
	SELECT	LTRIM(RTRIM(a.DRKY)) AS ShiftCode,
			LTRIM(RTRIM(a.DRDL01)) AS ShiftDesc
	FROM tas.syJDE_F0005 a
	WHERE 
		LTRIM(RTRIM(a.DRSY)) = '06' 
		AND UPPER(LTRIM(RTRIM(a.DRRT))) = 'SH'
		AND RTRIM(ISNULL(a.DRKY, '')) <> ''

GO

/* Testing:

	SELECT * FROM tas.Vw_ShiftCode a
	
*/
