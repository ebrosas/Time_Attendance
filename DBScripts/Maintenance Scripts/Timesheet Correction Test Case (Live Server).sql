	--With approved overtime
	SELECT TOP 100
		a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime, a.CorrectionCode, a.GradeCode, a.DIL_Entitlement,
		b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE 
		YEAR(a.DT) = 2016
		AND a.OTStartTime IS NOT NULL
		AND a.OTEndTime IS NOT NULL
	ORDER BY a.DT DESC 

	--With OT pending for approval
	SELECT a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime,
		b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE a.DT BETWEEN  '12/16/2016' AND '01/15/2017'
		AND a.OTStartTime IS NULL
		AND a.OTEndTime IS NULL
	ORDER BY a.DT DESC 

	--Without OT
	SELECT a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime, a.NoPayHours,
		b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE 
		a.DT BETWEEN  '12/16/2016' AND '01/15/2017'
		AND a.OTStartTime IS NULL
		AND a.OTEndTime IS NULL
		AND (b.OTstartTime IS NULL AND b.OTendTime IS NULL)
	ORDER BY a.DT DESC 

	--With NPH
	SELECT a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime, a.NoPayHours,
		b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE 
		a.DT BETWEEN  '12/16/2016' AND '01/15/2017'
		AND a.OTStartTime IS NULL
		AND a.OTEndTime IS NULL
		AND a.NoPayHours > 0
	ORDER BY a.DT DESC 

	--With NPH, No Correction Code
	SELECT TOP 100 a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime, a.NoPayHours, a.CorrectionCode,
		b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE 
		a.DT BETWEEN  '12/16/2016' AND '01/15/2017'
		AND a.NoPayHours > 0
		AND ISNULL(a.CorrectionCode, '') = ''
	ORDER BY a.DT DESC 

	--With Absent
	SELECT a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime, a.NoPayHours,
		a.*
	FROM tas.Tran_Timesheet a
	WHERE a.DT BETWEEN  '12/16/2016' AND '01/15/2017'
		AND a.RemarkCode = 'A'
	ORDER BY a.DT DESC 

	--Zero NPH
	SELECT a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.OTStartTime, a.OTEndTime, a.NoPayHours,
		b.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE 
		a.DT BETWEEN  '12/16/2016' AND '01/15/2017'
		AND ISNULL(a.NoPayHours, 0) = 0
		AND RTRIM(a.RemarkCode) <> 'A'
		AND ISNULL(a.LeaveType, '') = ''
	ORDER BY a.DT DESC 

	--For "Add Sh Allw Evening-Chng Shift"
	SELECT TOP 1000
		a.CorrectionCode, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.ShiftAllowance, a.Duration_ShiftAllowance_Evening, a.Duration_ShiftAllowance_Night,
		a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.ShiftAllowance = 0
		AND a.Duration_ShiftAllowance_Evening = 0
	ORDER BY a.DT DESC 

	--For "Add Sh Allw Night-Chng Shift"
	SELECT TOP 1000
		a.CorrectionCode, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.ShiftAllowance, a.Duration_ShiftAllowance_Evening, a.Duration_ShiftAllowance_Night,
		a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.ShiftAllowance = 0
		AND a.Duration_ShiftAllowance_Night = 0
	ORDER BY a.DT DESC 

	--For "Remove Shift Allow-evening shf"
	SELECT TOP 100
		a.CorrectionCode, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.ShiftAllowance, a.Duration_ShiftAllowance_Evening, a.Duration_ShiftAllowance_Night,
		a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.ShiftAllowance = 0
		AND a.Duration_ShiftAllowance_Evening > 0
	ORDER BY a.DT DESC 

	--For "Remove Shift Allow-night shift"
	SELECT TOP 100
		a.CorrectionCode, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.ShiftAllowance, a.Duration_ShiftAllowance_Evening, a.Duration_ShiftAllowance_Night,
		a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.ShiftAllowance = 1
		AND a.Duration_ShiftAllowance_Night > 0
	ORDER BY a.DT DESC 

	--For "Remove Shift Allo-not entitled"
	SELECT TOP 100
		a.CorrectionCode, a.EmpNo, a.DT, a.dtIN, a.dtOUT, a.ShiftAllowance, a.Duration_ShiftAllowance_Evening, a.Duration_ShiftAllowance_Night,
		a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.ShiftAllowance = 1
		AND 
		(
			a.Duration_ShiftAllowance_Night > 0
			OR a.Duration_ShiftAllowance_Evening > 0
		)
	ORDER BY a.DT DESC 

	--For "Mark Absent Leave Cancelled", "Mark Absent-Change Shift", "Mark Absent-Disciplinary Action", "Mark Absent During Gen. Strike"
	--Applicable
	SELECT TOP 100
		a.CorrectionCode, a.RemarkCode, a.ShiftCode, a.dtIN, a.dtOUT, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.EmpNo > 10000000
		AND (a.dtIN IS NOT NULL	AND a.dtOUT IS NOT NULL)
		AND ISNULL(a.RemarkCode, '') <> 'A'
		AND ISNULL(a.ShiftCode, '') <> 'O'
	ORDER BY a.DT DESC 

	--Not Applicable
	SELECT a.CorrectionCode, a.ShiftCode, a.GradeCode, a.IsSalStaff,
	 * FROM tas.Tran_Timesheet a
	WHERE 
		YEAR(a.DT) = 2016
		AND a.EmpNo > 10000000
		AND a.IsLastRow = 1
		AND a.ShiftCode = 'O'
	ORDER BY a.DT DESC 

	SELECT a.CorrectionCode, a.RemarkCode, a.GradeCode, a.IsSalStaff,
	 * FROM tas.Tran_Timesheet a
	WHERE 
		YEAR(a.DT) = 2016
		AND a.EmpNo > 10000000
		AND a.IsLastRow = 1
		AND a.RemarkCode = 'A'
	ORDER BY a.DT DESC 

	SELECT a.CorrectionCode, a.LeaveType, a.GradeCode, a.IsSalStaff,
	* FROM tas.Tran_Timesheet a
	WHERE 
		YEAR(a.DT) = 2016
		AND a.EmpNo > 10000000
		AND a.IsLastRow = 1
		AND ISNULL(a.LeaveType, '') <> '' 
	ORDER BY a.DT DESC 

	--Absent removal correction codes
	SELECT TOP 100
		a.CorrectionCode, a.RemarkCode, a.GradeCode, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND RTRIM(a.RemarkCode) = 'A'
	ORDER BY a.DT DESC 

	--For "Leave Cancelled" correction codes
	SELECT TOP 100
		a.CorrectionCode, a.LeaveType, a.GradeCode, a.DIL_Entitlement, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.LeaveType, '') <> ''
	ORDER BY a.DT DESC 

	--For "Mark DIL-Entitled by Admin" correction codes
	--Applicable
	SELECT TOP 100
		a.CorrectionCode, a.LeaveType, a.DIL_Entitlement, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.DIL_Entitlement, '') = ''
		AND a.GradeCode >= 9
		AND a.IsSalStaff = 1
	ORDER BY a.DT DESC 

	--Not Applicable
	SELECT TOP 100
		a.CorrectionCode, a.LeaveType, a.DIL_Entitlement, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.DIL_Entitlement, '') <> ''
		AND a.GradeCode >= 9
		AND a.IsSalStaff = 1
	ORDER BY a.DT DESC 

	--For "Remove DIL-Entitled by Admin" correction codes
	--Applicable
	SELECT TOP 100
		a.CorrectionCode, a.LeaveType, a.DIL_Entitlement, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.DIL_Entitlement, '') != ''
		AND a.GradeCode >= 9
		AND a.IsSalStaff = 1
	ORDER BY a.DT DESC 

	--Not Applicable
	SELECT TOP 100
		a.CorrectionCode, a.LeaveType, a.DIL_Entitlement, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.DIL_Entitlement, '') = ''
		AND a.GradeCode >= 9
		AND a.IsSalStaff = 1
	ORDER BY a.DT DESC 

	--For "Add Meal Voucher" correction codes
	--Applicable
	SELECT TOP 100
		a.CorrectionCode, a.MealVoucherEligibility, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.MealVoucherEligibility, '') != 'YA'
		AND a.GradeCode <= 9
		AND a.IsSalStaff = 0
	ORDER BY a.DT DESC 

	--Not Applicable
	SELECT TOP 100
		a.CorrectionCode, a.MealVoucherEligibility, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.MealVoucherEligibility, '') = 'YA'
	ORDER BY a.DT DESC 

	--For "Mark Off Change Shift" correction codes
	--Applicable
	SELECT TOP 1000
		a.CorrectionCode, a.ShiftCode, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.ShiftCode, '') <> 'O'
	ORDER BY a.DT DESC 

	--Not Applicable
	SELECT TOP 100
		a.CorrectionCode, a.ShiftCode, a.GradeCode, a.IsSalStaff, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		a.IsLastRow = 1
		AND a.EmpNo > 10000000
		AND ISNULL(a.ShiftCode, '') = 'O'
	ORDER BY a.DT DESC 

	--For "Local Seminar/Exhibition"
	--Applicable
	SELECT TOP 100
		a.CorrectionCode, a.RemarkCode, a.ShiftCode, a.dtIN, a.dtOUT, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.EmpNo > 10000000
		AND a.IsLastRow = 1
		AND 
		(
			ISNULL(a.RemarkCode, '') = 'A'
			OR ISNULL(a.ShiftCode, '') = 'O'
		)
	ORDER BY a.DT DESC 

	--For "Add Extra Pay-Adj last month"
	--Applicable
	SELECT TOP 1000
		a.GradeCode, a.IsSalStaff, a.CorrectionCode, a.RemarkCode, a.ShiftCode, a.dtIN, a.dtOUT, a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE 
		YEAR(a.DT) = 2016
		AND a.EmpNo > 10000000
		AND (a.dtIN IS NOT NULL	AND a.dtOUT IS NOT NULL)
		AND 
		(
			ISNULL(a.RemarkCode, '') = 'A'
			OR ISNULL(a.ShiftCode, '') = 'O'
		)
	ORDER BY a.DT DESC 

/*	Checking:

	SELECT a.CorrectionCode, a.RemarkCode, a.AbsenceReasonCode, a.AbsenceReasonColumn, a.LeaveType,
		a.* 
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
	WHERE a.IsLastRow = 1 
		AND a.EmpNo = 10008049
		AND a.DT = '29/03/2016'
	ORDER BY a.DT DESC 

*/
