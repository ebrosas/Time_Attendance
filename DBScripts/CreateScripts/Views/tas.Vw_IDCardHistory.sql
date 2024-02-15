/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_LicenseTypes
*	Description: Get the card history information
*
*	Date:			Author:		Rev. #:		Comments:
*	12/10/2021		Ervin		1.0			Created
*
************************************************************************************************************************************************/

CREATE VIEW tas.Vw_IDCardHistory
AS

	SELECT	a.HistoryID,
			a.EmpNo,
			a.IsContractor,
			a.CardRefNo,
			a.Remarks,
			a.CardGUID,
			a.CreatedDate,
			a.CreatedByEmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS 'CreatedByEmpName',
			a.CreatedByUser,
			a.LastUpdatedDate,
			a.LastUpdatedByEmpNo,
			LTRIM(RTRIM(c.YAALPH)) AS LastUpdatedByEmpName,
			a.LastUpdatedByUser
	FROM tas.IDCardHistory a WITH (NOLOCK)
		LEFT JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.CreatedByEmpNo = CAST(b.YAAN8 AS INT)
		LEFT JOIN tas.syJDE_F060116 c WITH (NOLOCK) ON a.LastUpdatedByEmpNo = CAST(c.YAAN8 AS INT)

GO 

/*	Debug:

	SELECT * FROM tas.Vw_IDCardHistory a

*/