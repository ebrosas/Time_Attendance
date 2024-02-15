USE [tas2]
GO

/****** Object:  View [tas].[GetRemark01]    Script Date: 25/08/2022 08:46:56 ******/
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
*	09/08/2019		Ervin		1.2			Added holiday code "HE" in the list of known public holidays
*	14/07/2021		Ervin		1.3			Added code to determine if day-off falls withing an existing leave request
*	13/01/2022		Ervin		1.4			Set ROA desciption to empty string if leave description is not null
*	13/06/2022		Ervin		1.5			Fixed bug reported by Mustafa wherein the ROA is shown even if the date does not fall withing the specified period
*	21/08/2022		Ervn		1.6			Refactored the logic in determining whether missing in or out
*********************************************************************************************************************************************************************************/

ALTER VIEW [tas].[GetRemark01]
AS

	SELECT	a.AutoId,
			lvDESC,
			rmDESC,
			CASE WHEN ISNULL(lvDESC, '') = '' THEN raDESC ELSE '' END AS raDESC,	--Rev. #1.4
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

			(	--Rev. #1.6
				CASE WHEN swp.One IS NULL AND a.IsLastRow = 1 AND ((a.dtIN IS NULL AND a.dtOUT IS NOT NULL) OR (a.dtIN IS NOT NULL AND a.dtOUT IS NULL)) THEN 'Missing Swipe IN or OUT' + CHAR(10) + CHAR(13) 
					WHEN swp.One IS NULL AND a.IsLastRow = 1 AND a.dtIN IS NULL AND a.dtOUT IS NULL THEN 'Missing Swipe IN and OUT' + CHAR(10) + CHAR(13) 
				ELSE '' END
			) +

			(CASE WHEN SwipeSource = 'M'  	THEN  'Manual TimeSheet'          	                   + CHAR(10)  + CHAR(13) ELSE '' END) +

			(CASE WHEN XP_Shifter IS NOT NULL THEN XP_Shifter                                          + CHAR(10)  + CHAR(13) ELSE '' END) +   
			(CASE WHEN XP_DayWrkr IS NOT NULL THEN XP_DayWrkr                                          + CHAR(10)  + CHAR(13) ELSE '' END) 	OtherRemarks,

			(CASE WHEN shiftcode IS NOT NULL AND actual_shiftcode IS NOT NULL AND shiftcode<>actual_shiftcode AND shiftcode<>'' AND shiftcode<>'O' THEN 'Difference in Actual and Scheduled Shift' + CHAR(10)  + CHAR(13) ELSE '' END) ShiftCodeDifference ,

			(CASE WHEN Cx.CxDESC IS NOT NULL THEN Cx.CxDESC + CHAR(10) + CHAR(13) ELSE '' END)  CustomRemarks,
			XP_Shifter,
			XP_DayWrkr,
			a.IsLastRow,
			a.RemarkCode
	FROM Tran_Timesheet a WITH (NOLOCK)

	--Leave
	--OUTER APPLY
	--(
	--	SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS lvDESC
	--	FROM tas.syJDE_F0005 WITH (NOLOCK)
	--	WHERE RTRIM(DRSY) = '58' 
	--		AND RTRIM(DRRT) = 'VC' 
	--		AND LTRIM(RTRIM(DRKY)) = RTRIM(a.LeaveType)
	--) LV
	OUTER APPLY		--Rev. #1.3
	(
		SELECT TOP 1 * FROM 
		(
			SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS lvDESC
			FROM tas.syJDE_F0005 WITH (NOLOCK)
			WHERE RTRIM(DRSY) = '58' 
				AND RTRIM(DRRT) = 'VC' 
				AND LTRIM(RTRIM(DRKY)) = RTRIM(a.LeaveType)

			UNION
        
			SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS lvDESC
			FROM tas.syJDE_F0005 WITH (NOLOCK)
			WHERE RTRIM(DRSY) = '58' 
				AND RTRIM(DRRT) = 'VC' 
				AND LTRIM(RTRIM(DRKY)) = tas.fnCheckIfLeaveExist(a.AutoID)
		) a
	) LV


	--Remark
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS rmDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'RM' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.RemarkCode)
	) RM

	--Reason Of absence
	OUTER APPLY
	(
		SELECT	LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, 
				LTRIM(RTRIM(DRKY)) AS Code,	
				LTRIM(RTRIM(DRDL01)) AS raDESC
		FROM tas.syJDE_F0005 x WITH (NOLOCK)
			CROSS APPLY		--Rev. #1.5
			(
				SELECT AutoID FROM tas.Tran_Absence  
				WHERE EmpNo = a.EmpNo 
					AND RTRIM(AbsenceReasonCode) = RTRIM(a.AbsenceReasonCode) 
					AND a.DT BETWEEN EffectiveDate AND EndingDate
			) y
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'RA' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.AbsenceReasonCode)
	) RA

	--Correction sets,  Set1
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS TxDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'T0' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.CorrectionCode)
	) Tx

	--Public Holiday  (applicable to all)
	OUTER APPLY
	(
		SELECT HolidayDate, RTRIM(Description) AS H_P_desc
		FROM tas.Master_Calendar WITH (NOLOCK)
		WHERE RTRIM(HolidayType) IN ('H', 'HE')		--Rev. #1.2 
			AND HolidayDate = a.DT
	) H_p

	--Day In Lieu For DayWorker  (calendar=DILdw , employee , dayworkder)
	OUTER APPLY
	(
		SELECT HolidayDate, RTRIM(Description) AS H_D_desc
		FROM tas.Master_Calendar WITH (NOLOCK)
		WHERE RTRIM(HolidayType) = 'D' 
			AND HolidayDate = a.DT 
			AND a.IsLastRow = 1
	) H_d

	--Ramadan (ramadan, muslim, employee)
	OUTER APPLY
	(
		SELECT HolidayDate, RTRIM(Description) AS H_R_desc
		FROM tas.Master_Calendar WITH (NOLOCK)
		WHERE RTRIM(HolidayType) = 'R' 
			AND HolidayDate = a.DT 
			AND a.IsLastRow = 1
	) H_r

	--Custom Remarks for unique situations. for e.g during power off, employees go early
	OUTER APPLY
	(
		SELECT LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) AS UDCKey, LTRIM(RTRIM(DRKY)) AS Code,	LTRIM(RTRIM(DRDL01)) AS CxDESC
		FROM tas.syJDE_F0005 WITH (NOLOCK)
		WHERE RTRIM(DRSY) = '55' 
			AND RTRIM(DRRT) = 'CX' 
			AND LTRIM(RTRIM(DRKY)) = RTRIM(a.RemarkCode)
	) Cx

	LEFT JOIN ( SELECT * FROM GetRemark01_MissingSwipes  WITH (NOLOCK)) Swp
	ON (a.autoid = swp.Autoid)

	LEFT JOIN
	(SELECT autoid , 'ExtraPay' XP_Shifter FROM tas.Tran_Timesheet WITH (NOLOCK) WHERE IsPublicHoliday=1 AND shiftcode ='O' AND isdayworker_or_shifter=0 AND islastRow=1 AND IsEmployee_OR_Contractor=1 ) XP_sh
	ON (a.autoid = XP_sh.autoid )

	LEFT JOIN
	(SELECT autoid , 'ExtraPay' XP_DayWrkr FROM tas.Tran_Timesheet WITH (NOLOCK) WHERE IsDILdayWorker=1 AND shiftcode ='O' AND isdayworker_or_shifter=1 AND islastRow=1 AND IsEmployee_OR_Contractor=1 AND IsSalStaff=0 ) XP_Dw
	ON (a.autoid = XP_dw.autoid )

GO


