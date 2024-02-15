/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_Tran_ShiftPatternUpdates
*	Description: Return the last 2 months data from "Tran_ShiftPatternUpdates" table
*
*	Date:			Author:		Rev. #:		Comments:
*	20/06/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_Tran_ShiftPatternUpdates
AS
	
	SELECT	AutoID,
			DateX,
			TxID,
			EmpNo,
			Effective_ShiftPatCode,
			Effective_ShiftPointer,
			Effective_ShiftCode,
			Effective_BusinessUnit
	FROM tas.Tran_ShiftPatternUpdates a WITH (NOLOCK)
	WHERE a.DateX BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(MONTH, -2, GETDATE()), 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))

GO


/*	Debugging:

	SELECT * FROM tas.Vw_Tran_ShiftPatternUpdates a

*/
		
