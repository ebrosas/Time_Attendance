USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_RemoveIncorrectOvertime]    Script Date: 19/10/2017 13:43:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/***************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_RemoveIncorrectOvertime
*	Description: This stored procedure is used to remove the incorrect overtime records
*
*	Date			Author		Revision No.	Comments:
*	27/07/2017		Ervin		1.0				Created
*	09/08/2017		Ervin		1.1				Added @actionType = 3 which is used to undo removal of incorrect OT records based on the temporary table
*	10/08/2017		Ervin		1.2				Added "@payPeriodStartDate" and "@payPeriodEndDate" parameters
*	29/08/2017		Ervin		1.3				Added filter condition that checks if "ShiftSpan_AwardOT" and "ShiftSpan_XID" are null
*	27/09/2017		Ervin		1.4				Added Process #4 - Check for corrected and approved missing swipes but overtime cannot be seen in the OT Approval Form in TAS
*	27/09/2017		Ervin		1.5				Added filter condition that checks if the employee worked double shift with gaps in between shift  
*	09/10/2017		Ervin		1.6				Added Process #5 - Check for missing overtime for employees who worked on 12 hour shift
**************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_RemoveIncorrectOvertime]
(	
	@actionType				TINYINT,		--(Note: 1 = Remove Incorrect Overtime; 2 = Undo Removal)
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenter				VARCHAR(12) = NULL,
	@empNo					INT = 0,
	@payPeriodStartDate		DATETIME = NULL,
	@payPeriodEndDate		DATETIME = NULL 
)
AS	
	--Define constants
	DECLARE @CONST_RETURN_OK		INT,
			@CONST_RETURN_ERROR		INT

	--Define other variables
	DECLARE @hasError				BIT,
			@retError				INT,
			@retErrorDesc			VARCHAR(200),
			@rowsAffected_TS		INT,
			@rowsAffected_TSE		INT			

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize other variables
	SELECT	@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@rowsAffected_TS		= 0,
			@rowsAffected_TSE		= 0

	--Start a transaction
	BEGIN TRAN T1

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

		IF ISNULL(@payPeriodStartDate, '') = '' OR @payPeriodStartDate = CONVERT(DATETIME, '')
			SET @payPeriodStartDate = NULL

		IF ISNULL(@payPeriodEndDate, '') = '' OR @payPeriodEndDate = CONVERT(DATETIME, '')
			SET @payPeriodEndDate = NULL

		IF @actionType = 1			--Remove incorrect overtime records in "Tran_Timesheet" and "Tran_Timesheet_Extra" tables
		BEGIN

			/***************************************************************************************************************************************
				Process #1 - Remove overtime details in "Tran_Timesheet_Extra" table for all unprocessed overtime 
			***************************************************************************************************************************************/
			--Delete existing log records
			DELETE FROM tas.OvertimeRemovalLog 
			WHERE RTRIM(SourceTableName) = 'Tran_Timesheet_Extra'
				AND TS_AutoID IN 
				(
					SELECT	a.AutoID
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
						--OUTER APPLY
						--(
						--	SELECT DISTINCT CostCenter, IsActive FROM tas.WorkplaceReaderSetting
						--	WHERE RTRIM(CostCenter) = RTRIM(a.BusinessUnit) 
						--		AND IsActive = 1
						--) c 
						INNER JOIN tas.Tran_Timesheet_Extra d ON a.AutoID = d.XID_AutoID
					WHERE 
						a.IsLastRow = 1
						AND a.EmpNo > 10000000
						AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
                            (a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND DATEDIFF(MINUTE, d.OTStartTime, d.OTEndTime) >= a.Duration_Required
						AND DATEDIFF(MINUTE, d.OTStartTime, d.OTEndTime) >= DATEDIFF(MINUTE, a.Shaved_IN, a.Shaved_OUT)
						AND ISNULL(a.CorrectionCode, '') = ''
						AND RTRIM(a.ShiftCode) <> 'O'
						AND ISNULL(a.IsPublicHoliday, 0) = 0
						AND (a.ShiftSpan IS NULL AND a.ShiftSpan_XID IS NULL AND a.ShiftSpanDate IS NULL)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND tas.fnIsDoubleShiftWithGap(a.EmpNo, a.DT) = 0	--Rev. #1.5
				)

			--Save the overtime records prior to applying the changes into the log table
			INSERT INTO tas.OvertimeRemovalLog
			(
				SourceTableName,
				EmpNo,
				DT,
				TS_AutoID,
				CostCenter,
				OTStartTime,
				OTEndTime,
				OTType,
				CreatedDate,
				CreatedByEmpNo,
				CreatedByUserID
			)
			SELECT	'Tran_Timesheet_Extra',
					a.EmpNo,
					a.DT,
					d.XID_AutoID,
					a.BusinessUnit,
					d.OTstartTime,
					d.OTendTime,
					d.OTtype,
					GETDATE(),
					0,
					'System Admin'
			FROM tas.Tran_Timesheet a
				INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
				--OUTER APPLY
				--(
				--	SELECT DISTINCT CostCenter, IsActive FROM tas.WorkplaceReaderSetting
				--	WHERE RTRIM(CostCenter) = RTRIM(a.BusinessUnit) 
				--		AND IsActive = 1
				--) c 
				INNER JOIN tas.Tran_Timesheet_Extra d ON a.AutoID = d.XID_AutoID
			WHERE 
				a.IsLastRow = 1
				AND a.EmpNo > 10000000
				AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
				AND 
				(
					(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
					OR
                    (a.DT = @startDate AND @startDate = @endDate)
					OR 
					(@startDate IS NULL AND @endDate IS NULL)
				)
				AND DATEDIFF(MINUTE, d.OTStartTime, d.OTEndTime) >= a.Duration_Required
				AND DATEDIFF(MINUTE, d.OTStartTime, d.OTEndTime) >= DATEDIFF(MINUTE, a.Shaved_IN, a.Shaved_OUT)
				AND ISNULL(a.CorrectionCode, '') = ''
				AND RTRIM(a.ShiftCode) <> 'O'
				AND ISNULL(a.IsPublicHoliday, 0) = 0
				AND (a.ShiftSpan IS NULL AND a.ShiftSpan_XID IS NULL AND a.ShiftSpanDate IS NULL)
				AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND tas.fnIsDoubleShiftWithGap(a.EmpNo, a.DT) = 0	--Rev. #1.5
			ORDER BY a.DT DESC, a.BusinessUnit, a.EmpNo 

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Remove the overtime information
				UPDATE tas.Tran_Timesheet_Extra
				SET OTstartTime = NULL,
					OTendTime = NULL,
					OTtype = NULL,
					LastUpdateUser = 'System Admin', 
					LastUpdateTime = GETDATE()
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
					--OUTER APPLY
					--(
					--	SELECT DISTINCT CostCenter, IsActive FROM tas.WorkplaceReaderSetting
					--	WHERE RTRIM(CostCenter) = RTRIM(a.BusinessUnit) 
					--		AND IsActive = 1
					--) c 
					INNER JOIN tas.Tran_Timesheet_Extra d ON a.AutoID = d.XID_AutoID
				WHERE 
					a.IsLastRow = 1
					AND a.EmpNo > 10000000
					AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
					AND 
					(
						(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
						OR
						(a.DT = @startDate AND @startDate = @endDate)
						OR 
						(@startDate IS NULL AND @endDate IS NULL)
					)
					AND DATEDIFF(MINUTE, d.OTStartTime, d.OTEndTime) >= a.Duration_Required
					AND DATEDIFF(MINUTE, d.OTStartTime, d.OTEndTime) >= DATEDIFF(MINUTE, a.Shaved_IN, a.Shaved_OUT)
					AND ISNULL(a.CorrectionCode, '') = ''
					AND RTRIM(a.ShiftCode) <> 'O'
					AND ISNULL(a.IsPublicHoliday, 0) = 0
					AND (a.ShiftSpan IS NULL AND a.ShiftSpan_XID IS NULL AND a.ShiftSpanDate IS NULL)
					AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND tas.fnIsDoubleShiftWithGap(a.EmpNo, a.DT) = 0	--Rev. #1.5

				--Get the number of affected records in the "Tran_Timesheet_Extra" table
				SELECT @rowsAffected_TSE = @@rowcount
			END 
			/****************************************************** End of Process #1 ***************************************************************/


			/***************************************************************************************************************************************
				Process #2 - Remove overtime details in "Tran_Timesheet" table for all already processed overtime 
			***************************************************************************************************************************************/
			--Delete existing log records
			DELETE FROM tas.OvertimeRemovalLog 
			WHERE RTRIM(SourceTableName) = 'Tran_Timesheet'
				AND TS_AutoID IN 
				(
					SELECT	a.AutoID
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
						--OUTER APPLY
						--(
						--	SELECT DISTINCT CostCenter, IsActive FROM tas.WorkplaceReaderSetting
						--	WHERE RTRIM(CostCenter) = RTRIM(a.BusinessUnit) 
						--		AND IsActive = 1
						--) c 
					WHERE 
						a.IsLastRow = 1
						AND a.EmpNo > 10000000
						AND (a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) >= a.Duration_Required
						AND DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) >= DATEDIFF(MINUTE, a.Shaved_IN, a.Shaved_OUT)
						AND ISNULL(a.CorrectionCode, '') = ''
						AND RTRIM(a.ShiftCode) <> 'O'
						AND ISNULL(a.IsPublicHoliday, 0) = 0
						AND (a.ShiftSpan IS NULL AND a.ShiftSpan_XID IS NULL AND a.ShiftSpanDate IS NULL)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND tas.fnIsDoubleShiftWithGap(a.EmpNo, a.DT) = 0	--Rev. #1.5
				)

			--Save the Timesheet records prior to applying the changes into the log table
			INSERT INTO tas.OvertimeRemovalLog
			(
				SourceTableName,
				EmpNo,
				DT,
				TS_AutoID,
				CostCenter,
				OTStartTime,
				OTEndTime,
				OTType,
				CreatedDate,
				CreatedByEmpNo,
				CreatedByUserID
			)
			SELECT	'Tran_Timesheet',
					a.EmpNo,
					a.DT,
					a.AutoID,
					a.BusinessUnit,
					a.OTStartTime,
					a.OTEndTime,
					a.OTType,
					GETDATE(),
					0,
					'System Admin'
			FROM tas.Tran_Timesheet a
				INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			WHERE 
				a.IsLastRow = 1
				AND a.EmpNo > 10000000
				AND (a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
				AND 
				(
					(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
					OR
                    (a.DT = @startDate AND @startDate = @endDate)
					OR 
					(@startDate IS NULL AND @endDate IS NULL)
				)
				AND DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) >= a.Duration_Required
				AND DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) >= DATEDIFF(MINUTE, a.Shaved_IN, a.Shaved_OUT)
				AND ISNULL(a.CorrectionCode, '') = ''
				AND RTRIM(a.ShiftCode) <> 'O'
				AND ISNULL(a.IsPublicHoliday, 0) = 0
				AND (a.ShiftSpan IS NULL AND a.ShiftSpan_XID IS NULL AND a.ShiftSpanDate IS NULL)
				AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)
				AND tas.fnIsDoubleShiftWithGap(a.EmpNo, a.DT) = 0	--Rev. #1.5
			ORDER BY a.DT DESC, a.BusinessUnit, a.EmpNo 

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				UPDATE tas.Tran_Timesheet
				SET OTStartTime = NULL,
					OTEndTime = NULL,
					OTType = NULL,
					Processed = 0,
					LastUpdateUser = 'System Admin', 
					LastUpdateTime = GETDATE()
				WHERE AutoID IN
				(
					SELECT AutoID
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
					WHERE a.IsLastRow = 1
						AND a.EmpNo > 10000000
						AND (a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) >= a.Duration_Required
						AND DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime) >= DATEDIFF(MINUTE, a.Shaved_IN, a.Shaved_OUT)
						AND ISNULL(a.CorrectionCode, '') = ''
						AND RTRIM(a.ShiftCode) <> 'O'
						AND ISNULL(a.IsPublicHoliday, 0) = 0
						AND (a.ShiftSpan IS NULL AND a.ShiftSpan_XID IS NULL AND a.ShiftSpanDate IS NULL)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND tas.fnIsDoubleShiftWithGap(a.EmpNo, a.DT) = 0	--Rev. #1.5
				)

				--Get the number of affected records in the "Tran_Timesheet" table
				SELECT @rowsAffected_TS = @@rowcount
			END 
			/****************************************************** End of Process #2 ***************************************************************/


			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				/***************************************************************************************************************************************
					Process #3 - Check for shift span records and then save the overtime details at the last row
				***************************************************************************************************************************************/
				IF EXISTS
                (
					SELECT a.LogID
					FROM tas.OvertimeRemovalLog a
						CROSS APPLY
						(
							SELECT AutoID FROM tas.Tran_Timesheet
							WHERE EmpNo = a.EmpNo
								AND DT = a.DT
								AND ISNULL(ShiftSpan_XID, 0) > 0	--(Note: If "ShiftSpan_XID" and "ShiftSpanDate" are not null, then the "ShiftSpan" value on the previous day is equal to 1 and "ShiftSpan_XID" are the same in both records.)
								AND ShiftSpanDate IS NOT NULL	
								AND (ShiftSpan_HoursDay1 > 0 AND ShiftSpan_HoursDay1 > Duration_Required)
						) b
					WHERE 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
				)
				BEGIN

					DECLARE	@shiftSpanAutoID		INT,
							@shiftSpanEmpNo			INT,
							@shiftSpanDT			DATETIME,
							@shiftShavedIn			DATETIME,
							@shiftSpanShavedOut		DATETIME,
							@isPublicHoliday		BIT,
							@isRamadan				BIT,
							@otType					VARCHAR(10),
							@logID					BIGINT 	

					DECLARE ShiftSpanCursor CURSOR READ_ONLY FOR
					SELECT b.AutoID, a.EmpNo, a.DT, b.Shaved_IN, b.Shaved_OUT, b.IsPublicHoliday, b.isRamadan, a.OTType, a.LogID
					FROM tas.OvertimeRemovalLog a
						CROSS APPLY
						(
							SELECT * FROM tas.Tran_Timesheet
							WHERE EmpNo = a.EmpNo
								AND DT = a.DT
								AND ISNULL(ShiftSpan_XID, 0) > 0	--(Note: If "ShiftSpan_XID" and "ShiftSpanDate" are not null, then the "ShiftSpan" value on the previous day is equal to 1 and "ShiftSpan_XID" are the same in both records.)
								AND ShiftSpanDate IS NOT NULL	
								AND (ShiftSpan_HoursDay1 > 0 AND ShiftSpan_HoursDay1 > Duration_Required)
						) b
					WHERE 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						
					OPEN ShiftSpanCursor
					FETCH NEXT FROM ShiftSpanCursor
					INTO @shiftSpanAutoID, @shiftSpanEmpNo, @shiftSpanDT, @shiftShavedIn, @shiftSpanShavedOut, @isPublicHoliday, @isRamadan, @otType, @logID

					WHILE @@FETCH_STATUS = 0
					BEGIN

						IF @isPublicHoliday = 1 OR @isRamadan = 1
						BEGIN
                        
							--Insert log record
							INSERT INTO tas.OvertimeShiftSpan
							(
								LogID,
								TableNameToUpdate,
								TSAutoIDSource,
								TSAutoIDTarget,
								OTStartTime,
								OTEndTime,
								OTType,
								CreatedDate,
								CreatedByEmpNo,
								CreatedByUserID
							)
							SELECT	@logID,
									'Tran_Timesheet',
									@shiftSpanAutoID,
									a.AutoID,
									@shiftShavedIn,	
									@shiftSpanShavedOut,
									@otType,
									GETDATE(),
									0,
									'System Admin' 
							FROM tas.Tran_Timesheet a
							WHERE a.EmpNo = @shiftSpanEmpNo
								AND a.DT = @shiftSpanDT
								AND a.IsLastRow = 1

							--Overtime is auto calculated 
							UPDATE tas.Tran_Timesheet
							SET OTStartTime = @shiftShavedIn,
								OTEndTime = @shiftSpanShavedOut,
								OTType = @otType,
								Processed = 0,
								LastUpdateUser = 'System Admin', 
								LastUpdateTime = GETDATE()
							WHERE 
								EmpNo = @shiftSpanEmpNo
								AND DT = @shiftSpanDT
								AND IsLastRow = 1
						END 
						
						ELSE
                        BEGIN

							--Insert log record
							INSERT INTO tas.OvertimeShiftSpan
							(
								LogID,
								TableNameToUpdate,
								TSAutoIDSource,
								TSAutoIDTarget,
								OTStartTime,
								OTEndTime,
								OTType,
								CreatedDate,
								CreatedByEmpNo,
								CreatedByUserID
							)
							SELECT	@logID,
									'Tran_Timesheet_Extra',
									@shiftSpanAutoID,
									a.AutoID,
									@shiftShavedIn,	
									@shiftSpanShavedOut,
									@otType,
									GETDATE(),
									0,
									'System Admin' 
							FROM tas.Tran_Timesheet a
							WHERE a.EmpNo = @shiftSpanEmpNo
								AND a.DT = @shiftSpanDT
								AND a.IsLastRow = 1

							--Overtime requires approval through the "Overtime & Meal Voucher Approva Form" in TAS
							UPDATE tas.Tran_Timesheet_Extra
							SET OTstartTime = @shiftShavedIn,
								OTendTime = @shiftSpanShavedOut,
								OTtype = @otType,
								LastUpdateUser = 'System Admin', 
								LastUpdateTime = GETDATE()
							FROM tas.Tran_Timesheet a
								INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
							WHERE 
								EmpNo = @shiftSpanEmpNo
								AND DT = @shiftSpanDT
								AND IsLastRow = 1
                        END 

						-- Retrieve next record
						FETCH NEXT FROM ShiftSpanCursor
						INTO @shiftSpanAutoID, @shiftSpanEmpNo, @shiftSpanDT, @shiftShavedIn, @shiftSpanShavedOut, @isPublicHoliday, @isRamadan, @otType, @logID

					END

					-- Close and deallocate
					CLOSE ShiftSpanCursor
					DEALLOCATE ShiftSpanCursor

					--Checks for error
					IF @@ERROR <> @CONST_RETURN_OK
					BEGIN
				
						SELECT	@retError = @CONST_RETURN_ERROR,
								@hasError = 1
					END
                END 
			END 

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				/***************************************************************************************************************************************
					Process #4 - Check for corrected and approved missing swipes but overtime cannot be seen in the OT Approval Form in TAS
				***************************************************************************************************************************************/
				IF EXISTS
				(
					SELECT a.AutoID
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
						INNER JOIN tas.Tran_WorkplaceSwipe c ON a.EmpNo = c.EmpNo AND a.DT = c.SwipeDate
						CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, c.TimeInWP, c.TimeOutWP, 0) d
					WHERE
						(a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
						AND (b.OTstartTime IS NULL OR b.OTendTime IS NULL)
						AND (c.TimeInWP IS NOT NULL AND c.TimeOutWP IS NOT NULL)
						AND a.IsLastRow = 1
						AND c.IsCorrected = 1
						AND c.IsClosed = 1	
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
				)
                BEGIN

					UPDATE tas.Tran_Timesheet_Extra
					SET tas.Tran_Timesheet_Extra.OTstartTime = d.OTStartTime,
						tas.Tran_Timesheet_Extra.OTendTime = d.OTEndTime,
						tas.Tran_Timesheet_Extra.LastUpdateUser = 'SYSTEM ADMIN',
						tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
						INNER JOIN tas.Tran_WorkplaceSwipe c ON a.EmpNo = c.EmpNo AND a.DT = c.SwipeDate
						CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, c.TimeInWP, c.TimeOutWP, 0) d
					WHERE
						(a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
						AND (b.OTstartTime IS NULL OR b.OTendTime IS NULL)
						AND (c.TimeInWP IS NOT NULL AND c.TimeOutWP IS NOT NULL)
						AND a.IsLastRow = 1
						AND c.IsCorrected = 1
						AND c.IsClosed = 1	
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
                END 

				/***************************************************************************************************************************************
					Process #5 - Check for missing overtime for employees who worked on 12 hour shift (Rev. #1.6)
				***************************************************************************************************************************************/
				IF EXISTS
				(
					SELECT a.AutoID
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
						INNER JOIN tas.Master_ShiftPatternTitles c ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode)
						INNER JOIN tas.Master_Employee_JDE_View_V2 d ON a.EmpNo = d.EmpNo
					WHERE 
						ISNULL(c.IsDayShift, 0) = 0
						AND ISNUMERIC(d.PayStatus) = 1
						AND DATEDIFF(HOUR, a.dtIN, a.dtOUT) >= 12						
						AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
						AND (b.OTstartTime IS NULL AND b.OTendTime IS NULL)
						AND ISNULL(a.CorrectionCode, '') = ''
						AND RTRIM(a.BusinessUnit) IN
						(
							SELECT RTRIM(LTRIM(MCMCU)) 
							FROM tas.syJDE_F0006
							WHERE   
								(MCSTYL IN ('*', ' ', 'BP', 'DA')) AND (MCCO IN ('00000', '00100', '00600'))
								AND ISNUMERIC(MCMCU) = 1
								AND (MCMCU BETWEEN 2110 AND 7910 OR MCMCU BETWEEN 6002000 AND 6007800)
								AND UPPER(RTRIM(ISNULL(MCRP06,''))) = 'Y'
						)						
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
				)
				BEGIN

					UPDATE tas.Tran_Timesheet_Extra
					SET tas.Tran_Timesheet_Extra.OTstartTime = e.OTStartTime,
						tas.Tran_Timesheet_Extra.OTendTime = e.OTEndTime,
						tas.Tran_Timesheet_Extra.LastUpdateUser = 'SYSTEM ADMIN',
						tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID
						INNER JOIN tas.Master_ShiftPatternTitles c ON RTRIM(a.ShiftPatCode) = RTRIM(c.ShiftPatCode)
						INNER JOIN tas.Master_Employee_JDE_View_V2 d ON a.EmpNo = d.EmpNo
						CROSS APPLY tas.fnGetProcessedWorkplaceDataToSync(a.EmpNo, a.DT, a.dtIN, a.dtOUT, 0) e
					WHERE 
						ISNULL(c.IsDayShift, 0) = 0
						AND ISNUMERIC(d.PayStatus) = 1
						AND DATEDIFF(HOUR, a.dtIN, a.dtOUT) >= 12						
						AND (a.OTStartTime IS NULL AND a.OTEndTime IS NULL)
						AND (b.OTstartTime IS NULL AND b.OTendTime IS NULL)
						AND ISNULL(a.CorrectionCode, '') = ''
						AND RTRIM(a.BusinessUnit) IN
						(
							SELECT RTRIM(LTRIM(MCMCU)) 
							FROM tas.syJDE_F0006
							WHERE   
								(MCSTYL IN ('*', ' ', 'BP', 'DA')) AND (MCCO IN ('00000', '00100', '00600'))
								AND ISNUMERIC(MCMCU) = 1
								AND (MCMCU BETWEEN 2110 AND 7910 OR MCMCU BETWEEN 6002000 AND 6007800)
								AND UPPER(RTRIM(ISNULL(MCRP06,''))) = 'Y'
						)
						AND 
						(
							(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(a.DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
                END 
			END 
		END 

		ELSE IF @actionType = 2		--Undo removal of overtime records
		BEGIN

			--Recover overtime information
			UPDATE tas.Tran_Timesheet_Extra
			SET OTstartTime = a.OTStartTime,
				OTendTime = a.OTEndTime,
				OTtype = a.OTType,
				LastUpdateUser = 'System Admin', 
				LastUpdateTime = GETDATE()
			FROM tas.OvertimeRemovalLog a
			WHERE 
				Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
				AND RTRIM(a.SourceTableName) = 'Tran_Timesheet_Extra'
				AND 
				(
					(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
					OR
                    (a.DT = @startDate AND @startDate = @endDate)
					OR 
					(@startDate IS NULL AND @endDate IS NULL)
				)
				AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)

			--Get the number of affected records in the "Tran_Timesheet_Extra" table
			SELECT @rowsAffected_TSE = @@rowcount

			--Recover Timesheet record
			UPDATE tas.Tran_Timesheet
			SET OTstartTime = a.OTStartTime,
				OTendTime = a.OTEndTime,
				OTtype = a.OTType,
				Processed = 1,
				LastUpdateUser = 'System Admin', 
				LastUpdateTime = GETDATE()
			FROM tas.OvertimeRemovalLog a
			WHERE 
				Tran_Timesheet.AutoID = a.TS_AutoID
				AND RTRIM(a.SourceTableName) = 'Tran_Timesheet'
				AND 
				(
					(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
					OR
                    (a.DT = @startDate AND @startDate = @endDate)
					OR 
					(@startDate IS NULL AND @endDate IS NULL)
				)
				AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)

			--Get the number of affected records in the "Tran_Timesheet" table
			SELECT @rowsAffected_TS = @@rowcount

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Delete shift span overtime log record
				DELETE FROM tas.OvertimeShiftSpan
				WHERE LogID IN
				(
					SELECT LogID
					FROM tas.OvertimeRemovalLog
					WHERE 
						(
							(DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (EmpNo = @empNo OR @empNo IS NULL)
				)

				--Checks for error
				IF @@ERROR <> @CONST_RETURN_OK
				BEGIN
				
					SELECT	@retError = @CONST_RETURN_ERROR,
							@hasError = 1
				END

				-- Checks if there's no error
				IF @retError = @CONST_RETURN_OK
				BEGIN

					--Delete removed overtime log records
					DELETE FROM tas.OvertimeRemovalLog
					WHERE 
						(
							(DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (EmpNo = @empNo OR @empNo IS NULL)
				END 
			END 
        END		

		ELSE IF @actionType = 3		--Undo removal of overtime records based on temporary table
		BEGIN

			--Recover overtime information
			UPDATE tas.Tran_Timesheet_Extra
			SET OTstartTime = a.OTStartTime,
				OTendTime = a.OTEndTime,
				OTtype = a.OTType,
				LastUpdateUser = 'System Admin', 
				LastUpdateTime = GETDATE()
			FROM tas.OvertimeRemovalLogRecovery a
			WHERE 
				Tran_Timesheet_Extra.XID_AutoID = a.TS_AutoID
				AND RTRIM(a.SourceTableName) = 'Tran_Timesheet_Extra'
				AND ISNULL(a.IsProcessed, 0) = 0
				AND 
				(
					(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
					OR
                    (a.DT = @startDate AND @startDate = @endDate)
					OR 
					(@startDate IS NULL AND @endDate IS NULL)
				)
				AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)

			--Get the number of affected records in the "Tran_Timesheet_Extra" table
			SELECT @rowsAffected_TSE = @@rowcount

			--Recover Timesheet record
			UPDATE tas.Tran_Timesheet
			SET OTstartTime = a.OTStartTime,
				OTendTime = a.OTEndTime,
				OTtype = a.OTType,
				Processed = CASE WHEN Tran_Timesheet.DT BETWEEN @payPeriodStartDate AND @payPeriodEndDate AND (@payPeriodStartDate IS NOT NULL AND @payPeriodEndDate IS NOT NULL) THEN 0 ELSE 1 END,
				LastUpdateUser = 'System Admin', 
				LastUpdateTime = GETDATE()
			FROM tas.OvertimeRemovalLogRecovery a
			WHERE 
				Tran_Timesheet.AutoID = a.TS_AutoID
				AND RTRIM(a.SourceTableName) = 'Tran_Timesheet'
				AND ISNULL(a.IsProcessed, 0) = 0
				AND 
				(
					(a.DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
					OR
                    (a.DT = @startDate AND @startDate = @endDate)
					OR 
					(@startDate IS NULL AND @endDate IS NULL)
				)
				AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
				AND (a.EmpNo = @empNo OR @empNo IS NULL)

			--Get the number of affected records in the "Tran_Timesheet" table
			SELECT @rowsAffected_TS = @@rowcount

			--Checks for error
			IF @@ERROR <> @CONST_RETURN_OK
			BEGIN
				
				SELECT	@retError = @CONST_RETURN_ERROR,
						@hasError = 1
			END

			-- Checks if there's no error
			IF @retError = @CONST_RETURN_OK
			BEGIN

				--Delete shift span overtime log record
				DELETE FROM tas.OvertimeShiftSpan
				WHERE LogID IN
				(
					SELECT LogID
					FROM tas.OvertimeRemovalLog
					WHERE 
						(
							(DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
							OR
							(DT = @startDate AND @startDate = @endDate)
							OR 
							(@startDate IS NULL AND @endDate IS NULL)
						)
						AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
						AND (EmpNo = @empNo OR @empNo IS NULL)
				)

				--Delete removed overtime log records
				DELETE FROM tas.OvertimeRemovalLog
				WHERE 
					(
						(DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
						OR
						(DT = @startDate AND @startDate = @endDate)
						OR 
						(@startDate IS NULL AND @endDate IS NULL)
					)
					AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (EmpNo = @empNo OR @empNo IS NULL)

				--Mark the record in "OvertimeRemovalLogRecovery" table as processed already
				UPDATE tas.OvertimeRemovalLogRecovery
				SET IsProcessed = 1
				WHERE 
					(
						(DT BETWEEN @startdate AND @endDate AND @startdate < @endDate)
						OR
						(DT = @startDate AND @startDate = @endDate)
						OR 
						(@startDate IS NULL AND @endDate IS NULL)
					)
					AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (EmpNo = @empNo OR @empNo IS NULL)
			END 
        END		

	END TRY

	BEGIN CATCH

		--Capture the error
		SELECT	@retError = CASE WHEN ERROR_NUMBER() > 0 THEN ERROR_NUMBER() ELSE @CONST_RETURN_ERROR END,
				@retErrorDesc = ERROR_MESSAGE(),
				@hasError = 1

	END CATCH

	IF @retError = @CONST_RETURN_OK
	BEGIN

		IF @@TRANCOUNT > 0
			COMMIT TRANSACTION		
	END

	ELSE
	BEGIN

		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION
	END

	--Return error information to the caller
	SELECT	@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected_TS AS TimesheetRowsAffected,
			@rowsAffected_TSE AS OvertimeRowsAffected


/*	Debugging:

PARAMETERS:
	@actionType				TINYINT,		--(Note: 1 = Remove Incorrect Overtime; 2 = Undo Removal)
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenter				VARCHAR(12) = NULL,
	@empNo					INT = 0,
	@payPeriodStartDate		DATETIME = NULL,
	@payPeriodEndDate		DATETIME = NULL 

	EXEC tas.Pr_RemoveIncorrectOvertime 1, '03/02/2016', '03/02/2016', '', 10008037		--Remove incorrect overtime
	EXEC tas.Pr_RemoveIncorrectOvertime 2, '03/02/2016', '03/02/2016', '', 10008037		--Undo overtime removal

	EXEC tas.Pr_RemoveIncorrectOvertime 1, '01/02/2015', '28/02/2015', '5200'		--Remove incorrect overtime (by cost center)
	EXEC tas.Pr_RemoveIncorrectOvertime 2, '01/02/2015', '28/02/2015', '5200'		--Undo overtime removal (by cost center)

	EXEC tas.Pr_RemoveIncorrectOvertime 1, '14/02/2015', '14/02/2015' 				--Remove incorrect overtime (same date)
	EXEC tas.Pr_RemoveIncorrectOvertime 2, '14/02/2015', '14/02/2015' 				--Undo overtime removal (same date)

*/

/*	Checking:

	SELECT * FROM tas.OvertimeRemovalLog a
	ORDER BY a.LogID

	DECLARE	@empNo		INT,
			@startDate	DATETIME,
			@endDate	DATETIME

	SELECT	@empNo		= 10001405,
			@startDate	= '30/03/2016',
			@endDate	= '31/03/2016'

	SELECT a.ShiftSpan, a.IsLastRow, a.IsPublicHoliday, a.isRamadan,
		CASE WHEN b.CostCenter IS NOT NULL THEN 1 ELSE 0 END AS IsRequiredToSwipeWP,
		a.ShiftCode, a.Actual_ShiftCode,
		a.DT, a.dtIN, a.dtOUT,
		a.Shaved_IN, a.Shaved_OUT,
		a.OTStartTime AS OTStartTime_TS, 
		a.OTEndTime AS OTEndTime_TS, 
		a.OTType AS OTType_TS,
		c.OTStartTime AS OTStartTime_TE, 
		c.OTEndTime AS OTEndTime_TE, 
		c.OTType AS OTType_TE,
		a.Duration_Required, a.Duration_Worked_Cumulative,
	a.* 
	FROM tas.Tran_Timesheet a
		OUTER APPLY
		(
			SELECT DISTINCT CostCenter, IsActive FROM tas.WorkplaceReaderSetting
			WHERE RTRIM(CostCenter) = RTRIM(a.BusinessUnit) 
				AND IsActive = 1
		) b 
		LEFT JOIN tas.Tran_Timesheet_Extra c ON a.AutoID = c.XID_AutoID
	WHERE a.EmpNo = @empNo
		AND a.DT BETWEEN @startDate AND @endDate

*/