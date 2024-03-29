USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnGetProcessedWorkplaceDataToSync]    Script Date: 05/09/2022 12:21:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************************************

User-defined Function Name		:	tas.fnGetProcessedWorkplaceDataToSync
Description						:	This tabled-view function is used to calculate the shaving time based on swipe in and out times at the workplace readers
Created By						:	Ervin O. Brosas
Date Created					:	25 July 2015

Parameters
	@empNo						:	The employee no. to process the swipe data
	@processDate				:	Refers to the Timesheet date 
	@workplaceSwipeIn			:	Refers to the swipe-in time at the workplace
	@workplaceSwipeOut			:	Refers to the swipe-out time at the workplace
	@correctionType				:	The workplace swipe correction types: 1 => Time-in; 2 => Time-out; 3 => Both

Revision History:
	1.0					EOB				2015.07.25 11:36
	Created

	1.1					EOB				2015.08.10 11:37
	Modified the calculation for the overtime start time

	1.2					EOB				2015.12.10 16:37
	Refactored the logic in calculation the OT Start Time

	1.3					EOB				2016.02.05 18:37
	Refactored calculation of Overtime and NPH based on @shiftCode

	1.4					EOB				2016.04.18 12:54
	Commented the code added with Rev. #1.2

	1.5					EOB				2016.04.18 13:16
	Check if overtime is auto approved for certain cost center in the "OTApprovalSetting" table

	1.6					EOB				2016.06.22 10:16
	Added validation that checks if the Shift Code used is correct

	1.7					EOB				2016.06.25 21:55
	Added extra checking to get the correct shift timing 

	1.8					EOB				2016.06.29 12:05
	Added @actualCostCenter variable that is used in the overtime calculation

	1.9					EOB				2016.06.30 11:52
	Set the value of "@otAutoApprove" to 1 if process date falls on Ramadan period

	2.0					EOB				2016.07.01 13:40
	Modified the logic in calculating the overtime check the value of @shiftSpan variable.

	2.1					EOB				2016.07.01 16:50
	Moved Shift Span Initiation process after the calculation of Shaving Time

	2.2					EOB				2016.07.03 11:00
	Modified the logic in calculating the Duration_Worked_Cumulative, NetMinutes and Overtime

	2.3					EOB				2016.07.09 11:35
	Added condition that checks if DATEDIFF(n, @shavedIn, @ShavedOut) = @sum_Duration_Worked_CumulativeDuration in calculating the total work duration. Modified the calculation of @sum_NetMinutes

	2.4					EOB				2017.03.16 13:38
	Added condition that set NoPayHour to zero if "LeaveType" is not null

	2.5					EOB				2017.03.21 08:17
	Added condition to set NoPayHours = Duration_Required if Duration_Worked_Cumulative is equal to zero

	2.6					EOB				2017.11.20 13:07
	Commented the code that sets the overtime start and end times equals to the shaving time

	2.7					EOB				2021.01.05 15:07
	Refactored code to enhance performance

	2.8					EOB				2021.01.28 15:11
	Modified the logic in fetching the correct Shift Code

	2.9					EOB				2021.02.07 11:54
	Fixed the bug found for identifying the correct shift code to use especially when an employee is day-off

****************************************************************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetProcessedWorkplaceDataToSync]
(
	@empNo						int,
	@processDate				datetime,
	@workplaceSwipeIn			datetime,
	@workplaceSwipeOut			datetime,
	@correctionType				tinyint
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo int,
	PayGrade int,
	ShiftPatCode varchar(2),
	ShiftCode varchar(10),
	Actual_ShiftCode varchar(10),
	DT datetime,
	dtIN datetime,
	dtOUT datetime,
	Shaved_IN datetime, 
	Shaved_OUT datetime,
	ArrivalFrom datetime,
	ArrivalTo datetime,		
	DepartFrom datetime,
	DepartTo datetime,
	NoPayHours int,
	Duration_Required int,
	Duration_Worked_Cumulative int,
	NetMinutes int,
	OTType varchar(10),
	OTStartTime datetime,
	OTEndTime datetime,
	OTAutoApprove bit,
	OTDuration int,
	EnableOT bit,
	ShiftSpan bit,
	ShiftAllowance bit,
	Duration_ShiftAllowance_Evening	int,
	Duration_ShiftAllowance_Night int
) 
AS 
BEGIN

	DECLARE @myTable TABLE 
	(
		EmpNo int,
		PayGrade int,
		ShiftPatCode varchar(2),
		ShiftCode varchar(10),
		Actual_ShiftCode varchar(10),
		DT datetime,
		dtIN datetime,
		dtOUT datetime,
		Shaved_IN datetime, 
		Shaved_OUT datetime,
		ArrivalFrom datetime,
		ArrivalTo datetime,		
		DepartFrom datetime,
		DepartTo datetime,
		NoPayHours int,
		Duration_Required int,
		Duration_Worked_Cumulative int,
		NetMinutes int,
		OTType varchar(10),
		OTStartTime datetime,
		OTEndTime datetime,
		OTAutoApprove bit,
		OTDuration int,
		EnableOT bit,
		ShiftSpan bit,
		ShiftAllowance bit,
		Duration_ShiftAllowance_Evening	int,
		Duration_ShiftAllowance_Night int
	) 

	--Declare flags
	DECLARE	@FLAG_Code_ROA_TrainingLocal		varchar(10),
			@FLAG_Code_ROA_TrainingForeign		varchar(10),
			@FLAG_Code_ContGroup_NonSal			varchar(10),
			@FLAG_Minutes_MinOT_NSS				int,
			@FLAG_Minutes_MinOT_SS				int,
			@FLAG_Minutes_MinOT_SS_Ramadan		int,
			@FLAG_Minutes_MinShiftAllowance		int

	--Initialize flags
	SELECT	@FLAG_Code_ROA_TrainingLocal	= RTRIM(Code_ROA_TrainingLocal),
			@FLAG_Code_ROA_TrainingForeign	= RTRIM(Code_ROA_TrainingForeign),
			@FLAG_Code_ContGroup_NonSal		= RTRIM(Code_ContGroup_NonSal),
			@FLAG_Minutes_MinOT_NSS			= Minutes_MinOT_NSS,
			@FLAG_Minutes_MinOT_SS			= Minutes_MinOT_SS,
			@FLAG_Minutes_MinOT_SS_Ramadan	= Minutes_MinOT_SS_Ramadan,
			@FLAG_Minutes_MinShiftAllowance	= Minutes_MinShiftAllowance
	FROM tas.System_Values

	--Declare field variables
	DECLARE @payGrade							int,
			@shiftPatCode						varchar(2),	
			@shiftCode							varchar(10),
			@actual_ShiftCode					varchar(10),
			@dtIN								datetime,
			@dtOUT								datetime,
			@shavedIN							datetime,
			@shavedOUT							datetime,
			@arrivalFrom						datetime,
			@arrivalTo							datetime,
			@departFrom							datetime,
			@departTo							datetime,
			@duration_Required					int,
			@duration_Worked_Cumulative			int,
			@netMinutes							int,
			@calculatedNPH						int,
			@noPayHours							int,	
			@enableOT							bit,	
			@otAutoApprove						bit,
			@otType								varchar(10),
			@otStartTime						datetime,
			@otEndTime							datetime,
			@otDuration							int,
			@shiftSpan							bit,
			@shiftAllowance						bit,
			@duration_ShiftAllowance_Evening	int,
			@duration_ShiftAllowance_Night		INT,
			@autoApprovedOTByCostCenter			BIT,
			@empCostCenter						VARCHAR(12) 

	--Initialize field variables
	SELECT	@payGrade							= 0,
			@shiftPatCode						= '',
			@shiftCode							= '',
			@actual_ShiftCode					= '',
			@dtIN								= NULL,
			@dtOUT								= NULL,
			@shavedIN							= NULL,
			@shavedOUT							= NULL,
			@arrivalFrom						= NULL,
			@arrivalTo							= NULL,
			@departFrom							= NULL,
			@departTo							= NULL,
			@duration_Worked_Cumulative			= 0,
			@netMinutes							= 0,
			@duration_Required					= 0,
			@calculatedNPH						= 0,
			@noPayHours							= 0,	
			@enableOT							= 0,
			@otAutoApprove						= 0,
			@otType								= NULL,
			@otStartTime						= NULL,
			@otEndTime							= NULL,
			@otDuration							= 0,
			@shiftSpan							= 0,
			@shiftAllowance						= 0,
			@duration_ShiftAllowance_Evening	= 0,
			@duration_ShiftAllowance_Night		= 0,
			@autoApprovedOTByCostCenter			= 0,
			@empCostCenter						= ''

	--Declare other variables
	DECLARE	@isSalStaff					bit,
			@isDayWorker_OR_Shifter		bit,	
			@isDILdayWorker				bit,
			@isDriver					bit,	
			@isLiasonOfficer			bit,
			@isEmployee_OR_Contractor	bit,
			@isPublicHoliday			bit,
			@isRamadan					bit,	
			@isMuslim					bit,	
			@absenceReasonCode			varchar(10),
			@contractorGroupCode		varchar(10),
			@leaveType					varchar(10),
			@sum_Duration_Worked_CumulativeDuration	INT,
			@sum_NetMinutes				INT,
			@actualCostCenter			VARCHAR(12)

	--Initialize other variables
	SELECT	@isSalStaff					= 0,
			@isDayWorker_OR_Shifter		= 0,
			@isDILdayWorker				= 0,
			@isDriver					= 0,	
			@isLiasonOfficer			= 0,
			@isEmployee_OR_Contractor	= 0,
			@isPublicHoliday			= 0,
			@isRamadan					= 0,
			@isMuslim					= 0,	
			@absenceReasonCode			= '',
			@contractorGroupCode		= '',
			@leaveType					= '',
			@sum_Duration_Worked_CumulativeDuration	= 0,
			@sum_NetMinutes				= 0,
			@actualCostCenter			= ''
			
	--Get the parent cost center
	SELECT @actualCostCenter = RTRIM(a.ActualCostCenter)
	FROM tas.Master_Employee_JDE_View_V2 a
	WHERE a.EmpNo = @empNo

	--Set values to variables and flags
	SELECT	@payGrade = CASE WHEN ISNUMERIC(GradeCode) = 1
							THEN CONVERT(INT, GradeCode)
							ELSE 0
						END,
			@shiftPatCode = RTRIM(ISNULL(ShiftPatCode, '')),
			@shiftCode = CASE WHEN RTRIM(b.Effective_ShiftCode) <> 'O' THEN RTRIM(b.Effective_ShiftCode) ELSE RTRIM(a.Actual_ShiftCode) END,	--Rev. #2.9
			@actual_ShiftCode = RTRIM(Actual_ShiftCode),
			@dtIN = dtIN,
			@dtOUT = dtOUT,
			@noPayHours = ISNULL(NoPayHours, 0),
			@duration_Required = ISNULL(Duration_Required, 0),
			@isSalStaff = ISNULL(IsSalStaff, 0),
			@isDayWorker_OR_Shifter = ISNULL(IsDayWorker_OR_Shifter, 0),
			@isDILdayWorker = ISNULL(IsDILdayWorker, 0),
			@isDriver = ISNULL(IsDriver, 0),
			@isLiasonOfficer = ISNULL(IsLiasonOfficer, 0),
			@isEmployee_OR_Contractor = ISNULL(IsEmployee_OR_Contractor, 0),
			@isPublicHoliday = ISNULL(IsPublicHoliday, 0),
			@isRamadan = ISNULL(IsRamadan, 0),
			@isMuslim = ISNULL(isMuslim, 0),			
			@absenceReasonCode = RTRIM(ISNULL(AbsenceReasonCode, '')),
			@contractorGroupCode = RTRIM(ISNULL(ContractorGroupCode, '')),
			@leaveType = RTRIM(ISNULL(LeaveType, '')),
			@empCostCenter = RTRIM(BusinessUnit),
			@shiftSpan = ShiftSpan
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		INNER JOIN tas.Tran_ShiftPatternUpdates b WITH (NOLOCK) ON a.EmpNo = b.EmpNo AND a.DT = b.DateX		--Rev. #2.8
	WHERE a.EmpNo = @empNo 		
		AND a.IsLastRow = 1
		AND a.DT = @processDate

	--Get the Shift timing info
	SELECT	@arrivalFrom = a.ArrivalFrom,
			@arrivalTo = a.ArrivalTo,
			@departFrom = a.DepartFrom,
			@departTo = a.DepartTo
	FROM tas.Master_ShiftTimes a WITH (NOLOCK)
	WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
		AND RTRIM(a.ShiftCode) = @shiftCode		--Rev. #2.8

	--Start of Rev. #1.6
	--Validate if shift timing is correct
	IF @workplaceSwipeIn IS NOT NULL	
		AND 
		(
			(ISNULL(@actual_ShiftCode , '') <> '' AND ISNULL(@shiftCode, '') <> '')
			AND
            @actual_ShiftCode <> @shiftCode
		)
		AND NOT CONVERT(TIME, @workplaceSwipeIn) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @arrivalTo)
	BEGIN

		--Get the Shift timing info based on @shiftCode
		SELECT	@arrivalFrom = a.ArrivalFrom,
				@arrivalTo = a.ArrivalTo,
				@departFrom = a.DepartFrom,
				@departTo = a.DepartTo
		FROM tas.Master_ShiftTimes a WITH (NOLOCK)
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
			AND RTRIM(a.ShiftCode) = @shiftCode

		--Start of Rev. #1.7
		--Check again if the correct shift timing is not found
		IF NOT CONVERT(TIME, @workplaceSwipeIn) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @arrivalTo)
		BEGIN

			--Search for the correct shift timing
			SELECT	@arrivalFrom = a.ArrivalFrom,
					@arrivalTo = a.ArrivalTo,
					@departFrom = a.DepartFrom,
					@departTo = a.DepartTo,
					@shiftCode = RTRIM(a.ShiftCode)
			FROM tas.Master_ShiftTimes a WITH (NOLOCK)
			WHERE CONVERT(TIME, @workplaceSwipeIn) BETWEEN CONVERT(TIME, a.ArrivalFrom)	AND CONVERT(TIME, a.ArrivalTo)
				AND 
				(
					CONVERT(TIME, @workplaceSwipeOut) BETWEEN CONVERT(TIME, a.DepartFrom)	AND CONVERT(TIME, a.DepartTo)
					OR
					CONVERT(TIME, @workplaceSwipeOut) > CONVERT(TIME, a.DepartTo)
				)
				AND RTRIM(a.ShiftPatCode) = @shiftPatCode
        END 
		--End of Rev. #1.7
    END	
	--End of Rev. #1.6

	/***************************************************************************************
		Calculate Shaving Time
	****************************************************************************************/
	IF ISNULL(@arrivalFrom, '') <> '' AND ISNULL(@arrivalTo, '') <> '' 
		AND ISNULL(@departFrom, '') <> '' AND ISNULL(@departTo, '') <> ''
	BEGIN

		IF CONVERT(TIME, @workplaceSwipeIn) BETWEEN CONVERT(TIME, @arrivalFrom) AND CONVERT(TIME, @arrivalTo)
			SET @shavedIN = CONVERT(DATETIME, CONVERT(VARCHAR, @workplaceSwipeIn, 12) + ' ' + CONVERT(VARCHAR, @arrivalTo, 108), 12)
		ELSE
			SET @shavedIN = @workplaceSwipeIn

		IF CONVERT(TIME, @workplaceSwipeOut) BETWEEN CONVERT(TIME, @departFrom) AND CONVERT(TIME, @departTo)
			SET @shavedOUT = CONVERT(DATETIME, CONVERT(VARCHAR, @workplaceSwipeOut, 12) + ' ' + CONVERT(VARCHAR, @departFrom, 108), 12)
		ELSE
			SET @shavedOUT = @workplaceSwipeOut
	END

	--Start of Rev. #2.1
	/************************************************************************************************************
		Shift Span Initialization
	************************************************************************************************************/
	IF	CONVERT(TIME, @shavedIN) < CONVERT(TIME, @departFrom) 
		AND CONVERT(TIME, @departTo) < CONVERT(TIME, @shavedOUT)
		AND (@shiftCode = 'N' OR @actual_ShiftCode = 'N')
		SET @shiftSpan = 1

	IF @shiftSpan = 1
	BEGIN

		--Set actual shift code to evening shift
		SET @actual_ShiftCode = 'E'

		--Recalculate @duration_ShiftAllowance_Evening and @duration_ShiftAllowance_Night
		--SELECT	@duration_ShiftAllowance_Evening = DATEDIFF(n, @shavedIn, @ShavedOut),
		--		@duration_ShiftAllowance_Night = DATEDIFF(n, @shavedIn, @ShavedOut)

		--IF @duration_ShiftAllowance_Evening >= @FLAG_Minutes_MinShiftAllowance
		--	OR @duration_ShiftAllowance_Night >= @FLAG_Minutes_MinShiftAllowance
		--	SET @shiftAllowance = 1
		--ELSE
		--	SET @shiftAllowance = 0
	END
	/**************************************** End of Shift Span Initialization ******************************************/

	/***************************************************************************************
		Calculate Total Work Duration
	****************************************************************************************/
	IF ISNULL(@shavedIn, '') <> '' AND ISNULL(@shavedOUT, '') <> ''
	BEGIN

		--Check if there are other records in the Timesheet
		IF EXISTS
		(
			SELECT a.AutoID FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.DT = @processDate
				AND ISNULL(a.IsLastRow, 0) = 0
		)
		BEGIN

			--Get the total work duration
			SELECT @sum_Duration_Worked_CumulativeDuration = SUM(Duration_Worked_Cumulative)
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.DT = @processDate
				AND ISNULL(a.IsLastRow, 0) = 0
		END

		IF	@isRamadan = 1 
			--AND @isMuslim = 1 
			AND ISNULL(@shiftSpan, 0) = 0
			AND @shavedIn IS NOT NULL 
			AND @ShavedOut IS NOT NULL
        BEGIN
			
			SET @duration_Worked_Cumulative = DATEDIFF(n, @shavedIn, @ShavedOut) 
			IF @duration_Worked_Cumulative < 0
				SET @duration_Worked_Cumulative = DATEDIFF(n, @shavedIn, @ShavedOut) + (24 * 60)
		END	

		ELSE
        BEGIN
        
			--Start of Rev. #2.2
			IF ISNULL(@isRamadan, 0) = 1
			BEGIN
			 
				IF DATEDIFF(n, @shavedIn, @ShavedOut) < 0
					SET @duration_Worked_Cumulative = (DATEDIFF(n, @shavedIn, @ShavedOut) + (24 * 60)) 
				ELSE 
					SET @duration_Worked_Cumulative = DATEDIFF(n, @shavedIn, @ShavedOut) 
			END

            ELSE
            BEGIN
            
				IF DATEDIFF(n, @shavedIn, @ShavedOut) < 0
				BEGIN
                
					IF DATEDIFF(n, @shavedIn, @ShavedOut) = @sum_Duration_Worked_CumulativeDuration		--Rev. #2.3
						SET @duration_Worked_Cumulative = DATEDIFF(n, @shavedIn, @ShavedOut) + (24 * 60)
					ELSE 
						SET @duration_Worked_Cumulative = (DATEDIFF(n, @shavedIn, @ShavedOut) + (24 * 60)) + @sum_Duration_Worked_CumulativeDuration
				END 

				ELSE 
				BEGIN
                
					IF DATEDIFF(n, @shavedIn, @ShavedOut) = @sum_Duration_Worked_CumulativeDuration		--Rev. #2.3
						SET @duration_Worked_Cumulative = DATEDIFF(n, @shavedIn, @ShavedOut) 
					ELSE
						SET @duration_Worked_Cumulative = DATEDIFF(n, @shavedIn, @ShavedOut) + @sum_Duration_Worked_CumulativeDuration
				END 
			END 
			--End of Rev. #2.2
		END 
	END


	/***************************************************************************************
		Set the official Time In/Out
	****************************************************************************************/	
	IF @correctionType = 1	
	BEGIN

		--Synchronize workplace swipe-in to Timesheet
		SET @dtIN = @workplaceSwipeIn
	END

	ELSE IF @correctionType = 2	
	BEGIN

		--Synchronize workplace swipe-out to Timesheet
		SET @dtOUT = @workplaceSwipeOut
	END

	ELSE IF @correctionType = 3	
	BEGIN

		--Synchronize workplace swipe in and out to Timesheet
		SELECT	@dtIN = @workplaceSwipeIn,
				@dtOUT = @workplaceSwipeOut
	END		

	--Start of Rev. #1.6
	ELSE 
	BEGIN

		--Synchronize workplace swipe in and out to Timesheet
		SELECT	@dtIN = @workplaceSwipeIn,
				@dtOUT = @workplaceSwipeOut
	END		
	--End of Rev. #1.6

	/***************************************************************************************
		Calculate Net Work Duration
	****************************************************************************************/
	--Calculate the value for "@netMinutes" based on the official time in and out
	IF ISNULL(@dtIN, '') <> '' AND ISNULL(@dtOUT, '') <> ''
	BEGIN	

		--Check if there are other records in the Timesheet
		IF EXISTS
		(
			SELECT a.AutoID FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.DT = @processDate
				AND ISNULL(a.IsLastRow, 0) = 0
		)
		BEGIN

			--Get the total work duration
			--SELECT @sum_NetMinutes = SUM(a.Duration_Worked)
			SELECT @sum_NetMinutes = DATEDIFF(n, a.dtIN, a.dtOUT)	--Rev. #2.3
			FROM tas.Tran_Timesheet a WITH (NOLOCK)
			WHERE a.EmpNo = @empNo
				AND a.DT = @processDate
				AND ISNULL(a.IsLastRow, 0) = 0

			IF @sum_NetMinutes < 0
			BEGIN
            
				SELECT @sum_NetMinutes = DATEDIFF(n, a.dtIN, a.dtOUT) + (24 * 60)
				FROM tas.Tran_Timesheet a WITH (NOLOCK)
				WHERE a.EmpNo = @empNo
					AND a.DT = @processDate
					AND ISNULL(a.IsLastRow, 0) = 0
			END 
		END

		IF	@isRamadan = 1 
			--AND @isMuslim = 1 
			AND ISNULL(@shiftSpan, 0) = 0
			AND @dtIN IS NOT NULL 
			AND @dtOUT IS NOT NULL
        BEGIN
			
			SET @netMinutes = DATEDIFF(n, @dtIN, @dtOUT)
			IF @netMinutes < 0
				SET @netMinutes = DATEDIFF(n, @dtIN, @dtOUT) + (24 * 60)
		END	
		ELSE
        BEGIN
        
			--Start of Rev. #2.2
			IF ISNULL(@isRamadan, 0) = 1
			BEGIN

				SET @netMinutes = DATEDIFF(n, @dtIN, @dtOUT) 
				IF @netMinutes < 0
					SET @netMinutes = (DATEDIFF(n, @dtIN, @dtOUT) + (24 * 60)) 
			END
            ELSE
            BEGIN

				SET @netMinutes = DATEDIFF(n, @dtIN, @dtOUT) + @sum_NetMinutes
				IF @netMinutes < 0
					SET @netMinutes = (DATEDIFF(n, @dtIN, @dtOUT) + (24 * 60)) + @sum_NetMinutes
            END 
			--End of Rev. #2.2
		END 
	END
	
	/************************************************************************************************************
		Shift Allowance Calculation

		Business Rules:
		- If an employee works more than 4 hours in the evening shift then he gets evening shift allowance
		- If an employee works more than 4 hours in the night shift then he gets night shift allowance
		- No shift allowance for day worker employees
	*************************************************************************************************************/
	SELECT	@duration_ShiftAllowance_Evening = tas.getOverLappingMinutes(dtIN, dtOUT , E1 , E2),
			@duration_ShiftAllowance_Night = tas.getOverLappingMinutes(dtIN, dtOUT , N1 , N2)
	FROM
	(
		SELECT	
			a.EmpNo,
			a.ShiftPatCode,
			a.SwipeDate,
			a.TimeInWP AS dtIN,
			a.TimeOutWP AS dtOut,
			CASE WHEN tas.hour(N1) > 12 
				THEN tas.fmtDate2(a.SwipeDate -1, tas.fmtTime(N1))
				ELSE tas.fmtDate2(a.SwipeDate, tas.fmtTime(N1))
			END AS N1,
			tas.fmtDate2(a.SwipeDate, tas.fmtTime(N2)) AS N2 ,
			tas.fmtDate2(a.SwipeDate, tas.fmtTime(E1)) AS E1 ,
			CASE WHEN tas.hour(E2) > 12 
				THEN tas.fmtDate2(a.SwipeDate, tas.fmtTime(E2))
				ELSE tas.fmtDate2(a.SwipeDate +1 , tas.fmtTime(E2))
			END AS E2 	
		FROM  tas.Tran_WorkplaceSwipe a WITH (NOLOCK)
			INNER JOIN tas.vuSA1 b WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode)
			INNER JOIN  tas.Tran_Timesheet c WITH (NOLOCK) ON a.EmpNo = c.EmpNo AND a.SwipeDate = c.DT
		WHERE
			a.SwipeDate = @processDate
			AND a.EmpNo = @empNo
			AND ISNULL(c.IsDayWorker_OR_Shifter, 0) = 0
			AND a.TimeInWP IS NOT NULL
			AND a.TimeOutWP IS NOT NULL
	) tblMain
	WHERE EmpNo = @empNo

	IF @duration_ShiftAllowance_Evening < 0
		SET @duration_ShiftAllowance_Evening = 0

	IF @duration_ShiftAllowance_Night < 0
		SET @duration_ShiftAllowance_Night = 0

	/********************************** End of Shift Allowance Calculation *************************************/

	/***************************************************************************************
		No-Pay-Hour Calculation
		
		Note: No NPH computation if:
        1. Driver/Liaison officer
        2. Calendar equal to holiday
        3. Scheduled shift code is day off
        4. Grade equal to salary staff
        5. If on Training  and training hours is specified ?
        6. Calendar equal to Day in Lieu, employee is a Day worker
		7. "LeaveType" is not equal to null
	****************************************************************************************/
	IF	@isSalStaff = 1 
		OR (@isDILdayWorker = 1 AND @isDayWorker_OR_Shifter = 1)
		OR (@isSalStaff = 0 AND @isDayWorker_OR_Shifter = 1)
		OR @isDriver = 1 
		OR @isLiasonOfficer = 1 
		OR @isPublicHoliday = 1		
		OR RTRIM(@shiftCode) = 'O'		--Rev. #1.3
		OR ISNULL(@leaveType, '') <> ''	--Rev. #2.4
	BEGIN

		--Set No Pay Hour to zero 
		SET @calculatedNPH = 0
	END

	ELSE
	BEGIN

		--Calculate No-Pay-Hour for shifter employees and non-salaried staff
		IF @duration_Required > 0 AND @duration_Worked_Cumulative > 0
		BEGIN
			
			SET @calculatedNPH = @duration_Required - @duration_Worked_Cumulative
			IF @calculatedNPH < 0
				SET @calculatedNPH = 0
		END

		ELSE IF @duration_Required > 0 AND @duration_Worked_Cumulative = 0	--Rev. #2.5
		BEGIN

			SET @calculatedNPH = @duration_Required
		END 

		ELSE
			SET @calculatedNPH = 0
	END
	

	/***************************************************************************************
		Overtime Calculation
		Note:
		- No overtime for Drivers, Liaison Officers, Salaried Staff
		- Full day overtime during public holidays, day-off, and for DIL workers
		- Overtime is auto calculated during public holidays
	***************************************************************************************/

	--Start of Rev. #1.5
	IF EXISTS
    (
		SELECT SettingID FROM tas.OTApprovalSetting a WITH (NOLOCK)
		WHERE RTRIM(a.CostCenter) = @empCostCenter
			AND @processDate BETWEEN a.EffectiveStartDate AND a.EffectiveEndDate
			AND a.IsActive = 1
	)
	BEGIN
    
		SET @autoApprovedOTByCostCenter = 1
	END 
	--End of Rev. #1.5

	IF	ISNULL(@shavedIN, '') = '' 
		OR ISNULL(@shavedOUT, '') = ''
		OR ISNULL(@shiftCode, '') = ''
		OR @isDriver = 1 
		OR @isLiasonOfficer = 1 
		OR (@isSalStaff = 1 AND @isDayWorker_OR_Shifter = 1)
		OR @absenceReasonCode IN (@FLAG_Code_ROA_TrainingLocal, @FLAG_Code_ROA_TrainingForeign)
		OR
		( 
			ISNULL(@isEmployee_OR_Contractor, 0) = 0 
			AND @contractorGroupCode <> @FLAG_Code_ContGroup_NonSal
		)
		OR
		(
			@isEmployee_OR_Contractor = 1 
			AND @payGrade = 0
			AND @actualCostCenter <> '7920'
		)
		OR
		(
			@isSalStaff = 1 
			AND @isDayWorker_OR_Shifter = 0
			AND @isRamadan = 1 
			AND @isEmployee_OR_Contractor = 1
			AND @isMuslim = 1
			AND 
			(
				@duration_Worked_Cumulative - @duration_Required < @FLAG_Minutes_MinOT_NSS
				OR @duration_Required < 0
			)
		)
		OR @leaveType <> ''
		OR @duration_Required + @duration_Worked_Cumulative < 0
	BEGIN

		SELECT	@enableOT = 0,
				@otAutoApprove = 0,
				@otStartTime = NULL,
				@otEndTime = NULL,
				@otType = NULL
	END

	ELSE
	BEGIN

		IF	@isPublicHoliday = 1					--Public holiday 
			OR @shiftCode = 'O'						--Day-off
			OR @isDILdayWorker = 1 					--Day-in-liue
		BEGIN

			--Full working day overtime
			SELECT	@enableOT = 1,
					@otStartTime = @shavedIN,
					@otEndTime = @shavedOUT

			--Determine overtime type
			IF @isPublicHoliday = 1 
			BEGIN
				SELECT	@otType = 'P',
						@otAutoApprove = 1	--Overtime is auto approved
			END

			ELSE IF @shiftCode = 'O'
				SET	@otType = 'O'

			ELSE IF @isDILdayWorker = 1
				SET	@otType = 'D'
		END
	
		ELSE
		BEGIN

			--Regular day overtime - extra working hours
			IF	@duration_Worked_Cumulative > @duration_Required
				OR (@shiftSpan = 1 AND CONVERT(TIME, @shavedOUT) = CONVERT(TIME, '23:00:00.000'))
			BEGIN

				DECLARE @otMinutes INT,
						@workDuration INT

				--Initialize variables
				SELECT	@otMinutes = 0,
						@workDuration = 0

				IF	@isRamadan = 1 
					--AND @isMuslim = 1
					AND @shavedIN IS NOT NULL 
					AND @shavedOUT IS NOT NULL
                BEGIN
                
					SELECT @workDuration = DATEDIFF(n, @shavedIN, @shavedOUT)
					IF @workDuration < 0
						SET @workDuration = DATEDIFF(n, @shavedIN, @shavedOUT) + (24 * 60)
				END 
				ELSE
					SET @workDuration = @duration_Worked_Cumulative

				--Calculate overtime duration
				--IF @shiftSpan = 1
				--	SET @otMinutes = @workDuration 
				--ELSE 
					SET @otMinutes = @workDuration - @duration_Required		--Rev. #2.2

				SELECT	@enableOT = 1,
						@otStartTime = DATEADD(n, @otMinutes * -1, @shavedOUT),	--Rev. #1.1

						--Start of Rev. #1.2
						--@otStartTime = CASE WHEN DATEDIFF(mi, @shavedIN, @shavedOUT) = @otMinutes
						--	THEN DATEADD(n, @otMinutes * -1, @shavedOUT)
						--	ELSE DATEADD(n, @otMinutes, @shavedIN)
						--	END, 
						--End of Rev. #1.2

						@otEndTime = @shavedOUT
			
				IF @otStartTime < @shavedIN
					SET @otStartTime = @shavedIN
				--ELSE IF CONVERT(TIME, @shavedOUT) = CONVERT(TIME, '23:00:00.000')		--Rev. #2.6
				--	SELECT	@otStartTime = @shavedIN,
				--			@otEndTime = @shavedOUT

				--Determine overtime type
				IF @isPublicHoliday = 1 
					SET @otType = 'P'
				ELSE IF @shiftCode = 'O'
					SET @otType = 'O'
				ELSE IF @isDILdayWorker = 1
					SET @otType = 'D'
				ELSE
					SET @otType = 'R'

				IF @autoApprovedOTByCostCenter = 1 OR @isRamadan = 1
					SET @otAutoApprove = 1	--Overtime is auto approved
			END
		END

		IF @enableOT = 1
		BEGIN
			
			--Calculate OT duration in minutes
			SET @otDuration = ISNULL(DATEDIFF(n, @otStartTime, @otEndTime), 0)

			/*****************************************************************************************************
				Perform additional overtime checking - 	Remove regular OT if less than parameter
			******************************************************************************************************/
			--30 minutes for Non-Salary Staff
			IF 
			(
				DATEDIFF(n, @dtIN, @dtOUT) < @FLAG_Minutes_MinOT_NSS
				OR @otDuration < @FLAG_Minutes_MinOT_NSS
			)
			AND @isSalStaff = 0
			AND @otType = 'R'
			BEGIN

				SELECT	@enableOT = 0,
						@otAutoApprove = 0,
						@otStartTime = NULL,
						@otEndTime = NULL,
						@otType = NULL
			END

			--360 minutes for Salary Staff during Non-Ramadan
			ELSE IF 
			(
				DATEDIFF(n, @dtIN, @dtOUT) < @FLAG_Minutes_MinOT_SS
				OR @otDuration < @FLAG_Minutes_MinOT_SS
			)
			AND @isSalStaff = 1
			AND @otType = 'R'
			AND @isRamadan = 0
			BEGIN

				SELECT	@enableOT = 0,
						@otAutoApprove = 0,
						@otStartTime = NULL,
						@otEndTime = NULL,
						@otType = NULL
			END

			--360 minutes for Salary Staff during Ramadan for Non-Muslims
			ELSE IF 
			(
				DATEDIFF(n, @dtIN, @dtOUT) < @FLAG_Minutes_MinOT_SS
				OR @otDuration < @FLAG_Minutes_MinOT_SS
			)
			AND @isSalStaff = 1
			AND @otType = 'R'
			AND @isRamadan = 1
			AND @isMuslim = 0
			BEGIN

				SELECT	@enableOT = 0,
						@otAutoApprove = 0,
						@otStartTime = NULL,
						@otEndTime = NULL,
						@otType = NULL
			END

			--120 minutes for Salary Staff during Ramadan for Muslim
			ELSE IF 
			(
				DATEDIFF(n, @dtIN, @dtOUT) < @FLAG_Minutes_MinOT_SS_Ramadan
				OR @otDuration < @FLAG_Minutes_MinOT_SS_Ramadan
			)
			AND @isSalStaff = 1
			AND @otType = 'R'
			AND @isRamadan = 1
			AND @isMuslim = 1
			BEGIN

				SELECT	@enableOT = 0,
						@otAutoApprove = 0,
						@otStartTime = NULL,
						@otEndTime = NULL,
						@otType = NULL
			END
		END
	END

	/************************************ End of Overtime Calucation ***************************************************************/

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@empNo,
			@payGrade,
			@shiftPatCode,
			@shiftCode,
			@actual_ShiftCode,
			@processDate AS DT,
			@dtIN, 
			@dtOUT,
			@shavedIN,
			@shavedOUT,
			@arrivalFrom,
			@arrivalTo,
			@departFrom,
			@departTo,
			@calculatedNPH AS NoPayHours,
			@duration_Required,
			@duration_Worked_Cumulative,
			@netMinutes,
			@otType,
			@otStartTime,
			@otEndTime,
			@otAutoApprove,
			@otDuration,
			@enableOT,
			@shiftSpan,
			@shiftAllowance,
			@duration_ShiftAllowance_Evening,
			@duration_ShiftAllowance_Night
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 

END

/*	Debugging:

Parameters:
	@empNo						int,
	@processDate				datetime,
	@workplaceSwipeIn			datetime,
	@workplaceSwipeOut			datetime,
	@correctionType				tinyint

	--Main Gate
	SELECT * FROM tas.fnGetProcessedWorkplaceDataToSync(10003483, '06/14/2016', '2016-06-13 22:48:10.000', '2016-06-14 07:04:19.000', 0)

	--Workplace
	SELECT * FROM tas.fnGetProcessedWorkplaceDataToSync(10003483, '06/14/2016', NULL, '2016-06-14 07:01:31.000', 0)

*/