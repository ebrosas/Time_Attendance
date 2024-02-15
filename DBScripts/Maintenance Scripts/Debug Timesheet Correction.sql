	SELECT	TOP 1000
			a.IsLastRow,			
			a.AutoID,
			a.EmpNo,
			a.GradeCode,
			a.IsSalStaff,
			a.DT,
			a.dtIN,
			a.dtOUT,
			a.CorrectionCode,
			a.NoPayHours,
			a.AbsenceReasonCode,
			a.AbsenceReasonColumn,
			a.LeaveType,
			a.RemarkCode,
			a.DIL_Entitlement,
			a.MealVoucherEligibility,
			a.OTStartTime,
			a.OTEndTime,
			a.OTType,
			a.ShiftAllowance,
			a.Duration_ShiftAllowance_Evening,
			a.Duration_ShiftAllowance_Night,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.* 
	FROM tas.Tran_Timesheet a
	WHERE 
		--a.IsLastRow = 0 AND 
		RTRIM(a.CorrectionCode) = 'RAAD'		
	ORDER BY a.DT DESC	

	--Extra Pay for all
	SELECT a.IsPublicHoliday, a.ShiftCode, a.IsDayWorker_OR_Shifter, a.IsLastRow, a.IsEmployee_OR_Contractor, a.IsSalStaff, a.IsDILdayWorker,
		a.CorrectionCode, a.RemarkCode, a.AbsenceReasonCode, a.AbsenceReasonColumn, a.LeaveType,
		a.* 
	FROM tas.Tran_Timesheet a
	WHERE 	
		a.IsLastRow = 1	
		AND RTRIM(a.CorrectionCode) = 'RAAD'		
	ORDER BY a.DT DESC	

	--For Extra Pay (Shift Worker)
	SELECT a.IsPublicHoliday, a.ShiftCode, a.IsDayWorker_OR_Shifter, a.IsLastRow, a.IsEmployee_OR_Contractor, a.IsSalStaff,  a.IsDILdayWorker,
		a.CorrectionCode, a.RemarkCode, a.AbsenceReasonCode, a.AbsenceReasonColumn, a.LeaveType,
		a.* 
	FROM tas.Tran_Timesheet a
	WHERE 	
		a.IsLastRow = 1	
		AND RTRIM(a.CorrectionCode) = 'RAAD'		
		AND a.IsPublicHoliday = 1 
		AND RTRIM(a.ShiftCode) = 'O' 
		AND a.IsDayWorker_OR_Shifter = 0
		AND a.IsEmployee_OR_Contractor = 1
	ORDER BY a.DT DESC	

	--For Extra Pay (Day Worker, Non-salary Staff)
	SELECT a.IsPublicHoliday, a.ShiftCode, a.IsDayWorker_OR_Shifter, a.IsLastRow, a.IsEmployee_OR_Contractor, a.IsSalStaff, a.IsDILdayWorker,
		a.CorrectionCode, a.RemarkCode, a.AbsenceReasonCode, a.AbsenceReasonColumn, a.LeaveType,
		a.* 
	FROM tas.Tran_Timesheet a
	WHERE 	
		a.IsLastRow = 1	
		AND RTRIM(a.CorrectionCode) = 'RAAD'		
		AND a.IsDILdayWorker=1
		AND RTRIM(a.ShiftCode) = 'O' 
		AND a.IsDayWorker_OR_Shifter = 1
		AND a.IsEmployee_OR_Contractor = 1
		AND a.IsSalStaff = 0
	ORDER BY a.DT DESC	