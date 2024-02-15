/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsDoubleShiftWithGap
*	Description: This function is used to check if the employee worked double shift with gap in between on the specified date
*
*	Date			Author		Rev. #		Comments:
*	27/09/2017		Ervin		1.0			Created
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnIsDoubleShiftWithGap 
(
	@empNo		INT,
	@dt			DATETIME 
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result BIT
	SET @result = 0
    
	IF EXISTS
    (
		SELECT a.AutoID
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
			LEFT JOIN tas.OvertimeRemovalLog c ON a.AutoID = c.TS_AutoID
		WHERE 
			RTRIM(ISNULL(a.ShiftCode, '')) <> 'O'
			AND a.Duration_Worked >= a.Duration_Required
			AND a.ShiftSpan_XID IS NULL
			AND a.ShiftSpanDate IS NULL 
			AND a.ShiftSpan IS NULL
			AND (a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL)
			AND a.DT = @dt
			AND a.EmpNo = @empNo 
			AND
			(
				SELECT COUNT(*)
				FROM tas.Tran_Timesheet 
				WHERE EmpNo = a.EmpNo
					AND DT = a.DT
					AND RTRIM(ISNULL(ShiftCode, '')) <> 'O'
					AND Duration_Worked >= Duration_Required
					AND ShiftSpan_XID IS NULL
					AND ShiftSpanDate IS NULL 
					AND ShiftSpan IS NULL
					AND (Shaved_IN IS NOT NULL AND Shaved_OUT IS NOT NULL)
			) > 1
			AND (b.OTstartTime IS NULL AND b.OTendTime IS NULL)
			AND (a.OTstartTime IS NULL AND a.OTendTime IS NULL)
	)
	SET @result = 1

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnIsDoubleShiftWithGap(10006112, '08/16/2017') 

*/