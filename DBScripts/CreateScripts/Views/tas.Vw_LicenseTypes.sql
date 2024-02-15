/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_LicenseTypes
*	Description: Get the list of all contractor licenses
*
*	Date:			Author:		Rev. #:		Comments:
*	21/09/2021		Ervin		1.0			Created
*
************************************************************************************************************************************************/

CREATE VIEW tas.Vw_LicenseTypes
AS

	SELECT	RTRIM(a.UDCCode) AS LicenseCode,
			RTRIM(a.UDCDesc1) AS LicenseDesc,
			CAST(a.UDCAmount AS INT) AS SequenceNo
	FROM tas.sy_UserDefinedCode a WITH (NOLOCK)
	WHERE UDCUDCGID = (SELECT UDCGID FROM tas.sy_UserDefinedCodeGroup WHERE (RTRIM(UDCGCode)) = 'LICENSETYP')
		
GO


/*	Debugging:

	SELECT * FROM tas.Vw_LicenseTypes a

*/