/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_ShiftPatternChange
*	Description: Get the shift pattern change history
*
*	Date:			Author:		Rev. #:		Comments:
*	27/07/2016		Ervin		1.0			Created
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_ShiftPatternChange
AS
	
	SELECT	a.AutoID,
			a.XID_AutoID,
			a.EmpNo,
			a.EffectiveDate,
			a.EndingDate,
			a.ShiftPatCode,
			a.ShiftPointer,
			a.ChangeType,
			CASE WHEN UPPER(RTRIM(a.ChangeType)) = 'D' THEN 'Permanent' ELSE 'Temporary' END AS ChangeTypeDesc,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.action_type AS ActionType,
			CASE WHEN UPPER(RTRIM(a.action_type)) = 'I' THEN 'Insert record' 
				WHEN UPPER(RTRIM(a.action_type)) = 'U' THEN 'Update record' 
				WHEN UPPER(RTRIM(a.action_type)) = 'D' THEN 'Delete record' 
				WHEN UPPER(RTRIM(a.action_type)) = 'R' THEN 'Retrieve record' 
				ELSE '' 
			END AS ActionTypeDesc,
			a.action_machine AS ActionMachine,
			a.action_time AS ActionTime
	FROM tas.AUDIT_Tran_ShiftPatternChanges a
	--WHERE a.ChangeType = 'T'

	--UNION

	--SELECT	0 AS AutoID,
	--		a.AutoID AS XID_AutoID,
	--		a.EmpNo,
	--		a.EffectiveDate,
	--		a.EndingDate,
	--		a.ShiftPatCode,
	--		a.ShiftPointer,
	--		a.ChangeType,
	--		CASE WHEN UPPER((a.ChangeType)) = 'D' THEN 'Permanent' ELSE 'Temporary' END AS ChangeTypeDesc,
	--		a.LastUpdateUser,
	--		a.LastUpdateTime,
	--		'' AS ActionType,
	--		'' AS ActionTypeDesc,
	--		'' AS ActionMachine,
	--		'' AS ActionTime
	--FROM tas.Tran_ShiftPatternChanges a
	--WHERE a.ChangeType = 'D'

GO 

/*	Debugging:

	SELECT * FROM tas.Vw_ShiftPatternChange

*/