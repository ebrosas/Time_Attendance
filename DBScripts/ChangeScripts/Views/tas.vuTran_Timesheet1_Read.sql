/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.vuTran_Timesheet1_Read 
*	Description: Retrieves data from "tas.Tran_Timesheet" table
*
*	Date:			Author:		Rev. #:		Comments:
*	22/09/2014		Ervin		1.0			Added condition to set Grade Code = 80 for all cost centers belonging to 8500 company code
*	16/10/2017		Ervin		1.1			Check if employee's special job catalog is set to "Medical Condition". If true, then set "IsDriver" field to 1.
*****************************************************************************************************************************************************************************/

ALTER VIEW tas.vuTran_Timesheet1_Read
AS

	SELECT	a.AutoID,
			a.EmpNo,
			a.DT,
			a.dtIN,
			a.dtOUT,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.ShiftAllowance,
			a.OTType,
			a.OTStartTime,
			a.OTEndTime,
			a.NoPayHours,
			a.BusinessUnit,
			CASE WHEN RTRIM(a.BusinessUnit) IN ('7920') --(SELECT LTRIM(RTRIM(MCCO)) FROM tas.syJDE_F0006 WHERE LTRIM(RTRIM(MCMCU)) = RTRIM(a.BusinessUnit)) = '00850' 
				THEN '80'
				ELSE RTRIM(a.GradeCode) 
				END
			AS GradeCode,
			a.AbsenceReasonCode,
			a.LeaveType,
			a.RemarkCode,
			a.DIL_Entitlement,
			a.AbsenceReasonColumn,
			a.CorrectionCode,
			a.Processed,
			a.OnCallOut,
			a.OnDuty,
			a.ShiftSpanDate,
			a.ShiftSpan,
			a.ProcessID,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.XXXXXXXXXXXX_100,
			a.JobCategoryCode,
			a.isMuslim,
			a.isRamadan,
			a.IsPublicHoliday,
			a.IsDILdayWorker,
			a.XXXXXXXXXXXX_200,
			a.In1,
			a.In2,
			a.Out1,
			a.Out2,
			a.Sch_A2,
			a.Sch_D1,
			a.Is_InOut_in_Shaving,
			a.XXXXXXXXXXXX_300,
			a.dtA1,
			a.dtA2,
			a.dtD1,
			a.dtD2,
			a.XXXXXXXXXXXX_400,
			a.Shaved_IN,
			a.Shaved_OUT,
			a.XXXXXXXXXXXX_500,
			a.IsEmployee_OR_Contractor,
			a.IsResigned,
			a.IsSalStaff,
			a.IsDayWorker_OR_Shifter,
			
			--Start of Rev. #1.1
			CASE WHEN tas.fnIsSpecialCatalogMedicalCondition(a.EmpNo) = 1
				THEN 1
				ELSE a.IsDriver
			END AS IsDriver,
			--End of Rev. #1.1

			a.IsLiasonOfficer,
			a.IsHedger,
			a.IsOnDutyRota,
			a.ContractorGroupCode,
			a.XXXXXXXXXXXX_600,
			a.IsLastRow,
			a.Duration_SubID,
			a.Duration_Shift,
			a.Duration_MaternityLeave,
			a.Duration_ROA,
			a.Duration_Required,
			a.Duration_Worked,
			a.Duration_Worked_Cumulative,
			a.XXXXXXXXXXXX_700,
			a.SwipeSource,
			a.XXXXXXXXXXXX_800,
			a.Duration_ShiftAllowance_Evening,
			a.Duration_ShiftAllowance_Night,
			a.XXXXXXXXXXXX_900,
			a.ShiftSpan_XID,
			a.ShiftSpan_HoursDay1,
			a.ShiftSpan_HoursDay2,
			a.ShiftSpan_AwardOT,
			a.ShiftSpan_2ndDayFullOT,
			a.XXXXXXXXXXXX_1000,
			a.NetMinutes,
			a.Upload_ID
			--a.MealVoucherEligibility
	FROM tas.Tran_Timesheet a, 
		tas.System_Values b
	WHERE 	
		a.DT BETWEEN tas.fmtDateNoTime(b.DT_SwipeLastProcessed) AND b.DT_SwipeNewProcess

GO
	
/*	Debugging:

	SELECT * FROM tas.Master_EmployeeAdditional a
	WHERE a.EmpNo = 10003673

	SELECT * FROM tas.vuTran_Timesheet1_Read a
	ORDER BY a.DT DESC, a.BusinessUnit, a.EmpNo

	SELECT a.IsDriver, * FROM tas.vuTran_Timesheet1_Read a
	WHERE a.EmpNo = 10003673
	
*/

