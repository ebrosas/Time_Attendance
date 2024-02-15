/********************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.vuTran_Timesheet2_CopyBack
*	Description: This view is used by the Timesheet Processing Service for inserting records in Tran_Timesheet table. It is called in "Syncronize_Permanent_Data" method.
*
*	Date:			Author:		Rev. #:		Comments:
*	12/02/2012		Ervin		1.0			Created
*	01/08/2022		Ervin		1.1			Added logic to prevent duplicate attendance records for specific employees
*	
*********************************************************************************************************************************************************************************/

ALTER VIEW tas.vuTran_Timesheet2_CopyBack 
AS 

	SELECT a.*, (CASE WHEN a.dtIN IS NOT NULL THEN a.dtIN WHEN a.dtOUT IS NOT NULL THEN a.dtOUT ELSE a.DT END)  dtSORT
	FROM tas.tmp_Tran_Timesheet a
	WHERE a.EmpNo NOT IN (10001211)

	UNION
    
	SELECT DISTINCT 
		b.AutoID,
		b.TS_AutoID,
		EmpNo,
		DT,
		b.dtIN,
		c.dtOUT,
		ShiftPatCode,
		ShiftCode,
		b.Actual_ShiftCode,
		ShiftAllowance,
		OTType,
		OTStartTime,
		OTEndTime,
		NoPayHours,
		BusinessUnit,
		GradeCode,
		AbsenceReasonCode,
		a.LeaveType,
		RemarkCode,
		DIL_Entitlement,
		AbsenceReasonColumn,
		CorrectionCode,
		Processed,
		OnCallOut,
		OnDuty,
		ShiftSpanDate,
		ShiftSpan,
		b.ProcessID,
		LastUpdateUser,
		LastUpdateTime,
		XXXXXXXXXXXX_100,
		JobCategoryCode,
		isMuslim,
		isRamadan,
		IsPublicHoliday,
		IsDILdayWorker,
		XXXXXXXXXXXX_200,
		b.In1,
		b.In2,
		b.Out1,
		b.Out2,
		b.Sch_A2,
		b.Sch_D1,
		Is_InOut_in_Shaving,
		XXXXXXXXXXXX_300,
		b.dtA1,
		b.dtA2,
		b.dtD1,
		b.dtD2,
		XXXXXXXXXXXX_400,
		b.Shaved_IN,
		c.Shaved_OUT,
		XXXXXXXXXXXX_500,
		IsEmployee_OR_Contractor,
		IsResigned,
		IsSalStaff,
		IsDayWorker_OR_Shifter,
		IsDriver,
		IsLiasonOfficer,
		IsHedger,
		IsOnDutyRota,
		ContractorGroupCode,
		XXXXXXXXXXXX_600,
		1 AS IsLastRow,
		1 AS Duration_SubID,
		Duration_Shift,
		Duration_MaternityLeave,
		Duration_ROA,
		Duration_Required,
		Duration_Worked,
		Duration_Worked_Cumulative,
		XXXXXXXXXXXX_700,
		SwipeSource,
		XXXXXXXXXXXX_800,
		Duration_ShiftAllowance_Evening,
		Duration_ShiftAllowance_Night,
		XXXXXXXXXXXX_900,
		ShiftSpan_XID,
		ShiftSpan_HoursDay1,
		ShiftSpan_HoursDay2,
		ShiftSpan_AwardOT,
		ShiftSpan_2ndDayFullOT,
		XXXXXXXXXXXX_1000,
		NetMinutes,
		Upload_ID,
		MealVoucherEligibility,
		CASE WHEN b.dtIN IS NOT NULL THEN b.dtIN WHEN c.dtOUT IS NOT NULL THEN c.dtOUT ELSE a.DT END AS dtSORT
	FROM tas.tmp_Tran_Timesheet a WITH (NOLOCK)
		OUTER APPLY 
		(
			SELECT TOP 1 dtIN, Shaved_IN, ProcessID, Actual_ShiftCode, In1, In2, Out1, Out2, Sch_A2, Sch_D1, dtA1, dtA2, dtD1, dtD2, AutoID, TS_AutoID
			FROM tas.tmp_Tran_Timesheet 
			WHERE EmpNo = a.EmpNo
				AND DT = a.DT
			ORDER BY AutoID
		) b
		OUTER APPLY 
		(
			SELECT TOP 1 dtOUT, Shaved_OUT, LeaveType
			FROM tas.Tran_Timesheet 
			WHERE EmpNo = a.EmpNo
				AND DT = a.DT
			ORDER BY AutoID DESC
		) c
	WHERE a.EmpNo IN (10001211)

GO

/*	Debug:

	SELECT * FROM tas.vuTran_Timesheet2_CopyBack  a
	WHERE EmpNo = 10001211
	ORDER BY a.EmpNo

	SELECT * FROM tas.vuTran_Timesheet2_CopyBack  a
	WHERE TS_AutoID IS NULL
	ORDER BY a.EmpNo, a.DT DESC

*/


