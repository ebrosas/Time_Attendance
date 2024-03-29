/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfUseWorkplaceSwipe
*	Description: This function is used to determine if Timesheet should use the workplace swipes instead of the main gate swipes 
*
*	Date:			Author:		Rev.#:		Comments:
*	17/08/2015		Ervin		1.0			Created
*	31/08/2015		Ervin		1.1			Fetch the workplace cost centers from "WorkplaceReaderSetting" table
*	04/09/2015		Ervin		1.2			Check if DateX is between @DT_SwipeNewProcess and @DT_SwipeLastProcessed
*	27/01/2021		Ervin		1.3			Refactored code to enhance performance
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnCheckIfUseWorkplaceSwipe
(
	@DT_SwipeLastProcessed	datetime,	
	@DT_SwipeNewProcess		datetime,
	@empNo					int	
)
RETURNS BIT
AS

BEGIN

	DECLARE	@useWorkplaceSwipe		bit,
			@isCostCenterIncluded	bit,
			@isShifter				bit,
			@isNonSalaryStaff		bit,
			@isShiftSpan			bit,
			@dtMIN					datetime,
			@dtMAX					datetime,
			@arrivalFrom			datetime,
			@arrivalTo				datetime,
			@departFrom				datetime,
			@departTo				datetime,
			@durationRequired		int,
			@workDuration			int,
			@shiftPatCode			varchar(2),	
			@shiftCode				varchar(10)			

	SELECT	@useWorkplaceSwipe		= 0,
			@isCostCenterIncluded	= 0,
			@isShifter				= 0,
			@isNonSalaryStaff		= 0,
			@isShiftSpan			= 0,
			@dtMIN					= null,
			@dtMAX					= null,
			@arrivalFrom			= null,
			@arrivalTo				= null,
			@departFrom				= null,
			@departTo				= null,
			@durationRequired		= 0,
			@workDuration			= 0,
			@shiftPatCode			= '',	
			@shiftCode				= ''			

	/***************************************************************************************************
		Check if the employee belongs to the cost centers where workplace swipes is implemented
	***************************************************************************************************/		
	IF EXISTS
	(
		--Rev. #1.1 
		SELECT a.EmpNo
		FROM tas.Master_Employee_JDE_View a WITH (NOLOCK)
			INNER JOIN 
			(
				SELECT DISTINCT CostCenter FROM tas.WorkplaceReaderSetting WITH (NOLOCK) WHERE IsActive = 1
			) b ON RTRIM(a.BusinessUnit) = RTRIM(b.CostCenter)
		WHERE a.EmpNo = @empNo		
	)
	BEGIN

		SET @isCostCenterIncluded = 1
	END	

	
	/*************************************************************************
		Get the shift pattern information
	**************************************************************************/		
	SELECT TOP 1
		@shiftPatCode = RTRIM(Effective_ShiftPatCode),
		@shiftCode = RTRIM(Effective_ShiftCode)
	FROM tas.Tran_ShiftPatternUpdates a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND a.DateX BETWEEN CONVERT(DATETIME, CONVERT(VARCHAR, @DT_SwipeLastProcessed, 12)) AND CONVERT(DATETIME, CONVERT(VARCHAR, @DT_SwipeNewProcess, 12)) --Rev. #1.2

	
	/*************************************************************************
		Check if employee is Shifter
	**************************************************************************/	
	SELECT @isShifter = CASE WHEN a.IsDayShift = 1 THEN 0 ELSE 1 END
	FROM tas.Master_ShiftPatternTitles a WITH (NOLOCK)
	WHERE RTRIM(a.ShiftPatCode) = RTRIM(@shiftPatCode)


	/*************************************************************************
		Check if employee is Non-salary Staff
	**************************************************************************/	
	SELECT @isNonSalaryStaff = CASE WHEN ISNULL(GradeCode, 0) <= 8 THEN 1 ELSE 0 END
	FROM tas.Master_Employee_JDE_View WITH (NOLOCK)
	WHERE EmpNo = @empNo


	/*************************************************************************
		Check if Shift Span is enabled
	**************************************************************************/
	IF ISNULL(@shiftPatCode, '') <> '' AND ISNULL(@shiftCode, '') <> ''
	BEGIN

		--Get the shift timing info
		SELECT	@arrivalFrom = a.ArrivalFrom,
				@arrivalTo = a.ArrivalTo,
				@departFrom = a.DepartFrom,
				@departTo = a.DepartTo,
				@durationRequired = DATEDIFF(n, a.ArrivalTo, a.DepartFrom)
		FROM tas.Master_ShiftTimes a WITH (NOLOCK)
		WHERE RTRIM(a.ShiftPatCode) = @shiftPatCode
			AND RTRIM(a.ShiftCode) = @shiftCode

		--Get the maximum and minimum swipe time
		--SELECT	@dtMIN = MIN(DT),
		--		@dtMAX = MAX(DT)
		--FROM tas.fnGetEmployeeSwipeRawData(@DT_SwipeLastProcessed, @DT_SwipeNewProcess, @empNo) 

		SELECT	@dtMIN = MIN(a.TimeDate),
				@dtMAX = MAX(a.TimeDate) 
		FROM tas.External_DSX_evnlog a WITH (NOLOCK)
		WHERE ISNULL(a.FName,'') <> ''
			AND (CASE WHEN ISNUMERIC(a.FName) = 1 
				THEN 
					CASE WHEN CONVERT(INT, a.FName) <= 9999 
					THEN CONVERT(INT, a.FName) + 10000000
					ELSE CONVERT(INT, a.FName) END
				ELSE 0 END) = @empNo
			AND a.TimeDate BETWEEN @DT_SwipeLastProcessed AND @DT_SwipeNewProcess

		--Set the work duration
		SET @workDuration = DATEDIFF(n, @dtMIN, @dtMAX)
		
		IF 
		(	@durationRequired > 0 
			AND @workDuration >= @durationRequired * 2			
		)
		BEGIN

			SET @isShiftSpan = 1
		END
	END

	
	/*************************************************************************
		Determine the final result
	**************************************************************************/
	IF	@isCostCenterIncluded = 1
		AND @isShifter = 1
		AND @isNonSalaryStaff = 1
		AND @isShiftSpan = 0
		--AND @shiftCode <> 'O'
	BEGIN

		--Enable the flag to use the workplace swipe
		SET @useWorkplaceSwipe = 1
	END
	
	RETURN @useWorkplaceSwipe
END


/*	Debugging:

Parameters:
	@DT_SwipeLastProcessed	datetime,	
	@DT_SwipeNewProcess		datetime,
	@empNo					int

	SELECT tas.fnCheckIfUseWorkplaceSwipe('2021-01-26 09:00:00.000', '2021-01-27 09:00:00.000', 10003631)

*/
