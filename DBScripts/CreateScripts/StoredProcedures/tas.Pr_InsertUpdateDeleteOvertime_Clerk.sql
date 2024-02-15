/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_InsertUpdateDeleteOvertime_Clerk
*	Description: This stored procedure is used in "Employee Overtime Entry" form
*
*	Date			Author		Revision No.	Comments:
*	26/07/2017		Ervin		1.0				Created
*	06/09/2017		Ervin		1.1				Added "RequestSubmissionDate" in the return recordset
*	11/09/2017		Ervin		1.2				Implemented new logic that calculates the OT start and end time based on the supplied duration from the UI
*	12/09/2017		Ervin		1.3				Refactored the logic in calculating the OT start and end times
*	19/11/2017		Ervin		1.4				Set the "OTStartTime", "OTEndTime", and "OTType" to null if @otDuration = 0
*	31/01/2018		Ervin		1.5				Fixed bug reported by users wherein the Clerk could not submit overtime for Aspire employees 
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_InsertUpdateDeleteOvertime_Clerk
(	
	@autoID						INT, 	
	@otReason					VARCHAR(10),	
	@comment					VARCHAR(1000),
	@userEmpNo					INT,
	@userEmpName				VARCHAR(100), 
	@userID						VARCHAR(50), 
	@otApproved					VARCHAR(1) = '0', 
	@mealVoucherEligibilityCode	VARCHAR(10) = NULL,
	@otDuration					INT = 0
	
)
AS	
	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT

	--Define workflow status variables
	DECLARE	@statusID					int,
			@statusCode					varchar(10),
			@statusDesc					varchar(200),
			@statusHandlingCode			varchar(50)
				
	--Define other variables
	DECLARE @hasError					BIT,
			@retError					INT,
			@retErrorDesc				VARCHAR(200),
			@rowsAffected_OTDetail		INT,
			@rowsAffected_OTRequest		INT,
			@otDifference				INT,
			@isOTApproved				BIT,
			@otStartTime				DATETIME,
			@otEndTime					DATETIME,
			@otStartTime_Orig			DATETIME,
			@otEndTime_Orig				DATETIME,
			@otDuration_Orig			INT,
			@otType						VARCHAR(10),
			@correctionCode				VARCHAR(10),
			@otRequestNo				BIGINT,
			@userEmail					VARCHAR(50),
			@requestSubmissionDate		DATETIME,
			@excessWorkDuration			INT,
			@totalWorkDuration			INT,
			@dtIN						DATETIME, 
			@dtOUT						DATETIME

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize workflow status variables
	SELECT	@statusID					= NULL,
			@statusCode					= NULL,
			@statusDesc					= NULL,
			@statusHandlingCode			= NULL

	--Initialize other variables
	SELECT	@hasError					= 0,
			@retError					= @CONST_RETURN_OK,
			@retErrorDesc				= '',
			@rowsAffected_OTDetail		= 0,
			@rowsAffected_OTRequest		= 0,
			@otDifference				= 0,
			@isOTApproved				= 0,
			@otStartTime				= NULL,
			@otEndTime					= NULL,
			@otStartTime_Orig			= NULL,
			@otEndTime_Orig				= NULL,
			@otType						= NULL,
			@correctionCode				= NULL,
			@otRequestNo				= 0,
			@userEmail					= NULL,
			@requestSubmissionDate		= NULL,
			@excessWorkDuration			= 0,
			@totalWorkDuration			= 0,
			@dtIN						= NULL, 
			@dtOUT						= NULL

	--Start a transaction
	--BEGIN TRAN T1

	BEGIN TRY

		--Get the email address of the user
		IF @userEmpNo > 0
		BEGIN

			SELECT @userEmail = RTRIM(a.EmpEmail)
			FROM tas.Master_Employee_JDE_View_V2 a
			WHERE a.EmpNo = @userEmpNo
		END 

		--Check for OT approval
		IF @otApproved = 'Y'
			SET @isOTApproved = 1
		ELSE 
			SET @isOTApproved = 0

		--Get the original overtime values before any data changes
		SELECT	@otStartTime_Orig = a.OTstartTime,
				@otEndTime_Orig = a.OTendTime,
				@otDuration_Orig = DATEDIFF(MINUTE, a.OTstartTime, a.OTendTime)
		FROM tas.Tran_Timesheet_Extra a
		WHERE XID_AutoID = @autoID

		IF @isOTApproved = 1	--Clerk granted overtime
		BEGIN

			--Get the Timesheet information
			SELECT	@totalWorkDuration = DATEDIFF(MINUTE, a.dtIN, a.dtOUT),
					@dtIN = a.dtIN,
					@dtOUT = a.dtOUT
			FROM tas.Tran_Timesheet a
			WHERE a.AutoID = @autoID

			--Get the work duration outside the shaving time in allowance
			/*
			SELECT @excessWorkDuration = CASE WHEN RTRIM(a.ShiftCode) = 'O' OR a.IsPublicHoliday = 1
											THEN 0
											ELSE 
												CASE WHEN DATEDIFF(n, CONVERT(TIME, a.dtIN), CONVERT(TIME, b.ArrivalTo)) > 0
													THEN DATEDIFF(n, CONVERT(TIME, a.dtIN), CONVERT(TIME, b.ArrivalTo)) 
													ELSE 0
												END 
										END
			FROM tas.Tran_Timesheet a
				LEFT JOIN tas.Master_ShiftTimes b ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode) 
					AND (CASE WHEN a.Duration_Required > 0 AND a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2))  --(Note: If total work duration is greater than or equals to the required work duration plus 1/2 of it then shift code will be based on the value of "Actual_ShiftCode" field)
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
							END) = RTRIM(b.ShiftCode)
			WHERE a.AutoID = @autoID
			*/

			--Calculate the overtime difference
			SELECT @otDifference = DateDiff(n, a.OTstartTime, a.OTendTime) 
			FROM tas.Tran_Timesheet_Extra a
			WHERE a.XID_AutoID = @autoID		
			
			IF @otDuration > 0 
			BEGIN
            
				IF @otReason IN ('CAL', 'CBD', 'CDF', 'CSR', 'COMS', 'COEW')
				BEGIN

					SELECT	@otStartTime = DATEADD(n, @otDuration * -1, a.OTEndTime),
							@otEndTime = a.OTEndTime,
							@otType = a.OTtype,
							@correctionCode = @otReason
					FROM tas.Tran_Timesheet_Extra a
					WHERE a.XID_AutoID = @autoID
				END

				ELSE
				BEGIN

					--Start of Rev. #1.3
					IF	@otDuration > @otDuration_Orig		--Check if the specifed overtime duration is greater than the original duration					
					BEGIN

						IF @otDuration = @totalWorkDuration
						BEGIN 

							--OT duration is equal to the total work duration
							--Therefore, set OTStartTime = dtIN and OTEndTime = dtOUT
							SELECT	@otStartTime = @dtIN,
									@otEndTime = @dtOUT,
									@otType = a.OTtype,
									@correctionCode = @otReason
							FROM tas.Tran_Timesheet_Extra a
							WHERE a.XID_AutoID = @autoID
						END 

						ELSE 
						BEGIN 
							
							--OT duration is greater than the original but less than the total work duration
							--Therefore, calculate OT Start Time backwards based on the OT End Time
							SELECT	@otStartTime = DATEADD(n, @otDuration * -1, a.OTendTime),
									@otEndTime = a.OTendTime,
									@otType = a.OTtype,
									@correctionCode = @otReason
							FROM tas.Tran_Timesheet_Extra a
							WHERE a.XID_AutoID = @autoID
						END 
                    END

					ELSE
                    BEGIN

						SELECT	@otStartTime = a.OTstartTime,
								@otEndTime = DATEADD(n, @otDuration, a.OTstartTime),
								@otType = a.OTtype,
								@correctionCode = @otReason
						FROM tas.Tran_Timesheet_Extra a
						WHERE a.XID_AutoID = @autoID
                    END 
                    --End of Rev. #1.3

					--Start of Rev. #1.2
					/*
					IF	@excessWorkDuration > 0
						AND @otDuration = @otDuration_Orig + @excessWorkDuration
					BEGIN

						--The specified duration is equal to the total OT duration plus excess work duration,
						--Therefore, calculate OT Start Time backwards based on OT End Time
						SELECT	@otStartTime = DATEADD(n, @otDuration * -1, a.OTendTime),
								@otEndTime = a.OTendTime,
								@otType = a.OTtype,
								@correctionCode = @otReason
						FROM tas.Tran_Timesheet_Extra a
						WHERE a.XID_AutoID = @autoID
                    END
                    
					ELSE
                    BEGIN
                    
						SELECT	@otStartTime = a.OTstartTime,
								@otEndTime = DATEADD(n, @otDuration, a.OTstartTime),
								@otType = a.OTtype,
								@correctionCode = @otReason
						FROM tas.Tran_Timesheet_Extra a
						WHERE a.XID_AutoID = @autoID
					END 
					--End of Rev. #1.2
					*/
				END
			END

            ELSE
            BEGIN

				SELECT	@otStartTime = NULL,	--Rev. #1.4
						@otEndTime = NULL,
						@otType = NULL,
						@correctionCode = @otReason
				FROM tas.Tran_Timesheet_Extra a
				WHERE a.XID_AutoID = @autoID
            END 
		END

		ELSE IF @isOTApproved = 0	--Clerk did not give overtime
		BEGIN

			SELECT	@otStartTime = NULL,
					@otEndTime = NULL,
					@otType = NULL,
					@correctionCode = @otReason
			FROM tas.Tran_Timesheet_Extra a
			WHERE a.XID_AutoID = @autoID
		END

		--Update the overtime table
		UPDATE tas.Tran_Timesheet_Extra
		SET OTstartTime = CASE WHEN @isOTApproved = 1 THEN @otStartTime ELSE OTstartTime END,
			OTendTime = CASE WHEN @isOTApproved = 1 THEN @otEndTime ELSE OTendTime END,
			Comment = @comment,
			OTApproved = @otApproved,
			OTReason = @otReason,
			Approved = NULL,
			LastUpdateUser = @userID,
			LastUpdateTime = GETDATE()
		WHERE XID_AutoID = @autoID

		--Get the number of affected overtime records
		SELECT @rowsAffected_OTDetail = @@rowcount

		--Checks for error
		IF @@ERROR <> @CONST_RETURN_OK
		BEGIN
				
			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1
		END

		-- Checks if there's no error
		IF @retError = @CONST_RETURN_OK
		BEGIN

			--Get the "Request Sent" status
			SELECT	@statusID	= UDCID,
					@statusCode = RTRIM(UDCCode), 
					@statusDesc = RTRIM(UDCDesc1),
					@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
			FROM tas.syJDE_UserDefinedCode a	
			WHERE RTRIM(a.UDCCode) = '02'
				AND a.UDCUDCGID = 9

			--Insert record to "OvertimeRequest" table
			INSERT INTO tas.OvertimeRequest
			(
				EmpNo,
				DT,
				TS_AutoID,
				CostCenter,
				OTStartTime,
				OTEndTime,
				OTType,
				CorrectionCode,
				MealVoucherEligibility,
				OTApproved,
				OTReason,
				OTComment,
				StatusID,
				StatusCode,
				StatusDesc,
				StatusHandlingCode,
				IsSubmittedForApproval,
				SubmittedDate,
				CreatedDate,
				CreatedByEmpNo,
				CreatedByUserID,
				CreatedByEmpName,
				CreatedByEmail,
				OTStartTime_Orig,
				OTEndTime_Orig
			)
			SELECT	a.EmpNo,
					a.DT,
					a.AutoID,
					c.BusinessUnit,
					@otStartTime,
					@otEndTime,
					@otType,
					@correctionCode,
					@mealVoucherEligibilityCode,
					@otApproved,
					@otReason,
					@comment,
					@statusID,
					@statusCode,
					@statusDesc,
					@statusHandlingCode,
					1,
					GETDATE(),
					GETDATE(),
					@userEmpNo,
					@userID,
					@userEmpName,
					@userEmail,
					@otStartTime_Orig,
					@otEndTime_Orig
			FROM tas.Tran_Timesheet a
				INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
				INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo	--Rev. #1.5
			WHERE a.AutoID = @autoID

			--Get the identity seed
			SELECT @otRequestNo = SCOPE_IDENTITY()

			IF @otRequestNo > 0
			BEGIN

				--Get the submission date
				SELECT	@requestSubmissionDate = SubmittedDate
				FROM tas.OvertimeRequest a
				WHERE a.OTRequestNo = @otRequestNo
            END 

			--Get the number of inserted overtime request record
			SELECT @rowsAffected_OTRequest = @@rowcount

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
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
			@rowsAffected_OTDetail AS OvertimeRowsAffected,
			@rowsAffected_OTRequest AS OvertimeRequestRowsAffected,
			@otRequestNo AS OTRequestNo,
			@requestSubmissionDate AS RequestSubmissionDate		--Rev. #1.1


/*	Debugging:

PARAMETERS:
	@autoID						INT, 	
	@otReason					VARCHAR(10),	
	@comment					VARCHAR(1000),
	@userID						VARCHAR(30), 
	@userEmpNo					INT,
	@otApproved					VARCHAR(1) = '0', 
	@mealVoucherEligibilityCode	VARCHAR(10) = NULL,
	@otDuration					INT = 0

	EXEC tas.Pr_InsertUpdateDeleteOvertime_Clerk

*/

/*	Checking:

	SELECT * FROM tas.OvertimeRequest a

	SELECT	b.*, 
		a.EmpNo,
		a.BusinessUnit,
		a.DT,
		a.dtIN,
		a.dtOUT,
		a.OTStartTime,
		a.OTEndTime,
		a.OTType,
		a.MealVoucherEligibility,
		a.* 
	FROM tas.Tran_Timesheet a 
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
	WHERE a.EmpNo = 10002160
		AND a.IsLastRow = 1
		AND a.DT IN ('11/01/2016', '10/27/2016')

	
	BEGIN TRAN T1

	UPDATE tas.OvertimeRequest
	SET StatusID = 113,
		StatusCode = '02',
		StatusDesc = 'Request Sent',
		StatusHandlingCode = 'Open',
		IsSubmittedForApproval = 1,
		SubmittedByEmpNo = 10003632,
		SubmittedDate = GETDATE()
	WHERE OTRequestNo = 1

	UPDATE tas.Tran_Timesheet_Extra
	SET	Approved = 0,
		LastUpdateUser = NULL,
		LastUpdateTime = NULL,
		OTApproved = 0,
		OTReason = ''
	WHERE XID_AutoID = 4832477

	UPDATE tas.Tran_Timesheet
	SET	OTStartTime =NULL,
		OTEndTime = NULL,
		OTType = NULL,
		LastUpdateUser = NULL,
		LastUpdateTime = NULL
	WHERE AutoID = 4832477

	COMMIT TRAN T1
	ROLLBACK TRAN T1

*/