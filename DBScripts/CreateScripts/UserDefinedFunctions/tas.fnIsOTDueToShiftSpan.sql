/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsOTDueToShiftSpan
*	Description: This function is used to check if the overtime is a result of shift span due to double continous shift
*
*	Date			Author		Rev. #		Comments:
*	30/08/2017		Ervin		1.0			Created
**********************************************************************************************************************************************/

CREATE FUNCTION tas.fnIsOTDueToShiftSpan 
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
		SELECT AutoID FROM tas.Tran_Timesheet a
		WHERE a.EmpNo = @empNo
			AND a.DT = @dt
			AND 
			(
				(a.ShiftSpan IS NULL AND a.ShiftSpanDate IS NOT NULL AND ISNULL(a.ShiftSpan_XID, 0) > 0   AND a.ShiftSpan_AwardOT = 0)
				OR
				(a.ShiftSpan = 1 AND a.ShiftSpanDate IS NULL AND ISNULL(a.ShiftSpan_XID, 0) > 0  AND a.ShiftSpan_AwardOT = 1) 
			)
	)
	SET @result = 1

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnIsOTDueToShiftSpan(10006112, '08/28/2017') 

*/