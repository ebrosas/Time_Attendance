/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetShiftPatternInfo
*	Description: Retrieves the employee's shift pattern information
*
*	Date			Author		Revision No.	Comments:
*	07/06/2016		Ervin		1.0				Created
*	29/01/2017		Ervin		1.1				Fixed bug in the WHERE filter clause to query information based on the value of "ShiftPatCode"
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetShiftPatternInfo
(   
	@empNo	INT
)
AS

	SELECT	a.AutoID,
			a.EmpNo,
			ISNULL(a.ShiftPatCode, b.Effective_ShiftPatCode) AS ShiftPatCode,
			b.Effective_ShiftCode AS ShiftCode,
			RTRIM(b.Effective_ShiftCode) + tas.fnGetShiftCodes(RTRIM(a.ShiftPatCode)) AS ShiftCodeArray,
			a.ShiftPointer,
			a.WorkingBusinessUnit,
			c.BusinessUnitName AS WorkingBusinessUnitName,
			a.LastUpdateUser,
			a.LastUpdateTime
	FROM tas.Master_EmployeeAdditional a
		LEFT JOIN tas.Tran_ShiftPatternUpdates b ON a.EmpNo = b.EmpNo AND b.DateX = CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.WorkingBusinessUnit) = RTRIM(c.BusinessUnit)
	WHERE a.EmpNo = @empNo

GO 

/*	Debugging:

	EXEC tas.Pr_GetShiftPatternInfo 10003752
	select tas.fnGetShiftCodes('I')

*/


