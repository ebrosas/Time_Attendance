/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_TimesheetLogDetail
*	Description: This stored procedure is used to retrieve Timesheet Processing log information
*
*	Date			Author		Rev.#		Comments:
*	02/07/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_TimesheetLogDetail
(   
	@loadType		TINYINT,
	@processDate	DATETIME
)
AS
	
	IF @loadType = 1			--Get SPU logs
	BEGIN

		SELECT * FROM tas.SPUTransactionLog a
		WHERE a.SPU_Date = @processDate
		ORDER BY a.AutoID DESC 
	END 

	ELSE IF @loadType = 2		--Get Timesheet Process Service logs
	BEGIN
    
		SELECT * FROM tas.Tran_Timesheet_ProcessMessages a
		WHERE CONVERT(DATETIME, CONVERT(VARCHAR, a.ProcessDate, 12)) = @processDate
		ORDER BY a.ProcessDate DESC
	END 	

	

	ELSE IF @loadType = 3	--Get Shift Pointer count
	BEGIN

		SELECT	a.Effective_ShiftPatCode AS ShiftPatCode, 
				a.Effective_ShiftCode AS ShiftCode, 
				a.Effective_ShiftPointer AS ShiftPointer, 
				COUNT(a.AutoID) AS EmpCount
		FROM tas.Tran_ShiftPatternUpdates a
		WHERE a.DateX = @processDate
			AND empno > 10000000
		GROUP BY a.Effective_ShiftPatCode, a.Effective_ShiftCode, a.Effective_ShiftPointer
		ORDER BY a.Effective_ShiftPatCode, a.Effective_ShiftPointer
	END 

	

GO 

/*
	
PARAMETERS:
	@loadType		TINYINT,
	@processDate	DATETIME

	EXEC tas.Pr_TimesheetLogDetail 1, '07/03/2018'		--SPU logs
	EXEC tas.Pr_TimesheetLogDetail 2, '07/03/2018'		--Timesheet Process logs
	EXEC tas.Pr_TimesheetLogDetail 3, '07/03/2018'		--Shift Pointer counts

*/