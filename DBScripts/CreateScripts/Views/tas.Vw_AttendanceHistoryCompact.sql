/***************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_AttendanceHistoryCompact
*	Description: Get the employee attendance history records
*
*	Date:			Author:		Rev. #:		Comments:
*	19/08/2018		Ervin		1.0			Created
*****************************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_AttendanceHistoryCompact
AS
	
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
			
			--CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
			--	THEN DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT) 
			--	ELSE 0
			--END AS ShavedWorkDurationMinutes,
			--CASE WHEN a.Shaved_IN IS NOT NULL AND a.Shaved_OUT IS NOT NULL 
			--	THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.Shaved_IN, a.Shaved_OUT)) 
			--	ELSE ''
			--END AS ShavedWorkDurationHours,

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
			(CASE RTRIM(ISNULL(a.LeaveType, '')) WHEN 'DD' THEN 'Day In Lieu' ELSE '' END) AS Remarks,
			--CASE WHEN RTRIM(a.BusinessUnit) IN (SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WHERE IsActive = 1) AND CAST(a.GradeCode AS INT) <= 8 AND c.IsDayShift = 0 THEN 1 ELSE 0 END AS RequiredToSwipeAtWorkplace,
			--CASE WHEN ISNULL(e.timeIN, '') <> ''
			--	THEN DATEADD(MINUTE, tas.fmtHHmm_Min(e.timeIn), e.dtIN)
			--	ELSE d.TimeInMG
			--END AS TimeInMG,

			--CASE WHEN d.CorrectionType IN (1, 3)	--(Note: 1 = Workplace Time In; 3 = Both)
			--	THEN
			--		CASE WHEN d.IsCorrected = 1 AND d.IsClosed = 1 AND RTRIM(ISNULL(d.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN d.TimeInWP ELSE NULL END 
			--	ELSE
			--		d.TimeInWP
			--END AS TimeInWP,
			--CASE WHEN d.CorrectionType IN (2, 3)	--(Note: 2 = Workplace Time Out; 3 = Both)
			--	THEN
			--		CASE WHEN d.IsCorrected = 1 AND d.IsClosed = 1 AND RTRIM(ISNULL(d.StatusHandlingCode, '')) NOT IN ('Cancelled', 'Rejected') THEN d.TimeOutWP ELSE NULL END 
			--	ELSE
			--		d.TimeOutWP
			--END AS TimeOutWP,
			--CASE WHEN ISNULL(e.[timeOUT], '') <> ''
			--	THEN DATEADD(MINUTE, tas.fmtHHmm_Min(e.[timeOUT]), e.dtOUT)
			--	ELSE d.TimeOutMG
			--END AS TimeOutMG,
			--a.IsLastRow,
			--a.ShiftSpan,
			a.LastUpdateUser,
			a.LastUpdateTime					
	FROM tas.Tran_Timesheet	a WITH (NOLOCK)
		LEFT JOIN tas.GetRemark02 b WITH (NOLOCK) ON a.AutoId = b.AutoId
		LEFT JOIN tas.Master_ShiftPatternTitles c WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode) 	
		--LEFT JOIN tas.Tran_WorkplaceSwipe d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.SwipeDate
		OUTER APPLY
        (
			SELECT TOP 1 * FROM tas.Tran_ManualAttendance WITH (NOLOCK)
			WHERE EmpNo = a.EmpNo
				AND a.DT BETWEEN dtIN AND dtOUT
			ORDER BY AutoID DESC 
		) e
	WHERE a.EmpNo NOT IN  
	(
		SELECT EmpNo FROM tas.EmployeeContractorMapping  WITH (NOLOCK)
		WHERE PrimaryIDNoType = 1
	)
	
GO 


/*	Debugg:

	SELECT * FROM tas.Vw_AttendanceHistoryCompact a
	WHERE a.EmpNo = 10001905
	ORDER BY a.DT DESC
	
*/


