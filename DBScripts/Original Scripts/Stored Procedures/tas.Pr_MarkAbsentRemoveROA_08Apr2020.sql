USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_MarkAbsentRemoveROA]    Script Date: 08/04/2020 10:07:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_MarkAbsentRemoveROA
*	Description: This stored procedure is used to flag the employee as absent and remove the ROA in the attendance sheet
*
*	Date			Author		Rev.# 		Comments:
*	13/05/2019		Ervin		1.0			Created
*	03/06/2019		Ervin		1.1			Modified the logic in @actionType = 1
*	12/12/2019		Ervin		1.2			Added filter condition to check if DT is less than today's date
*	15/03/2020		Ervin		1.3			Added condition to check if record does not exists in "ROAAbsentLog" table
*	19/03/2020		Ervin		1.4			Fixed bug reported by HR wherein employee not flagged as absent but local training though training is part time only for specific days
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_MarkAbsentRemoveROA]
(	
	@actionType		TINYINT,		--(Note: 1 => Mark absent remove ROA, 2 => Calculate No-pay-hour remove ROA, 3 = Undo ROA corrections)	
	@startDate		DATETIME,
	@endDate		DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= ''
)
AS	

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT
			--@CONST_MARO				VARCHAR(10)

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected			INT			

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1
			--@CONST_MARO				= 'MARO'	--Mark Absent - Remove Dayoff

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected			= 0				

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @actionType = 1			--Mark absent remove ROA
	BEGIN

		IF EXISTS
        (
			SELECT	1
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				INNER JOIN tas.Tran_Absence c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT BETWEEN c.EffectiveDate AND c.EndingDate --AND RTRIM(a.AbsenceReasonCode) = RTRIM(c.AbsenceReasonCode)  
			WHERE 
				a.IsLastRow = 1
				AND 
				(
					RTRIM(a.AbsenceReasonColumn) LIKE '%ROA%'
					OR RTRIM(a.AbsenceReasonColumn) = 'LV'
				)
				AND RTRIM(b.Effective_ShiftCode) <> 'O'
				AND ISNULL(a.IsPublicHoliday, 0) = 0
				AND a.Duration_Worked_Cumulative = 0
				AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
				AND ISNULL(a.RemarkCode, '') = ''
				AND RTRIM(c.AbsenceReasonCode) = 'LP'
				AND RTRIM(a.LeaveType) NOT IN
				(
					--Retrieve all leave type codes
					SELECT LTRIM(RTRIM(DRKY)) 
					FROM tas.syJDE_F0005 WITH (NOLOCK)
					WHERE LTRIM(RTRIM(DRSY)) = '58' 
						AND LTRIM(RTRIM(DRRT)) = 'VC'
						AND LTRIM(RTRIM(DRSPHD)) IN ('AL', 'SL', 'UL')
				)
				AND ISNULL(a.CorrectionCode, '') = ''
				AND a.DT BETWEEN @startDate AND @endDate
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)	
				AND a.DT < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Rev. #1.2
				AND NOT EXISTS		--Rev. #1.3
				(
					SELECT 1 FROM tas.ROAAbsentLog
					WHERE EmpNo = a.EmpNo
						AND DT = a.DT 
						AND TSAutoID = a.AutoID
				)		
		)
		BEGIN
        
			--Update attendance sheet
			UPDATE tas.Tran_Timesheet
			SET tas.Tran_Timesheet.RemarkCode = 'A',
				tas.Tran_Timesheet.LeaveType = NULL,
				tas.Tran_Timesheet.AbsenceReasonCode = NULL,
				tas.Tran_Timesheet.AbsenceReasonColumn = NULL,
				tas.Tran_Timesheet.Processed = 0,
				tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
				tas.Tran_Timesheet.LastUpdateTime = GETDATE()
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				INNER JOIN tas.Tran_Absence c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT BETWEEN c.EffectiveDate AND c.EndingDate --AND RTRIM(a.AbsenceReasonCode) = RTRIM(c.AbsenceReasonCode)  
			WHERE 
				a.IsLastRow = 1
				AND 
				(
					RTRIM(a.AbsenceReasonColumn) LIKE '%ROA%'
					OR RTRIM(a.AbsenceReasonColumn) = 'LV'
				)
				AND RTRIM(b.Effective_ShiftCode) <> 'O'
				AND ISNULL(a.IsPublicHoliday, 0) = 0
				AND a.Duration_Worked_Cumulative = 0
				AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
				AND ISNULL(a.RemarkCode, '') = ''
				AND RTRIM(c.AbsenceReasonCode) = 'LP'
				AND RTRIM(a.LeaveType) NOT IN
				(
					--Retrieve all leave type codes
					SELECT LTRIM(RTRIM(DRKY)) 
					FROM tas.syJDE_F0005 WITH (NOLOCK)
					WHERE LTRIM(RTRIM(DRSY)) = '58' 
						AND LTRIM(RTRIM(DRRT)) = 'VC'
						AND LTRIM(RTRIM(DRSPHD)) IN ('AL', 'SL', 'UL')
				)
				AND ISNULL(a.CorrectionCode, '') = ''
				AND a.DT BETWEEN @startDate AND @endDate
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
				AND a.DT < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Rev. #1.2
				AND NOT EXISTS		--Rev. #1.3
				(
					SELECT 1 FROM tas.ROAAbsentLog
					WHERE EmpNo = a.EmpNo
						AND DT = a.DT 
						AND TSAutoID = a.AutoID
				)	

			--Get the number of affected records in the "Tran_Timesheet" table
			SELECT @rowsAffected = @@rowcount 								
					
			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Insert the log record first
				INSERT INTO tas.ROAAbsentLog
				(
					TSAutoID,
					EmpNo,
					DT,
					CostCenter,
					ShiftPatCode,
					ShiftCode,
					CorrectionCode,
					RemarkCode,
					LeaveType,
					AbsenceReasonCode,
					AbsenceReasonColumn,
					NoPayHours,
					Processed,
					CreatedDate,
					CreatedByEmpNo,
					CreatedByUserID
				)
				SELECT	DISTINCT
						a.AutoID,
						a.EmpNo, 
						a.DT,
						RTRIM(a.BusinessUnit),
						a.ShiftPatCode,
						a.ShiftCode,
						a.CorrectionCode,
						a.RemarkCode,
						a.LeaveType,
						a.AbsenceReasonCode,
						a.AbsenceReasonColumn,
						a.NoPayHours,
						a.Processed,
						GETDATE(),
						0,
						'System Admin'
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
					INNER JOIN tas.Tran_Absence c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT BETWEEN c.EffectiveDate AND c.EndingDate --AND RTRIM(a.AbsenceReasonCode) = RTRIM(c.AbsenceReasonCode)  
				WHERE 
					a.IsLastRow = 1
					AND 
					(
						RTRIM(a.AbsenceReasonColumn) LIKE '%ROA%'
						OR RTRIM(a.AbsenceReasonColumn) = 'LV'
					)
					AND RTRIM(b.Effective_ShiftCode) <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND a.Duration_Worked_Cumulative = 0
					AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
					AND ISNULL(a.RemarkCode, '') = ''
					AND RTRIM(c.AbsenceReasonCode) = 'LP'
					AND RTRIM(a.LeaveType) NOT IN
					(
						--Retrieve all leave type codes
						SELECT LTRIM(RTRIM(DRKY)) 
						FROM tas.syJDE_F0005 WITH (NOLOCK)
						WHERE LTRIM(RTRIM(DRSY)) = '58' 
							AND LTRIM(RTRIM(DRRT)) = 'VC'
							AND LTRIM(RTRIM(DRSPHD)) IN ('AL', 'SL', 'UL')
					)
					AND ISNULL(a.CorrectionCode, '') = ''
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)		
					AND a.DT < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Rev. #1.2				
					AND NOT EXISTS		--Rev. #1.3
					(
						SELECT 1 FROM tas.ROAAbsentLog
						WHERE EmpNo = a.EmpNo
							AND DT = a.DT 
							AND TSAutoID = a.AutoID
					)	

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
			END 
		END 
	END 

	ELSE IF @actionType = 2		--Calculate No-pay-hours
	BEGIN

		IF EXISTS
        (
			SELECT 1
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				INNER JOIN tas.Tran_Absence c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT BETWEEN c.EffectiveDate AND c.EndingDate AND RTRIM(a.AbsenceReasonCode) = RTRIM(c.AbsenceReasonCode) AND UPPER(FORMAT(a.DT, 'ddd')) = RTRIM(c.[DayOfWeek])  
			WHERE 
				a.IsLastRow = 1
				AND 
				(
					RTRIM(a.AbsenceReasonColumn) LIKE '%ROA%'
					OR RTRIM(a.AbsenceReasonColumn) = 'LV'
				)
				AND RTRIM(b.Effective_ShiftCode) <> 'O'
				AND ISNULL(a.IsPublicHoliday, 0) = 0
				AND a.Duration_Required > 0
				AND a.Duration_Worked_Cumulative > 0
				AND a.Duration_ROA > 0
				--AND ((tas.fnCalculateTimeDifference(LEFT(c.StartTime, 2) + ':' + RIGHT(c.StartTime, 2) + ':00', LEFT(c.EndTime, 2) + ':' + RIGHT(c.EndTime, 2) + ':00') + a.Duration_Worked_Cumulative) < a.Duration_Required)
				AND ((a.Duration_ROA + a.Duration_Worked_Cumulative) < a.Duration_Required)
				AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
				AND ISNULL(a.RemarkCode, '') = ''
				AND RTRIM(c.AbsenceReasonCode) = 'LP'
				AND RTRIM(a.LeaveType) NOT IN
				(
					--Exclude the following leave types: Annual Leave, Sick Leave, Injury Leave
					SELECT LTRIM(RTRIM(DRKY)) 
					FROM tas.syJDE_F0005 WITH (NOLOCK)
					WHERE LTRIM(RTRIM(DRSY)) = '58' 
						AND LTRIM(RTRIM(DRRT)) = 'VC'
						AND LTRIM(RTRIM(DRSPHD)) IN ('AL', 'SL', 'UL')
				)
				AND ISNULL(a.NoPayHours, 0) = 0
				AND (ISNULL(c.StartTime, '') <> '' AND ISNULL(c.EndTime, '') <> '')
				AND NOT 
				(
					a.IsSalStaff = 1
					OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)
					OR (a.IsSalStaff = 0 AND a.IsDayWorker_OR_Shifter = 1)
					OR a.IsDriver = 1 
					OR a.IsLiasonOfficer = 1 
				)
				AND ISNULL(a.CorrectionCode, '') = ''
				AND a.DT BETWEEN @startDate AND @endDate
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
				AND a.DT < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Rev. #1.2
		)
		BEGIN
        
			--Insert the log record first
			INSERT INTO tas.ROAAbsentLog
			(
				TSAutoID,
				EmpNo,
				DT,
				CostCenter,
				ShiftPatCode,
				ShiftCode,
				CorrectionCode,
				RemarkCode,
				LeaveType,
				AbsenceReasonCode,
				AbsenceReasonColumn,
				NoPayHours,
				Processed,
				CreatedDate,
				CreatedByEmpNo,
				CreatedByUserID
			)
			SELECT	DISTINCT
					a.AutoID,
					a.EmpNo, 
					a.DT,
					RTRIM(a.BusinessUnit),
					a.ShiftPatCode,
					a.ShiftCode,
					a.CorrectionCode,
					a.RemarkCode,
					a.LeaveType,
					a.AbsenceReasonCode,
					a.AbsenceReasonColumn,
					a.NoPayHours,
					a.Processed,
					GETDATE(),
					0,
					'System Admin'
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
				INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
				INNER JOIN tas.Tran_Absence c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT BETWEEN c.EffectiveDate AND c.EndingDate AND RTRIM(a.AbsenceReasonCode) = RTRIM(c.AbsenceReasonCode) AND UPPER(FORMAT(a.DT, 'ddd')) = RTRIM(c.[DayOfWeek])  
			WHERE 
				a.IsLastRow = 1
				AND 
				(
					RTRIM(a.AbsenceReasonColumn) LIKE '%ROA%'
					OR RTRIM(a.AbsenceReasonColumn) = 'LV'
				)
				AND RTRIM(b.Effective_ShiftCode) <> 'O'
				AND ISNULL(a.IsPublicHoliday, 0) = 0
				AND a.Duration_Required > 0
				AND a.Duration_Worked_Cumulative > 0
				AND a.Duration_ROA > 0
				--AND ((tas.fnCalculateTimeDifference(LEFT(c.StartTime, 2) + ':' + RIGHT(c.StartTime, 2) + ':00', LEFT(c.EndTime, 2) + ':' + RIGHT(c.EndTime, 2) + ':00') + a.Duration_Worked_Cumulative) < a.Duration_Required)
				AND ((a.Duration_ROA + a.Duration_Worked_Cumulative) < a.Duration_Required)
				AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
				AND ISNULL(a.RemarkCode, '') = ''
				AND RTRIM(c.AbsenceReasonCode) = 'LP'
				AND RTRIM(a.LeaveType) NOT IN
				(
					--Exclude the following leave types: Annual Leave, Sick Leave, Injury Leave
					SELECT LTRIM(RTRIM(DRKY)) 
					FROM tas.syJDE_F0005 WITH (NOLOCK)
					WHERE LTRIM(RTRIM(DRSY)) = '58' 
						AND LTRIM(RTRIM(DRRT)) = 'VC'
						AND LTRIM(RTRIM(DRSPHD)) IN ('AL', 'SL', 'UL')
				)				
				AND ISNULL(a.NoPayHours, 0) = 0
				AND (ISNULL(c.StartTime, '') <> '' AND ISNULL(c.EndTime, '') <> '')
				AND NOT 
				(
					a.IsSalStaff = 1
					OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)
					OR (a.IsSalStaff = 0 AND a.IsDayWorker_OR_Shifter = 1)
					OR a.IsDriver = 1 
					OR a.IsLiasonOfficer = 1 
				)
				AND ISNULL(a.CorrectionCode, '') = ''
				AND a.DT BETWEEN @startDate AND @endDate
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)		
				AND a.DT < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))			--Rev. #1.2					
					
			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Update attendance sheet
				UPDATE tas.Tran_Timesheet
				SET --tas.Tran_Timesheet.NoPayHours = a.Duration_Required - ((tas.fnCalculateTimeDifference(LEFT(c.StartTime, 2) + ':' + RIGHT(c.StartTime, 2) + ':00', LEFT(c.EndTime, 2) + ':' + RIGHT(c.EndTime, 2) + ':00') + a.Duration_Worked_Cumulative)),
					tas.Tran_Timesheet.NoPayHours = a.Duration_Required - (a.Duration_ROA + a.Duration_Worked_Cumulative),
					tas.Tran_Timesheet.LeaveType = NULL,
					tas.Tran_Timesheet.AbsenceReasonCode = NULL,
					tas.Tran_Timesheet.AbsenceReasonColumn = NULL,
					tas.Tran_Timesheet.Processed = 0,
					tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
					tas.Tran_Timesheet.LastUpdateTime = GETDATE()
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
					INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX
					INNER JOIN tas.Tran_Absence c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.DT BETWEEN c.EffectiveDate AND c.EndingDate AND RTRIM(a.AbsenceReasonCode) = RTRIM(c.AbsenceReasonCode) AND UPPER(FORMAT(a.DT, 'ddd')) = RTRIM(c.[DayOfWeek])  
				WHERE 
					a.IsLastRow = 1
					AND 
					(
						RTRIM(a.AbsenceReasonColumn) LIKE '%ROA%'
						OR RTRIM(a.AbsenceReasonColumn) = 'LV'
					)
					AND RTRIM(b.Effective_ShiftCode) <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND a.Duration_Required > 0
					AND a.Duration_Worked_Cumulative > 0
					AND a.Duration_ROA > 0
					--AND ((tas.fnCalculateTimeDifference(LEFT(c.StartTime, 2) + ':' + RIGHT(c.StartTime, 2) + ':00', LEFT(c.EndTime, 2) + ':' + RIGHT(c.EndTime, 2) + ':00') + a.Duration_Worked_Cumulative) < a.Duration_Required)
					AND ((a.Duration_ROA + a.Duration_Worked_Cumulative) < a.Duration_Required)
					AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))
					AND ISNULL(a.RemarkCode, '') = ''
					AND RTRIM(c.AbsenceReasonCode) = 'LP'
					AND RTRIM(a.LeaveType) NOT IN
					(
						--Exclude the following leave types: Annual Leave, Sick Leave, Injury Leave
						SELECT LTRIM(RTRIM(DRKY)) 
						FROM tas.syJDE_F0005 WITH (NOLOCK)
						WHERE LTRIM(RTRIM(DRSY)) = '58' 
							AND LTRIM(RTRIM(DRRT)) = 'VC'
							AND LTRIM(RTRIM(DRSPHD)) IN ('AL', 'SL', 'UL')
					)
					AND ISNULL(a.NoPayHours, 0) = 0
					AND (ISNULL(c.StartTime, '') <> '' AND ISNULL(c.EndTime, '') <> '')
					AND NOT 
					(
						a.IsSalStaff = 1
						OR (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)
						OR (a.IsSalStaff = 0 AND a.IsDayWorker_OR_Shifter = 1)
						OR a.IsDriver = 1 
						OR a.IsLiasonOfficer = 1 
					)
					AND ISNULL(a.CorrectionCode, '') = ''
					AND a.DT BETWEEN @startDate AND @endDate
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(a.BusinessUnit) = @costCenter OR @costCenter IS NULL)
					AND a.DT < CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))		--Rev. #1.2

				--Get the number of affected records in the "Tran_Timesheet" table
				SELECT @rowsAffected = @@rowcount 

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END
			END 
		END 
	END 

	ELSE IF @actionType = 3		--Undo removal of dayoff
	BEGIN

		UPDATE tas.Tran_Timesheet
		SET tas.Tran_Timesheet.RemarkCode = b.RemarkCode,
			tas.Tran_Timesheet.NoPayHours = b.NoPayHours,
			tas.Tran_Timesheet.LeaveType = b.LeaveType,
			tas.Tran_Timesheet.AbsenceReasonCode = b.AbsenceReasonCode,
			tas.Tran_Timesheet.AbsenceReasonColumn = b.AbsenceReasonColumn,
			tas.Tran_Timesheet.Processed = b.Processed,
			tas.Tran_Timesheet.LastUpdateUser = 'System Admin',
			tas.Tran_Timesheet.LastUpdateTime = GETDATE()
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.ROAAbsentLog b WITH (NOLOCK) ON a.AutoID = b.TSAutoID
		WHERE 
			b.DT BETWEEN @startDate AND @endDate
			AND (b.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.CostCenter) = @costCenter OR @costCenter IS NULL)
			AND a.IsLastRow = 1
		
		--Get the number of affected records in the "Tran_Timesheet" table
		SELECT @rowsAffected = @@rowcount 

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		-- Checks if there's no error
		IF @retError = @CONST_RETURN_OK
		BEGIN

			DELETE FROM tas.ROAAbsentLog 
			WHERE DT BETWEEN @startDate AND @endDate
				AND (EmpNo = @empNo OR @empNo IS NULL)
				AND (RTRIM(CostCenter) = @costCenter OR @costCenter IS NULL)
		END 
	END 

	--Return error information to the caller
	--SELECT	@hasError AS HasError, 
	--		@retError AS ErrorCode, 
	--		@retErrorDesc AS ErrorDescription,
	--		@rowsAffected AS RowsAffected

