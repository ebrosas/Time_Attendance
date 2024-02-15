USE [tas2]
GO

/****** Object:  View [tas].[GetRemark02]    Script Date: 27/03/2019 12:14:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

ALTER VIEW [tas].[GetRemark02] AS
SELECT 

	AutoId,

	(CASE WHEN LVdesc IS NOT NULL THEN RTRIM(LVdesc)                                            + CHAR(10)  + CHAR(13) ELSE '' END) LVDesc,
	(CASE WHEN RMdesc IS NOT NULL AND LVdesc IS NULL  THEN RTRIM(RMdesc)          + CHAR(10)  + CHAR(13) ELSE '' END) RMdesc,
	(CASE WHEN RAdesc IS NOT NULL THEN /*'AbsenceReason = ' +*/ RTRIM(RAdesc)                   + CHAR(10)  + CHAR(13) ELSE '' END) RAdesc,
	(CASE WHEN TxDesc IS NOT NULL THEN 'CorrectionCode = ' + RTRIM(TxDesc)                      + CHAR(10)  + CHAR(13) ELSE '' END) TxDesc,
	
	
	(CASE WHEN H_P_desc IS NOT NULL       THEN 'PublicHoliday - ' + RTRIM(H_P_desc)             + CHAR(10)  + CHAR(13) ELSE '' END) H_P_desc ,
	(CASE WHEN H_D_desc IS NOT NULL       THEN 'HolidayInLieu for DayWorker - ' + RTRIM(H_D_desc)   + CHAR(10)  + CHAR(13) ELSE '' END) H_D_desc ,
	(CASE WHEN H_R_desc IS NOT NULL       THEN 'Ramadan'                                        + CHAR(10)  + CHAR(13) ELSE '' END) H_R_desc ,
	
	(CASE WHEN ShiftSpan=1 			THEN 	'Shift Span  (1st Day)'  + CHAR(10)  + CHAR(13) 
	      WHEN ShiftSpanDate IS NOT NULL	THEN  	'Shift Span  (2nd Day)'  + CHAR(10)  + CHAR(13) 
	ELSE '' END) TxtShiftSpan ,


	(CASE WHEN ShiftCode='O' AND LVdesc IS NULL AND islastrow=1 	THEN /*'Scheduled Shift =*/ 'Day Off'       + CHAR(10)  + CHAR(13) ELSE '' END) DayOff ,

	(CASE WHEN IsResigned=1       	THEN 'ALERT <Member Resigned>'                              + CHAR(10)  + CHAR(13) ELSE '' END) Resigned,
	
	OtherRemarks,

	ShiftCodeDifference,

	CustomRemarks
	
FROM GetRemark01 R

GO


