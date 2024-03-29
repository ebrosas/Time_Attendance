USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_TranTimesheet_CRUD]    Script Date: 17/05/2017 09:39:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Tran_WorkplaceSwipe_CRUD
*	Description: Performs retrieve, insert, update, and delete operations against the "tas.Tran_Timesheet" table
*
*	Date:			Author:		Rev.#:		Comments:
*	01/08/2016		Ervin		1.0			Created
*	26/12/2016		Ervin		1.1			Refactored the logic in the Update operation
*	28/02/2017		Ervin		1.1			Added logic for "Change Scheduled Shift"
**************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_TranTimesheet_CRUD]
(
	@actionType						TINYINT,	
	@autoID							INT,
	@correctionCode					VARCHAR(10),
	@otType							VARCHAR(10),
	@otStartTime					DATETIME,
	@otEndTime						DATETIME,
	@noPayHours						INT,
	@shiftCode						VARCHAR(10),
	@shiftAllowance					BIT,
	@durationShiftAllowanceEvening	INT,
	@durationShiftAllowanceNight	INT,
	@dilEntitlement					VARCHAR(10),
	@remarkCode						VARCHAR(10),
	@userID							VARCHAR(50)	
)
AS	
	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT,
			@CONST_ANAD				VARCHAR(10),
			@CONST_CNWP				VARCHAR(10),
			@CONST_RNAP				VARCHAR(10),
			@CONST_RNCB				VARCHAR(10),
			@CONST_RNCS				VARCHAR(10),
			@CONST_RNDF				VARCHAR(10),
			@CONST_RNDP				VARCHAR(10),
			@CONST_RNLE				VARCHAR(10),
			@CONST_RNMR				VARCHAR(10),
			@CONST_RNOP				VARCHAR(10),
			@CONST_RNSL				VARCHAR(10),
			@CONST_RNSO				VARCHAR(10),
			@CONST_RNST				VARCHAR(10),
			@CONST_ASES				VARCHAR(10),
			@CONST_ASNS				VARCHAR(10),
			@CONST_RSES				VARCHAR(10),
			@CONST_RSNS				VARCHAR(10),
			@CONST_RSNE				VARCHAR(10),
			@CONST_AOBT				VARCHAR(10),
			@CONST_AOCS				VARCHAR(10),
			@CONST_AOMA				VARCHAR(10),
			@CONST_COCA				VARCHAR(10),
			@CONST_COCS				VARCHAR(10),
			@CONST_ROAL				VARCHAR(10),
			@CONST_ROCS				VARCHAR(10),
			@CONST_RODO				VARCHAR(10),
			@CONST_ROMA				VARCHAR(10),
			@CONST_ACS				VARCHAR(10),
			@CONST_AL				VARCHAR(10),
			@CONST_BD				VARCHAR(10),
			@CONST_CAL				VARCHAR(10),
			@CONST_CBD				VARCHAR(10),
			@CONST_CCS				VARCHAR(10),
			@CONST_CDF				VARCHAR(10),
			@CONST_COEW				VARCHAR(10),
			@CONST_COMS				VARCHAR(10),
			@CONST_CSR				VARCHAR(10),
			@CONST_DF				VARCHAR(10),
			@CONST_EW				VARCHAR(10),
			@CONST_MA				VARCHAR(10),
			@CONST_MS				VARCHAR(10),
			@CONST_PD				VARCHAR(10),
			@CONST_PH				VARCHAR(10),
			@CONST_PM				VARCHAR(10),
			@CONST_SD				VARCHAR(10),
			@CONST_SR				VARCHAR(10),
			@CONST_TR				VARCHAR(10),
			@CONST_MACL				VARCHAR(10),
			@CONST_MACS				VARCHAR(10),
			@CONST_MADA				VARCHAR(10),
			@CONST_MAGS				VARCHAR(10),
			@CONST_RAAP				VARCHAR(10),
			@CONST_RABT				VARCHAR(10),
			@CONST_RACB				VARCHAR(10),
			@CONST_RACS				VARCHAR(10),
			@CONST_RADF				VARCHAR(10),
			@CONST_RADL				VARCHAR(10),
			@CONST_RADO				VARCHAR(10),
			@CONST_RADP				VARCHAR(10),
			@CONST_RAEA				VARCHAR(10),
			@CONST_RAGD				VARCHAR(10),
			@CONST_RAJC				VARCHAR(10),
			@CONST_RALE				VARCHAR(10),
			@CONST_RAMT				VARCHAR(10),
			@CONST_RAPH				VARCHAR(10),
			@CONST_RASA				VARCHAR(10),
			@CONST_RASL				VARCHAR(10),
			@CONST_RASP				VARCHAR(10),
			@CONST_RASR				VARCHAR(10),
			@CONST_RAST				VARCHAR(10),
			@CONST_HD				VARCHAR(10),
			@CONST_RWLC				VARCHAR(10),
			@CONST_MDEA				VARCHAR(10),
			@CONST_RDEA				VARCHAR(10),
			@CONST_ADDM				VARCHAR(10),
			@CONST_RMVD				VARCHAR(10),
			@CONST_MOCS				VARCHAR(10),
			@CONST_ALSE				VARCHAR(10),
			@CONST_RAAD				VARCHAR(10),
			@CONST_CSS				VARCHAR(10)

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1,
			@CONST_ANAD				= 'ANAD',	
			@CONST_CNWP				= 'CNWP',	
			@CONST_RNAP				= 'RNAP',	
			@CONST_RNCB				= 'RNCB',
			@CONST_RNCS				= 'RNCS',
			@CONST_RNDF				= 'RNDF',
			@CONST_RNDP				= 'RNDP',
			@CONST_RNLE				= 'RNLE',
			@CONST_RNMR				= 'RNMR',
			@CONST_RNOP				= 'RNOP',
			@CONST_RNSL				= 'RNSL',
			@CONST_RNSO				= 'RNSO',
			@CONST_RNST				= 'RNST',
			@CONST_ASES				= 'ASES',
			@CONST_ASNS				= 'ASNS',
			@CONST_RSES				= 'RSES',
			@CONST_RSNS				= 'RSNS',
			@CONST_RSNE				= 'RSNE',
			@CONST_AOBT				= 'AOBT',
			@CONST_AOCS				= 'AOCS',
			@CONST_AOMA				= 'AOMA',
			@CONST_COCA				= 'COCA',
			@CONST_COCS				= 'COCS',
			@CONST_ROAL				= 'ROAL',
			@CONST_ROCS				= 'ROCS',
			@CONST_RODO				= 'RODO',
			@CONST_ROMA				= 'ROMA',
			@CONST_ACS				= 'ACS',
			@CONST_AL				= 'AL',
			@CONST_BD				= 'BD',
			@CONST_CAL				= 'CAL',
			@CONST_CBD				= 'CBD',
			@CONST_CCS				= 'CCS',
			@CONST_CDF				= 'CDF',
			@CONST_COEW				= 'COEW',
			@CONST_COMS				= 'COMS',
			@CONST_CSR				= 'CSR',
			@CONST_DF				= 'DF',
			@CONST_EW				= 'EW',
			@CONST_MA				= 'MA',
			@CONST_MS				= 'MS',
			@CONST_PD				= 'PD',
			@CONST_PH				= 'PH',
			@CONST_PM				= 'PM',
			@CONST_SD				= 'SD',
			@CONST_SR				= 'SR',
			@CONST_TR				= 'TR',
			@CONST_MACL				= 'MACL',
			@CONST_MACS				= 'MACS',
			@CONST_MADA				= 'MADA',
			@CONST_MAGS				= 'MAGS',
			@CONST_RAAP				= 'RAAP',
			@CONST_RABT				= 'RABT',
			@CONST_RACB				= 'RACB',
			@CONST_RACS				= 'RACS',
			@CONST_RADF				= 'RADF',
			@CONST_RADL				= 'RADL',
			@CONST_RADO				= 'RADO',
			@CONST_RADP				= 'RADP',
			@CONST_RAEA				= 'RAEA',
			@CONST_RAGD				= 'RAGD',
			@CONST_RAJC				= 'RAJC',
			@CONST_RALE				= 'RALE',
			@CONST_RAMT				= 'RAMT',
			@CONST_RAPH				= 'RAPH',
			@CONST_RASA				= 'RASA',
			@CONST_RASL				= 'RASL',
			@CONST_RASP				= 'RASP',
			@CONST_RASR				= 'RASR',
			@CONST_RAST				= 'RAST',
			@CONST_HD				= 'HD',
			@CONST_RWLC				= 'RWLC',
			@CONST_MDEA				= 'MDEA',
			@CONST_RDEA				= 'RDEA',
			@CONST_ADDM				= 'ADDM',
			@CONST_RMVD				= 'RMVD',
			@CONST_MOCS				= 'MOCS',
			@CONST_ALSE				= 'ALSE',
			@CONST_RAAD				= 'RAAD',
			@CONST_CSS				= 'CSS'

	--Define variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@newID					INT,
			@rowsAffected			INT

	--Initialize variables
	SELECT	@hasError			= 0,
			@retError			= @CONST_RETURN_OK,
			@retErrorDesc		= '',
			@newID				= 0,
			@rowsAffected		= 0

	IF @actionType = 0			--Check existing record
	BEGIN

		SELECT * FROM tas.Tran_Timesheet a
		WHERE a.AutoID = @autoID
	END

	ELSE IF @actionType = 2  --Update existing record
	BEGIN

		--Start a transaction
		BEGIN TRAN T1

		BEGIN TRY

			IF RTRIM(@correctionCode) IN 
			(
				@CONST_ANAD,		--Add No Pay Hour-Adjustment
				@CONST_CNWP,		--Change NPH-With permission
				@CONST_RNAP,		--Remove NoPay Access problem
				@CONST_RNCB,		--Remove NoPayHour-Co. Business
				@CONST_RNCS,		--Remove NoPayHour-Change Shift
				@CONST_RNDF,		--Remove NoPayHour-Death Family
				@CONST_RNDP,		--Remove NoPayHour-w/ permission
				@CONST_RNLE,		--Remove No Pay Leave Entered
				@CONST_RNMR,		--Remove NoPayHour-Medical reason
				@CONST_RNOP,		--Remove NoPayHour-Baby born
				@CONST_RNSL,		--Remove No Pay SL
				@CONST_RNSO,		--Remove NoPay/Special Occasion
				@CONST_RNST			--Remove NoPay sport team
			)
			BEGIN
            
				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					NoPayHours = @noPayHours,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
			END 

			ELSE IF RTRIM(@correctionCode) = @CONST_ASES	--Add Sh Allw Evening-Chng Shift
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					ShiftAllowance = 1,
					Duration_ShiftAllowance_Evening = @durationShiftAllowanceEvening,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_ASNS	--Add Sh Allw Night-Chng Shift
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					ShiftAllowance = 1,
					Duration_ShiftAllowance_Night = @durationShiftAllowanceNight,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RSES	--Remove Shift Allow-evening shf
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					ShiftAllowance = 0,
					Duration_ShiftAllowance_Evening = 0,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RSNS	--Remove Shift Allow-night shift
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					ShiftAllowance = 0,
					Duration_ShiftAllowance_Night = 0,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RSNE	--Remove Shift Allo-not entitled
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					ShiftAllowance = 0,
					Duration_ShiftAllowance_Evening = 0,
					Duration_ShiftAllowance_Night = 0,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 			

			ELSE IF RTRIM(@correctionCode) IN 
			(
				@CONST_AOBT,	--Add OT Busines Trip during Eid
				@CONST_AOCS,	--Add Overtime-Change Shift
				@CONST_AOMA,	--Add OT manager approved
				@CONST_COCA,	--Change Overtime-Call Out
				@CONST_COCS,	--Change Overtime-Change Shift
				@CONST_ROAL,	--Remove OT-against last month
				@CONST_ROCS,	--Remove OT-Change Shift
				@CONST_RODO,	--Remove OT-Day Off
				@CONST_ROMA,	--Remove OT Manager approval
				@CONST_ACS,		--ADD OT Change Shift
				@CONST_AL,		--Annual Leave
				@CONST_BD,		--Break Down
				@CONST_CAL,		--Call out Annual Leave
				@CONST_CBD,		--Call out Break Down
				@CONST_CCS,		--Change OT Change Shift
				@CONST_CDF,		--Call out Family Death
				@CONST_COEW,	--Call Out Extra Work
				@CONST_COMS,	--Call Out Manpower Shortage
				@CONST_CSR,		--Call out Sick
				@CONST_DF,		--Family Death
				@CONST_EW,		--Extra Work/ Special Task
				@CONST_MA,		--Add OT Manager Approval
				@CONST_MS,		--Manpower Shortage
				@CONST_PD,		--Project / Development
				@CONST_PH,		--Public Holiday
				@CONST_PM,		--Planned Maintenance
				@CONST_SD,		--Shutdown
				@CONST_SR,		--Leave (Sick,Injury,Light Duty)
				@CONST_TR		--Training
			)
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					OTType = @otType,
					OTStartTime = @otStartTime,
					OTEndTime = @otEndTime,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 		
			
			ELSE IF RTRIM(@correctionCode) IN
			(
				@CONST_MACL,		--Mark Absent Leave Cancelled
				@CONST_MACS,		--Mark Absent-Change Shift
				@CONST_MADA,		--Mark Absent-Disciplinary Action
				@CONST_MAGS			--Mark Absent During Gen. Strike
			)	
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					RemarkCode = 'A',
					LeaveType = NULL,
					AbsenceReasonCode = NULL,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) IN
			(
				@CONST_RAAP,	--Remove Absent Access Problem
				@CONST_RACB,	--Remove Absent-Child Birth
				@CONST_RACS,	--Remove Absent-Change Shift
				@CONST_RADF,	--Remove Absent-Death of Family
				@CONST_RADL,	--Remove Absent DIL
				@CONST_RADO,	--Remove Absent-Day Off
				@CONST_RADP,	--Remove Absent Deducted Payroll
				@CONST_RAEA,	--Remove Absent-Excused
				@CONST_RAGD,	--Remove Absent-Give DIL
				@CONST_RAJC,	--Remove Absent-Attend Trade U.				
				@CONST_RAMT,	--Remove Absent-Manual Timesheet
				@CONST_RAPH,	--Remove absent - Public Holiday
				@CONST_RASA,	--Remove Absent Special Assignmt				
				@CONST_RASP,	--Remove Absent - Change Shift P
				@CONST_RASR,	--Remove Absent Sec. Restriction
				@CONST_RAST		--Remove Absent Sport Team
			)	
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					RemarkCode = NULL,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RABT	--Remove Absent Business Trip
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					RemarkCode = NULL,
					AbsenceReasonCode = CASE WHEN ISNULL(AbsenceReasonCode, '') = '' THEN 'BT' ELSE AbsenceReasonCode END,
					AbsenceReasonColumn = CASE WHEN ISNULL(AbsenceReasonColumn, '') = '' THEN 'ROA' ELSE AbsenceReasonColumn END,
					LeaveType = CASE WHEN ISNULL(LeaveType, '') = '' THEN 'BT' ELSE LeaveType END,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RALE	--Remove Absent Leave Entered
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					RemarkCode = NULL,
					LeaveType = CASE WHEN ISNULL(LeaveType, '') = '' THEN 'AL' ELSE LeaveType END,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RASL	--Remove Absent Sick Leave
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,
					RemarkCode = NULL,
					LeaveType = CASE WHEN ISNULL(LeaveType, '') = '' THEN 'SLP' ELSE LeaveType END,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_HD		--Half Day Leave
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					LeaveType = 'HD',
					AbsenceReasonColumn = 'LV',
					RemarkCode = NULL,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RWLC	--Leave Cancelled
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					LeaveType = '',
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) IN
			(
				@CONST_MDEA,	--Mark DIL-Entitled by Admin
				@CONST_RDEA		--Remove DIL-Entitled by Admin
			)
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					DIL_Entitlement = RTRIM(@dilEntitlement),
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_ADDM	--Add Meal Voucher
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					MealVoucherEligibility = 'YA',
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RMVD	--Remove Meal Voucher Duplicate
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					MealVoucherEligibility = 'Y',
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_MOCS	--Mark Off Change Shift
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,		
					ShiftCode = @shiftCode,			
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_ALSE		--Local Seminar/Exhibition
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					RemarkCode = NULL,
					AbsenceReasonColumn = 'ROA',
					AbsenceReasonCode = 'LS',
					LeaveType = 'LS',
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_RAAD		--Add Extra Pay-Adj last month
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					RemarkCode = NULL,
					Processed = 0,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 

			ELSE IF RTRIM(@correctionCode) = @CONST_CSS			--Change Scheduled Shift
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET CorrectionCode = @correctionCode,					
					ShiftCode = @shiftCode,
					LastUpdateUser = @userID,
					LastUpdateTime = GETDATE()
				WHERE AutoID = @autoID

				SELECT @rowsAffected = @@rowcount 
            END 
			

		END TRY

		BEGIN CATCH

			--Capture the error
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1

		END CATCH

		IF @retError = @CONST_RETURN_OK
			COMMIT TRANSACTION T1		
		ELSE
			ROLLBACK TRANSACTION T1

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription, 
				0 AS NewIdentityID,
				@rowsAffected AS RowsAffected
	END

	ELSE IF (@actionType = 3)  --Delete existing record 
	BEGIN

		--Start a transaction
		BEGIN TRAN T1

		BEGIN TRY

			DELETE FROM tas.Tran_Timesheet
			WHERE AutoID = @autoID

			SELECT @rowsAffected = @@rowcount

		END TRY

		BEGIN CATCH

			--Capture the error
			SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
					@retErrorDesc = ERROR_MESSAGE(),
					@hasError = 1

		END CATCH

		IF @retError = @CONST_RETURN_OK
			COMMIT TRANSACTION T1		
		ELSE
			ROLLBACK TRANSACTION T1

		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription, 
				0 AS NewIdentityID,
				@rowsAffected AS RowsAffected
	END

