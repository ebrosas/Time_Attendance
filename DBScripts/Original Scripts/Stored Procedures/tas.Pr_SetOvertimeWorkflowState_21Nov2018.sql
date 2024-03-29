USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_SetOvertimeWorkflowState]    Script Date: 21/11/2018 11:25:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_SetOvertimeWorkflowState
*	Description: This stored procedure is used to process the workflow of the "Overtime Online Approval System"
*
*	Date			Author		Rev.#		Comments:
*	26/07/2017		Ervin		1.0			Created
*	06/09/2017		Ervin		1.1			Disable the line of code that set SubmittedDate = NULL in the Cancel operation
*	10/09/2017		Ervin		1.2			Added code to add Routine History record if an approver already approved the OT request
*	12/09/2017		Ervin		1.3			Added logic to bypass the workflow to next level if the creator of the request is a Shift Supervisor
*	16/11/2017		Ervin		1.4			Fixed bug reported by Mustafa wherein the Last Update Emp. Name field in the grid does not show the correct name of the last person who approved and closed the workflow
*	20/11/2017		Ervin		1.5			Modified the workflow stage "Approval by Head of Department / Shift Supervisor". Checks if the creator is the Supervisor, if true then requisition will be assigned to Head of Department only
*	12/12/2017		Ervin		1.6			Set the workflow into Closed status if the OT reason code is "Remove OT - Not Entitled (RONE)" 
*	24/12/2017		Ervin		1.7			Added "@otComment" parameter. Update the value of "OTComment" field if the @otComment parameter is not NULL. Add rountine history for any changes in the original OT remarks.
*	27/12/2017		Ervin		1.8			Added functionality to customized the workflow to "CREATOR => SR. SUPERVISOR => HR Validator" if the OT reason code is any of the following: ROCS, RODO, ROMA
*	03/01/2018		Ervin		1.9			Set the "IsDelivered" flag to 1 to avoid incorrect sending of system notification		
*	03/01/2018		Ervin		2.0			Added filter to check if the creator is a Senior Supervisor. If true, then don't create "Approval by Senior Supervisor" workflow activity
*	21/01/2018		Ervin		2.1			Set the value of "IsHold" to NULL
*****************************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_SetOvertimeWorkflowState]
(
	@actionType				TINYINT,	--(Note: 1 = Create workflow; 2 = Get next activity; 3 = Undo request submission; 4 = Reassign to Other Approver) 
	@otRequestNo			BIGINT,
	@tsAutoID				INT,
	@currentUserID			VARCHAR(50),
	@createdByEmpNo			INT = 0,
	@createdByEmpName		VARCHAR(50) = '',
	@assigneeEmpNo			INT = 0,
	@assigneeEmpName		VARCHAR(50) = '',
	@isApproved				BIT = NULL,
	@appRemarks				VARCHAR(300) = NULL,
	@requestSubmissionDate	DATETIME = NULL,
	@otComment				VARCHAR(1000) = NULL
)
AS
	
	--Define constants
	DECLARE @CONST_RETURN_OK			INT,
			@CONST_RETURN_ERROR			INT

	--Initialize constants
	SELECT	@CONST_RETURN_OK			= 0,
			@CONST_RETURN_ERROR			= -1

	--Define workflow status variables
	DECLARE	@statusID					INT,
			@statusCode					VARCHAR(10),
			@statusDesc					VARCHAR(200),
			@statusHandlingCode			VARCHAR(50),
			@currentlyAssignedEmpNo		INT,
			@currentlyAssignedEmpName	VARCHAR(50),
			@currentlyAssignedEmpEmail	VARCHAR(50),
			@serviceProviderTypeCode	VARCHAR(10),
			@emailSourceName			VARCHAR(30),
			@emailCCRecipient			VARCHAR(200),
			@emailCCRecipientType		INT,
			@wfModuleCode				VARCHAR(10),
			@sequenceType				INT,
			@actionMemberType			TINYINT,
			@approvalRole				VARCHAR(500),
			@bypassIfAlreadyApproved	BIT,
			@isBypassApprover			BIT,
			@currentActionRole			INT,
			@nextActionRole				INT,
			@correctionCode				VARCHAR(10),
			@origOTComment				VARCHAR(1000),
			@isOTCommentModified		BIT 

	--Initialize workflow status variables
	SELECT	@statusID					= NULL,
			@statusCode					= NULL,
			@statusDesc					= NULL,
			@statusHandlingCode			= NULL,
			@currentlyAssignedEmpNo		= NULL,
			@currentlyAssignedEmpName	= NULL,
			@currentlyAssignedEmpEmail	= NULL,			
			@serviceProviderTypeCode	= NULL,
			@emailSourceName			= '',
			@emailCCRecipient			= '',
			@emailCCRecipientType		= 0,
			@wfModuleCode				= NULL,
			@sequenceType				= 0,
			@actionMemberType			= 0,
			@approvalRole				= '',
			@bypassIfAlreadyApproved	= 0,
			@isBypassApprover			= 0,
			@currentActionRole			= 0,
			@nextActionRole				= 0,
			@correctionCode				= NULL,
			@origOTComment				= NULL,
			@isOTCommentModified		= 0
			
	--Define variables
	DECLARE	@isWorkflowCompleted				BIT,
			@isWFUpdateSuccess					BIT,
			@hasError							BIT,
			@retError							INT,
			@rowsAffected						INT,
			@retErrorDesc						VARCHAR(200),	
			@currentWorkflowTransactionID		BIGINT,
			@currentRequestSubmissionDate		DATETIME,
			@currentSequenceNo					INT,
			@currentActivityCode				VARCHAR(20),
			@currentActionMemberCode			VARCHAR(10),
			@nextActivityCode					VARCHAR(20),
			@tempNextActivityCode				VARCHAR(20),
			@nextActionMemberCode				VARCHAR(10),
			@empNo								INT,
			@costCenter							VARCHAR(12),
			@isFinalAct							BIT,			--This flag determines whether the current activity is the final one hence closing the workflow process
			@syncWorkplaceToTimesheet			BIT,			--This flag determines whether the workplace swipe will be processed in the Timesheet
			@timeSheetRecordProcessed			INT,			--Refers to the number of affected records in the "Tran_Timesheet" table
			@overtimeDetailProcessed			INT,			--Refers to the number of affected records in the "Tran_Timesheet_Extra" table
			@overtimeRequestProcessed			INT,			--Refers to the number of affected records in the "OvertimeRequest" table	
			@otApproved							VARCHAR(1),		--Flag that determines whether overtime is granted
			@groupCode							VARCHAR(3),
			@isCreatorSupervisor				BIT,			--Flag that determines whether the creator of the request is a Shift Supervisor
			@loopCounter						INT,			--Used to customized the workflow that applies to the following OT correction codes: ROCS, RODO, ROMA
			@maxWFTransID						INT,			--Used to customized the workflow that applies to the following OT correction codes: ROCS, RODO, ROMA
			@activityCode						VARCHAR(20),	--Used to customized the workflow that applies to the following OT correction codes: ROCS, RODO, ROMA
			@newSequenceNo						INT				--Used to customized the workflow that applies to the following OT correction codes: ROCS, RODO, ROMA

	--Initialize variables
	SELECT	@isWorkflowCompleted				= 0,
			@isWFUpdateSuccess					= 0,
			@hasError							= 0,
			@retError							= @CONST_RETURN_OK,
			@rowsAffected						= 0,
			@retErrorDesc						= '',
			@currentWorkflowTransactionID		= 0,
			@currentRequestSubmissionDate		= NULL,
			@currentSequenceNo					= 0,
			@currentActivityCode				= '',
			@currentActionMemberCode			= '',
			@nextActivityCode					= '',
			@tempNextActivityCode				= '',
			@nextActionMemberCode				= '',
			@empNo								= 0,
			@costCenter							= '',
			@isFinalAct							= 0,
			@syncWorkplaceToTimesheet			= 0,
			@timeSheetRecordProcessed			= 0,
			@overtimeDetailProcessed			= 0,
			@overtimeRequestProcessed			= 0,
			@otApproved							= NULL,
			@groupCode							= NULL,
			@isCreatorSupervisor				= 0

	--Start a transaction
	--BEGIN TRANSACTION

	BEGIN TRY

		--Validate parameters
		IF ISNULL(@requestSubmissionDate, '') = ''
			SET @requestSubmissionDate = NULL

		--Get the Employee No. and Cost Center
		SELECT	@empNo = a.EmpNo,
				@costCenter = RTRIM(a.CostCenter),
				@correctionCode = RTRIM(a.CorrectionCode)
		FROM tas.OvertimeRequest a
		WHERE a.OTRequestNo = @otRequestNo

		--Get the original overtime comments entered by the Clerk
		SELECT @origOTComment = Comment
		FROM tas.OvertimeRequest a
			INNER JOIN tas.Tran_Timesheet_Extra b ON a.TS_AutoID = b.XID_AutoID
		WHERE a.OTRequestNo = @otRequestNo

		/**************************************************************************************
			Rev. #1.6 - Check if the OT Reason code is "Remove OT - Not Entitled (RONE)"
		**************************************************************************************/
		IF @correctionCode = 'RONE'
		BEGIN
			
			--Set the requisition into close status
			SET @isWorkflowCompleted = 1

			--Get the "Closed By User" status
			SELECT	@statusID	= UDCID,
					@statusCode = RTRIM(UDCCode), 
					@statusDesc = RTRIM(UDCDesc1),
					@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
			FROM tas.syJDE_UserDefinedCode a	
			WHERE RTRIM(a.UDCCode) = '99'
				AND a.UDCUDCGID = 9

			--Insert history record
			INSERT INTO tas.OvertimeWFRoutineHistory
			(						
				OTRequestNo,
				TS_AutoID,
				RequestSubmissionDate,
				HistDesc,
				HistCreatedBy,
				HistCreatedName,
				HistCreatedDate
			)
			SELECT	@otRequestNo,
					@tsAutoID,
					@requestSubmissionDate,
					ISNULL(@statusDesc, '') AS HistDesc, 
					@createdByEmpNo AS HistCreatedBy,
					@createdByEmpName AS HistCreatedName,
					GETDATE() AS HistCreatedDate

			--Update the overtime record
			UPDATE tas.OvertimeRequest
			SET	StatusID = @statusID,
				StatusCode = @statusCode,
				StatusDesc = @statusDesc,
				StatusHandlingCode = @statusHandlingCode,
				CurrentlyAssignedEmpNo = NULL,
				CurrentlyAssignedEmpName = NULL,
				CurrentlyAssignedEmpEmail = NULL,
				ServiceProviderTypeCode = NULL,
				DistListCode = NULL,
				IsClosed = 1,
				ClosedDate = GETDATE(),
				LastUpdateTime = GETDATE(),
				LastUpdateEmpNo = @createdByEmpNo,
				LastUpdateEmpName = @createdByEmpName,
				LastUpdateUserID = @currentUserID
			WHERE OTRequestNo = @otRequestNo

			--Get the number of affected overtime request records 
			SELECT @overtimeRequestProcessed = @@ROWCOUNT

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Determine whether overtime is granted
				SELECT @otApproved = a.OTApproved
				FROM tas.OvertimeRequest a
				WHERE a.OTRequestNo = @otRequestNo

				--Update the Timesheet record
				UPDATE tas.Tran_TimeSheet 
				SET OTstartTime = NULL, 
					OTendTime = NULL,
					OTType = NULL,
					CorrectionCode = a.CorrectionCode, 
					Processed = 0,
					LastUpdateUser = @currentUserID, 
					LastUpdateTime = GETDATE()
				FROM tas.OvertimeRequest a
				WHERE Tran_TimeSheet.AutoID = a.TS_AutoID
					AND a.OTRequestNo = @otRequestNo

				--Get the number of affected Timesheet records 
				SELECT @timeSheetRecordProcessed = @@ROWCOUNT

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Update the overtime record
					UPDATE tas.Tran_Timesheet_Extra 
					SET	Approved = 0,
						LastUpdateUser = @currentUserID, 
						LastUpdateTime = GETDATE()
					FROM tas.OvertimeRequest a
					WHERE Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
						AND a.OTRequestNo = @otRequestNo

					--Get the number of updated Overtime Details records 
					SELECT @overtimeDetailProcessed = @@ROWCOUNT
				END 
			END 

			--Exit the process
			GOTO EXIT_POINT
		END
		/*********************************** End of Rev. #1.6 ************************************/

		IF ISNULL(@costCenter, '') <> ''
		BEGIN
        
			--Get the cost center group code
			SELECT @groupCode = RTRIM(a.GroupCode)
			FROM tas.Master_BusinessUnit_JDE_V2 a
			WHERE RTRIM(a.BusinessUnit) = @costCenter

			IF ISNULL(@groupCode, '') <> ''
			BEGIN
            
				--Get the WF template to use based on the group code and cost center
				SELECT @wfModuleCode = RTRIM(a.WFModuleCode)
				FROM tas.OvertimeWFCostCenterMapping a
				WHERE 
					(
						(RTRIM(a.CostCenter) = @costCenter AND a.IsWFByCostCenter = 1)
						OR
						(RTRIM(a.GroupCode) = @groupCode AND ISNULL(a.IsWFByCostCenter, 0) = 0)
					)
					AND a.IsActive = 1
			END 
		END 

		IF ISNULL(@wfModuleCode, '') = ''
		BEGIN

			SELECT	@retError = @CONST_RETURN_ERROR,
					@hasError = 1, 
					@retError = 69, 
					@retErrorDesc = 'No workflow approval process has been configured for ' + @costCenter + ' cost center.'
        END 

		IF @actionType = 1			--Create the workflow
		BEGIN

			SET @currentSequenceNo = 1

			IF NOT EXISTS
			(
				SELECT WorkflowTransactionID 
				FROM tas.OvertimeWFTransactionActivity
				WHERE OTRequestNo = @otRequestNo	
					AND TS_AutoID = @tsAutoID
					AND RequestSubmissionDate = @requestSubmissionDate
					AND RTRIM(WFModuleCode) = @wfModuleCode
			)
			BEGIN

				IF @correctionCode IN	--Rev. #1.8
				(
					'ROMA',		--Remove OT Manager approval    
					'ROCS',		--Remove OT-Change Shift        
					'RODO'		--Remove OT-Day Off             
				)
				BEGIN

					IF @wfModuleCode IN ('OTADM7300', 'OTADM7250', 'OTADM7200')		--Stores, Medical, and Safety
					BEGIN
                    
						--Copy the workflow template into the transaction table (Workflow: Creator => HR Validator)
						INSERT INTO tas.OvertimeWFTransactionActivity
						(
							OTRequestNo,
							TS_AutoID,
							WFModuleCode,
							ActivityCode,
							NextActivityCode,
							ActivityDesc1,
							ActivityDesc2,
							WFActivityTypeCode,
							SequenceNo,
							SequenceType,
							ApprovalType,
							ActionRole,
							ActionMemberCode,
							ActionMemberType,
							ServiceProviderTypeCode,
							ParameterSourceTable,
							ParameterName,
							ParameterDataType,
							ConditionCheckValue,
							ConditionCheckDataType,
							EmailSourceName,
							EmailCCRecipient,
							EmailCCRecipientType,
							IsCurrent,
							IsCompleted,
							IsFinalAct,
							ActStatusID,
							RequestSubmissionDate,
							BypassIfAlreadyApproved,
							CreatedByUser,
							CreatedDate,
							CreatedByUserEmpNo,
							CreatedByUserEmpName
						)
						SELECT	@otRequestNo,
								@tsAutoID,
								@wfModuleCode,
								ActivityCode,
								NextActivityCode,
								ActivityDesc1,
								ActivityDesc2,
								WFActivityTypeCode,
								SequenceNo,
								SequenceType,
								ApprovalType,
								ActionRole,
								ActionMemberCode,
								ActionMemberType,
								ServiceProviderTypeCode,
								ParameterSourceTable,
								ParameterName,
								ParameterDataType,
								ConditionCheckValue,
								ConditionCheckDataType,
								EmailSourceName,
								EmailCCRecipient,
								EmailCCRecipientType,
								NULL AS IsCurrent,
								NULL AS IsCompleted,
								IsFinalAct,
								106 AS ActStatusID,
								@requestSubmissionDate,
								BypassIfAlreadyApproved,
								'' AS CreatedByUser,
								GETDATE(),
								@createdByEmpNo,
								@createdByEmpName
						FROM tas.OvertimeWFActivityTemplate a
						WHERE RTRIM(a.WFModuleCode) = @wfModuleCode
							AND 
							(
								RTRIM(a.ActivityCode) LIKE '%HRVALIDATOR%'
								OR RTRIM(a.ActivityCode) LIKE '%ENDWF%'
							)
						ORDER BY a.SequenceNo

						/***********************************************************************************
							Loop through all the records in the workflow to reset the Sequence No.
						***********************************************************************************/
						--Initialize variables
						SELECT	@loopCounter		= 0, 
								@maxWFTransID		= 0,
								@activityCode		= '',
								@newSequenceNo		= 1

						SELECT	@loopCounter = MIN(a.WorkflowTransactionID), 
								@maxWFTransID = MAX(a.WorkflowTransactionID) 
						FROM tas.OvertimeWFTransactionActivity a
						WHERE a.OTRequestNo = @otRequestNo
							AND RTRIM(a.WFModuleCode) = @wfModuleCode
							AND 
							(
								RTRIM(a.ActivityCode) LIKE '%HRVALIDATOR%'
								OR RTRIM(a.ActivityCode) LIKE '%ENDWF%'
							)
						
						WHILE (@loopCounter IS NOT NULL AND @loopCounter <= @maxWFTransID)
						BEGIN

							SELECT @activityCode = RTRIM(a.ActivityCode)
							FROM tas.OvertimeWFTransactionActivity a
							WHERE a.OTRequestNo = @otRequestNo
								AND a.WorkflowTransactionID = @loopCounter
						   
						   --To handle gaps in the looping column value
							IF (@@ROWCOUNT = 0)
							BEGIN

								SET @loopCounter = @loopCounter + 1 
								CONTINUE
							END
 
							UPDATE tas.OvertimeWFTransactionActivity
							SET SequenceNo = @newSequenceNo 
							WHERE OTRequestNo = @otRequestNo
								AND WorkflowTransactionID = @loopCounter

							SELECT	@loopCounter = @loopCounter  + 1,
									@newSequenceNo = @newSequenceNo + 1        
						END
					END
                    
					ELSE	--Production, Engineering, QC & Lab
                    BEGIN

						--Copy the workflow template into the transaction table (Workflow Process: Creator => Sr. Supervisor => HR Validator)
						IF EXISTS
                        (
							SELECT a.EmpNo FROM tas.Vw_SeniorSupervisor a
							WHERE RTRIM(a.CostCenter) = @costCenter
						)
						BEGIN
                        
							IF NOT EXISTS 
							(
								SELECT a.EmpNo FROM tas.Vw_SeniorSupervisor a
								WHERE RTRIM(a.CostCenter) = @costCenter
									AND @createdByEmpNo IN (a.EmpNo)	--Rev. #2.0
							)
							BEGIN 
                            
								--Add "Approval by Senior Supervisor" workflow only if there is Sr. Supervisor found based on the Originator's cost center 
								INSERT INTO tas.OvertimeWFTransactionActivity
								(
									OTRequestNo,
									TS_AutoID,
									WFModuleCode,
									ActivityCode,
									NextActivityCode,
									ActivityDesc1,
									ActivityDesc2,
									WFActivityTypeCode,
									SequenceNo,
									SequenceType,
									ApprovalType,
									ActionRole,
									ActionMemberCode,
									ActionMemberType,
									ServiceProviderTypeCode,
									ParameterSourceTable,
									ParameterName,
									ParameterDataType,
									ConditionCheckValue,
									ConditionCheckDataType,
									EmailSourceName,
									EmailCCRecipient,
									EmailCCRecipientType,
									IsCurrent,
									IsCompleted,
									IsFinalAct,
									ActStatusID,
									RequestSubmissionDate,
									BypassIfAlreadyApproved,
									CreatedByUser,
									CreatedDate,
									CreatedByUserEmpNo,
									CreatedByUserEmpName
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@wfModuleCode,
										ActivityCode,
										NextActivityCode,
										'Approval by Senior Supervisor' AS ActivityDesc1,
										'Senior Supervisor' AS ActivityDesc2,								
										WFActivityTypeCode,
										SequenceNo,
										1 AS SequenceType,	--(Note: Set the sequence type to 1)
										ApprovalType,
										ActionRole,
										'SENIORSUPV' AS ActionMemberCode,	--(Note: Set the distribution list code to SENIORSUPV)
										ActionMemberType,
										ServiceProviderTypeCode,
										ParameterSourceTable,
										ParameterName,
										ParameterDataType,
										ConditionCheckValue,
										ConditionCheckDataType,
										EmailSourceName,
										EmailCCRecipient,
										EmailCCRecipientType,
										NULL AS IsCurrent,
										NULL AS IsCompleted,
										IsFinalAct,
										106 AS ActStatusID,
										@requestSubmissionDate,
										BypassIfAlreadyApproved,
										'' AS CreatedByUser,
										GETDATE(),
										@createdByEmpNo,
										@createdByEmpName
								FROM tas.OvertimeWFActivityTemplate a
								WHERE RTRIM(a.WFModuleCode) = @wfModuleCode
									AND (RTRIM(a.ActivityCode) LIKE '%HEADNSUPER%')
							END 
						END 

						--Add "Validation by HR" and "End of workflow process" activities
						INSERT INTO tas.OvertimeWFTransactionActivity
						(
							OTRequestNo,
							TS_AutoID,
							WFModuleCode,
							ActivityCode,
							NextActivityCode,
							ActivityDesc1,
							ActivityDesc2,
							WFActivityTypeCode,
							SequenceNo,
							SequenceType,
							ApprovalType,
							ActionRole,
							ActionMemberCode,
							ActionMemberType,
							ServiceProviderTypeCode,
							ParameterSourceTable,
							ParameterName,
							ParameterDataType,
							ConditionCheckValue,
							ConditionCheckDataType,
							EmailSourceName,
							EmailCCRecipient,
							EmailCCRecipientType,
							IsCurrent,
							IsCompleted,
							IsFinalAct,
							ActStatusID,
							RequestSubmissionDate,
							BypassIfAlreadyApproved,
							CreatedByUser,
							CreatedDate,
							CreatedByUserEmpNo,
							CreatedByUserEmpName
						)
						SELECT	@otRequestNo,
								@tsAutoID,
								@wfModuleCode,
								ActivityCode,
								NextActivityCode,
								a.ActivityDesc1,
								a.ActivityDesc2,								
								WFActivityTypeCode,
								SequenceNo,
								SequenceType,
								ApprovalType,
								ActionRole,
								ActionMemberCode,
								ActionMemberType,
								ServiceProviderTypeCode,
								ParameterSourceTable,
								ParameterName,
								ParameterDataType,
								ConditionCheckValue,
								ConditionCheckDataType,
								EmailSourceName,
								EmailCCRecipient,
								EmailCCRecipientType,
								NULL AS IsCurrent,
								NULL AS IsCompleted,
								IsFinalAct,
								106 AS ActStatusID,
								@requestSubmissionDate,
								BypassIfAlreadyApproved,
								'' AS CreatedByUser,
								GETDATE(),
								@createdByEmpNo,
								@createdByEmpName
						FROM tas.OvertimeWFActivityTemplate a
						WHERE RTRIM(a.WFModuleCode) = @wfModuleCode
							AND 
							(
								RTRIM(a.ActivityCode) LIKE '%HRVALIDATOR%'
								OR RTRIM(a.ActivityCode) LIKE '%ENDWF%'
							)
						ORDER BY a.SequenceNo

						/***********************************************************************************
							Loop through all the records in the workflow to reset the Sequence No.
						***********************************************************************************/
						--Initialize variables
						SELECT	@loopCounter		= 0, 
								@maxWFTransID		= 0,
								@activityCode		= '',
								@newSequenceNo		= 1

						SELECT	@loopCounter = MIN(a.WorkflowTransactionID), 
								@maxWFTransID = MAX(a.WorkflowTransactionID) 
						FROM tas.OvertimeWFTransactionActivity a
						WHERE a.OTRequestNo = @otRequestNo
							AND RTRIM(a.WFModuleCode) = @wfModuleCode
							AND 
							(
								RTRIM(a.ActivityCode) LIKE '%HEADNSUPER%'
								OR RTRIM(a.ActivityCode) LIKE '%HRVALIDATOR%'
								OR RTRIM(a.ActivityCode) LIKE '%ENDWF%'
							)
						
						WHILE (@loopCounter IS NOT NULL AND @loopCounter <= @maxWFTransID)
						BEGIN

							SELECT @activityCode = RTRIM(a.ActivityCode)
							FROM tas.OvertimeWFTransactionActivity a
							WHERE a.OTRequestNo = @otRequestNo
								AND a.WorkflowTransactionID = @loopCounter
						   
						   --To handle gaps in the looping column value
							IF (@@ROWCOUNT = 0)
							BEGIN

								SET @loopCounter = @loopCounter + 1 
								CONTINUE
							END
 
							UPDATE tas.OvertimeWFTransactionActivity
							SET SequenceNo = @newSequenceNo 
							WHERE OTRequestNo = @otRequestNo
								AND WorkflowTransactionID = @loopCounter

							SELECT	@loopCounter = @loopCounter  + 1,
									@newSequenceNo = @newSequenceNo + 1        
						END
                    END 
                END 

				ELSE
                BEGIN
                
					--Copy the workflow template into the transaction table
					INSERT INTO tas.OvertimeWFTransactionActivity
					(
						OTRequestNo,
						TS_AutoID,
						WFModuleCode,
						ActivityCode,
						NextActivityCode,
						ActivityDesc1,
						ActivityDesc2,
						WFActivityTypeCode,
						SequenceNo,
						SequenceType,
						ApprovalType,
						ActionRole,
						ActionMemberCode,
						ActionMemberType,
						ServiceProviderTypeCode,
						ParameterSourceTable,
						ParameterName,
						ParameterDataType,
						ConditionCheckValue,
						ConditionCheckDataType,
						EmailSourceName,
						EmailCCRecipient,
						EmailCCRecipientType,
						IsCurrent,
						IsCompleted,
						IsFinalAct,
						ActStatusID,
						RequestSubmissionDate,
						BypassIfAlreadyApproved,
						CreatedByUser,
						CreatedDate,
						CreatedByUserEmpNo,
						CreatedByUserEmpName
					)
					SELECT	@otRequestNo,
							@tsAutoID,
							@wfModuleCode,
							ActivityCode,
							NextActivityCode,
							ActivityDesc1,
							ActivityDesc2,
							WFActivityTypeCode,
							SequenceNo,
							SequenceType,
							ApprovalType,
							ActionRole,
							ActionMemberCode,
							ActionMemberType,
							ServiceProviderTypeCode,
							ParameterSourceTable,
							ParameterName,
							ParameterDataType,
							ConditionCheckValue,
							ConditionCheckDataType,
							EmailSourceName,
							EmailCCRecipient,
							EmailCCRecipientType,
							NULL AS IsCurrent,
							NULL AS IsCompleted,
							IsFinalAct,
							106 AS ActStatusID,
							@requestSubmissionDate,
							BypassIfAlreadyApproved,
							'' AS CreatedByUser,
							GETDATE(),
							@createdByEmpNo,
							@createdByEmpName
					FROM tas.OvertimeWFActivityTemplate a
					WHERE RTRIM(a.WFModuleCode) = @wfModuleCode
					ORDER BY a.SequenceNo
				END
                
				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Start of Rev. #1.3
					IF @groupCode IN ('PRD', 'ENG', 'QC', 'S&M') 
					BEGIN

						--Determine if the creator is a Shift Supervisor only when the originator's cost center group code is any of the ff: Production, Engineering, QC, and Security
						SELECT @isCreatorSupervisor = tas.fnCheckIfCreatorIsSupervisor(@createdByEmpNo) 
					END 

					--Part of Rev. #1.5
					--IF @isCreatorSupervisor = 1
					--BEGIN

					--	--Bypass the first WF activity
					--	UPDATE tas.OvertimeWFTransactionActivity
					--	SET IsCurrent = 1,
					--		IsCompleted = 1,
					--		ActStatusID = 108
					--	WHERE OTRequestNo = @otRequestNo
					--		AND TS_AutoID = @tsAutoID
					--		AND RTRIM(WFModuleCode) = @wfModuleCode
					--		AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
					--		AND SequenceNo = @currentSequenceNo

					--	--Increment the sequence no.
					--	SET @currentSequenceNo = 2

					--	--Set the current WF activity
					--	UPDATE tas.OvertimeWFTransactionActivity
					--	SET IsCurrent = 1,
					--		IsCompleted = 0,
					--		ActStatusID = 107
					--	WHERE OTRequestNo = @otRequestNo
					--		AND TS_AutoID = @tsAutoID
					--		AND RTRIM(WFModuleCode) = @wfModuleCode
					--		AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
					--		AND SequenceNo = @currentSequenceNo
     --               END
                    
					--ELSE
     --               BEGIN 

						--Set the current WF activity
						UPDATE tas.OvertimeWFTransactionActivity
						SET IsCurrent = 1,
							IsCompleted = 0,
							ActStatusID = 107
						WHERE OTRequestNo = @otRequestNo
							AND TS_AutoID = @tsAutoID
							AND RTRIM(WFModuleCode) = @wfModuleCode
							AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
							AND SequenceNo = @currentSequenceNo
					--END 

					--Get the number of rows affected
					SELECT @rowsAffected = @@ROWCOUNT

					IF @rowsAffected > 0
					BEGIN

						--Get the current workflow information
						SELECT	@currentWorkflowTransactionID = WorkflowTransactionID,
								@currentRequestSubmissionDate = RequestSubmissionDate,
								@emailSourceName = RTRIM(EmailSourceName),
								@emailCCRecipient = RTRIM(EmailCCRecipient),
								@emailCCRecipientType = EmailCCRecipientType,
								@currentSequenceNo = SequenceNo,
								@currentActivityCode = RTRIM(ActivityCode),
								@currentActionMemberCode = RTRIM(ActionMemberCode),
								@serviceProviderTypeCode = RTRIM(a.ServiceProviderTypeCode),
								@sequenceType = a.SequenceType,
								@actionMemberType = a.ActionMemberType
						FROM tas.OvertimeWFTransactionActivity a
						WHERE OTRequestNo = @otRequestNo
							AND TS_AutoID = @tsAutoID
							AND RTRIM(WFModuleCode) = @wfModuleCode
							AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
							AND IsCurrent = 1
							AND ISNULL(IsCompleted, 0) = 0

						IF ISNULL(@currentActionMemberCode, '') <> ''
						BEGIN

							IF @sequenceType = 2	--The currently assignee consist of multiple persons
							BEGIN
                            
								IF @actionMemberType = 3	--Distribution List Group (Note: Action members will be fetched from the Common Admin System)
								BEGIN
                                
									--Insert email delivery record
									INSERT INTO tas.OvertimeWFEmailDelivery
									(								
										OTRequestNo,
										TS_AutoID,
										RequestSubmissionDate,
										CurrentlyAssignedEmpNo,
										CurrentlyAssignedEmpName,
										CurrentlyAssignedEmpEmail,
										ActivityCode,
										ActionMemberCode,
										EmailSourceName,
										EmailCCRecipient,
										EmailCCRecipientType,
										IsDelivered,
										CreatedByEmpNo,
										CreatedByEmpName,
										CreatedDate
									)
									SELECT	@otRequestNo,
											@tsAutoID,
											@currentRequestSubmissionDate,
											a.EmpNo,
											a.EmpName,
											a.EmpEmail,
											@currentActivityCode,
											@currentActionMemberCode,
											@emailSourceName,
											@emailCCRecipient,
											@emailCCRecipientType,
											0 AS IsDelivered,
											@createdByEmpNo,
											@createdByEmpName,
											GETDATE()
									FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, 'ALL', 0) a

									--Populate distribution member table
									INSERT INTO tas.OvertimeDistributionMember
									(
										OTRequestNo,
										WorkflowTransactionID,
										EmpNo,
										EmpName,
										EmpEmail,
										CreatedDate,
										CreatedByEmpNo,
										CreatedByUserID
									)
									SELECT	@otRequestNo,
											@currentWorkflowTransactionID,
											a.EmpNo,
											a.EmpName,
											a.EmpEmail,
											GETDATE(),
											@createdByEmpNo,
											@currentUserID
									FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, 'ALL', 0) a
								END

								ELSE IF @actionMemberType = 1	--Builtin Distribution Group 
								BEGIN

									--Part of Rev. #1.5
									IF	@isCreatorSupervisor = 1 
										AND RTRIM(@currentActionMemberCode) = 'HEADNSUPER'
									BEGIN

										--Insert email delivery record
										INSERT INTO tas.OvertimeWFEmailDelivery
										(								
											OTRequestNo,
											TS_AutoID,
											RequestSubmissionDate,
											CurrentlyAssignedEmpNo,
											CurrentlyAssignedEmpName,
											CurrentlyAssignedEmpEmail,
											ActivityCode,
											ActionMemberCode,
											EmailSourceName,
											EmailCCRecipient,
											EmailCCRecipientType,
											IsDelivered,
											CreatedByEmpNo,
											CreatedByEmpName,
											CreatedDate
										)
										SELECT	@otRequestNo,
												@tsAutoID,
												@currentRequestSubmissionDate,
												a.EmpNo,
												a.EmpName,
												a.EmpEmail,
												@currentActivityCode,
												@currentActionMemberCode,
												@emailSourceName,
												@emailCCRecipient,
												@emailCCRecipientType,
												0 AS IsDelivered,
												@createdByEmpNo,
												@createdByEmpName,
												GETDATE()
										FROM tas.fnGetWFActionMemberOvertimeRequest('CCSUPERDNT', @costCenter, 0, @otRequestNo) a

										--Populate distribution member table
										INSERT INTO tas.OvertimeDistributionMember
										(
											OTRequestNo,
											WorkflowTransactionID,
											EmpNo,
											EmpName,
											EmpEmail,
											CreatedDate,
											CreatedByEmpNo,
											CreatedByUserID
										)
										SELECT	@otRequestNo,
												@currentWorkflowTransactionID,
												a.EmpNo,
												a.EmpName,
												a.EmpEmail,
												GETDATE(),
												@createdByEmpNo,
												@currentUserID
										FROM tas.fnGetWFActionMemberOvertimeRequest('CCSUPERDNT', @costCenter, 0, @otRequestNo) a
                                    END
                                    
									ELSE
                                    BEGIN
                                    
										--Insert email delivery record
										INSERT INTO tas.OvertimeWFEmailDelivery
										(								
											OTRequestNo,
											TS_AutoID,
											RequestSubmissionDate,
											CurrentlyAssignedEmpNo,
											CurrentlyAssignedEmpName,
											CurrentlyAssignedEmpEmail,
											ActivityCode,
											ActionMemberCode,
											EmailSourceName,
											EmailCCRecipient,
											EmailCCRecipientType,
											IsDelivered,
											CreatedByEmpNo,
											CreatedByEmpName,
											CreatedDate
										)
										SELECT	@otRequestNo,
												@tsAutoID,
												@currentRequestSubmissionDate,
												a.EmpNo,
												a.EmpName,
												a.EmpEmail,
												@currentActivityCode,
												@currentActionMemberCode,
												@emailSourceName,
												@emailCCRecipient,
												@emailCCRecipientType,
												0 AS IsDelivered,
												@createdByEmpNo,
												@createdByEmpName,
												GETDATE()
										FROM tas.fnGetWFActionMemberOvertimeRequest(@currentActionMemberCode, @costCenter, 0, @otRequestNo) a

										--Populate distribution member table
										INSERT INTO tas.OvertimeDistributionMember
										(
											OTRequestNo,
											WorkflowTransactionID,
											EmpNo,
											EmpName,
											EmpEmail,
											CreatedDate,
											CreatedByEmpNo,
											CreatedByUserID
										)
										SELECT	@otRequestNo,
												@currentWorkflowTransactionID,
												a.EmpNo,
												a.EmpName,
												a.EmpEmail,
												GETDATE(),
												@createdByEmpNo,
												@currentUserID
										FROM tas.fnGetWFActionMemberOvertimeRequest(@currentActionMemberCode, @costCenter, 0, @otRequestNo) a
									END 
								END 

                                ELSE
                                BEGIN

									--Get the action members for the Builtin Group							
									SELECT	@currentlyAssignedEmpNo		= EmpNo,
											@currentlyAssignedEmpName	= EmpName,
											@currentlyAssignedEmpEmail	= EmpEmail										
									FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, '', 0) a

									--Insert email delivery record
									INSERT INTO tas.OvertimeWFEmailDelivery
									(								
										OTRequestNo,
										TS_AutoID,
										RequestSubmissionDate,
										CurrentlyAssignedEmpNo,
										CurrentlyAssignedEmpName,
										CurrentlyAssignedEmpEmail,
										ActivityCode,
										ActionMemberCode,
										EmailSourceName,
										EmailCCRecipient,
										EmailCCRecipientType,
										IsDelivered,
										CreatedByEmpNo,
										CreatedByEmpName,
										CreatedDate
									)
									SELECT	@otRequestNo,
											@tsAutoID,
											@currentRequestSubmissionDate,
											a.EmpNo,
											a.EmpName,
											a.EmpEmail,
											@currentActivityCode,
											@currentActionMemberCode,
											@emailSourceName,
											@emailCCRecipient,
											@emailCCRecipientType,
											0 AS IsDelivered,
											@createdByEmpNo,
											@createdByEmpName,
											GETDATE()
									FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, '', 0) a

									--Populate distribution member table
									INSERT INTO tas.OvertimeDistributionMember
									(
										OTRequestNo,
										WorkflowTransactionID,
										EmpNo,
										EmpName,
										EmpEmail,
										CreatedDate,
										CreatedByEmpNo,
										CreatedByUserID
									)
									SELECT	@otRequestNo,
											@currentWorkflowTransactionID,
											a.EmpNo,
											a.EmpName,
											a.EmpEmail,
											GETDATE(),
											@createdByEmpNo,
											@currentUserID
									FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, '', 0) a
                                END 
							END 

							ELSE		--The default assignee is a single person
                            BEGIN

								--Get the currently assigned employee							
								SELECT	@currentlyAssignedEmpNo		= EmpNo,
										@currentlyAssignedEmpName	= EmpName,
										@currentlyAssignedEmpEmail	= EmpEmail										
								FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, @costCenter, @empNo)

								--Insert email delivery record
								INSERT INTO tas.OvertimeWFEmailDelivery
								(								
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									CurrentlyAssignedEmpNo,
									CurrentlyAssignedEmpName,
									CurrentlyAssignedEmpEmail,
									ActivityCode,
									ActionMemberCode,
									EmailSourceName,
									EmailCCRecipient,
									EmailCCRecipientType,
									IsDelivered,
									CreatedByEmpNo,
									CreatedByEmpName,
									CreatedDate
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										@currentlyAssignedEmpNo,
										@currentlyAssignedEmpName,
										@currentlyAssignedEmpEmail,
										@currentActivityCode,
										@currentActionMemberCode,
										@emailSourceName,
										@emailCCRecipient,
										@emailCCRecipientType,
										0 AS IsDelivered,
										@createdByEmpNo,
										@createdByEmpName,
										GETDATE()
                            END 

							--Checks for error
							IF @@ERROR <> @CONST_RETURN_OK
							BEGIN
				
								SELECT	@retError = @CONST_RETURN_ERROR,
										@hasError = 1
							END

							-- Checks if there's no error
							IF @retError = @CONST_RETURN_OK
							BEGIN

								--Set the flag
								SET @isWFUpdateSuccess = 1
							END
						END
					END
				END
			END
		END

		ELSE IF @actionType = 2		--Get the next workflow activity
		BEGIN

			--Get the current workflow information
			SELECT	@currentWorkflowTransactionID = WorkflowTransactionID,
					@currentRequestSubmissionDate = RequestSubmissionDate,
					@emailSourceName = RTRIM(EmailSourceName),
					@emailCCRecipient = RTRIM(EmailCCRecipient),
					@emailCCRecipientType = EmailCCRecipientType,
					@currentSequenceNo = SequenceNo,
					@approvalRole = RTRIM(a.ActivityDesc2),
					@currentActionRole = a.ActionRole
			FROM tas.OvertimeWFTransactionActivity a
			WHERE 
				a.OTRequestNo = @otRequestNo
				AND a.TS_AutoID = @tsAutoID
				AND RTRIM(WFModuleCode) = @wfModuleCode
				AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
				AND a.IsCurrent = 1
				AND ISNULL(a.IsCompleted, 0) = 0

			IF @currentWorkflowTransactionID > 0
			BEGIN

				--Close the current workflow activity
				UPDATE tas.OvertimeWFTransactionActivity
				SET IsCompleted = 1,
					ActStatusID = 109,
					LastUpdateTime = GETDATE(),
					LastUpdateEmpNo = @assigneeEmpNo,
					LastUpdateEmpName = @assigneeEmpName
				WHERE WorkflowTransactionID = @currentWorkflowTransactionID

				--Set the "IsDelivered" flag to 1 to avoid incorrect sending of system notification (Rev. #1.9)
				UPDATE tas.OvertimeWFEmailDelivery
				SET IsDelivered = 1
				WHERE OTRequestNo = @otRequestNo

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					IF @isApproved = 1
					BEGIN

SKIP_HERE_IF_BYPASSED:
						--Get the next workflow activity 
						--Check first if the current WF activity has value in the "NextActivityCode" fields
						SELECT	@tempNextActivityCode = RTRIM(a.NextActivityCode)				
						FROM tas.OvertimeWFTransactionActivity a
						WHERE WorkflowTransactionID = @currentWorkflowTransactionID

						IF ISNULL(@tempNextActivityCode, '') <> ''
						BEGIN
                        
							SELECT	@nextActivityCode = RTRIM(ActivityCode),
									@isFinalAct = ISNULL(IsFinalAct, 0),
									@bypassIfAlreadyApproved = a.BypassIfAlreadyApproved							
							FROM tas.OvertimeWFTransactionActivity a
							WHERE 
								a.OTRequestNo = @otRequestNo
								AND a.TS_AutoID = @tsAutoID
								AND RTRIM(a.WFModuleCode) = @wfModuleCode
								AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
								AND RTRIM(a.ActivityCode) = @tempNextActivityCode
						END 

						ELSE
                        BEGIN

							SELECT	@nextActivityCode = RTRIM(ActivityCode),
									@isFinalAct = ISNULL(IsFinalAct, 0),
									@bypassIfAlreadyApproved = BypassIfAlreadyApproved														
							FROM tas.OvertimeWFTransactionActivity 
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RTRIM(WFModuleCode) = @wfModuleCode
								AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
								AND SequenceNo = @currentSequenceNo + 1
                        END 

						--Check if the current WF activity is the final one. If true, then close the workflow.
						IF @isFinalAct = 1 
						BEGIN

							--Close the workflow
							UPDATE tas.OvertimeWFTransactionActivity
							SET IsCurrent = 1,
								IsCompleted = 1,
								ActStatusID = 109,
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @assigneeEmpNo,
								LastUpdateEmpName = @assigneeEmpName
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RTRIM(WFModuleCode) = @wfModuleCode
								AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
								AND RTRIM(ActivityCode) = @nextActivityCode

							--Set the flags
							SELECT	@isWFUpdateSuccess = 1,
									@isWorkflowCompleted = 1
						END

						ELSE
						BEGIN

							--Set the flags for the next activity
							UPDATE tas.OvertimeWFTransactionActivity
							SET IsCurrent = 1,
								IsCompleted = 0,
								ActStatusID = 107
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RTRIM(WFModuleCode) = @wfModuleCode
								AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
								AND RTRIM(ActivityCode) = @nextActivityCode

							--Checks for error
							IF @@ERROR <> @CONST_RETURN_OK
							BEGIN
				
								SELECT	@retError = @CONST_RETURN_ERROR,
										@hasError = 1
							END

							-- Checks if there's no error
							IF @retError = @CONST_RETURN_OK
							BEGIN

								--Get the next action member code and other details
								SELECT	@nextActionMemberCode = RTRIM(a.ActionMemberCode),
										@emailSourceName = RTRIM(a.EmailSourceName),
										@emailCCRecipient = RTRIM(a.EmailCCRecipient),
										@emailCCRecipientType = a.EmailCCRecipientType,
										@sequenceType = a.SequenceType,
										@actionMemberType = a.ActionMemberType,
										@nextActionRole = a.ActionRole,
										@bypassIfAlreadyApproved = a.BypassIfAlreadyApproved
								FROM tas.OvertimeWFTransactionActivity a
								WHERE 
									OTRequestNo = @otRequestNo
									AND TS_AutoID = @tsAutoID
									AND RTRIM(WFModuleCode) = @wfModuleCode
									AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
									AND IsCurrent = 1
									AND ISNULL(IsCompleted, 0) = 0

								IF ISNULL(@nextActionMemberCode, '') <> ''
								BEGIN

									IF @sequenceType = 2	--The currently assignee consist of multiple persons
									BEGIN
                            
										IF @actionMemberType = 3	--Distribution Group to be fetched from the Common Admin System
										BEGIN
                                
											--Insert email delivery record
											INSERT INTO tas.OvertimeWFEmailDelivery
											(								
												OTRequestNo,
												TS_AutoID,
												RequestSubmissionDate,
												CurrentlyAssignedEmpNo,
												CurrentlyAssignedEmpName,
												CurrentlyAssignedEmpEmail,
												ActivityCode,
												ActionMemberCode,
												EmailSourceName,
												EmailCCRecipient,
												EmailCCRecipientType,
												IsDelivered,
												CreatedByEmpNo,
												CreatedByEmpName,
												CreatedDate
											)
											SELECT	@otRequestNo,
													@tsAutoID,
													@currentRequestSubmissionDate,
													a.EmpNo,
													a.EmpName,
													a.EmpEmail,
													@currentActivityCode,
													@currentActionMemberCode,
													@emailSourceName,
													@emailCCRecipient,
													@emailCCRecipientType,
													0 AS IsDelivered,
													@createdByEmpNo,
													@createdByEmpName,
													GETDATE()
											FROM tas.fnGetActionMember_WithSubstitute_V2(@nextActionMemberCode, 'ALL', 0) a

											--Populate distribution member table
											INSERT INTO tas.OvertimeDistributionMember
											(
												OTRequestNo,
												WorkflowTransactionID,
												EmpNo,
												EmpName,
												EmpEmail,
												CreatedDate,
												CreatedByEmpNo,
												CreatedByUserID
											)
											SELECT	@otRequestNo,
													@currentWorkflowTransactionID,
													a.EmpNo,
													a.EmpName,
													a.EmpEmail,
													GETDATE(),
													@createdByEmpNo,
													@currentUserID
											FROM tas.fnGetActionMember_WithSubstitute_V2(@nextActionMemberCode, 'ALL', 0) a
										END

										ELSE
										BEGIN

											--Insert email delivery record
											INSERT INTO tas.OvertimeWFEmailDelivery
											(								
												OTRequestNo,
												TS_AutoID,
												RequestSubmissionDate,
												CurrentlyAssignedEmpNo,
												CurrentlyAssignedEmpName,
												CurrentlyAssignedEmpEmail,
												ActivityCode,
												ActionMemberCode,
												EmailSourceName,
												EmailCCRecipient,
												EmailCCRecipientType,
												IsDelivered,
												CreatedByEmpNo,
												CreatedByEmpName,
												CreatedDate
											)
											SELECT	@otRequestNo,
													@tsAutoID,
													@currentRequestSubmissionDate,
													a.EmpNo,
													a.EmpName,
													a.EmpEmail,
													@currentActivityCode,
													@currentActionMemberCode,
													@emailSourceName,
													@emailCCRecipient,
													@emailCCRecipientType,
													0 AS IsDelivered,
													@createdByEmpNo,
													@createdByEmpName,
													GETDATE()
											FROM tas.fnGetActionMember_WithSubstitute_V2(@nextActionMemberCode, @costCenter, 0) a

											--Populate distribution member table
											INSERT INTO tas.OvertimeDistributionMember
											(
												OTRequestNo,
												WorkflowTransactionID,
												EmpNo,
												EmpName,
												EmpEmail,
												CreatedDate,
												CreatedByEmpNo,
												CreatedByUserID
											)
											SELECT	@otRequestNo,
													@currentWorkflowTransactionID,
													a.EmpNo,
													a.EmpName,
													a.EmpEmail,
													GETDATE(),
													@createdByEmpNo,
													@currentUserID
											FROM tas.fnGetActionMember_WithSubstitute_V2(@nextActionMemberCode, @costCenter, 0) a
										END 
									END 

									ELSE	--The default assignee consist of 1 person
                                    BEGIN 

										SELECT	@currentlyAssignedEmpNo		= EmpNo,
												@currentlyAssignedEmpName	= EmpName,
												@currentlyAssignedEmpEmail	= EmpEmail										
										FROM tas.fnGetActionMember_WithSubstitute_V2(@nextActionMemberCode, @costCenter, @empNo)

										--Insert email delivery record
										INSERT INTO tas.OvertimeWFEmailDelivery
										(										
											OTRequestNo,
											TS_AutoID,
											RequestSubmissionDate,
											CurrentlyAssignedEmpNo,
											CurrentlyAssignedEmpName,
											CurrentlyAssignedEmpEmail,
											ActivityCode,
											ActionMemberCode,
											EmailSourceName,
											EmailCCRecipient,
											EmailCCRecipientType,
											IsDelivered,
											CreatedByEmpNo,
											CreatedByEmpName,
											CreatedDate
										)
										SELECT	@otRequestNo,
												@tsAutoID,
												@currentRequestSubmissionDate,
												@currentlyAssignedEmpNo,
												@currentlyAssignedEmpName,
												@currentlyAssignedEmpEmail,
												@nextActivityCode,
												@nextActionMemberCode,
												@emailSourceName,
												@emailCCRecipient,
												@emailCCRecipientType,
												0 AS IsDelivered,
												@assigneeEmpNo,
												@assigneeEmpName,
												GETDATE()
									END 

									--Checks for error
									IF @@ERROR <> @CONST_RETURN_OK
									BEGIN
				
										SELECT	@retError = @CONST_RETURN_ERROR,
												@hasError = 1
									END

									-- Checks if there's no error
									IF @retError = @CONST_RETURN_OK
									BEGIN

										--Set the flag
										SET @isWFUpdateSuccess = 1
									END
								END
							END
						END
					END

					ELSE
					BEGIN

						--Approver has rejected the request in the current workflow activity
						--Set the remaining activities status to 108 (Bypassed)
						UPDATE tas.OvertimeWFTransactionActivity
						SET IsCurrent = 0,
							IsCompleted = 0,
							ActStatusID = 108
						WHERE 
							OTRequestNo = @otRequestNo
							AND TS_AutoID = @tsAutoID
							AND RTRIM(WFModuleCode) = @wfModuleCode
							AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
							AND SequenceNo > @currentSequenceNo 

						--Get the minimum submission date
						/*
						DECLARE @minSubmissionDate DATETIME	
						SELECT TOP 1 @minSubmissionDate = RequestSubmissionDate
						FROM tas.OvertimeWFTransactionActivity
						WHERE 
							OTRequestNo = @otRequestNo
							AND TS_AutoID = @tsAutoID
							AND RTRIM(WFModuleCode) = @wfModuleCode
						ORDER BY RequestSubmissionDate ASC

						IF @minSubmissionDate IS NOT NULL
						BEGIN
							
							UPDATE tas.OvertimeRequest
							SET SubmittedDate = DATEADD(dd, -1, @minSubmissionDate)
							WHERE OTRequestNo = @otRequestNo

							UPDATE tas.OvertimeWFTransactionActivity
							SET RequestSubmissionDate = DATEADD(dd, -1, @minSubmissionDate)
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RTRIM(WFModuleCode) = @wfModuleCode
								AND RequestSubmissionDate = @requestSubmissionDate

							UPDATE tas.OvertimeWFEmailDelivery
							SET RequestSubmissionDate = DATEADD(dd, -1, @minSubmissionDate)
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RequestSubmissionDate = @requestSubmissionDate

							UPDATE tas.OvertimeWFRoutineHistory
							SET RequestSubmissionDate = DATEADD(dd, -1, @minSubmissionDate)
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RequestSubmissionDate = @requestSubmissionDate

							UPDATE tas.OvertimeWFApprovalHistory
							SET RequestSubmissionDate = DATEADD(dd, -1, @minSubmissionDate)
							WHERE 
								OTRequestNo = @otRequestNo
								AND TS_AutoID = @tsAutoID
								AND RequestSubmissionDate = @requestSubmissionDate

							SET @currentRequestSubmissionDate = DATEADD(dd, -1, @minSubmissionDate)
						END
						*/

						--Set the flags
						SELECT	@isWFUpdateSuccess = 1,
								@isWorkflowCompleted = 1
					END
				END
			END
		END

		ELSE IF @actionType = 3		--Undo request submission
		BEGIN

			IF EXISTS
			(
				SELECT WorkflowTransactionID 
				FROM tas.OvertimeWFTransactionActivity
				WHERE OTRequestNo = @otRequestNo 
					AND TS_AutoID = @tsAutoID
					AND RTRIM(WFModuleCode) = @wfModuleCode
					AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
			)
			BEGIN

				--Delete the workflow activities
				DELETE FROM tas.OvertimeWFTransactionActivity
				WHERE OTRequestNo = @otRequestNo 
					AND TS_AutoID = @tsAutoID
					AND RTRIM(WFModuleCode) = @wfModuleCode
					AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)

				--Delete the workflow history records
				DELETE FROM tas.OvertimeWFRoutineHistory
				WHERE OTRequestNo = @otRequestNo
					AND TS_AutoID = @tsAutoID
					AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)

				--Delete the workflow approval records
				DELETE FROM tas.OvertimeWFApprovalHistory
				WHERE OTRequestNo = @otRequestNo
					AND TS_AutoID = @tsAutoID
					AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)

				--Delete email delivery records
				DELETE FROM tas.OvertimeWFEmailDelivery
				WHERE OTRequestNo = @otRequestNo
					AND TS_AutoID = @tsAutoID
					AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
						
				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Reset the overtime request record 
					UPDATE tas.OvertimeRequest
					SET	StatusID = NULL,
						StatusCode = NULL,
						StatusDesc = NULL,
						StatusHandlingCode = NULL,
						CurrentlyAssignedEmpNo = NULL,
						CurrentlyAssignedEmpName = NULL,
						CurrentlyAssignedEmpEmail = NULL,
						ServiceProviderTypeCode = NULL,
						DistListCode = NULL,
						IsClosed = NULL,
						ClosedDate = NULL,
						IsSubmittedForApproval = NULL
						--SubmittedDate = NULL	--Rev. #1.1
					WHERE OTRequestNo = @otRequestNo

					--Get the number of updated overtime record
					SELECT @overtimeRequestProcessed = @@ROWCOUNT

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END
				END
			END
		END

		IF @actionType IN (1, 2)
		BEGIN

			/***********************************************************
				Update overtime records to set the workflow status
			***********************************************************/
			--Get the current workflow information
			SELECT	@currentRequestSubmissionDate = a.RequestSubmissionDate,
					@serviceProviderTypeCode = RTRIM(a.ServiceProviderTypeCode),
					@currentActionMemberCode = RTRIM(a.ActionMemberCode),
					@currentActionRole = a.ActionRole,
					@bypassIfAlreadyApproved = a.BypassIfAlreadyApproved
			FROM tas.OvertimeWFTransactionActivity a
			WHERE 
				a.OTRequestNo = @otRequestNo
				AND a.TS_AutoID = @tsAutoID
				AND RTRIM(a.WFModuleCode) = @wfModuleCode
				AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)	
				AND a.IsCurrent = 1
				AND ISNULL(a.IsCompleted, 0) = 0

			IF @isWorkflowCompleted = 1
			BEGIN

				IF @isApproved = 1
				BEGIN

					IF @otComment IS NOT NULL AND @otComment <> @origOTComment
					BEGIN

						--Set the flag
						SET @isOTCommentModified = 1

						--Insert history record for comments changed (Rev. #1.7)
						INSERT INTO tas.OvertimeWFRoutineHistory
						(						
							OTRequestNo,
							TS_AutoID,
							RequestSubmissionDate,
							HistDesc,
							HistCreatedBy,
							HistCreatedName,
							HistCreatedDate
						)
						SELECT	@otRequestNo,
								@tsAutoID,
								@currentRequestSubmissionDate,
								'Overtime remarks was modified by ' + UPPER(RTRIM(@assigneeEmpName)) AS HistDesc, 
								ISNULL(@assigneeEmpNo, 0) AS HistCreatedBy,
								@assigneeEmpName AS HistCreatedName,
								GETDATE() AS HistCreatedDate
                    END 

					--Get the "Closed by Approver" status
					SELECT	@statusID	= UDCID,
							@statusCode = RTRIM(UDCCode), 
							@statusDesc = RTRIM(UDCDesc1),
							@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
					FROM tas.syJDE_UserDefinedCode a	
					WHERE RTRIM(a.UDCCode) = '123'
						AND a.UDCUDCGID = 9

					--Insert history record
					INSERT INTO tas.OvertimeWFRoutineHistory
					(						
						OTRequestNo,
						TS_AutoID,
						RequestSubmissionDate,
						HistDesc,
						HistCreatedBy,
						HistCreatedName,
						HistCreatedDate
					)
					SELECT	@otRequestNo,
							@tsAutoID,
							@currentRequestSubmissionDate,
							ISNULL(@statusDesc, '') AS HistDesc, 
							ISNULL(@assigneeEmpNo, 0) AS HistCreatedBy,
							@assigneeEmpName AS HistCreatedName,
							GETDATE() AS HistCreatedDate

					--Insert Approval record
					INSERT INTO tas.OvertimeWFApprovalHistory
					(
						OTRequestNo,
						TS_AutoID,
						RequestSubmissionDate,
						AppApproved,
						AppRemarks,
						AppRoutineSeq,
						AppCreatedBy,
						AppCreatedName,
						AppCreatedDate,
						ApprovalRole,
						ActionRole
					)
					SELECT	@otRequestNo,
							@tsAutoID,
							@currentRequestSubmissionDate,
							ISNULL(@isApproved, 0) AS AppApproved,
							@appRemarks AS AppRemarks,
							@currentSequenceNo AS AppRoutineSeq,
							ISNULL(@assigneeEmpNo, 0) AS AppCreatedBy,
							@assigneeEmpName AS AppCreatedName,
							GETDATE() AS AppCreatedDate,
							@approvalRole,
							@currentActionRole

					--Update the overtime record
					UPDATE tas.OvertimeRequest
					SET	StatusID = @statusID,
						StatusCode = @statusCode,
						StatusDesc = @statusDesc,
						StatusHandlingCode = @statusHandlingCode,
						CurrentlyAssignedEmpNo = NULL,
						CurrentlyAssignedEmpName = NULL,
						CurrentlyAssignedEmpEmail = NULL,
						ServiceProviderTypeCode = NULL,
						DistListCode = NULL,
						IsClosed = 1,
						ClosedDate = GETDATE(),
						LastUpdateTime = GETDATE(),
						LastUpdateEmpNo = @assigneeEmpNo,
						LastUpdateEmpName = @assigneeEmpName,
						LastUpdateUserID = @currentUserID,
						OTComment = CASE WHEN @otComment IS NOT NULL THEN @otComment ELSE OTComment END,	--Rev. #1.7 
						IsOTCommentModified = @isOTCommentModified,
						IsHold = NULL	--Rev. #2.1
					WHERE OTRequestNo = @otRequestNo

					--Get the number of affected overtime request records 
					SELECT @overtimeRequestProcessed = @@ROWCOUNT

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END

					-- Checks if there's no error
					IF @retError = @CONST_RETURN_OK
					BEGIN

						--Determine whether overtime is granted
						SELECT @otApproved = a.OTApproved
						FROM tas.OvertimeRequest a
						WHERE a.OTRequestNo = @otRequestNo

						--Update the Timesheet record
						UPDATE tas.Tran_TimeSheet 
						SET OTstartTime = CASE WHEN @otApproved = 'Y' THEN a.OTstartTime ELSE NULL END, 
							OTendTime = CASE WHEN @otApproved = 'Y' THEN a.OTEndTime ELSE NULL END,
							OTType = CASE WHEN @otApproved = 'Y' THEN a.OTType ELSE NULL END,
							CorrectionCode = a.CorrectionCode, 
							MealVoucherEligibility = a.MealVoucherEligibility,
							Processed = 0,
							LastUpdateUser = @currentUserID, 
							LastUpdateTime = GETDATE()
						FROM tas.OvertimeRequest a
						WHERE  	
							Tran_TimeSheet.AutoID = a.TS_AutoID
							AND a.OTRequestNo = @otRequestNo

						--Get the number of affected Timesheet records 
						SELECT @timeSheetRecordProcessed = @@ROWCOUNT

						--Checks for error
						IF @@ERROR <> @CONST_RETURN_OK
						BEGIN
				
							SELECT	@retError = @CONST_RETURN_ERROR,
									@hasError = 1
						END

						-- Checks if there's no error
						IF @retError = @CONST_RETURN_OK
						BEGIN

							--Update the overtime record
							UPDATE tas.Tran_Timesheet_Extra 
							SET	Approved = CASE WHEN @otApproved = 'Y' THEN 1 ELSE 0 END,
								LastUpdateUser = @currentUserID, 
								LastUpdateTime = GETDATE()
							FROM tas.OvertimeRequest a
							WHERE  	
								Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
								AND a.OTRequestNo = @otRequestNo

							--Get the number of updated Overtime Details records 
							SELECT @overtimeDetailProcessed = @@ROWCOUNT
						END 
					END 
				END

				ELSE
				BEGIN

					IF @otComment IS NOT NULL AND @otComment <> @origOTComment
					BEGIN

						--Set the flag
						SET @isOTCommentModified = 1

						--Insert history record for comments changed (Rev. #1.7)
						INSERT INTO tas.OvertimeWFRoutineHistory
						(						
							OTRequestNo,
							TS_AutoID,
							RequestSubmissionDate,
							HistDesc,
							HistCreatedBy,
							HistCreatedName,
							HistCreatedDate
						)
						SELECT	@otRequestNo,
								@tsAutoID,
								@currentRequestSubmissionDate,
								'Overtime remarks was modified by ' + UPPER(RTRIM(@assigneeEmpName)) AS HistDesc, 
								ISNULL(@assigneeEmpNo, 0) AS HistCreatedBy,
								@assigneeEmpName AS HistCreatedName,
								GETDATE() AS HistCreatedDate
                    END 

					--Get the "Rejected By Approver" status
					SELECT	@statusID	= UDCID,
							@statusCode = RTRIM(UDCCode), 
							@statusDesc = RTRIM(UDCDesc1),
							@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
					FROM tas.syJDE_UserDefinedCode a	
					WHERE RTRIM(a.UDCCode) = '110'
						AND a.UDCUDCGID = 9

					--Insert "Rejected By Approver" History record
					INSERT INTO tas.OvertimeWFRoutineHistory
					(						
						OTRequestNo,
						TS_AutoID,
						RequestSubmissionDate,
						HistDesc,
						HistCreatedBy,
						HistCreatedName,
						HistCreatedDate
					)
					SELECT	@otRequestNo,
							@tsAutoID,
							@currentRequestSubmissionDate,
							'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@assigneeEmpName)) + ')' AS HistDesc, 
							@assigneeEmpNo AS HistCreatedBy,
							RTRIM(@assigneeEmpName) AS HistCreatedName,
							GETDATE() AS HistCreatedDate

					--Insert Approval record
					INSERT INTO tas.OvertimeWFApprovalHistory
					(						
						OTRequestNo,
						TS_AutoID,
						RequestSubmissionDate,
						AppApproved,
						AppRemarks,
						AppRoutineSeq,
						AppCreatedBy,
						AppCreatedName,
						AppCreatedDate,
						ApprovalRole,
						ActionRole
					)
					SELECT	@otRequestNo,
							@tsAutoID,
							@currentRequestSubmissionDate,
							ISNULL(@isApproved, 0) AS AppApproved,
							@appRemarks AS AppRemarks,
							@currentSequenceNo AS AppRoutineSeq,
							ISNULL(@assigneeEmpNo, 0) AS AppCreatedBy,
							@assigneeEmpName AS AppCreatedName,
							GETDATE() AS AppCreatedDate,
							@approvalRole,
							@currentActionRole

					--Update the overtime request record
					UPDATE tas.OvertimeRequest
					SET	StatusID = @statusID,
						StatusCode = @statusCode,
						StatusDesc = @statusDesc,
						StatusHandlingCode = @statusHandlingCode,
						CurrentlyAssignedEmpNo = NULL,
						CurrentlyAssignedEmpName = NULL,
						CurrentlyAssignedEmpEmail = NULL,
						ServiceProviderTypeCode = NULL,
						DistListCode = NULL,
						IsClosed = 1,
						ClosedDate = GETDATE(),
						LastUpdateTime = GETDATE(),
						LastUpdateEmpNo = @assigneeEmpNo,
						LastUpdateEmpName = @assigneeEmpName,	--Rev. #1.4
						LastUpdateUserID = @currentUserID,
						OTComment = CASE WHEN @otComment IS NOT NULL THEN @otComment ELSE OTComment END,	--Rev. #1.7
						IsOTCommentModified = @isOTCommentModified,
						IsHold = NULL	--Rev. #2.1
					WHERE OTRequestNo = @otRequestNo

					--Get the number of swipe records affected
					SELECT @overtimeRequestProcessed = @@ROWCOUNT	

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END

					-- Checks if there's no error
					IF @retError = @CONST_RETURN_OK
					BEGIN

						--Update "Tran_Timesheet_Extra" table to set overtime to unprocessed
						UPDATE tas.Tran_Timesheet_Extra
						SET Approved = 0,
							Tran_Timesheet_Extra.OTApproved = '0',
							OTReason = NULL,
							Comment = NULL,
							OTstartTime = a.OTStartTime_Orig,	
							OTendTime = a.OTEndTime_Orig,
							LastUpdateUser = @currentUserID, 
							LastUpdateTime = GETDATE()
						FROM tas.OvertimeRequest a
						WHERE 
							Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
							AND a.OTRequestNo = @otRequestNo 	
					END 
				END
			END

			ELSE
			BEGIN
			
				IF @isWFUpdateSuccess = 1
				BEGIN

					IF @actionType = 1	--Create workflow
					BEGIN
				
						--Get the "Request Sent" status
						SELECT	@statusID	= UDCID,
								@statusCode = RTRIM(UDCCode), 
								@statusDesc = RTRIM(UDCDesc1),
								@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
						FROM tas.syJDE_UserDefinedCode a	
						WHERE RTRIM(a.UDCCode) = '02'
							AND a.UDCUDCGID = 9

						--Insert "Request Sent" history record
						INSERT INTO tas.OvertimeWFRoutineHistory
						(
							OTRequestNo,
							TS_AutoID,
							RequestSubmissionDate,
							HistDesc,
							HistCreatedBy,
							HistCreatedName,
							HistCreatedDate
						)
						SELECT	@otRequestNo,
								@tsAutoID,
								@currentRequestSubmissionDate,
								'Open - ' + RTRIM(@statusDesc) AS HistDesc, 
								@createdByEmpNo AS HistCreatedBy,
								RTRIM(@createdByEmpName) AS HistCreatedName,
								GETDATE() AS HistCreatedDate

						--Get the "Waiting For Approval" status
						SELECT	@statusID	= UDCID,
								@statusCode = RTRIM(UDCCode), 
								@statusDesc = RTRIM(UDCDesc1),
								@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
						FROM tas.syJDE_UserDefinedCode a	
						WHERE RTRIM(a.UDCCode) = '05'
							AND a.UDCUDCGID = 9

						IF @sequenceType = 2	--The current assignee consist of multiple persons
						BEGIN
                            
							IF @actionMemberType = 3	--Distribution List Group (Note: Action members will be fetched from the Common Admin System)
							BEGIN

								--Insert "Waiting For Approval" history record
								INSERT INTO tas.OvertimeWFRoutineHistory
								(
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									HistDesc,
									HistCreatedBy,
									HistCreatedName,
									HistCreatedDate
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(a.EmpName)) + ')' AS HistDesc, 
										a.EmpNo  AS HistCreatedBy,
										a.EmpName AS HistCreatedName,
										GETDATE() AS HistCreatedDate	
								FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, 'ALL', 0) a
							END
							
							ELSE IF @actionMemberType = 1	--Builtin Distribution Group 
							BEGIN

								--Part of Rev. #1.5
								IF	@isCreatorSupervisor = 1 
									AND RTRIM(@currentActionMemberCode) = 'HEADNSUPER'
								BEGIN

									--Insert "Waiting For Approval" history record
									INSERT INTO tas.OvertimeWFRoutineHistory
									(
										OTRequestNo,
										TS_AutoID,
										RequestSubmissionDate,
										HistDesc,
										HistCreatedBy,
										HistCreatedName,
										HistCreatedDate
									)
									SELECT	@otRequestNo,
											@tsAutoID,
											@currentRequestSubmissionDate,
											'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(a.EmpName)) + ')' AS HistDesc, 
											a.EmpNo  AS HistCreatedBy,
											a.EmpName AS HistCreatedName,
											GETDATE() AS HistCreatedDate	
									FROM tas.fnGetWFActionMemberOvertimeRequest('CCSUPERDNT', @costCenter, 0, @otRequestNo) a
                                END

								ELSE
                                BEGIN 

									--Insert "Waiting For Approval" history record
									INSERT INTO tas.OvertimeWFRoutineHistory
									(
										OTRequestNo,
										TS_AutoID,
										RequestSubmissionDate,
										HistDesc,
										HistCreatedBy,
										HistCreatedName,
										HistCreatedDate
									)
									SELECT	@otRequestNo,
											@tsAutoID,
											@currentRequestSubmissionDate,
											'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(a.EmpName)) + ')' AS HistDesc, 
											a.EmpNo  AS HistCreatedBy,
											a.EmpName AS HistCreatedName,
											GETDATE() AS HistCreatedDate	
									FROM tas.fnGetWFActionMemberOvertimeRequest(@currentActionMemberCode, @costCenter, 0, @otRequestNo) a
								END 
							END
                            
							ELSE
                            BEGIN

								--Individual Employee						
								--Insert "Waiting For Approval" history record
								INSERT INTO tas.OvertimeWFRoutineHistory
								(
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									HistDesc,
									HistCreatedBy,
									HistCreatedName,
									HistCreatedDate
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(a.EmpName)) + ')' AS HistDesc, 
										a.EmpNo  AS HistCreatedBy,
										a.EmpName AS HistCreatedName,
										GETDATE() AS HistCreatedDate	
								FROM tas.fnGetActionMember_WithSubstitute_V2(@currentActionMemberCode, '', 0) a
                            END 

							--Update the overtime request
							UPDATE tas.OvertimeRequest
							SET	StatusID = @statusID,
								StatusCode = @statusCode,
								StatusDesc = @statusDesc,
								StatusHandlingCode = @statusHandlingCode,
								CurrentlyAssignedEmpNo = NULL,			--Note: Set to NULL if assignee is a distribution group
								CurrentlyAssignedEmpName = NULL,		--Note: Set to NULL if assignee is a distribution group
								CurrentlyAssignedEmpEmail = NULL,		--Note: Set to NULL if assignee is a distribution group
								ServiceProviderTypeCode = @serviceProviderTypeCode,
								DistListCode = @currentActionMemberCode,	
								IsClosed = NULL,
								ClosedDate = NULL,
								IsSubmittedForApproval = 1,
								SubmittedDate = @currentRequestSubmissionDate,
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @createdByEmpNo,
								LastUpdateUserID = @currentUserID,
								LastUpdateEmpName = @createdByEmpName
							WHERE OTRequestNo = @otRequestNo

							--Get the number of swipe records affected
							SELECT @overtimeRequestProcessed = @@ROWCOUNT	
                        END
                        
						ELSE
                        BEGIN
							
							--Note: The current assignee is a single person 

							--Insert "Waiting For Approval" history record
							INSERT INTO tas.OvertimeWFRoutineHistory
							(
								OTRequestNo,
								TS_AutoID,
								RequestSubmissionDate,
								HistDesc,
								HistCreatedBy,
								HistCreatedName,
								HistCreatedDate
							)
							SELECT	@otRequestNo,
									@tsAutoID,
									@currentRequestSubmissionDate,
									'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@currentlyAssignedEmpName)) + ')' AS HistDesc, 
									ISNULL(@currentlyAssignedEmpNo, 0) AS HistCreatedBy,
									ISNULL(@currentlyAssignedEmpName, '') AS HistCreatedName,
									GETDATE() AS HistCreatedDate	

							--Update the overtime request
							UPDATE tas.OvertimeRequest
							SET	StatusID = @statusID,
								StatusCode = @statusCode,
								StatusDesc = @statusDesc,
								StatusHandlingCode = @statusHandlingCode,
								CurrentlyAssignedEmpNo = @currentlyAssignedEmpNo,
								CurrentlyAssignedEmpName = @currentlyAssignedEmpName,
								CurrentlyAssignedEmpEmail = @currentlyAssignedEmpEmail,
								ServiceProviderTypeCode = @serviceProviderTypeCode,
								
								--DistListCode = NULL,	--Note: Set to NULL if current assigne is a single person
								DistListCode = @currentActionMemberCode,	--Rev. #1.8

								IsClosed = NULL,
								ClosedDate = NULL,
								IsSubmittedForApproval = 1,
								SubmittedDate = @currentRequestSubmissionDate,
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @createdByEmpNo,
								LastUpdateUserID = @currentUserID,
								LastUpdateEmpName = @createdByEmpName
							WHERE OTRequestNo = @otRequestNo

							--Get the number of swipe records affected
							SELECT @overtimeRequestProcessed = @@ROWCOUNT		
						END 																				
					END
			
					ELSE
					BEGIN

						IF @isApproved = 1
						BEGIN

							IF @otComment IS NOT NULL AND @otComment <> @origOTComment
							BEGIN

								--Set the flag
								SET @isOTCommentModified = 1

								--Insert history record for comments changed (Rev. #1.7)
								INSERT INTO tas.OvertimeWFRoutineHistory
								(						
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									HistDesc,
									HistCreatedBy,
									HistCreatedName,
									HistCreatedDate
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										'Overtime remarks was modified by ' + UPPER(RTRIM(@assigneeEmpName)) AS HistDesc, 
										ISNULL(@assigneeEmpNo, 0) AS HistCreatedBy,
										@assigneeEmpName AS HistCreatedName,
										GETDATE() AS HistCreatedDate
							END 

							--Get the "Approved By Approver" status
							SELECT	@statusID	= UDCID,
									@statusCode = RTRIM(UDCCode), 
									@statusDesc = RTRIM(UDCDesc1),
									@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
							FROM tas.syJDE_UserDefinedCode a	
							WHERE RTRIM(a.UDCCode) = '120'
								AND a.UDCUDCGID = 9

							--Insert "Approved By Approver" routine history record
							IF NOT EXISTS
                            (
								SELECT a.AutoID FROM tas.OvertimeWFRoutineHistory a
								WHERE a.OTRequestNo = @otRequestNo
									AND a.TS_AutoID = @tsAutoID
									AND a.RequestSubmissionDate = @currentRequestSubmissionDate
									AND RTRIM(a.HistDesc) = 'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@assigneeEmpName)) + ')'
							)
							BEGIN 

								INSERT INTO tas.OvertimeWFRoutineHistory
								(
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									HistDesc,
									HistCreatedBy,
									HistCreatedName,
									HistCreatedDate
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@assigneeEmpName)) + ')' AS HistDesc, 
										@assigneeEmpNo AS HistCreatedBy,
										RTRIM(@assigneeEmpName) AS HistCreatedName,
										GETDATE() AS HistCreatedDate
							END 

							--Get the "Assigned to Next Approver" status
							SELECT	@statusID	= UDCID,
									@statusCode = RTRIM(UDCCode), 
									@statusDesc = RTRIM(UDCDesc1),
									@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
							FROM tas.syJDE_UserDefinedCode a	
							WHERE RTRIM(a.UDCCode) = '121'
								AND a.UDCUDCGID = 9

							--Insert Approval record
							IF NOT EXISTS
                            (
								SELECT a.AutoID FROM tas.OvertimeWFApprovalHistory a
								WHERE a.OTRequestNo = @otRequestNo
									AND a.TS_AutoID = @tsAutoID
									AND a.RequestSubmissionDate = @currentRequestSubmissionDate
									AND RTRIM(a.ApprovalRole) = RTRIM(@approvalRole)
							)
							BEGIN 

								INSERT INTO tas.OvertimeWFApprovalHistory
								(
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									AppApproved,
									AppRemarks,
									AppRoutineSeq,
									AppCreatedBy,
									AppCreatedName,
									AppCreatedDate,
									ApprovalRole,
									ActionRole
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										ISNULL(@isApproved, 0) AS AppApproved,
										@appRemarks AS AppRemarks,
										@currentSequenceNo AS AppRoutineSeq,
										ISNULL(@assigneeEmpNo, 0) AS AppCreatedBy,
										@assigneeEmpName AS AppCreatedName,
										GETDATE() AS AppCreatedDate,
										@approvalRole,
										@currentActionRole
							END 

							--Check if the current workflow activity can be bypassed if current approver already approved the requisition
							--Note: This applies only to individual assignee
							IF @currentlyAssignedEmpNo > 0 AND @bypassIfAlreadyApproved = 1
							BEGIN
											
								SELECT @isBypassApprover = tas.fnCheckIfBypassOTApproval(@otRequestNo, @currentlyAssignedEmpNo, @currentActionRole) 
											
								IF @isBypassApprover = 1
								BEGIN

									--Set the flag to bypass the current WF activity
									UPDATE tas.OvertimeWFTransactionActivity
									SET IsCurrent = 0,
										IsCompleted = 1,
										ActStatusID = 108,
										LastUpdateTime = GETDATE(),
										LastUpdateEmpNo = @assigneeEmpNo,
										LastUpdateEmpName = @assigneeEmpName
									WHERE 
										OTRequestNo = @otRequestNo
										AND TS_AutoID = @tsAutoID
										AND RTRIM(WFModuleCode) = @wfModuleCode
										AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)	
										AND IsCurrent = 1
										AND ISNULL(IsCompleted, 0) = 0

									--Delete email delivery records
									UPDATE tas.OvertimeWFEmailDelivery
									SET IsDelivered = 1
									WHERE OTRequestNo = @otRequestNo
										AND TS_AutoID = @tsAutoID
										AND (RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)	
										AND RTRIM(ActivityCode) = @nextActivityCode

									--Set the current workflow activity information
									SELECT	@currentWorkflowTransactionID = a.WorkflowTransactionID,
											@currentSequenceNo = a.SequenceNo
									FROM tas.OvertimeWFTransactionActivity a
									WHERE a.OTRequestNo = @otRequestNo
										AND a.TS_AutoID = @tsAutoID
										AND RTRIM(a.WFModuleCode) = @wfModuleCode
										AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)
										AND RTRIM(ActivityCode) = @nextActivityCode	

									--Get the "Closed by Approver" status
									SELECT	@statusID	= UDCID,
											@statusCode = RTRIM(UDCCode), 
											@statusDesc = RTRIM(UDCDesc1),
											@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
									FROM tas.syJDE_UserDefinedCode a	
									WHERE RTRIM(a.UDCCode) = '123'
										AND a.UDCUDCGID = 9

									--Start of Rev. #1.2
									--Insert Routine History record if approver already approved the request
									INSERT INTO tas.OvertimeWFRoutineHistory
									(						
										OTRequestNo,
										TS_AutoID,
										RequestSubmissionDate,
										HistDesc,
										HistCreatedBy,
										HistCreatedName,
										HistCreatedDate
									)
									SELECT	@otRequestNo,
											@tsAutoID,
											@currentRequestSubmissionDate,
											'Bypassed ' + RTRIM(@currentlyAssignedEmpName) + ' (Employee already approved the request)' AS HistDesc, 
											ISNULL(@currentlyAssignedEmpNo, 0) AS HistCreatedBy, 
											@currentlyAssignedEmpName AS HistCreatedName,
											GETDATE() AS HistCreatedDate
									--End of Rev. #1.2

									GOTO SKIP_HERE_IF_BYPASSED
                                END 
							END 

							--Insert "Assigned to Next Approver" history record
							INSERT INTO tas.OvertimeWFRoutineHistory
							(
								OTRequestNo,
								TS_AutoID,
								RequestSubmissionDate,
								HistDesc,
								HistCreatedBy,
								HistCreatedName,
								HistCreatedDate
							)
							SELECT	@otRequestNo,
									@tsAutoID,
									@currentRequestSubmissionDate,
									'Status Changed - ' + RTRIM(@statusDesc) + ' (' + RTRIM(@currentlyAssignedEmpName) + ')' AS HistDesc, 
									ISNULL(@currentlyAssignedEmpNo, 0) AS HistCreatedBy,
									ISNULL(@currentlyAssignedEmpName, '') AS HistCreatedName,
									GETDATE() AS HistCreatedDate

							--Update the overtime record
							UPDATE tas.OvertimeRequest
							SET	StatusID = @statusID,
								StatusCode = @statusCode,
								StatusDesc = @statusDesc,
								StatusHandlingCode = @statusHandlingCode,
								CurrentlyAssignedEmpNo = @currentlyAssignedEmpNo,
								CurrentlyAssignedEmpName = @currentlyAssignedEmpName,
								CurrentlyAssignedEmpEmail = @currentlyAssignedEmpEmail,
								ServiceProviderTypeCode = @serviceProviderTypeCode,
								DistListCode = @currentActionMemberCode,
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @assigneeEmpNo,
								LastUpdateUserID = @currentUserID,
								LastUpdateEmpName = @assigneeEmpName,
								OTComment = CASE WHEN @otComment IS NOT NULL THEN @otComment ELSE OTComment END,	--Rev. #1.7
								IsOTCommentModified = @isOTCommentModified,
								IsHold = NULL	--Rev. #2.1
							WHERE OTRequestNo = @otRequestNo

							--Get the number of affected overtime records 
							SELECT @overtimeRequestProcessed = @@ROWCOUNT	
						END

						ELSE 
						BEGIN

							IF @otComment IS NOT NULL AND @otComment <> @origOTComment
							BEGIN

								--Set the flag
								SET @isOTCommentModified = 1

								--Insert history record for comments changed (Rev. #1.7)
								INSERT INTO tas.OvertimeWFRoutineHistory
								(						
									OTRequestNo,
									TS_AutoID,
									RequestSubmissionDate,
									HistDesc,
									HistCreatedBy,
									HistCreatedName,
									HistCreatedDate
								)
								SELECT	@otRequestNo,
										@tsAutoID,
										@currentRequestSubmissionDate,
										'Overtime remarks was modified by ' + UPPER(RTRIM(@assigneeEmpName)) AS HistDesc, 
										ISNULL(@assigneeEmpNo, 0) AS HistCreatedBy,
										@assigneeEmpName AS HistCreatedName,
										GETDATE() AS HistCreatedDate
							END 

							--Get the "Rejected By Approver" status
							SELECT	@statusID	= UDCID,
									@statusCode = RTRIM(UDCCode), 
									@statusDesc = RTRIM(UDCDesc1),
									@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
							FROM tas.syJDE_UserDefinedCode a	
							WHERE RTRIM(a.UDCCode) = '110'
								AND a.UDCUDCGID = 9

							--Insert "Rejected By Approver" History record
							INSERT INTO tas.OvertimeWFRoutineHistory
							(
								OTRequestNo,
								TS_AutoID,
								RequestSubmissionDate,
								HistDesc,
								HistCreatedBy,
								HistCreatedName,
								HistCreatedDate
							)
							SELECT	@otRequestNo,
									@tsAutoID,
									@currentRequestSubmissionDate,
									'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@assigneeEmpName)) + ')' AS HistDesc, 
									@assigneeEmpNo AS HistCreatedBy,
									RTRIM(@assigneeEmpName) AS HistCreatedName,
									GETDATE() AS HistCreatedDate

							--Insert Approval record
							INSERT INTO tas.OvertimeWFApprovalHistory
							(
								OTRequestNo,
								TS_AutoID,
								RequestSubmissionDate,
								AppApproved,
								AppRemarks,
								AppRoutineSeq,
								AppCreatedBy,
								AppCreatedName,
								AppCreatedDate,
								ApprovalRole,
								ActionRole
							)
							SELECT	@otRequestNo,
									@tsAutoID,
									@currentRequestSubmissionDate,
									ISNULL(@isApproved, 0) AS AppApproved,
									@appRemarks AS AppRemarks,
									@currentSequenceNo AS AppRoutineSeq,
									ISNULL(@assigneeEmpNo, 0) AS AppCreatedBy,
									@assigneeEmpName AS AppCreatedName,
									GETDATE() AS AppCreatedDate,
									@approvalRole,
									@currentActionRole

							--Update the overtime record
							UPDATE tas.OvertimeRequest
							SET	StatusID = @statusID,
								StatusCode = @statusCode,
								StatusDesc = @statusDesc,
								StatusHandlingCode = @statusHandlingCode,
								CurrentlyAssignedEmpNo = NULL,
								CurrentlyAssignedEmpName = NULL,
								CurrentlyAssignedEmpEmail = NULL,
								ServiceProviderTypeCode = NULL,
								DistListCode = NULL,
								IsClosed = 1,
								ClosedDate = GETDATE(),
								LastUpdateTime = GETDATE(),
								LastUpdateEmpNo = @assigneeEmpNo,
								LastUpdateUserID = @currentUserID,
								LastUpdateEmpName = @assigneeEmpName,
								OTComment = CASE WHEN @otComment IS NOT NULL THEN @otComment ELSE OTComment END,	--Rev. #1.7
								IsOTCommentModified = @isOTCommentModified,
								IsHold = NULL	--Rev. #2.1
							WHERE OTRequestNo = @otRequestNo

							--Get the number of swipe records affected
							SELECT @overtimeRequestProcessed = @@ROWCOUNT	

							--Checks for error
							IF @@ERROR <> @CONST_RETURN_OK
							BEGIN
				
								SELECT	@retError = @CONST_RETURN_ERROR,
										@hasError = 1
							END

							-- Checks if there's no error
							IF @retError = @CONST_RETURN_OK
							BEGIN

								--Update "Tran_Timesheet_Extra" table to set overtime to unprocessed
								UPDATE tas.Tran_Timesheet_Extra
								SET Approved = 0,
									Tran_Timesheet_Extra.OTApproved = '0',
									OTReason = NULL,
									Comment = NULL,
									OTstartTime = a.OTStartTime_Orig,	
									OTendTime = a.OTEndTime_Orig,
									LastUpdateUser = @currentUserID, 
									LastUpdateTime = GETDATE()
								FROM tas.OvertimeRequest a
								WHERE 
									Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
									AND a.OTRequestNo = @otRequestNo 	
							END 
						END												
					END
				END
			END

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END
		END

		ELSE IF @actionType = 4		--Reassign to Other Approver
		BEGIN

			--Get the employee info
			IF ISNULL(@assigneeEmpNo, 0) > 0
			BEGIN
				
				--Get the current workflow information
				SELECT	@currentRequestSubmissionDate = RequestSubmissionDate,
						@serviceProviderTypeCode = RTRIM(a.ServiceProviderTypeCode),
						@currentActionMemberCode = RTRIM(ActionMemberCode)
				FROM tas.OvertimeWFTransactionActivity a
				WHERE 
					a.OTRequestNo = @otRequestNo
					AND a.TS_AutoID = @tsAutoID
					AND RTRIM(a.WFModuleCode) = @wfModuleCode
					AND (a.RequestSubmissionDate = @requestSubmissionDate OR @requestSubmissionDate IS NULL)	--Rev. #1.5
					AND a.IsCurrent = 1
					AND ISNULL(a.IsCompleted, 0) = 0

				--Get the approver employee information
				SELECT	@currentlyAssignedEmpNo = a.EmpNo,
						@currentlyAssignedEmpName = RTRIM(a.EmpName),
						@currentlyAssignedEmpEmail = LTRIM(RTRIM(b.EAEMAL))
				FROM tas.Master_Employee_JDE_View a
					LEFT JOIN tas.syJDE_F01151 b ON a.EmpNo = b.EAAN8 AND b.EAIDLN = 0 AND b.EARCK7 = 1 AND UPPER(LTRIM(RTRIM(b.EAETP))) = 'E' 
				WHERE EmpNo = @assigneeEmpNo

				--Get the "Reassigned to Other Approver" status
				SELECT	@statusID	= UDCID,
						@statusCode = RTRIM(UDCCode), 
						@statusDesc = RTRIM(UDCDesc1),
						@statusHandlingCode = RTRIM(UDCSpecialHandlingCode)
				FROM tas.syJDE_UserDefinedCode a	
				WHERE RTRIM(a.UDCCode) = '122'
					AND a.UDCUDCGID = 9

				--Insert "Reassigned to Other Approver" history record
				INSERT INTO tas.OvertimeWFRoutineHistory
				(
					OTRequestNo,
					TS_AutoID,
					RequestSubmissionDate,
					HistDesc,
					HistCreatedBy,
					HistCreatedName,
					HistCreatedDate
				)
				SELECT	@otRequestNo,
						@tsAutoID,
						@currentRequestSubmissionDate,

						--Start of Rev. #1.5
						CASE WHEN ISNULL(@appRemarks, '') <> '' 
							THEN 'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@currentlyAssignedEmpName)) + ')' + ' - ' + RTRIM(@appRemarks)  
							ELSE 'Status Changed - ' + RTRIM(@statusDesc) + ' (' + UPPER(RTRIM(@currentlyAssignedEmpName)) + ')'
						END AS HistDesc, 
						--End of Rev. #1.5

						ISNULL(@currentlyAssignedEmpNo, 0) AS HistCreatedBy,
						ISNULL(@currentlyAssignedEmpName, '') AS HistCreatedName,
						GETDATE() AS HistCreatedDate

				--Update the overtime record
				UPDATE tas.OvertimeRequest
				SET	StatusID = @statusID,
					StatusCode = @statusCode,
					StatusDesc = @statusDesc,
					StatusHandlingCode = @statusHandlingCode,
					CurrentlyAssignedEmpNo = @currentlyAssignedEmpNo,
					CurrentlyAssignedEmpName = @currentlyAssignedEmpName,
					CurrentlyAssignedEmpEmail = @currentlyAssignedEmpEmail,
					ServiceProviderTypeCode = @serviceProviderTypeCode,
					DistListCode = @currentActionMemberCode,
					LastUpdateTime = GETDATE(),
					LastUpdateEmpNo = @createdByEmpNo,
					LastUpdateUserID = @currentUserID,
					LastUpdateEmpName = @createdByEmpName
				WHERE OTRequestNo = @otRequestNo

				--Get the number of swipe records affected
				SELECT @overtimeRequestProcessed = @@ROWCOUNT	
			END
		END

	END TRY
	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

EXIT_POINT:

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

	--Return worflow update results to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@overtimeRequestProcessed AS OTRequestRecordProcessed,
			@timeSheetRecordProcessed AS TimeSheetRecordProcessed,
			@overtimeDetailProcessed AS OTDetailRecordProcessed,
			@isWorkflowCompleted AS IsWorkflowCompleted,
			@currentlyAssignedEmpNo AS CurrentlyAssignedEmpNo,
			@currentlyAssignedEmpName AS CurrentlyAssignedEmpName,
			@currentlyAssignedEmpEmail AS CurrentlyAssignedEmpEmail,
			@emailSourceName AS EmailSourceName,
			@emailCCRecipient AS EmailCCRecipient,
			@emailCCRecipientType AS EmailCCRecipientType

