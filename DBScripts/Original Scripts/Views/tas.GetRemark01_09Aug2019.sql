USE [tas2]
GO

/****** Object:  View [tas].[GetRemark01]    Script Date: 09/08/2019 15:01:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/********************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.GetRemark01
*	Description: Get the employee attendance history records
*
*	Date:			Author:		Rev. #:		Comments:
*	12/02/2012		Ervin		1.0			Created
*	27/03/2019		Ervin		1.1			Refactored the code
*********************************************************************************************************************************************************************************/

ALTER VIEW [tas].[GetRemark01] 
AS

	SELECT	a.AutoId,
			lvDESC,
			rmDESC,
			raDESC,
			txDESC,
			H_P_desc,
			H_D_desc,
			H_R_desc,
			shiftSpan,
			shiftSpanDate,
			IsRamadan,
			IsPublicHoliday,
			IsDILdayWorker,
			ShiftCode,
			IsResigned,
		
			--Added by Zaharan Haleed on 01st Feb 2012-----------------------------------------
			(CASE ISNULL(a.DIL_Entitlement,'XX')
				WHEN 'EA' THEN '[DIL entitled by Admin]' + CHAR(10) + CHAR(13)
				WHEN 'ES' THEN '[DIL entitled by System]' + CHAR(10) + CHAR(13)
				WHEN 'UA' THEN '[DIL used by Admin]' + CHAR(10) + CHAR(13)
				WHEN 'UD' THEN '[DIL used by System]' + CHAR(10) + CHAR(13)
				WHEN 'AD' THEN '[DIL Approved]' + CHAR(10) + CHAR(13)
				ELSE ''
			END) +
			------------------------------------------------------------------------------------

			(CASE WHEN swp.One IS NULL AND a.islastrow=1 THEN 'Missing Swipe IN or OUT'                + CHAR(10) + CHAR(13) ELSE '' END) +

			(CASE WHEN SwipeSource = 'M'  	THEN  'Manual TimeSheet'          	                   + CHAR(10)  + CHAR(13) ELSE '' END) +

			(CASE WHEN XP_Shifter IS NOT NULL THEN XP_Shifter                                          + CHAR(10)  + CHAR(13) ELSE '' END) +   
			(CASE WHEN XP_DayWrkr IS NOT NULL THEN XP_DayWrkr                                          + CHAR(10)  + CHAR(13) ELSE '' END) 	OtherRemarks,

			(CASE WHEN shiftcode IS NOT NULL AND actual_shiftcode IS NOT NULL AND shiftcode<>actual_shiftcode AND shiftcode<>'' AND shiftcode<>'O' THEN 'Difference in Actual and Scheduled Shift' + CHAR(10)  + CHAR(13) ELSE '' END) ShiftCodeDifference ,

			(CASE WHEN Cx.CxDESC IS NOT NULL THEN Cx.CxDESC + CHAR(10) + CHAR(13) ELSE '' END)  CustomRemarks,
			XP_Shifter,
			XP_DayWrkr,
			a.IsLastRow,
			a.RemarkCode
	FROM Tran_Timesheet a

	--LEFT JOIN  --Leave
	--(SELECT UdcKey,Code,Description lvDESC FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='58  -VC') LV
	--ON (a.LeaveType=LV.code) 
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS lvDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '58' 
			AND RTRIM(DRRT) = 'VC' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.LeaveType)
	) LV


	--LEFT JOIN  --Remark
	--(SELECT UdcKey,Code,Description  rmDESC FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -RM') RM
	--ON (a.RemarkCode=RM.code) 
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS rmDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'RM' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.RemarkCode)
	) RM

	--LEFT JOIN  --Reason Of absence
	--(SELECT UdcKey,Code,Description raDESC FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -RA') RA
	--ON (a.AbsenceReasonCode=RA.code) 
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS raDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'RA' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.AbsenceReasonCode)
	) RA

	--LEFT JOIN   --correction sets,  Set1
	--(SELECT UdcKey,Code,Description TxDESC 	FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -T0') Tx
	--ON (a.CorrectionCode=Tx.code) 
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS TxDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'T0' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.CorrectionCode)
	) Tx

	--LEFT JOIN  --Public Holiday  (applicable to all)
	--(SELECT HolidayDate ,Description H_P_desc FROM tas.Master_Calendar WHERE HolidayType='H' ) H_p
	--ON ( tas.fmtDate(a.DT) = tas.fmtDate(H_p.HolidayDate) 
	OUTER APPLY
	(
		SELECT HolidayDate, RTRIM(Description) AS H_P_desc
		FROM tas.Master_Calendar WITH (NOLOCK)
		WHERE RTRIM(HolidayType) = 'H' 
			AND HolidayDate = a.DT
	) H_p

	--LEFT JOIN  --Day In Lieu For DayWorker  (calendar=DILdw , employee , dayworkder)
	--(SELECT HolidayDate ,Description H_D_desc FROM tas.Master_Calendar WHERE HolidayType='D' ) H_d
	--ON ( tas.fmtDate(a.DT) = tas.fmtDate(H_d.HolidayDate) AND islastrow=1  ) 
	OUTER APPLY
	(
		SELECT HolidayDate, RTRIM(Description) AS H_D_desc
		FROM tas.Master_Calendar WITH (NOLOCK)
		WHERE RTRIM(HolidayType) = 'D' 
			AND HolidayDate = a.DT 
			AND a.IsLastRow = 1
	) H_d

	--LEFT JOIN  --Ramadan (ramadan, muslim, employee)
	--(SELECT HolidayDate ,Description H_R_desc FROM Master_Calendar WHERE HolidayType='R' ) H_r
	--ON ( tas.fmtDate(a.DT) = tas.fmtDate(H_r.HolidayDate) AND islastrow=1    ) 
	OUTER APPLY
	(
		SELECT HolidayDate, RTRIM(Description) AS H_R_desc
		FROM tas.Master_Calendar WITH (NOLOCK)
		WHERE RTRIM(HolidayType) = 'R' 
			AND HolidayDate = a.DT 
			AND a.IsLastRow = 1
	) H_r

	--LEFT JOIN  --Custom Remarks for unique situations. for e.g during power off, employees go early
	--(SELECT UdcKey,Code,Description CxDESC 	FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -Cx') Cx
	--ON (a.RemarkCode=Cx.code) 
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS CxDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'CX' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.RemarkCode)
	) Cx

	LEFT JOIN ( SELECT * FROM GetRemark01_MissingSwipes ) Swp
	ON (a.autoid = swp.Autoid)

	LEFT JOIN
	(SELECT autoid , 'ExtraPay' XP_Shifter FROM tas.Tran_Timesheet WITH (NOLOCK) WHERE IsPublicHoliday=1 AND shiftcode ='O' AND isdayworker_or_shifter=0 AND islastRow=1 AND IsEmployee_OR_Contractor=1 ) XP_sh
	ON (a.autoid = XP_sh.autoid )

	LEFT JOIN
	(SELECT autoid , 'ExtraPay' XP_DayWrkr FROM tas.Tran_Timesheet WITH (NOLOCK) WHERE IsDILdayWorker=1 AND shiftcode ='O' AND isdayworker_or_shifter=1 AND islastRow=1 AND IsEmployee_OR_Contractor=1 AND IsSalStaff=0 ) XP_Dw
	ON (a.autoid = XP_dw.autoid )

GO


