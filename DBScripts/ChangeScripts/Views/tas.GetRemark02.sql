/********************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.GetRemark02
*	Description: Get the employee attendance history records
*
*	Date:			Author:		Rev. #:		Comments:
*	12/02/2012		Ervin		1.0			Created
*	27/03/2019		Ervin		1.1			Refactored the code
*********************************************************************************************************************************************************************************/

ALTER VIEW tas.GetRemark02 
AS

	SELECT	AutoId,
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


			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND a.LVdesc IS NULL AND a.IsLastRow = 1 AND RTRIM(a.RemarkCode) <> 'A'  
				THEN 'Day Off' + CHAR(10) + CHAR(13) 
				ELSE '' 
			END AS DayOff,

			CASE WHEN IsResigned = 1 
				THEN 'ALERT <Member Resigned>' + CHAR(10)  + CHAR(13) 
				ELSE '' 
			END AS Resigned,

			OtherRemarks,
			ShiftCodeDifference,
			CustomRemarks
	FROM tas.GetRemark01 a

GO

/*	Debug:

	SELECT * FROM tas.GetRemark02 a
	WHERE a.AutoId = 4481655

*/


