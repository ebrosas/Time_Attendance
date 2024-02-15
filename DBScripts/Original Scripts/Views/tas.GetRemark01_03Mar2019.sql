USE [tas2]
GO

/****** Object:  View [tas].[GetRemark01]    Script Date: 27/03/2019 12:14:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO




ALTER                      VIEW [tas].[GetRemark01] AS
--sssaaa
--REFRESH_Master_UDCValues_JDE

SELECT 

	T.AutoId,

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

--	(case when dtIN  is null and dtOUT is not null 	then  'Missing Swipe IN'          	   + CHAR(10)  + CHAR(13) else '' end) +
--	(case when dtOUT is null and dtIN  is not null 	then  'Missing Swipe OUT'            	   + CHAR(10)  + CHAR(13) else '' end) +

	--Added by Zaharan Haleed on 01st Feb 2012-----------------------------------------
	(CASE ISNULL(T.DIL_Entitlement,'XX')
		WHEN 'EA' THEN '[DIL entitled by Admin]' + CHAR(10) + CHAR(13)
		WHEN 'ES' THEN '[DIL entitled by System]' + CHAR(10) + CHAR(13)
		WHEN 'UA' THEN '[DIL used by Admin]' + CHAR(10) + CHAR(13)
		WHEN 'UD' THEN '[DIL used by System]' + CHAR(10) + CHAR(13)
		WHEN 'AD' THEN '[DIL Approved]' + CHAR(10) + CHAR(13)
		ELSE ''
	END) +
	------------------------------------------------------------------------------------
	(CASE WHEN swp.One IS NULL AND T.islastrow=1 THEN 'Missing Swipe IN or OUT'                + CHAR(10) + CHAR(13) ELSE '' END) +

	(CASE WHEN SwipeSource = 'M'  	THEN  'Manual TimeSheet'          	                   + CHAR(10)  + CHAR(13) ELSE '' END) +

	(CASE WHEN XP_Shifter IS NOT NULL THEN XP_Shifter                                          + CHAR(10)  + CHAR(13) ELSE '' END) +   
	(CASE WHEN XP_DayWrkr IS NOT NULL THEN XP_DayWrkr                                          + CHAR(10)  + CHAR(13) ELSE '' END) 	OtherRemarks,

	(CASE WHEN shiftcode IS NOT NULL AND actual_shiftcode IS NOT NULL AND shiftcode<>actual_shiftcode AND shiftcode<>'' AND shiftcode<>'O' THEN 'Difference in Actual and Scheduled Shift' + CHAR(10)  + CHAR(13) ELSE '' END) ShiftCodeDifference ,

	(CASE WHEN Cx.CxDESC IS NOT NULL THEN Cx.CxDESC + CHAR(10) + CHAR(13) ELSE '' END)  CustomRemarks,
	XP_Shifter,
	XP_DayWrkr,
	IsLastRow

	
FROM Tran_Timesheet T

LEFT JOIN  --Leave
(SELECT UdcKey,Code,Description lvDESC FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='58  -VC') LV
ON (T.LeaveType=LV.code) 

LEFT JOIN  --Remark
(SELECT UdcKey,Code,Description  rmDESC FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -RM') RM
ON (T.RemarkCode=RM.code) 

LEFT JOIN  --Reason Of absence
(SELECT UdcKey,Code,Description raDESC FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -RA') RA
ON (T.AbsenceReasonCode=RA.code) 

LEFT JOIN   --correction sets,  Set1
(SELECT UdcKey,Code,Description TxDESC 	FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -T0') Tx
ON (T.CorrectionCode=Tx.code) 

LEFT JOIN  --Public Holiday  (applicable to all)
(SELECT HolidayDate ,Description H_P_desc FROM Master_Calendar WHERE HolidayType='H' ) H_p
ON ( tas.fmtDate(T.DT) = tas.fmtDate(H_p.HolidayDate) /*and islastrow=1*/ ) 

LEFT JOIN  --Day In Lieu For DayWorker  (calendar=DILdw , employee , dayworkder)
(SELECT HolidayDate ,Description H_D_desc FROM Master_Calendar WHERE HolidayType='D' ) H_d
ON ( tas.fmtDate(T.DT) = tas.fmtDate(H_d.HolidayDate) AND islastrow=1  ) 

LEFT JOIN  --Ramadan (ramadan, muslim, employee)
(SELECT HolidayDate ,Description H_R_desc FROM Master_Calendar WHERE HolidayType='R' ) H_r
ON ( tas.fmtDate(T.DT) = tas.fmtDate(H_r.HolidayDate) AND islastrow=1    ) 

LEFT JOIN  --Custom Remarks for unique situations. for e.g during power off, employees go early
(SELECT UdcKey,Code,Description CxDESC 	FROM  REFRESH_Master_UDCValues_JDE WHERE udckey='55  -Cx') Cx
ON (T.RemarkCode=Cx.code) 

LEFT JOIN ( SELECT * FROM GetRemark01_MissingSwipes ) Swp
ON (T.autoid = swp.Autoid)

LEFT JOIN
(SELECT autoid , 'ExtraPay' XP_Shifter FROM tran_timesheet WHERE IsPublicHoliday=1 AND shiftcode ='O' AND isdayworker_or_shifter=0 AND islastRow=1 AND IsEmployee_OR_Contractor=1 ) XP_sh
ON (T.autoid = XP_sh.autoid )

LEFT JOIN
(SELECT autoid , 'ExtraPay' XP_DayWrkr FROM tran_timesheet WHERE IsDILdayWorker=1 AND shiftcode ='O' AND isdayworker_or_shifter=1 AND islastRow=1 AND IsEmployee_OR_Contractor=1 AND IsSalStaff=0 ) XP_Dw
ON (T.autoid = XP_dw.autoid )




GO


