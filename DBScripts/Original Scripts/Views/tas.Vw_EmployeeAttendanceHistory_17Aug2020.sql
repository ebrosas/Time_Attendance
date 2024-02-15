USE [tas2]
GO

/****** Object:  View [tas].[Vw_EmployeeAttendanceHistory]    Script Date: 17/08/2020 14:21:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/***************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EmployeeAttendanceHistory
*	Description: Get the employee attendance history records
*
*	Date:			Author:		Rev. #:		Comments:
*	08/11/2016		Ervin		1.0			Created
*	17/11/2016		Ervin		1.1			Modified the code in calculating the "DayOffDuration" and "WorkDurationMinutes"
*	19/11/2016		Ervin		1.2			Added union join to "EmployeeContractorMapping" table
*	31/01/2017		Ervin		1.3			Fetch the Time IN/OUT in the workplace readers
*	15/02/2017		Ervin		1.4			Added code to display "Day In Lieu" in the Remarks field if LeaveType = 'DD'
*	22/02/2017		Ervin		1.5			Added "IsLastRow" and "ShiftSpan"
*	26/02/2017		Ervin		1.6			Display the correction in workplace Time In/Out only when approved by HR
*	28/02/2017		Ervin		1.7			Added condition that will show the value of "TimeInWP" and "TimeOutWP" fields only when correction is approved and workflow is closed
*	05/03/2017		Ervin		1.8			Added join to "Tran_ManualAttendance" table to check for manual attendance
*	17/05/2017		Ervin		1.9			Added "LastUpdateUser" and "LastUpdateTime" fields	
*	16/08/2017		Ervin		2.0			Fixed the bug in the calculation of overtime duration
*	08/07/2018		Ervin		2.1			Enabled fetching attendance records from year 2004 to 2013
*	14/11/2018		Ervin		2.2			Added condition to fetch the ShiftCode in "Tran_ShiftPatternUpdates" table if it is null in "Tran_Timesheet" table
*	06/01/2019		Ervin		2.3			Enabled fetching the data from the archive table
*	15/01/2019		Ervin		2.4			Added "IsPublicHoliday" in the result set
*	02/07/2019		Ervin		2.5			Added "RelativeTypeName" and "DeathRemarks" fields
*	20/08/2019		Ervin		2.6			Modified the logic for setting the Remarks field if it is DIL
*	12/09/2019		Ervin		2.7			Disabled fetching the attendance of Contractors because it takes too much time to load the data
*****************************************************************************************************************************************************************************************************/

ALTER VIEW [tas].[Vw_EmployeeAttendanceHistory]
AS
	
	SELECT	a.AutoID,
			a.EmpNo,
			a.BusinessUnit,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			CASE WHEN ISNULL(a.ShiftCode, '') = ''
				THEN RTRIM(f.Effective_ShiftCode)
				ELSE RTRIM(a.ShiftCode)
			END AS ShiftCode,
			a.Actual_ShiftCode,
			
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN a.Duration_Worked_Cumulative 
				ELSE 0
			END AS WorkDurationCumulative,
			CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL 
				--THEN SUM(DATEDIFF(n, a.dtIN, a.dtOUT)) OVER(ORDER BY a.AutoID ROWS UNBOUNDED PRECEDING) 
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) 
				ELSE 0
			END AS WorkDurationMinutes,
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN tas.fmtMIN_HHmm(a.Duration_Worked_Cumulative) 
				ELSE ''
			END AS WorkDurationHours,
			
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
				ELSE 0
			END AS ShavedWorkDurationMinutes,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
				ELSE ''
			END AS ShavedWorkDurationHours,

			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN 
					CASE WHEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) < 0	--Rev. #2.0
						THEN 1440 + DATEDIFF(n, a.OTStartTime, a.OTEndTime)
						ELSE DATEDIFF(n, a.OTStartTime, a.OTEndTime)
					END 
				ELSE 0
			END AS OTDurationMinutes,			
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN tas.fmtMIN_HHmm
					(
						CASE WHEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) < 0	--Rev. #2.0
							THEN 1440 + DATEDIFF(n, a.OTStartTime, a.OTEndTime)
							ELSE DATEDIFF(n, a.OTStartTime, a.OTEndTime)
						END 
					) 
				ELSE ''
			END AS OTDurationHours,
			a.NoPayHours,
			tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))) AS Duration_Required,

			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND (a.dtIN IS NULL AND a.dtOUT IS NULL) 
				THEN 
					CASE WHEN ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0 
						THEN tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'M')
						ELSE tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'D')
					END 
				ELSE 0
			END AS DayOffDuration,

			b.LVDesc + b.RMdesc + b.RAdesc + b.TxDesc + b.H_P_desc + b.H_D_desc + b.H_R_desc + b.TxtShiftSpan + b.DayOff + b.OtherRemarks +
			(
				CASE ISNULL(a.DIL_Entitlement, '')
					WHEN 'EA' THEN ' - DIL Entitled by Admin'
					WHEN 'ES' THEN ' - DIL Entitled by System'
					WHEN 'UA' THEN ' - DIL used by Admin'
					WHEN 'UD' THEN ' - DIL used by System'
					WHEN 'AD' THEN ' - DIL Approved'
					ELSE ''
				END
			) +
			(CASE WHEN RTRIM(ISNULL(a.LeaveType, '')) = 'DD' OR RTRIM(ISNULL(a.AbsenceReasonCode, '')) = 'DD' THEN 'Day In Lieu' ELSE '' END) AS Remarks,
			CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND c.IsDayShift = 0 THEN 1 ELSE 0 END AS RequiredToSwipeAtWorkplace,
			CASE WHEN ISNULL(e.timeIN, '') <> ''
				THEN DATEADD(MINUTE, tas.fmtHHmm_Min(e.timeIn), e.dtIN)
				ELSE d.TimeInMG
			END AS TimeInMG,

			CASE WHEN d.CorrectionType IN (1, 3)	--(Note: 1 = Workplace Time In; 3 = Both)
				THEN
					CASE WHEN d.IsCorrected = 1 AND d.IsClosed = 1 AND RTRIM(ISNULL(d.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN d.TimeInWP ELSE NULL END 
				ELSE
					d.TimeInWP
			END AS TimeInWP,
			CASE WHEN d.CorrectionType IN (2, 3)	--(Note: 2 = Workplace Time Out; 3 = Both)
				THEN
					CASE WHEN d.IsCorrected = 1 AND d.IsClosed = 1 AND RTRIM(ISNULL(d.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN d.TimeOutWP ELSE NULL END 
				ELSE
					d.TimeOutWP
			END AS TimeOutWP,
			CASE WHEN ISNULL(e.[timeOUT], '') <> ''
				THEN DATEADD(MINUTE, tas.fmtHHmm_Min(e.[timeOUT]), e.dtOUT)
				ELSE d.TimeOutMG
			END AS TimeOutMG,
			a.IsLastRow,
			a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.IsPublicHoliday,
			a.CorrectionCode,
			CASE 
				WHEN RTRIM(a.CorrectionCode) = 'RAD0' THEN g.OtherRelativeType
				WHEN RTRIM(a.CorrectionCode) IN ('RAD1', 'RAD2', 'RAD3', 'RAD4') THEN h.RelativeTypeName
				ELSE NULL
			END AS RelativeTypeName,	--Rev. #2.5
			g.Remarks AS DeathRemarks				--Rev. #2.5		
	FROM tas.Tran_Timesheet	a WITH (NOLOCK)
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId
		LEFT JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) 	
		LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.SwipeDate
		OUTER APPLY
        (
			SELECT TOP 1 * FROM tas.Tran_ManualAttendance WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND a.DT BETWEEN dtIN AND dtOUT
			ORDER BY AutoID DESC 
		) e
		LEFT JOIN tas.Tran_ShiftPatternUpdates f WITH (NOLOCK) ON a.EmpNo = f.EmpNo AND a.DT = f.DateX	--Rev. #2.2
		LEFT JOIN tas.DeathReasonOfAbsence g WITH (NOLOCK) ON a.EmpNo = g.EmpNo AND a.DT = g.DT AND RTRIM(a.BusinessUnit) = RTRIM(g.CostCenter) AND RTRIM(a.CorrectionCode) = RTRIM(g.CorrectionCode)
		LEFT JOIN tas.FamilyRelativeSetting h WITH (NOLOCK) ON RTRIM(g.RelativeTypeCode) = RTRIM(h.RelativeTypeCode)
	WHERE a.EmpNo NOT IN  
	(
		SELECT EmpNo FROM tas.EmployeeContractorMapping  WITH (NOLOCK)
		WHERE PrimaryIDNoType = 1
	)

	/*	Note: Disabled the attendance of Contractors because it takes too much time to load the data
	UNION

	--Rev. #1.2 - Get attendance record of the Contractor using the mapping configuration
	SELECT	a.AutoID,
			c.EmpNo,
			RTRIM(c.CostCenter) AS BusinessUnit,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN a.Duration_Worked_Cumulative 
				ELSE 0
			END AS WorkDurationCumulative,
			CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL 
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) 
				ELSE 0
			END AS WorkDurationMinutes,

			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN tas.fmtMIN_HHmm(a.Duration_Worked_Cumulative) 
				ELSE ''
			END AS WorkDurationHours,

			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
				ELSE 0
			END AS ShavedWorkDurationMinutes,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
				ELSE ''
			END AS ShavedWorkDurationHours,

			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN 
					CASE WHEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) < 0	--Rev. #2.0
						THEN 1440 + DATEDIFF(n, a.OTStartTime, a.OTEndTime)
						ELSE DATEDIFF(n, a.OTStartTime, a.OTEndTime)
					END 
				ELSE 0
			END AS OTDurationMinutes,
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN tas.fmtMIN_HHmm
					(
						CASE WHEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) < 0	--Rev. #2.0
							THEN 1440 + DATEDIFF(n, a.OTStartTime, a.OTEndTime)
							ELSE DATEDIFF(n, a.OTStartTime, a.OTEndTime)
						END 
					) 
				ELSE ''
			END AS OTDurationHours,
			a.NoPayHours,
			tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))) AS Duration_Required,

			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND (a.dtIN IS NULL AND a.dtOUT IS NULL) 
				THEN 
					CASE WHEN ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0 
						THEN tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'M')
						ELSE tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'D')
					END 
				ELSE 0
			END AS DayOffDuration,

			b.LVDesc + b.RMdesc + b.RAdesc + b.TxDesc + b.H_P_desc + b.H_D_desc + b.H_R_desc + b.TxtShiftSpan + b.DayOff + b.OtherRemarks +
			(
				CASE ISNULL(a.DIL_Entitlement, '')
					WHEN 'EA' THEN ' - DIL Entitled by Admin'
					WHEN 'ES' THEN ' - DIL Entitled by System'
					WHEN 'UA' THEN ' - DIL used by Admin'
					WHEN 'UD' THEN ' - DIL used by System'
					WHEN 'AD' THEN ' - DIL Approved'
					ELSE ''
				END
			) +
			(CASE WHEN RTRIM(ISNULL(a.LeaveType, '')) = 'DD' OR RTRIM(ISNULL(a.AbsenceReasonCode, '')) = 'DD' THEN 'Day In Lieu' ELSE '' END) AS Remarks,
			0 AS RequiredToSwipeAtWorkplace,
			NULL AS TimeInMG,
			NULL AS TimeInWP,
			NULL AS TimeOutWP,
			NULL AS TimeOutMG,
			a.IsLastRow,
			a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.IsPublicHoliday,
			a.CorrectionCode,
			NULL AS RelativeTypeName,
			NULL AS DeathRemarks				
	FROM tas.EmployeeContractorMapping c WITH (NOLOCK)
		INNER JOIN tas.Tran_Timesheet a WITH (NOLOCK) ON c.ContractorNo = a.EmpNo
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId		
	WHERE c.PrimaryIDNoType = 1
	*/
	
	/*	Note: Uncomment this block of code to fetch attendance records from the Timesheet archive table from year 2013 and below
	UNION

	SELECT	a.AutoID,
			a.EmpNo,
			a.BusinessUnit,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN a.Duration_Worked_Cumulative 
				ELSE 0
			END AS WorkDurationCumulative,
			CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL 
				--THEN SUM(DATEDIFF(n, a.dtIN, a.dtOUT)) OVER(ORDER BY a.AutoID ROWS UNBOUNDED PRECEDING) 
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) 
				ELSE 0
			END AS WorkDurationMinutes,
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN tas.fmtMIN_HHmm(a.Duration_Worked_Cumulative) 
				ELSE ''
			END AS WorkDurationHours,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
				ELSE 0
			END AS ShavedWorkDurationMinutes,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
				ELSE ''
			END AS ShavedWorkDurationHours,

			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
				ELSE 0
			END AS OTDurationMinutes,
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
				ELSE ''
			END AS OTDurationHours,
			a.NoPayHours,
			tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))) AS Duration_Required,

			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND (a.dtIN IS NULL AND a.dtOUT IS NULL) 
				THEN 
					CASE WHEN ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0 
						THEN tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'M')
						ELSE tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'D')
					END 
				ELSE 0
			END AS DayOffDuration,

			b.LVDesc + b.RMdesc + b.RAdesc + b.TxDesc + b.H_P_desc + b.H_D_desc + b.H_R_desc + b.TxtShiftSpan + b.DayOff + b.OtherRemarks +
			(
				CASE ISNULL(a.DIL_Entitlement, '')
					WHEN 'EA' THEN ' - DIL Entitled by Admin'
					WHEN 'ES' THEN ' - DIL Entitled by System'
					WHEN 'UA' THEN ' - DIL used by Admin'
					WHEN 'UD' THEN ' - DIL used by System'
					WHEN 'AD' THEN ' - DIL Approved'
					ELSE ''
				END
			) AS Remarks,
			CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND c.IsDayShift = 0 THEN 1 ELSE 0 END AS RequiredToSwipeAtWorkplace,
			d.TimeInMG,
			d.TimeInWP,
			d.TimeOutWP,
			d.TimeOutMG,
			a.IsLastRow,
			a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.IsPublicHoliday,
			a.CorrectionCode,
			NULL AS RelativeTypeName,
			NULL AS DeathRemarks													
	FROM tas.sy_TimesheetArchive a WITH (NOLOCK)
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId
		LEFT JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) 	
		LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.SwipeDate
	*/

	/*	Note: Uncomment this block of code to fetch attendance records from the Timesheet archive table for year 2011
	UNION

	SELECT	a.AutoID,
			a.EmpNo,
			a.BusinessUnit,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN a.Duration_Worked_Cumulative 
				ELSE 0
			END AS WorkDurationCumulative,
			CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL 
				--THEN SUM(DATEDIFF(n, a.dtIN, a.dtOUT)) OVER(ORDER BY a.AutoID ROWS UNBOUNDED PRECEDING) 
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) 
				ELSE 0
			END AS WorkDurationMinutes,

			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN tas.fmtMIN_HHmm(a.Duration_Worked_Cumulative) 
				ELSE ''
			END AS WorkDurationHours,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
				ELSE 0
			END AS ShavedWorkDurationMinutes,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
				ELSE ''
			END AS ShavedWorkDurationHours,

			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
				ELSE 0
			END AS OTDurationMinutes,
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
				ELSE ''
			END AS OTDurationHours,
			a.NoPayHours,
			tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))) AS Duration_Required,

			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND (a.dtIN IS NULL AND a.dtOUT IS NULL) 
				THEN 
					CASE WHEN ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0 
						THEN tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'M')
						ELSE tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'D')
					END 
				ELSE 0
			END AS DayOffDuration,

			b.LVDesc + b.RMdesc + b.RAdesc + b.TxDesc + b.H_P_desc + b.H_D_desc + b.H_R_desc + b.TxtShiftSpan + b.DayOff + b.OtherRemarks +
			(
				CASE ISNULL(a.DIL_Entitlement, '')
					WHEN 'EA' THEN ' - DIL Entitled by Admin'
					WHEN 'ES' THEN ' - DIL Entitled by System'
					WHEN 'UA' THEN ' - DIL used by Admin'
					WHEN 'UD' THEN ' - DIL used by System'
					WHEN 'AD' THEN ' - DIL Approved'
					ELSE ''
				END
			) AS Remarks,
			CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND c.IsDayShift = 0 THEN 1 ELSE 0 END AS RequiredToSwipeAtWorkplace,
			d.TimeInMG,
			d.TimeInWP,
			d.TimeOutWP,
			d.TimeOutMG,
			a.IsLastRow,
			a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.IsPublicHoliday,
			a.CorrectionCode,
			NULL AS RelativeTypeName,
			NULL AS DeathRemarks													
	FROM Archive.dbo.tas2_Tran_Timesheet_2011 a WITH (NOLOCK)
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId
		LEFT JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) 	
		LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.SwipeDate
	*/

	/*	Note: Uncomment this block of code to fetch attendance records from the Timesheet archive table for year 2012
	UNION

	SELECT	a.AutoID,
			a.EmpNo,
			a.BusinessUnit,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN a.Duration_Worked_Cumulative 
				ELSE 0
			END AS WorkDurationCumulative,
			CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL 
				--THEN SUM(DATEDIFF(n, a.dtIN, a.dtOUT)) OVER(ORDER BY a.AutoID ROWS UNBOUNDED PRECEDING) 
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) 
				ELSE 0
			END AS WorkDurationMinutes,

			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN tas.fmtMIN_HHmm(a.Duration_Worked_Cumulative) 
				ELSE ''
			END AS WorkDurationHours,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
				ELSE 0
			END AS ShavedWorkDurationMinutes,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
				ELSE ''
			END AS ShavedWorkDurationHours,

			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
				ELSE 0
			END AS OTDurationMinutes,
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
				ELSE ''
			END AS OTDurationHours,
			a.NoPayHours,
			tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))) AS Duration_Required,

			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND (a.dtIN IS NULL AND a.dtOUT IS NULL) 
				THEN 
					CASE WHEN ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0 
						THEN tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'M')
						ELSE tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'D')
					END 
				ELSE 0
			END AS DayOffDuration,

			b.LVDesc + b.RMdesc + b.RAdesc + b.TxDesc + b.H_P_desc + b.H_D_desc + b.H_R_desc + b.TxtShiftSpan + b.DayOff + b.OtherRemarks +
			(
				CASE ISNULL(a.DIL_Entitlement, '')
					WHEN 'EA' THEN ' - DIL Entitled by Admin'
					WHEN 'ES' THEN ' - DIL Entitled by System'
					WHEN 'UA' THEN ' - DIL used by Admin'
					WHEN 'UD' THEN ' - DIL used by System'
					WHEN 'AD' THEN ' - DIL Approved'
					ELSE ''
				END
			) AS Remarks,
			CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND c.IsDayShift = 0 THEN 1 ELSE 0 END AS RequiredToSwipeAtWorkplace,
			d.TimeInMG,
			d.TimeInWP,
			d.TimeOutWP,
			d.TimeOutMG,
			a.IsLastRow,
			a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.IsPublicHoliday,
			a.CorrectionCode,
			NULL AS RelativeTypeName,
			NULL AS DeathRemarks													
	FROM Archive.dbo.tas2_Tran_Timesheet_2012 a WITH (NOLOCK)
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId
		LEFT JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) 	
		LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.SwipeDate
	*/

	/*	Note: Uncomment this block of code to fetch attendance records from the Timesheet archive table for year 2013
	UNION

	SELECT	a.AutoID,
			a.EmpNo,
			a.BusinessUnit,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			
			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN a.Duration_Worked_Cumulative 
				ELSE 0
			END AS WorkDurationCumulative,
			CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL 
				--THEN SUM(DATEDIFF(n, a.dtIN, a.dtOUT)) OVER(ORDER BY a.AutoID ROWS UNBOUNDED PRECEDING) 
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) 
				ELSE 0
			END AS WorkDurationMinutes,

			CASE WHEN ISNULL(a.Duration_Worked_Cumulative, 0) > 0
				THEN tas.fmtMIN_HHmm(a.Duration_Worked_Cumulative) 
				ELSE ''
			END AS WorkDurationHours,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
				ELSE 0
			END AS ShavedWorkDurationMinutes,
			CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
				ELSE ''
			END AS ShavedWorkDurationHours,

			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime) 
				ELSE 0
			END AS OTDurationMinutes,
			CASE WHEN a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
				ELSE ''
			END AS OTDurationHours,
			a.NoPayHours,
			tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))) AS Duration_Required,

			CASE WHEN RTRIM(a.ShiftCode) = 'O' AND (a.dtIN IS NULL AND a.dtOUT IS NULL) 
				THEN 
					CASE WHEN ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0 
						THEN tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'M')
						ELSE tas.fnGetRequiredWorkDuration(RTRIM(a.ShiftPatCode), 'D')
					END 
				ELSE 0
			END AS DayOffDuration,

			b.LVDesc + b.RMdesc + b.RAdesc + b.TxDesc + b.H_P_desc + b.H_D_desc + b.H_R_desc + b.TxtShiftSpan + b.DayOff + b.OtherRemarks +
			(
				CASE ISNULL(a.DIL_Entitlement, '')
					WHEN 'EA' THEN ' - DIL Entitled by Admin'
					WHEN 'ES' THEN ' - DIL Entitled by System'
					WHEN 'UA' THEN ' - DIL used by Admin'
					WHEN 'UD' THEN ' - DIL used by System'
					WHEN 'AD' THEN ' - DIL Approved'
					ELSE ''
				END
			) AS Remarks,
			CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND c.IsDayShift = 0 THEN 1 ELSE 0 END AS RequiredToSwipeAtWorkplace,
			d.TimeInMG,
			d.TimeInWP,
			d.TimeOutWP,
			d.TimeOutMG,
			a.IsLastRow,
			a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.IsPublicHoliday,
			a.CorrectionCode,
			NULL AS RelativeTypeName,
			NULL AS DeathRemarks													
	FROM Archive.dbo.tas2_Tran_Timesheet_2013 a WITH (NOLOCK)
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId
		LEFT JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) 	
		LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.SwipeDate
	*/
GO


