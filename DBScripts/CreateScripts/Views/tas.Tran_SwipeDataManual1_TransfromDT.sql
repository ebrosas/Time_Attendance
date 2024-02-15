/****************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Tran_SwipeDataManual1_TransfromDT
*	Description: Retrieves the manual swipe data logged by the Security Personnel from the Main Gate
*
*	Date:			Author:		Rev. #:		Comments:
*	27/01/2021		Ervin		1.1			Refactored the code. Returned only the top 200 records
*****************************************************************************************************************************************/

ALTER VIEW tas.Tran_SwipeDataManual1_TransfromDT 
AS

	SELECT	TOP 200
			autoid,
			empno, 
			tas.add_HHMM_TO_date(dtIN , timeIN ) dtIN,
			tas.add_HHMM_TO_date(dtOUT , timeOUT ) dtOUT
	FROM tas.Tran_ManualAttendance a WITH (NOLOCK)
	ORDER BY a.AutoID DESC

GO


