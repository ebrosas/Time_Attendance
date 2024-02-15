	SELECT a.NetMinutes, a.BusinessUnit, a.AutoID, a.LastUpdateTime, a.LastUpdateUser, a.Processed, a.IsLastRow, a.ShiftSpan, NoPayHours, a.isRamadan, a.isMuslim, 
		a.DT, a.dtIN, a.dtOUT, a.Duration_Shift, a.Duration_Required, Duration_Worked, Duration_Worked_Cumulative, NetMinutes,
		a.CorrectionCode,  ShiftSpan, 
		Shaved_IN, Shaved_OUT, a.OTStartTime, a.OTEndTime,
	 * FROM tas.Tran_Timesheet a
	WHERE a.EmpNo = 10001766 
		AND a.DT = '26/03/2016'