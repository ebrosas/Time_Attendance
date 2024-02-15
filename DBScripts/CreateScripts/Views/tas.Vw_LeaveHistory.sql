/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_LeaveHistory
*	Description: Get the employee's leave history records
*
*	Date:			Author:		Rev. #:		Comments:
*	27/07/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_LeaveHistory
AS
	
	SELECT	a.LRY58VCRQN AS AutoID, 
			CAST(a.LRAN8 AS INT) AS EmpNo, 
			tas.ConvertFromJulian(a.LRY58VCOFD) AS FromDate, 
			tas.ConvertFromJulian(a.LRY58VCOTD) AS ToDate, 
			CAST(a.LRY58VCVCD AS VARCHAR(3)) AS LeaveCode,
			LTRIM(RTRIM(b.DRDL01)) AS LeaveDesc
	FROM tas.syJDE_F58LV13 a
		LEFT JOIN tas.syJDE_F0005 b ON LTRIM(RTRIM(a.LRY58VCVCD)) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'VC'
	WHERE a.LRY58VCAFG NOT IN ('C', 'W', 'D', 'R')
		AND a.LRAN8 > 10000000

	UNION

	SELECT	a.AutoID, 
			a.EmpNo, 
			a.EffectiveDate AS FromDate, 
			a.EndingDate AS ToDate, 
			AbsenceReasonCode AS LeaveCode,
			CASE WHEN RTRIM(a.AbsenceReasonCode) = 'DD' THEN 'Day-in-lieu' ELSE LTRIM(RTRIM(b.DRDL01)) END AS LeaveDesc 
	FROM tas.Tran_Absence a
		LEFT JOIN tas.syJDE_F0005 b ON UPPER(RTRIM(a.AbsenceReasonCode)) = LTRIM(RTRIM(b.DRKY)) AND LTRIM(RTRIM(DRSY)) = '55' AND LTRIM(RTRIM(DRRT)) = 'RA'

GO 

/*	Debugging:

	--Retrieve all leave type codes
	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'VC'
	ORDER BY LTRIM(RTRIM(DRKY))

	SELECT * FROM tas.Vw_LeaveHistory a
	WHERE a.EmpNo = 10003632
	ORDER BY a.FromDate DESC

	SELECT * FROM tas.syJDE_F58LV13 a
	WHERE a.LRAN8 = 10003632
	ORDER BY a.LRY58VCRQN DESC

*/