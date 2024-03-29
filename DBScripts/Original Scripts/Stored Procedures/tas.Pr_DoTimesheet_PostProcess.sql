USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_DoTimesheet_PostProcess]    Script Date: 16/10/2017 14:36:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/***********************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_DoTimesheet_PostProcess
*	Description: Do data updates in the Timesheet table after the Timesheet Processing Service completed
*
*	Date:			Author:		Rev.#:		Comments:
*	25/09/2014		Ervin		1.0			Created
*	15/12/2015		Ervin		1.1			Fixed bug wherein OT for Aspire employees were not calculated if Shift Code is day-off.
											Commented filter condition "Duration_Required > 0" and added "ISNULL(a.CorrectionCode, '') = ''"
*	11/01/2016		Ervin		1.2			Commented filter condition: Duration_Worked_Cumulative > Duration_Required 	
*	07/02/2016		Ervin		1.3			Get the cost from the value of "YAHMCU" field in F060116 table		
*	09/02/2016		Ervin		1.4			Commented the filter condition that checks for "IsDayWorker_OR_Shifter"							
*	16/02/2016		Ervin		1.5			Returned the working cost center instead of the home cost center
*	16/03/2016		Ervin		1.6			Modified the condition in checking the Shift Code
*	29/06/2016		Ervin		1.7			Removed filter that disable overtime correction if the "CorrectionCode" field contains a value
*	17/07/2016		Ervin		1.8			Refactored the condition the checks if overtime is valid
*	20/04/2017		Ervin		1.9			Commented the filter condition that checks if IsSalStaff = 0
**********************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_DoTimesheet_PostProcess]
(
	@actionTypeID		int,		--(Note: 1 => Process OT) 
	@startDate			datetime,
	@endDate			datetime,
	@costCenter			varchar(12),	
	@empNo				int = 0
	
)
AS

	--Define constants
	DECLARE @CONST_RETURN_OK		int,
			@CONST_RETURN_ERROR		int		

	--Define variables
	DECLARE @rowsAffected					int,
			@hasError						bit,
			@retError						int,
			@retErrorDesc					varchar(200),	
			@code_OTtype_Regular			varchar(10),	--OT Code for regular working day
			@code_OTtype_PublicHoliday		varchar(10),	--OT Code for public holiday
			@code_OTtype_DILdw				varchar(10),	--OT Code for DIL 
			@code_OTtype_DayOff				varchar(10),	--OT Code for day-off
			@minutes_MinOT_NSS				int,			--Minimum OT during regular days
			@minutes_MinOT_SS				int,			--Minimum OT during Non-Ramadan
			@minutes_MinOT_SS_Ramadan		int,			--Minimum OT during Ramadan
			@CODE_Shift_Off					varchar(10),
			@OTStartTime_Orig				datetime,
			@OTEndTime_Orig					datetime,		
			@OTType_Orig					varchar(10)

	--Initialize constants
	SELECT	@CONST_RETURN_OK		= 0,
			@CONST_RETURN_ERROR		= -1

	--Initialize variables
	SELECT	@rowsAffected			= 0,
			@hasError				= 0,
			@retError				= @CONST_RETURN_OK,
			@retErrorDesc			= '',
			@OTStartTime_Orig		= NULL,
			@OTEndTime_Orig			= NULL,		
			@OTType_Orig			= ''

	SELECT	@code_OTtype_Regular = RTRIM(Code_OTtype_Regular),
			@code_OTtype_PublicHoliday = RTRIM(Code_OTtype_PublicHoliday),
			@code_OTtype_DILdw = RTRIM(Code_OTtype_DILdw),
			@code_OTtype_DayOff = RTRIM(Code_OTtype_DayOff), 
			@minutes_MinOT_NSS = Minutes_MinOT_NSS,
			@minutes_MinOT_SS = Minutes_MinOT_SS,
			@CODE_Shift_Off = RTRIM(CODE_Shift_Off)
	FROM tas.System_Values

	IF @empNo = 0
		SET @empNo = NULL

	BEGIN TRY

		IF @actionTypeID = 1	--Process OT records for Aspire Employees
		BEGIN

			--Find existing records
			IF EXISTS
			(
				SELECT AutoID 
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
				WHERE 
					CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
					AND IsLastRow = 1
					
					--Start of Rev. #1.8
					AND 
					(
						(
							a.Duration_Worked_Cumulative > a.Duration_Required
							AND
							a.Duration_Worked_Cumulative - a.Duration_Required > 30
						)
						OR
						(
							a.Duration_Worked_Cumulative <= a.Duration_Required 
							AND 
							a.IsPublicHoliday = 1
						)
						OR
						(
							RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
						)
					)	
					--End of Rev. #1.8

					AND Shaved_IN IS NOT NULL
					AND Shaved_OUT IS NOT NULL 
					AND ISNULL(IsDriver, 0) = 0
					AND ISNULL(IsLiasonOfficer, 0) = 0
					--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
					--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
					AND ISNULL(AbsenceReasonCode, '') = ''
					AND ISNULL(LeaveType, '') = ''	
					--AND ISNULL(a.CorrectionCode, '') = ''		--Rev. #1.7	
					--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
					--(
					--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
					--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
					--)	
					AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
					AND NOT EXISTS
					(
						SELECT XID_AutoID FROM tas.Tran_Timesheet_Extra
						WHERE XID_AutoID = a.AutoID --AND Approved = 1
							AND ISNULL(OTstartTime, '') <> ''
							AND ISNULL(OTendTime, '') <> ''
					)					
			)
			BEGIN

				--Save OT details to variables
				SELECT	@OTStartTime_Orig	= OTStartTime,
						@OTEndTime_Orig		= OTEndTime,		
						@OTType_Orig		= OTType
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
				WHERE
					CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
					AND (a.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
					AND IsLastRow = 1

					--Start of Rev. #1.8
					AND 
					(
						(
							a.Duration_Worked_Cumulative > a.Duration_Required
							AND
							a.Duration_Worked_Cumulative - a.Duration_Required > 30
						)
						OR
						(
							a.Duration_Worked_Cumulative <= a.Duration_Required 
							AND 
							a.IsPublicHoliday = 1
						)
						OR
						(
							RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
						)
					)	
					--End of Rev. #1.8

					AND Shaved_IN IS NOT NULL
					AND Shaved_OUT IS NOT NULL 
					AND ISNULL(IsDriver, 0) = 0
					AND ISNULL(IsLiasonOfficer, 0) = 0
					--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
					--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
					AND ISNULL(AbsenceReasonCode, '') = ''
					AND ISNULL(LeaveType, '') = ''		
					--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #1.7
					--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
					--(
					--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
					--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
					--)	
					AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
					AND NOT EXISTS
					(
						SELECT XID_AutoID FROM tas.Tran_Timesheet_Extra
						WHERE XID_AutoID = a.AutoID --AND Approved = 1
							AND ISNULL(OTstartTime, '') <> ''
							AND ISNULL(OTendTime, '') <> ''
					)

				--Remove OT information in the Timesheet
				UPDATE tas.Tran_Timesheet
				SET tas.Tran_Timesheet.OTStartTime = NULL,
					tas.Tran_Timesheet.OTEndTime = NULL,
					tas.Tran_Timesheet.OTType = NULL
				FROM tas.Tran_Timesheet a
					INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
				WHERE
					CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
					AND (EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
					AND IsLastRow = 1

					--Start of Rev. #1.8
					AND 
					(
						(
							a.Duration_Worked_Cumulative > a.Duration_Required
							AND
							a.Duration_Worked_Cumulative - a.Duration_Required > 30
						)
						OR
						(
							a.Duration_Worked_Cumulative <= a.Duration_Required 
							AND 
							a.IsPublicHoliday = 1
						)
						OR
						(
							RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
						)
					)	
					--End of Rev. #1.8

					AND Shaved_IN IS NOT NULL
					AND Shaved_OUT IS NOT NULL 
					AND ISNULL(IsDriver, 0) = 0
					AND ISNULL(IsLiasonOfficer, 0) = 0
					--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
					--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
					AND ISNULL(AbsenceReasonCode, '') = ''
					AND ISNULL(LeaveType, '') = ''		
					--AND ISNULL(CorrectionCode, '') = ''		--Rev. #1.7
					--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
					--(
					--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
					--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
					--)	
					AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
					AND NOT EXISTS
					(
						SELECT XID_AutoID FROM tas.Tran_Timesheet_Extra
						WHERE XID_AutoID = AutoID --AND Approved = 1
							AND ISNULL(OTstartTime, '') <> ''
							AND ISNULL(OTendTime, '') <> ''
					)

				--Get the number of rows updated
				SET @rowsAffected = @@ROWCOUNT

				--IF @rowsAffected > 0
				--BEGIN

					IF NOT EXISTS 
					(
						SELECT XID_AutoID 
						FROM tas.Tran_Timesheet_Extra a
							INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID
							INNER JOIN tas.syJDE_F060116 c ON b.EmpNo = c.YAAN8
						WHERE
							CONVERT(VARCHAR, b.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
							AND (b.EmpNo = @empNo OR @empNo IS NULL)
							AND (RTRIM(LTRIM(c.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
							AND b.IsLastRow = 1

							--Start of Rev. #1.8
							AND 
							(
								(
									b.Duration_Worked_Cumulative > b.Duration_Required
									AND
									b.Duration_Worked_Cumulative - b.Duration_Required > 30
								)
								OR
								(
									b.Duration_Worked_Cumulative <= b.Duration_Required 
									AND 
									b.IsPublicHoliday = 1
								)
								OR
								(
									RTRIM(b.ShiftCode) = 'O' AND b.Duration_Worked_Cumulative > 0
								)
							)	
							--End of Rev. #1.8

							AND b.Shaved_IN IS NOT NULL
							AND b.Shaved_OUT IS NOT NULL 
							AND ISNULL(b.IsDriver, 0) = 0
							AND ISNULL(b.IsLiasonOfficer, 0) = 0
							--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
							--AND ISNULL(b.IsDayWorker_OR_Shifter, 0) = 0 
							AND ISNULL(b.AbsenceReasonCode, '') = ''
							AND ISNULL(b.LeaveType, '') = ''	
							--AND ISNULL(b.CorrectionCode, '') = ''	--Rev. #1.7
							--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
							--(
							--	DATEDIFF(n, b.dtIN, b.dtOUT) > @minutes_MinOT_NSS	
							--	AND (RTRIM(b.OTType) = @code_OTtype_Regular OR ISNULL(b.OTType, '') = '')
							--)	
							AND RTRIM(ISNULL(b.OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
					)
					BEGIN

						--Insert record to "Tran_Timesheet_Extra" table
						INSERT INTO tas.Tran_Timesheet_Extra
						(
							XID_AutoID,
							OTstartTime,
							OTendTime,
							OTtype,
							Approved,
							LastUpdateUser,
							LastUpdateTime,
							Comment,
							OTApproved,
							OTReason)
						SELECT * FROM
						(
							SELECT
								AutoID,

								--Set the OT Start Time
								CASE 
									WHEN a.IsPublicHoliday = 1 THEN a.Shaved_IN
									WHEN RTRIM(ISNULL(a.ShiftCode, '')) = @CODE_Shift_Off THEN a.Shaved_IN
									WHEN IsDILdayWorker = 1 AND ISNULL(a.IsSalStaff, 0) = 0 AND a.IsDayWorker_OR_Shifter = 1 THEN a.Shaved_IN
									ELSE DATEADD(n, (ISNULL(a.Duration_Worked_Cumulative, 0) - ISNULL(Duration_Required, 0)) * -1, a.Shaved_OUT)
								END AS OTstartTime,
						
								--Set the OT End Time
								a.Shaved_OUT AS OTendTime, 

								--Set the OT Type
								CASE 
									WHEN a.IsPublicHoliday = 1 THEN @code_OTtype_PublicHoliday
									WHEN RTRIM(ISNULL(a.ShiftCode, '')) = @CODE_Shift_Off THEN @code_OTtype_DayOff
									WHEN IsDILdayWorker = 1 AND ISNULL(a.IsSalStaff, 0) = 0 AND a.IsDayWorker_OR_Shifter = 1 THEN @code_OTtype_DILdw
									ELSE @code_OTtype_Regular
								END AS OTtype,

								0 AS Approved, 
								'System Admin' AS LastUpdateUser, 
								GETDATE() AS LastUpdateTime, 
								NULL AS Comment, 
								0 AS OTApproved, 
								NULL AS OTReason
							FROM tas.Tran_Timesheet a
								INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
							WHERE
								CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
								AND (a.EmpNo = @empNo OR @empNo IS NULL)
								AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
								AND IsLastRow = 1

								--Start of Rev. #1.8
								AND 
								(
									(
										a.Duration_Worked_Cumulative > a.Duration_Required
										AND
										a.Duration_Worked_Cumulative - a.Duration_Required > 30
									)
									OR
									(
										a.Duration_Worked_Cumulative <= a.Duration_Required 
										AND 
										a.IsPublicHoliday = 1
									)
									OR
									(
										RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
									)
								)	
								--End of Rev. #1.8

								AND Shaved_IN IS NOT NULL
								AND Shaved_OUT IS NOT NULL 
								AND ISNULL(IsDriver, 0) = 0
								AND ISNULL(IsLiasonOfficer, 0) = 0
								--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
								--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
								AND ISNULL(AbsenceReasonCode, '') = ''
								AND ISNULL(LeaveType, '') = ''	
								--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #1.7
								--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
								--(
								--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
								--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
								--)	
								AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
								AND NOT EXISTS
								(
									SELECT XID_AutoID FROM tas.Tran_Timesheet_Extra
									WHERE XID_AutoID = AutoID --AND Approved = 1
										AND ISNULL(OTstartTime, '') <> ''
										AND ISNULL(OTendTime, '') <> ''
								)
							) A
							--WHERE 
							--(
							--	DATEDIFF(n, A.OTstartTime, A.OTendTime) > @minutes_MinOT_NSS	
							--	AND 
							--	(RTRIM(A.OTType) = @code_OTtype_Regular OR ISNULL(A.OTType, '') = '')
							--)
					END

					ELSE 
					BEGIN

						--Update existing records
						UPDATE tas.Tran_Timesheet_Extra
						SET tas.Tran_Timesheet_Extra.OTstartTime = CASE 
																		WHEN b.IsPublicHoliday = 1 THEN b.Shaved_IN
																		WHEN RTRIM(ISNULL(b.ShiftCode, '')) = @CODE_Shift_Off THEN b.Shaved_IN
																		WHEN b.IsDILdayWorker = 1 AND ISNULL(b.IsSalStaff, 0) = 0 AND b.IsDayWorker_OR_Shifter = 1 THEN b.Shaved_IN
																		ELSE DATEADD(n, (ISNULL(b.Duration_Worked_Cumulative, 0) - ISNULL(b.Duration_Required, 0)) * -1, b.Shaved_OUT)
																	END,
							tas.Tran_Timesheet_Extra.OTendTime = b.Shaved_OUT,
							tas.Tran_Timesheet_Extra.OTtype =	CASE 
																	WHEN b.IsPublicHoliday = 1 THEN @code_OTtype_PublicHoliday
																	WHEN RTRIM(ISNULL(b.ShiftCode, '')) = @CODE_Shift_Off THEN @code_OTtype_DayOff
																	WHEN b.IsDILdayWorker = 1 AND ISNULL(b.IsSalStaff, 0) = 0 AND b.IsDayWorker_OR_Shifter = 1 THEN @code_OTtype_DILdw
																	ELSE @code_OTtype_Regular
																END,
							tas.Tran_Timesheet_Extra.LastUpdateUser = 'System Admin',
							tas.Tran_Timesheet_Extra.LastUpdateTime = GETDATE()
						FROM tas.Tran_Timesheet_Extra a
							INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID
							INNER JOIN tas.syJDE_F060116 c ON b.EmpNo = c.YAAN8
						WHERE
							CONVERT(VARCHAR, b.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
							AND (b.EmpNo = @empNo OR @empNo IS NULL)
							AND (RTRIM(LTRIM(c.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
							AND b.IsLastRow = 1

							--Start of Rev. #1.8
							AND 
							(
								(
									b.Duration_Worked_Cumulative > b.Duration_Required
									AND
									b.Duration_Worked_Cumulative - b.Duration_Required > 30
								)
								OR
								(
									b.Duration_Worked_Cumulative <= b.Duration_Required 
									AND 
									b.IsPublicHoliday = 1
								)
								OR
								(
									RTRIM(b.ShiftCode) = 'O' AND b.Duration_Worked_Cumulative > 0
								)
							)	
							--End of Rev. #1.8

							AND b.Shaved_IN IS NOT NULL
							AND b.Shaved_OUT IS NOT NULL 
							AND ISNULL(b.IsDriver, 0) = 0
							AND ISNULL(b.IsLiasonOfficer, 0) = 0
							--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
							--AND ISNULL(b.IsDayWorker_OR_Shifter, 0) = 0 
							AND ISNULL(b.AbsenceReasonCode, '') = ''
							AND ISNULL(b.LeaveType, '') = ''
							--AND ISNULL(b.CorrectionCode, '') = ''	--Rev. #1.7		
							--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
							--(
							--	DATEDIFF(n, b.dtIN, b.dtOUT) > @minutes_MinOT_NSS	
							--	AND (RTRIM(b.OTType) = @code_OTtype_Regular OR ISNULL(b.OTType, '') = '')
							--)	
							AND RTRIM(ISNULL(b.OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
					END

					--Get the number of rows updated
					SET @rowsAffected = @@ROWCOUNT

					IF @rowsAffected > 0
					BEGIN
						
						IF NOT EXISTS 
						(
							SELECT LogID 
							FROM tas.Tran_Timesheet_Log a
								INNER JOIN tas.Tran_Timesheet b ON a.TSAutoID = b.AutoID
								INNER JOIN tas.syJDE_F060116 c ON b.EmpNo = c.YAAN8
							WHERE
								CONVERT(VARCHAR, b.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
								AND (b.EmpNo = @empNo OR @empNo IS NULL)
								AND (RTRIM(LTRIM(c.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
								AND b.IsLastRow = 1

								--Start of Rev. #1.8
								AND 
								(
									(
										b.Duration_Worked_Cumulative > b.Duration_Required
										AND
										b.Duration_Worked_Cumulative - b.Duration_Required > 30
									)
									OR
									(
										b.Duration_Worked_Cumulative <= b.Duration_Required 
										AND 
										b.IsPublicHoliday = 1
									)
									OR
									(
										RTRIM(b.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
									)
								)	
								--End of Rev. #1.8

								AND b.Shaved_IN IS NOT NULL
								AND b.Shaved_OUT IS NOT NULL 
								AND ISNULL(b.IsDriver, 0) = 0
								AND ISNULL(b.IsLiasonOfficer, 0) = 0
								--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
								--AND ISNULL(b.IsDayWorker_OR_Shifter, 0) = 0 
								AND ISNULL(b.AbsenceReasonCode, '') = ''
								AND ISNULL(b.LeaveType, '') = ''	
								--AND ISNULL(b.CorrectionCode, '') = ''	--Rev. #1.7	
								--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
								--(
								--	DATEDIFF(n, b.dtIN, b.dtOUT) > @minutes_MinOT_NSS	
								--	AND (RTRIM(b.OTType) = @code_OTtype_Regular OR ISNULL(b.OTType, '') = '')
								--)	
								AND RTRIM(ISNULL(b.OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
						)
						BEGIN

							--Insert log history record
							INSERT INTO tas.Tran_Timesheet_Log
							(
								[TSAutoID]
							   ,[EmpNo]
							   ,[DT]
							   ,[dtIN]
							   ,[dtOUT]
							   ,[ShiftPatCode]
							   ,[ShiftCode]
							   ,[Actual_ShiftCode]
							   ,[BusinessUnit]
							   ,[GradeCode]
							   ,[Field1Name]
							   ,[Field1Value]
							   ,[Field2Name]
							   ,[Field2Value]
							   ,[Field3Name]
							   ,[Field3Value]
							   ,[Field4Name]
							   ,[Field4Value]
							   ,[Field5Name]
							   ,[Field5Value]
							   ,[Field6Name]
							   ,[Field6Value]
							)
							SELECT
								a.AutoID,
								a.EmpNo, 
								a.DT, 
								a.dtIN, 
								a.dtOUT, 
								a.ShiftPatCode, 
								a.ShiftCode, 
								a.Actual_ShiftCode, 
								a.BusinessUnit, 
								a.GradeCode, 

								'OTstartTime_Extra' AS Field1Name, 
								CONVERT(VARCHAR(200),
									CASE 
										WHEN a.IsPublicHoliday = 1 THEN a.Shaved_IN
										WHEN RTRIM(ISNULL(a.ShiftCode, '')) = @CODE_Shift_Off THEN a.Shaved_IN
										WHEN IsDILdayWorker = 1 AND ISNULL(a.IsSalStaff, 0) = 0 AND a.IsDayWorker_OR_Shifter = 1 THEN a.Shaved_IN
										ELSE DATEADD(n, (ISNULL(a.Duration_Worked_Cumulative, 0) - ISNULL(Duration_Required, 0)) * -1, a.Shaved_OUT)
									END, 126
								) AS Field1Value,

								'OTendTime_Extra' AS Field2Name, 
								CONVERT(VARCHAR(200), a.Shaved_OUT, 126) AS Field2Value,

								'OTtype_Extra' AS Field3Name, 
								CONVERT(VARCHAR(200),
									CASE 
										WHEN a.IsPublicHoliday = 1 THEN @code_OTtype_PublicHoliday
										WHEN RTRIM(ISNULL(a.ShiftCode, '')) = @CODE_Shift_Off THEN @code_OTtype_DayOff
										WHEN IsDILdayWorker = 1 AND ISNULL(a.IsSalStaff, 0) = 0 AND a.IsDayWorker_OR_Shifter = 1 THEN @code_OTtype_DILdw
										ELSE @code_OTtype_Regular
									END
								) AS Field3Value,

								'OTStartTime_TS' AS Field4Name,
								CONVERT(VARCHAR, @OTStartTime_Orig, 126) AS Field4Value,
								'OTEndTime_TS' AS Field5Name,
								CONVERT(VARCHAR, @OTEndTime_Orig, 126) AS Field5Value,
								'OTType_TS' AS Field6Name,
								RTRIM(@OTType_Orig) AS Field6Value
							FROM tas.Tran_Timesheet a
								INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
							WHERE
								CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
								AND (a.EmpNo = @empNo OR @empNo IS NULL)
								AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
								AND IsLastRow = 1

								--Start of Rev. #1.8
								AND 
								(
									(
										a.Duration_Worked_Cumulative > a.Duration_Required
										AND
										a.Duration_Worked_Cumulative - a.Duration_Required > 30
									)
									OR
									(
										a.Duration_Worked_Cumulative <= a.Duration_Required 
										AND 
										a.IsPublicHoliday = 1
									)
									OR
									(
										RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
									)
								)	
								--End of Rev. #1.8

								AND Shaved_IN IS NOT NULL
								AND Shaved_OUT IS NOT NULL 
								AND ISNULL(IsDriver, 0) = 0
								AND ISNULL(IsLiasonOfficer, 0) = 0
								--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
								--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
								AND ISNULL(AbsenceReasonCode, '') = ''
								AND ISNULL(LeaveType, '') = ''	
								--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #1.7
								--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
								--(
								--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
								--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
								--)	
								AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
						END
					END
				--END

				--ELSE 
				--BEGIN
					
				--	SELECT	@hasError = 1,
				--			@retError = @CONST_RETURN_ERROR,
				--			@retErrorDesc = 'No record has been updated in the Tran_Timesheet table.'
				
				--	GOTO EXIT_POINT
				--END
			END
		END

		ELSE IF @actionTypeID = 2	--Undo processing of OT records for Aspire Employees
		BEGIN

			IF EXISTS 
			(
				SELECT a.XID_AutoID 
				FROM tas.Tran_Timesheet_Extra a
					INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID
					INNER JOIN tas.syJDE_F060116 c ON b.EmpNo = c.YAAN8
				WHERE
					CONVERT(VARCHAR, b.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
					AND (b.EmpNo = @empNo OR @empNo IS NULL)
					AND (RTRIM(LTRIM(c.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
					AND b.IsLastRow = 1

					--Start of Rev. #1.8
					AND 
					(
						(
							b.Duration_Worked_Cumulative > b.Duration_Required
							AND
							b.Duration_Worked_Cumulative - b.Duration_Required > 30
						)
						OR
						(
							b.Duration_Worked_Cumulative <= b.Duration_Required 
							AND 
							b.IsPublicHoliday = 1
						)
						OR
						(
							RTRIM(b.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
						)
					)	
					--End of Rev. #1.8

					AND b.Shaved_IN IS NOT NULL
					AND b.Shaved_OUT IS NOT NULL 
					AND ISNULL(b.IsDriver, 0) = 0
					AND ISNULL(b.IsLiasonOfficer, 0) = 0
					--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
					--AND ISNULL(b.IsDayWorker_OR_Shifter, 0) = 0 
					AND ISNULL(b.AbsenceReasonCode, '') = ''
					AND ISNULL(b.LeaveType, '') = ''	
					--AND ISNULL(b.CorrectionCode, '') = ''	--Rev. #1.7
					--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
					--(
					--	DATEDIFF(n, b.dtIN, b.dtOUT) > @minutes_MinOT_NSS	
					--	AND (RTRIM(b.OTType) = @code_OTtype_Regular OR ISNULL(b.OTType, '') = '')
					--)	
					AND RTRIM(ISNULL(b.OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
			)
			BEGIN

				--Delete records in the OT Approval table
				DELETE FROM tas.Tran_Timesheet_Extra
				WHERE XID_AutoID IN
				(
					SELECT a.AutoID 
					FROM tas.Tran_Timesheet a
						INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
					WHERE
						CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
						AND (a.EmpNo = @empNo OR @empNo IS NULL)
						AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
						AND IsLastRow = 1

						--Start of Rev. #1.8
						AND 
						(
							(
								a.Duration_Worked_Cumulative > a.Duration_Required
								AND
								a.Duration_Worked_Cumulative - a.Duration_Required > 30
							)
							OR
							(
								a.Duration_Worked_Cumulative <= a.Duration_Required 
								AND 
								a.IsPublicHoliday = 1
							)
							OR
							(
								RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
							)
						)	
						--End of Rev. #1.8

						AND Shaved_IN IS NOT NULL
						AND Shaved_OUT IS NOT NULL 
						AND ISNULL(IsDriver, 0) = 0
						AND ISNULL(IsLiasonOfficer, 0) = 0
						--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
						--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
						AND ISNULL(AbsenceReasonCode, '') = ''
						AND ISNULL(LeaveType, '') = ''	
						--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #1.7
						--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
						--(
						--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
						--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
						--)	
						AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
				)

				--Get the number of rows updated
				SET @rowsAffected = @@ROWCOUNT

				IF @rowsAffected > 0
				BEGIN

					--Delete log history records 
					DELETE FROM tas.Tran_Timesheet_Log
					WHERE TSAutoID IN
					(
						SELECT a.AutoID 
						FROM tas.Tran_Timesheet a
							INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
						WHERE
							CONVERT(VARCHAR, DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
							AND (a.EmpNo = @empNo OR @empNo IS NULL)
							AND (RTRIM(LTRIM(b.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
							AND IsLastRow = 1

							--Start of Rev. #1.8
							AND 
							(
								(
									a.Duration_Worked_Cumulative > a.Duration_Required
									AND
									a.Duration_Worked_Cumulative - a.Duration_Required > 30
								)
								OR
								(
									a.Duration_Worked_Cumulative <= a.Duration_Required 
									AND 
									a.IsPublicHoliday = 1
								)
								OR
								(
									RTRIM(a.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0
								)
							)	
							--End of Rev. #1.8

							AND Shaved_IN IS NOT NULL
							AND Shaved_OUT IS NOT NULL 
							AND ISNULL(IsDriver, 0) = 0
							AND ISNULL(IsLiasonOfficer, 0) = 0
							--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
							--AND ISNULL(IsDayWorker_OR_Shifter, 0) = 0 
							AND ISNULL(AbsenceReasonCode, '') = ''
							AND ISNULL(LeaveType, '') = ''		
							--AND ISNULL(a.CorrectionCode, '') = ''	--Rev. #1.7
							--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
							--(
							--	DATEDIFF(n, dtIN, dtOUT) > @minutes_MinOT_NSS	
							--	AND (RTRIM(OTType) = @code_OTtype_Regular OR ISNULL(OTType, '') = '')
							--)	
							AND RTRIM(ISNULL(OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
					)
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

EXIT_POINT:

	IF @retError = @CONST_RETURN_OK
	BEGIN

		IF @@TRANCOUNT > 0
			COMMIT TRANSACTION;		
	END

	ELSE
	BEGIN

		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION
	END

	IF @actionTypeID = 1 and ISNULL(@hasError, 0) = 0 
	BEGIN

		--Return overtime records in 
		SELECT 
			b.AutoID, b.DT, b.EmpNo, 
			RTRIM(LTRIM(c.YAMCU)) AS BusinessUnit,	--Rev. #1.5 
			a.OTstartTime, a.OTendTime, a.OTtype,
			@hasError AS HasError, 
			@retError AS ErrorCode, 
			@retErrorDesc AS ErrorDescription,
			@rowsAffected AS RowsAffected
		FROM tas.Tran_Timesheet_Extra a
			INNER JOIN tas.Tran_Timesheet b ON a.XID_AutoID = b.AutoID
			INNER JOIN tas.syJDE_F060116 c ON b.EmpNo = c.YAAN8
		WHERE
			CONVERT(VARCHAR, b.DT, 12) BETWEEN CONVERT(VARCHAR, @startDate, 12) AND CONVERT(VARCHAR, @endDate, 12) 
			AND (b.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(LTRIM(c.YAHMCU)) = RTRIM(@costCenter))	--Rev. #1.3
			AND b.IsLastRow = 1
			AND 
			(
				Duration_Worked_Cumulative > Duration_Required	--Rev. #1.2
				OR 
				(RTRIM(b.ShiftCode) = 'O' AND b.Duration_Worked_Cumulative > 0)
				--(ISNULL(b.Actual_ShiftCode, b.ShiftCode) = 'O' AND Duration_Worked_Cumulative > 0)
			)
			AND b.Shaved_IN IS NOT NULL
			AND b.Shaved_OUT IS NOT NULL 
			AND ISNULL(b.IsDriver, 0) = 0
			AND ISNULL(b.IsLiasonOfficer, 0) = 0
			--AND ISNULL(IsSalStaff, 0) = 0		--Rev. #1.9
			--AND ISNULL(b.IsDayWorker_OR_Shifter, 0) = 0 
			AND ISNULL(b.AbsenceReasonCode, '') = ''
			AND ISNULL(b.LeaveType, '') = ''
			--AND ISNULL(b.CorrectionCode, '') = ''	--Rev. #1.7	
			--AND	--(Note: Check if OT duration is greater than @minutes_MinOT_NSS during regular days)	
			--(
			--	DATEDIFF(n, b.dtIN, b.dtOUT) > @minutes_MinOT_NSS	
			--	AND (RTRIM(b.OTType) = @code_OTtype_Regular OR ISNULL(b.OTType, '') = '')
			--)	
			AND RTRIM(ISNULL(b.OTType, '')) <> @code_OTtype_PublicHoliday	--(Note: Exclude public holidays since OT is auto calculated)
			--AND
			--(
			--	DATEDIFF(n, a.OTstartTime, a.OTendTime) > @minutes_MinOT_NSS	
			--	AND 
			--	(RTRIM(a.OTType) = @code_OTtype_Regular OR ISNULL(a.OTType, '') = '')
			--)
		ORDER BY b.DT DESC, b.BusinessUnit, b.EmpNo
	END

	ELSE 
	BEGIN
		
		--Return error information to the caller
		SELECT	@hasError AS HasError, 
				@retError AS ErrorCode, 
				@retErrorDesc AS ErrorDescription,
				@rowsAffected AS RowsAffected
	END

