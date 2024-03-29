USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_AutoTimesheetCorrection]    Script Date: 07/09/2021 08:39:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***********************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_AutoTimesheetCorrection
*	Description: This stored procedure is used to perform various background jobs to rectify Timesheet records 
*
*	Date			Author		Revision No.	Comments:
*	11/10/2017		Ervin		1.0				Created
*	31/10/2017		Ervin		1.1				Refactored the filter condition that checks if date is between @startDate and @endDate
*	20/11/2017		Ervin		1.2				Added new process that will correct overtime records wherein full shift overtime is given by the system though the employee is not on day-off or date is not a public holiday
*	18/01/2018		Ervin		1.3				Added new process to calculate the No-pay-hour for employees who worked double shift but did not complete the required work duration in the first shift
*	25/01/2018		Ervin		1.4				Refactored the filter condition in processing No-pay-hours
*	25/02/2018		Ervin		1.5				Added new process to set the value of "IsDriver" field to 1 for all employees that belong to the ff job catalogs: Child Care Period, Medical Condition, Excused - Job Requirement
*	19/03/2018		Ervin		1.6				Set the @startDate and @endDate filter sto fall between "EffectiveDate" and "EndingDate" fields
*	04/04/2018		Ervin		1.7				Refactored the logic in filtering by date range for the recovery of reason on absences
*	04/06/2018		Ervin		1.8				Added logic to automate the creation of new Shift Pattern Change record for each temporary shift pattern changes that have due date
*	19/11/2018		Ervin		1.9				Added logic to auto correct the NoPayHours for attendance records where ShiftSpan = 1 but did not complete the required work duration
*	19/11/2018		Ervin		2.0				Remove the Leave Type in the attendance record if the employee's scheduled shift is Day-off	(Rev. #2.0)
*	05/05/2019		Ervin		2.1				Modified the logic in setting the Leave Type during day-off
*	14/05/2019		Ervin		2.2				Implemented the removal of ROA and marking the employee as absent or has NPH 
*	23/09/2019		Ervin		2.3				Remove Absences if there are ROA created for the day
*	09/12/2019		Ervin		2.4				Automate the process of granting DIL to employees during special holiday 
*	16/01/2020		Ervin		2.5				Added code to check and remove the absent for employees who have ROA entry in TAS and was marked absent for the day
*	04/03/2020		Ervin		2.6				Mark absent to all employees who are on half-day leave but did not come to work
*	21/12/2020		Ervin		2.7				Modified "Recover Reason of Absences that have been removed when an employee comes to work", commented filter condition CorrectionCode = NULL and dtIN IS NOT NULL
*	02/02/2021		Ervin		2.8				Commented codes that updated the overtime and NPH duration as solution to the bugs reported by HR through Helpdesk No. 122294
*************************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_AutoTimesheetCorrection]
(	
	@actionType				TINYINT,		--(Note: 1 = Perform Timesheet data corrections)
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenter				VARCHAR(12) = NULL,
	@empNo					INT = 0
)
AS	

	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected_TS		INT			

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected_TS		= 0

	--Start a transaction
	--BEGIN TRAN T1

	BEGIN TRY

		--Validate parameters
		IF ISNULL(@startDate, '') = '' OR CONVERT(DATETIME, '') = @startDate
			SET @startDate = NULL

		IF ISNULL(@endDate, '') = '' OR CONVERT(DATETIME, '') = @endDate
			SET @endDate = NULL

		IF ISNULL(@costCenter, '') = '' OR ISNULL(@costCenter, '') = '0'
			SET @costCenter = NULL

		IF ISNULL(@empNo, 0) = 0
			SET @empNo = NULL

		IF @actionType = 1		--Perform series of database jobs to modify Timesheet records
		BEGIN
		
			/***********************************************************************************************************************
				Recover Reason of Absences that have been removed when an employee comes to work
			***********************************************************************************************************************/
			IF EXISTS
			(
				SELECT a.AutoID
				FROM tas.Tran_Absence a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate AND b.IsLastRow = 1
				WHERE 
					(	--Rev. #1.7
						(a.EffectiveDate >= @startDate AND a.EndingDate <= @endDate)
						OR 
						(@startDate BETWEEN a.EffectiveDate AND a.EndingDate AND @endDate BETWEEN a.EffectiveDate AND a.EndingDate)
					)	
					AND b.DT BETWEEN @startDate AND @endDate
					AND (b.dtIN IS NOT NULL)
					AND ISNULL(b.IsPublicHoliday, 0) = 0
					--AND ISNULL(b.CorrectionCode, '') = ''
					AND 
					(
						ISNULL(b.AbsenceReasonColumn, '') = '' 
						OR RTRIM(b.AbsenceReasonColumn) = 'Day Off'
					)
					AND 
					(
						ISNULL(b.AbsenceReasonCode, '') = ''
						OR ISNULL(b.LeaveType, '') = ''
					)
					AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
			)
            BEGIN
			
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.AbsenceReasonColumn = CASE WHEN RTRIM(b.AbsenceReasonColumn) = 'Day Off' THEN b.AbsenceReasonColumn ELSE 'ROA' END,
					tas.Tran_Timesheet.AbsenceReasonCode = RTRIM(a.AbsenceReasonCode),
					tas.Tran_Timesheet.LeaveType = RTRIM(a.AbsenceReasonCode),
					tas.Tran_Timesheet.NoPayHours = 0,
					tas.Tran_Timesheet.RemarkCode = CASE WHEN RTRIM(b.RemarkCode) = 'A' THEN NULL ELSE b.RemarkCode END,	--Rev. #2.3
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM tas.Tran_Absence a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate AND b.IsLastRow = 1
				WHERE 
					(	--Rev. #1.7
						(a.EffectiveDate >= @startDate AND a.EndingDate <= @endDate)
						OR 
						(@startDate BETWEEN a.EffectiveDate AND a.EndingDate AND @endDate BETWEEN a.EffectiveDate AND a.EndingDate)
					)	
					AND b.DT BETWEEN @startDate AND @endDate
					AND (b.dtIN IS NOT NULL)
					AND ISNULL(b.IsPublicHoliday, 0) = 0
					--AND ISNULL(b.CorrectionCode, '') = ''
					AND 
					(
						ISNULL(b.AbsenceReasonColumn, '') = '' 
						OR RTRIM(b.AbsenceReasonColumn) = 'Day Off'
					)
					AND 
					(
						ISNULL(b.AbsenceReasonCode, '') = ''
						OR ISNULL(b.LeaveType, '') = ''
					)
					AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)

				--Get the number of affected records in the "Tran_Timesheet" table
				SELECT @rowsAffected_TS = @@rowcount

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
            END 

			/**********************************************************************************************************************************************************************************
				Auto correct overtime records wherein the system grants full shift overtime though the employee is not on day-off and date is not a public or in-lieu holidays	(Rev. #1.2)
			***********************************************************************************************************************************************************************************/
			/* Code below commented on 02-Feb-2021 as per Helpdesk No. 122294 (Rev. #2.8)
			IF EXISTS
            (
				SELECT a.AutoID
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
					CROSS APPLY tas.fnGetProcessedTimesheetData(a.AutoID, b.OTtype, a.dtIN, a.dtOUT) c
					LEFT JOIN tas.OvertimeRequest d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.AutoID = d.TS_AutoID AND RTRIM(d.StatusHandlingCode) NOT IN ('Cancelled', 'Rejected')
				WHERE 
					a.DT BETWEEN @startDate AND @endDate
					AND a.IsLastRow = 1
					AND (b.OTstartTime IS NOT NULL AND b.OTendTime IS NOT NULL)
					AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
					AND a.ShiftCode <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.IsDILdayWorker, 0) = 0
					AND tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 0
					AND 
					(
						a.Shaved_IN = b.OTstartTime
						AND a.Shaved_OUT = b.OTendTime
					)
					AND ISNULL(d.OTRequestNo, 0) = 0
			)
			BEGIN

				UPDATE tas.Tran_Timesheet_Extra
				SET tas.Tran_Timesheet_Extra.OTstartTime = c.OTStartTime,
					tas.Tran_Timesheet_Extra.OTendTime = c.OTEndTime,
					tas.Tran_Timesheet_Extra.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet_Extra b WITH (NOLOCK) ON a.AutoID = b.XID_AutoID	
					CROSS APPLY tas.fnGetProcessedTimesheetData(a.AutoID, b.OTtype, a.dtIN, a.dtOUT) c
					LEFT JOIN tas.OvertimeRequest d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.AutoID = d.TS_AutoID AND RTRIM(d.StatusHandlingCode) NOT IN ('Cancelled', 'Rejected')
				WHERE 
					a.DT BETWEEN @startDate AND @endDate
					AND a.IsLastRow = 1
					AND (b.OTstartTime IS NOT NULL AND b.OTendTime IS NOT NULL)
					AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
					AND a.ShiftCode <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.IsDILdayWorker, 0) = 0
					AND tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 0
					AND 
					(
						a.Shaved_IN = b.OTstartTime
						AND a.Shaved_OUT = b.OTendTime
					)
					AND ISNULL(d.OTRequestNo, 0) = 0

				IF ISNULL(@rowsAffected_TS, 0) = 0
				BEGIN
                
					--Get the number of affected records in the "Tran_Timesheet" table
					SELECT @rowsAffected_TS = @@rowcount
				END 

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
            END 
			*/

			/**********************************************************************************************************************************************************************************
				Calculate No-Pay-Hour for employees who worked double shift but did not complete the required work duration in the first shift (Rev. #1.3)
			***********************************************************************************************************************************************************************************/
			/* Code below commented on 02-Feb-2021 as per Helpdesk No. 122294 (Rev. #2.8)
			IF EXISTS
			(
				SELECT a.AutoID 
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Master_Employee_JDE b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL
				WHERE 
					a.ShiftSpan = 1 
					AND a.IsLastRow = 1		
					AND (a.Duration_Required > 0  AND a.Duration_Worked_Cumulative < a.Duration_Required)
					AND ISNULL(a.NoPayHours, 0) = 0
					AND ISNULL(a.CorrectionCode, '') = ''
					AND ISNULL(a.LeaveType, '') = ''
					AND ISNULL(a.ShiftCode, '') <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.AbsenceReasonCode, '') = ''
					AND NOT
					(
						a.IsSalStaff = 1 
						OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)
						OR (a.IsSalStaff = 0 AND a.IsDayWorker_OR_Shifter = 1)
						OR a.IsDriver = 1 
						OR a.isLiasonOfficer = 1 
					)		
					--AND
					--(
					--	SELECT TOP 1 Duration_Worked_Cumulative 
					--	FROM tas.Tran_Timesheet
					--	WHERE EmpNo = a.EmpNo
					--		AND DT = DATEADD(DAY, 1, a.DT)
					--	ORDER BY dtIN
					--) >= 480
					AND a.DT BETWEEN @startDate AND @endDate
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
			)
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.NoPayHours = a.Duration_Required - a.Duration_Worked_Cumulative,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE(),
					tas.Tran_Timesheet.Processed = 0
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Master_Employee_JDE b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL
				WHERE 
					a.ShiftSpan = 1 
					AND a.IsLastRow = 1		
					AND (a.Duration_Required > 0  AND a.Duration_Worked_Cumulative < a.Duration_Required)
					AND ISNULL(a.NoPayHours, 0) = 0
					AND ISNULL(a.CorrectionCode, '') = ''
					AND ISNULL(a.LeaveType, '') = ''
					AND ISNULL(a.ShiftCode, '') <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.AbsenceReasonCode, '') = ''
					AND NOT
					(
						a.IsSalStaff = 1 
						OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)
						OR (a.IsSalStaff = 0 AND a.IsDayWorker_OR_Shifter = 1)
						OR a.IsDriver = 1 
						OR a.isLiasonOfficer = 1 
					)		
					--AND
					--(
					--	SELECT TOP 1 Duration_Worked_Cumulative 
					--	FROM tas.Tran_Timesheet
					--	WHERE EmpNo = a.EmpNo
					--		AND DT = DATEADD(DAY, 1, a.DT)
					--	ORDER BY dtIN
					--) >= 480
					AND a.DT BETWEEN @startDate AND @endDate
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)

				IF ISNULL(@rowsAffected_TS, 0) = 0				
				BEGIN
                
					--Get the number of affected records in the "Tran_Timesheet" table
					SELECT @rowsAffected_TS = @@rowcount
				END 

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
            END 
			*/

			/**********************************************************************************************************************************************************************************
				Set the value of "IsDriver" field to 1 if the employee belongs to any of the ff job catalogs: Child Care Period, Medical Condition, Excused - Job Requirement	(Rev. #1.5)
			***********************************************************************************************************************************************************************************/
			IF EXISTS
            (
				SELECT a.AutoID 
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE 
					a.DT BETWEEN @startDate AND @endDate
					AND tas.fnIsSpecialCatalogMedicalCondition(a.EmpNo) = 1
					AND ISNULL(a.IsDriver, 0) = 0
			)
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET IsDriver = 1,
					NoPayHours = 0
				WHERE 
					DT BETWEEN @startDate AND @endDate
					AND tas.fnIsSpecialCatalogMedicalCondition(EmpNo) = 1
					AND ISNULL(IsDriver, 0) = 0
            END 

			/**********************************************************************************************************************************************************************************
				Auto correct No-pay-hours for attendance record wherein ShiftSpan = 1 but did not complete the required work duration 	(Rev. #1.9)
			***********************************************************************************************************************************************************************************/
			/*	Code below commented on 02-Feb-2021 as per Helpdesk No. 122294 (Rev. #2.8)
			IF EXISTS
            (
				SELECT a.AutoID
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Master_Employee_JDE b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL
				WHERE 
					a.ShiftSpan = 1 
					AND a.IsLastRow = 1		
					AND (a.Duration_Required > 0  AND a.Duration_Worked_Cumulative < a.Duration_Required)
					AND ISNULL(a.NoPayHours, 0) = 0
					AND ISNULL(a.CorrectionCode, '') = ''
					AND ISNULL(a.LeaveType, '') = ''
					AND ISNULL(a.ShiftCode, '') <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.AbsenceReasonCode, '') = ''
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
			)
			BEGIN

				UPDATE tas.Tran_Timesheet 
				SET NoPayHours = a.Duration_Required - a.Duration_Worked_Cumulative
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Master_Employee_JDE b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL
				WHERE 
					a.ShiftSpan = 1 
					AND a.IsLastRow = 1		
					AND (a.Duration_Required > 0  AND a.Duration_Worked_Cumulative < a.Duration_Required)
					AND ISNULL(a.NoPayHours, 0) = 0
					AND ISNULL(a.CorrectionCode, '') = ''
					AND ISNULL(a.LeaveType, '') = ''
					AND ISNULL(a.ShiftCode, '') <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND ISNULL(a.AbsenceReasonCode, '') = ''
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
            END 
			*/

			/**********************************************************************************************************************************************************************************
				Remove the Leave Type in the attendance record if the employee's scheduled shift is Day-off	(Rev. #2.0)
			***********************************************************************************************************************************************************************************/
			IF EXISTS
            (
				SELECT 1
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				WHERE 
					a.IsLastRow = 1
					AND ISNULL(a.LeaveType, '') <> '' 
					AND RTRIM(b.Effective_ShiftCode) = 'O'
					AND ISNULL(a.AbsenceReasonCode, '') = ''
					AND RTRIM(a.CorrectionCode) NOT IN ('RDUL', 'RDSL', 'RDIL')		--Rev. #2.1
					AND a.DT BETWEEN @startDate AND @endDate
			)
			BEGIN

				UPDATE tas.Tran_Timesheet 
				SET LeaveType = ''
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				WHERE 
					a.IsLastRow = 1
					AND ISNULL(a.LeaveType, '') <> '' 
					AND RTRIM(b.Effective_ShiftCode) = 'O'
					AND ISNULL(a.AbsenceReasonCode, '') = ''
					AND RTRIM(a.CorrectionCode) NOT IN ('RDUL', 'RDSL', 'RDIL')		--Rev. #2.1
					AND a.DT BETWEEN @startDate AND @endDate
            END 

			/**********************************************************************************************************************************************************************************
				Automate the creation of new Shift Pattern Change record for each temporary shift pattern changes that have due date (Rev. #1.8)
			***********************************************************************************************************************************************************************************/
			EXEC tas.Pr_AutomateShiftPatternEntry 

			/**********************************************************************************************************************************************************************************
				Rev. #2.2 - as per Helpdesk No. 86714:
				- Automate the removal of Reason of Absence and marking the employee as absent in case he/she did not come to work though has "(LP) Local TR/Part Time" reason of absence.
				- Also, automate the calculation of NPH is the employee leave the workplace earlier to attend the training.
			***********************************************************************************************************************************************************************************/
			EXEC tas.Pr_MarkAbsentRemoveROA 1, @startDate, @endDate			--Mark absent remove ROA
			EXEC tas.Pr_MarkAbsentRemoveROA 2, @startDate, @endDate 		--Calculate NPH remove ROA

			/**********************************************************************************************************************************************************************************
				Rev. #2.4 - Automate the process of granting DIL to employees during special holiday based on the following rules:
				For Salary Staff Employees (Grade 9 and above):
				•	DIL will be given to all employees who are on leave; either annual leave, sick leave, and injury leave. No DIL will be given to those who are absent on that day.

				For Shift Workers Employee (Grade 8 and below):
				•	Will be considered normal day for them, no extra pay. 
				•	Overtime will be given in excess to the required work duration. Those who are schedules to be day-off but come to work will be given regular overtime equal to the total work duration.
				•	DIL will be given to all employees who are on leave; either annual leave, sick leave, and injury leave. No DIL will be given to those who are absent on that day.
			***********************************************************************************************************************************************************************************/
			IF EXISTS 
			(
				SELECT 1 FROM tas.DILSpecialHolidaySetup a WITH (NOLOCK)
				WHERE ISNULL(a.IsProcessed, 0) = 0
					AND a.HolidayDate = CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, -1, GETDATE()), 12))
			)
			BEGIN

				DECLARE @holidayDate DATETIME = NULL

				--Get the holiday date to process
				SELECT TOP 1 @holidayDate = a.HolidayDate
				FROM tas.DILSpecialHolidaySetup a WITH (NOLOCK)
				WHERE ISNULL(a.IsProcessed, 0) = 0
					AND a.HolidayDate = CONVERT(DATETIME, CONVERT(VARCHAR, DATEADD(DAY, -1, GETDATE()), 12))

				IF ISNULL(@holidayDate, '') <> CAST(NULL AS DATETIME)
					EXEC tas.Pr_GrantDILSpecialHoliday 1, @holidayDate		
            END 


			/**********************************************************************************************************************************
				Rev. #2.5 - Process the Reason of Absence when an employee was marked absent on the days within the effective date
			***********************************************************************************************************************************/
			IF EXISTS
			(
				SELECT 1
				FROM tas.Tran_Absence a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate
				WHERE 
					(	
						(a.EffectiveDate >= @startDate AND a.EndingDate <= @endDate)
						OR 
						(@startDate BETWEEN a.EffectiveDate AND a.EndingDate AND @endDate BETWEEN a.EffectiveDate AND a.EndingDate)
					)	
					AND ISNULL(b.RemarkCode, '') = 'A'
					AND b.IsLastRow = 1
					AND b.DT BETWEEN @startDate AND @endDate
					AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
			)
            BEGIN
			
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.AbsenceReasonColumn = CASE WHEN RTRIM(b.AbsenceReasonColumn) = 'Day Off' THEN b.AbsenceReasonColumn ELSE 'ROA' END,
					tas.Tran_Timesheet.AbsenceReasonCode = RTRIM(a.AbsenceReasonCode),
					tas.Tran_Timesheet.LeaveType = RTRIM(a.AbsenceReasonCode),
					tas.Tran_Timesheet.RemarkCode = NULL,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM tas.Tran_Absence a WITH (NOLOCK)
					INNER JOIN tas.Tran_Timesheet b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate
				WHERE 
					(	
						(a.EffectiveDate >= @startDate AND a.EndingDate <= @endDate)
						OR 
						(@startDate BETWEEN a.EffectiveDate AND a.EndingDate AND @endDate BETWEEN a.EffectiveDate AND a.EndingDate)
					)	
					AND ISNULL(b.RemarkCode, '') = 'A'
					AND b.IsLastRow = 1
					AND b.DT BETWEEN @startDate AND @endDate
					AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
            END 


			/**********************************************************************************************************************************************************************************
				Rev. #2.6 - Mark absent to all employees who are on half-day leave but did not come to work
			***********************************************************************************************************************************************************************************/
			IF EXISTS
            (
				SELECT 1 FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE RTRIM(a.LeaveType) = 'HD'
					AND (a.dtIN IS NULL AND a.dtOUT IS NULL)
					AND a.IsLastRow = 1
					AND RTRIM(a.RemarkCode) <> 'A'
					AND a.DT BETWEEN @startDate AND @endDate
			)
			BEGIN

				UPDATE tas.Tran_Timesheet 
				SET RemarkCode = 'A',
					LeaveType = '',
					AbsenceReasonCode = '',
					AbsenceReasonColumn = ''
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE RTRIM(a.LeaveType) = 'HD'
					AND (a.dtIN IS NULL AND a.dtOUT IS NULL)
					AND a.IsLastRow = 1
					AND RTRIM(a.RemarkCode) <> 'A'
					AND a.DT BETWEEN @startDate AND @endDate
            END 
		END 

	END TRY

	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

	--IF @retError = @CONST_RETURN_OK
	--BEGIN

	--	IF @@TRANCOUNT > 0
	--		COMMIT TRANSACTION		
	--END

	--ELSE
	--BEGIN

	--	IF @@TRANCOUNT > 0
	--		ROLLBACK TRANSACTION
	--END

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected_TS AS TimesheetRowsAffected


/*	Debugging:

PARAMETERS:
	@actionType				TINYINT,		--(Note: 1 = Recover Reason of Absence)
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenter				VARCHAR(12) = NULL,
	@empNo					INT = 0

	--Recover Reason of Absences that have been removed due coming to workplace
	EXEC tas.Pr_AutoTimesheetCorrection 1, '03/01/2018', '03/12/2018', '', 10003180

	--Calculate No-Pay-Hour for employees who worked double shift but did not complete the required work duration in the first shift (Rev. #1.3)
	EXEC tas.Pr_AutoTimesheetCorrection 1, '01/16/2018', '02/15/2018', '', 10006016

	EXEC tas.Pr_AutoTimesheetCorrection 1, '10/16/2018', '11/15/2018'

*/

