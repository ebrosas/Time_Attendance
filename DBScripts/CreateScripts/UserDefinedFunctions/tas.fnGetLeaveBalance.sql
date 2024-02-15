/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsTASAdministrator
*	Description: This function is used to fetch the employee's current leave balance
*
*	Date			Author		Rev. #		Comments:
*	05/07/2020		Ervin		1.0			Created
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetLeaveBalance 
(
	@empNo			INT,
	@leaveType		VARCHAR(5),
	@inquireDate	SMALLDATETIME
)
RETURNS FLOAT  
AS
BEGIN

	DECLARE	@leaveBalance FLOAT = 0

	-- Declare necessary variables
	DECLARE @company varchar(5)
	DECLARE @leaveEntitlementPerMonth float
	DECLARE @leaveEntitlementPerDay float
	DECLARE @leaveEntitlementUOM char(1)
	DECLARE @newLeaveBalance float
	DECLARE @newLeaveBalanceUOM char(1)
	DECLARE @month float
	DECLARE @year float

	DECLARE @workHoursPerDay float
	DECLARE @workHoursPerYear float
	DECLARE @lastDateUpdated smalldatetime
	DECLARE @numberOfDays int

	DECLARE @approvalFlag char(1)
	DECLARE @remark varchar(11)
	DECLARE @duration float
	DECLARE @leaveDecimal decimal(12,3)		--Rev. #1.4

	-- Define constants -----------------------------
	DECLARE @LEAVE_TYPE_ANNUAL varchar(5)
	DECLARE @LEAVE_TYPE_INJURY varchar(5)
	DECLARE @LEAVE_TYPE_UNPAID_LEAVE varchar(5)
	DECLARE @LEAVE_TYPE_SICK_LEAVE varchar(5)
	DECLARE @LEAVE_TYPE_HAJ_LEAVE varchar(5)

	DECLARE @SICK_LEAVE_MAX int
	DECLARE @INJURY_LEAVE_MAX int
	DECLARE @UNPAID_LEAVE_MAX int	

	SELECT @LEAVE_TYPE_ANNUAL		= 'AL'
	SELECT @LEAVE_TYPE_INJURY		= 'IL'
	SELECT @LEAVE_TYPE_UNPAID_LEAVE	= 'UL'
	SELECT @LEAVE_TYPE_SICK_LEAVE	= 'SLP'
	SELECT @LEAVE_TYPE_HAJ_LEAVE	= 'HL'

	SELECT @SICK_LEAVE_MAX		= 182
	SELECT @INJURY_LEAVE_MAX	= 365
	SELECT @UNPAID_LEAVE_MAX	= 90
	-- End of Define constants ----------------------

	/*****************************************************************************
		Revision 1.3 -  Initialize the flag that is used to determine whether 
						leave balance is calculated on monthly basis
	******************************************************************************/
	DECLARE @enableLeaveByMonth bit		
	SET @enableLeaveByMonth = 0
	/*************************** END *************************************************/
	
	IF YEAR(@inquireDate) < YEAR(GETDATE())		--Rev. #1.8
	BEGIN

		SELECT TOP 1 @company = a.LBCO,
			@leaveEntitlementPerMonth = (a.LBY58VCVDR / 10000), 
			@leaveEntitlementUOM = a.LBY58VCUOM,
			@newLeaveBalance = (a.LBY58VCNVT / 10000), 
			@newLeaveBalanceUOM = a.LBY58VCNVM,
			@leaveBalance = (a.LBY58VCVCT / 10000), 
			@month = a.LBY58VCMTH, 
			@year = a.LBY59PYEAR
		FROM tas.syJDE_F58LV11 a WITH (NOLOCK)
		WHERE a.LBAN8 = @empNo
			AND a.LBY58VCVCD = @leaveType
			AND a.LBY59PYEAR = YEAR(@inquireDate)
		ORDER BY a.LBY59PYEAR DESC, a.LBY58VCMTH DESC
	END
	
	ELSE
	BEGIN

		-- Retrieve the last record from the leave balances
		SELECT TOP 1 @company = a.LBCO,
			@leaveEntitlementPerMonth = (a.LBY58VCVDR / 10000), 
			@leaveEntitlementUOM = a.LBY58VCUOM,
			@newLeaveBalance = (a.LBY58VCNVT / 10000), 
			@newLeaveBalanceUOM = a.LBY58VCNVM,
			@leaveBalance = (a.LBY58VCVCT / 10000), 
			@month = a.LBY58VCMTH, 
			@year = a.LBY59PYEAR
		FROM tas.syJDE_F58LV11 a WITH (NOLOCK)
		WHERE a.LBAN8 = @empNo 
			AND a.LBY58VCVCD = @leaveType 
			AND a.LBY58VCROV = ''
		ORDER BY a.LBY59PYEAR DESC, a.LBY58VCMTH DESC
	END 


	/*********************************************************************************
		Revision 1.3 - Set @inquireDate to current date if company code is '00850'
	**********************************************************************************/
	IF RTRIM(@company) = '00850' 
	BEGIN

		--SET @inquireDate = CONVERT(VARCHAR(10), GETDATE(), 101)
		SET @enableLeaveByMonth = 1
	END
	/*************************** END *************************************************/

	--PRINT 'Company: ' + @company
	--PRINT 'Leave Entitlement per Month: ' + CONVERT(varchar(20), @leaveEntitlementPerMonth)
	--PRINT 'Entitlement Unit of Measure: ' + @leaveEntitlementUOM
	--PRINT 'New Balance: ' + CONVERT(varchar(20), @newLeaveBalance)
	--PRINT 'New Balance Unit of Measure: ' + @newLeaveBalanceUOM
	--PRINT 'Leave Balance: ' + CONVERT(varchar(20), @leaveBalance)
	--PRINT 'Month: ' + CONVERT(varchar(5), @month)
	--PRINT 'Year: ' + CONVERT(varchar(5), @year)

	-- Retrieve the working hours per day and per year
	IF EXISTS(SELECT a.YKID, a.YKIH
				FROM tas.sy_F069096 AS a WITH (NOLOCK)
				WHERE a.YKCO = @company)
		SELECT @workHoursPerDay = (a.YKID / 100), @workHoursPerYear = (a.YKIH / 100)
				FROM tas.sy_F069096 AS a WITH (NOLOCK)
				WHERE a.YKCO = @company

	ELSE
		SELECT @workHoursPerDay = (a.YKID / 100), @workHoursPerYear = (a.YKIH / 100)
				FROM tas.sy_F069096 AS a WITH (NOLOCK)
				WHERE a.YKCO = '00000'

	--PRINT 'Work Hours Per Day: ' + CONVERT(varchar(20), @workHoursPerDay)
	--PRINT 'Work Hours Per Year: ' + CONVERT(varchar(20), @workHoursPerYear)

	-- Compute the leave entitlement per day
	IF @leaveType = @LEAVE_TYPE_ANNUAL
		SELECT @leaveEntitlementPerDay = @leaveEntitlementPerMonth / 240 -- 30 days

	ELSE IF @leaveType IN (@LEAVE_TYPE_INJURY, @LEAVE_TYPE_SICK_LEAVE)
		SELECT @leaveEntitlementPerDay = @leaveEntitlementPerMonth / @workHoursPerYear

	--PRINT 'Leave Entitlement per Day: ' + CONVERT(varchar(20), @leaveEntitlementPerDay)

	-- Compute the leave balance based on the unit of measure
	IF @newLeaveBalanceUOM = 'H'
		SELECT @leaveBalance = @newLeaveBalance / @workHoursPerDay

	ELSE IF @newLeaveBalanceUOM = 'D'
		SELECT @leaveBalance = @newLeaveBalance

	--PRINT 'New Leave Balance / UOM: ' + CONVERT(varchar(20), @leaveBalance)

	-- Set the last date of the month
	SELECT @lastDateUpdated = CONVERT(smalldatetime,
		CONVERT(varchar(5), @month) + '/1/' + CONVERT(varchar(5), @year))
	SELECT @lastDateUpdated = DATEADD(dd, -1, DATEADD(mm, 1, @lastDateUpdated))
	--PRINT 'Last Date Updated: ' + CONVERT(varchar(20), @lastDateUpdated, 101)

	--  Checks if leave type is not Haj
	IF @leaveType <> @LEAVE_TYPE_HAJ_LEAVE
	BEGIN

		-- Compute for the days difference between the last updated date and inquiry date
		SELECT @numberOfDays = DATEDIFF(dd, CONVERT(VARCHAR, @lastDateUpdated, 12), @inquireDate)
		--PRINT 'Number of days difference: ' + CONVERT(varchar(10), @numberOfDays)

		-- Compute the new balance
		SELECT @leaveBalance = @leaveBalance + (@numberOfDays * @leaveEntitlementPerDay)

		--Round @leaveBalance to the nearest 0.5. Remove the decimal digits in the product of @numberOfDays * @leaveEntitlementPerDay. (Revision #1.2)	
		--SELECT @leaveBalance = (ROUND(@leaveBalance * 2, 0) / 2) + (FLOOR(@numberOfDays * @leaveEntitlementPerDay))	--(Revision #1.2)	
		--SELECT @leaveBalance = ROUND((@leaveBalance + (@numberOfDays * @leaveEntitlementPerDay)) * 2, 0) / 2	--(Revision #1.2)	

		-- Checks for maximum limit
		IF @leaveType = @LEAVE_TYPE_SICK_LEAVE AND @leaveBalance > @SICK_LEAVE_MAX
			SELECT @leaveBalance = @SICK_LEAVE_MAX

		ELSE IF @leaveType = @LEAVE_TYPE_INJURY AND @leaveBalance > @INJURY_LEAVE_MAX
			SELECT @leaveBalance = @INJURY_LEAVE_MAX

		ELSE IF @leaveType = @LEAVE_TYPE_UNPAID_LEAVE AND @leaveBalance > @UNPAID_LEAVE_MAX
			SELECT @leaveBalance = @UNPAID_LEAVE_MAX
 
		--PRINT 'Leave Balance (with days difference) / UOM: ' + CONVERT(varchar(20), @leaveBalance)
	END


	/************************************************************************************************
		Revision 1.3 - Build the cursor table wherein the Leave Start Date is checked against 
			the current date for Aspire employees and @inquireDate for other employees
	************************************************************************************************/
	IF @enableLeaveByMonth = 1
	BEGIN

		--For Aspire employees
		DECLARE FutureLeavesCursor CURSOR READ_ONLY FOR
		SELECT a.XXY58VCAFG, a.XXRMK, a.XXY58VCVDR / 10000
			FROM tas.sy_F55LVINQ AS a WITH (NOLOCK)
				INNER JOIN tas.sy_LeaveRequisition AS b WITH (NOLOCK) ON a.XXY58VCRQN = b.RequisitionNo
			WHERE a.XXAN8 = @empNo AND a.XXY58VCVCD = @leaveType 
				AND tas.ConvertFromJulian(a.XXY58VCOFD) > GETDATE()
				AND (CASE WHEN a.XXY58VCAFG IN ('A', 'N') AND b.RequestStatusSpecialHandlingCode = 'Open' THEN 'Closed' ELSE b.RequestStatusSpecialHandlingCode END) = 'Open'	--Rev. #1.7
		OPEN FutureLeavesCursor
		FETCH NEXT FROM FutureLeavesCursor
		INTO @approvalFlag, @remark, @duration
	END

	ELSE
	BEGIN

		--For other employees
		DECLARE FutureLeavesCursor CURSOR READ_ONLY FOR
		SELECT a.XXY58VCAFG, a.XXRMK, a.XXY58VCVDR / 10000
			FROM tas.sy_F55LVINQ AS a WITH (NOLOCK)
				INNER JOIN tas.sy_LeaveRequisition AS b WITH (NOLOCK) ON a.XXY58VCRQN = b.RequisitionNo
			WHERE a.XXAN8 = @empNo AND a.XXY58VCVCD = @leaveType 
				AND tas.ConvertFromJulian(a.XXY58VCOFD) < @inquireDate
				AND (CASE WHEN a.XXY58VCAFG IN ('A', 'N') AND b.RequestStatusSpecialHandlingCode = 'Open' THEN 'Closed' ELSE b.RequestStatusSpecialHandlingCode END) = 'Open'	--Rev. #1.7
		OPEN FutureLeavesCursor
		FETCH NEXT FROM FutureLeavesCursor
		INTO @approvalFlag, @remark, @duration
	END
	/************************************ END *************************************************/

	WHILE @@FETCH_STATUS = 0
	BEGIN

		-- Checks if satisfy the condition
		IF (@approvalFlag NOT IN ('C', 'R', 'D') AND @remark <> 'Encash') OR
			(@approvalFlag = 'A' AND @remark = 'Encash')
		BEGIN

			-- Checks if adjustment
			IF @remark = 'Adjustment' OR @approvalFlag = 'W'
				SELECT @leaveBalance = @leaveBalance - @duration

			ELSE
				SELECT @leaveBalance = @leaveBalance + @duration

		END

		-- Retrieve next record
		FETCH NEXT FROM FutureLeavesCursor
		INTO @approvalFlag, @remark, @duration

	END

	-- Close and deallocate
	CLOSE FutureLeavesCursor
	DEALLOCATE FutureLeavesCursor

	/************************************************************************************************
		Revision 1.4 - Calculate leave balance based on the remainder value
	************************************************************************************************/
	SELECT @leaveDecimal = (CONVERT(DECIMAL(12,3), @leaveBalance) % 1)
	--PRINT 'Leave Balance Remainder:' + CONVERT(varchar, @leaveDecimal)

	IF @leaveBalance < 0	--Rev. #1.9
	BEGIN
    
		IF ABS(@leaveDecimal) < 0.500
			SET @leaveBalance = CAST(@leaveBalance AS INT)
		ELSE
			SET @leaveBalance = FLOOR(@leaveBalance) + 0.500
	END
    
	ELSE
    BEGIN
    
		IF @leaveDecimal < 0.500
			SET @leaveBalance = FLOOR(@leaveBalance)
		ELSE
			SET @leaveBalance = FLOOR(@leaveBalance) + 0.500
	END 
	
	RETURN @leaveBalance 

END 

/*	Debug:

	SELECT tas.fnGetLeaveBalance (10003632, 'AL', '12/31/2020')

PARAMETERS:
	@empNo			INT,
	@leaveType		VARCHAR(5),
	@inquireDate	SMALLDATETIME

*/