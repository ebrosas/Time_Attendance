USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_AutoTimesheetCorrection]    Script Date: 25/02/2018 16:03:33 ******/
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
				FROM tas.Tran_Absence a
					INNER JOIN tas.Tran_Timesheet b ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate
					CROSS APPLY
					(
						SELECT * FROM tas.syJDE_F0005 
						WHERE LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) = '55-RA'
							AND LTRIM(RTRIM(DRKY)) = RTRIM(a.AbsenceReasonCode)
					) c
				WHERE 
					a.EffectiveDate >= @startDate
					AND a.EndingDate <= @endDate
					AND b.DT BETWEEN @startDate AND @endDate
					AND (b.dtIN IS NOT NULL)
					AND ISNULL(b.IsPublicHoliday, 0) = 0
					AND ISNULL(b.CorrectionCode, '') = ''
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
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM tas.Tran_Absence a
					INNER JOIN tas.Tran_Timesheet b ON a.EmpNo = b.EmpNo AND b.DT BETWEEN a.EffectiveDate AND a.EndingDate
					CROSS APPLY
					(
						SELECT * FROM tas.syJDE_F0005 
						WHERE LTRIM(RTRIM(DRSY)) + '-' + LTRIM(RTRIM(DRRT)) = '55-RA'
							AND LTRIM(RTRIM(DRKY)) = RTRIM(a.AbsenceReasonCode)
					) c
				WHERE 
					a.EffectiveDate >= @startDate
					AND a.EndingDate <= @endDate
					AND b.DT BETWEEN @startDate AND @endDate
					AND (b.dtIN IS NOT NULL)
					AND ISNULL(b.IsPublicHoliday, 0) = 0
					AND ISNULL(b.CorrectionCode, '') = ''
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
			IF EXISTS
            (
				SELECT a.AutoID
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID	
					CROSS APPLY tas.fnGetProcessedTimesheetData(a.AutoID, b.OTtype, a.dtIN, a.dtOUT) c
					LEFT JOIN tas.OvertimeRequest d ON a.EmpNo = d.EmpNo AND a.AutoID = d.TS_AutoID AND RTRIM(d.StatusHandlingCode) NOT IN ('Cancelled', 'Rejected')
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
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID	
					CROSS APPLY tas.fnGetProcessedTimesheetData(a.AutoID, b.OTtype, a.dtIN, a.dtOUT) c
					LEFT JOIN tas.OvertimeRequest d ON a.EmpNo = d.EmpNo AND a.AutoID = d.TS_AutoID AND RTRIM(d.StatusHandlingCode) NOT IN ('Cancelled', 'Rejected')
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

			/**********************************************************************************************************************************************************************************
				Calculate No-Pay-Hour for employees who worked double shift but did not complete the required work duration in the first shift (Rev. #1.3)
			***********************************************************************************************************************************************************************************/
			IF EXISTS
			(
				SELECT a.AutoID 
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL
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
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo AND b.DateResigned IS NULL
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
	EXEC tas.Pr_AutoTimesheetCorrection 1, '10/16/2017', '11/15/2017'

	--Calculate No-Pay-Hour for employees who worked double shift but did not complete the required work duration in the first shift (Rev. #1.3)
	EXEC tas.Pr_AutoTimesheetCorrection 1, '12/16/2017', '01/15/2018', '', 10003574

*/

