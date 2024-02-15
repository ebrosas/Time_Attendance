/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeLongStay
*	Description: This stored procedure is used to remove day-off and mark as absent
*
*	Date			Author		Revision No.	Comments:
*	07/04/2019		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeLongStay
(	
	@processDate			DATETIME,
	@longStayThreshold		INT,
	@empNo					INT = 0,
	@costCenter				VARCHAR(12) = ''
)
AS	

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 
		
	DECLARE	@currentDate DATETIME 
	SELECT @currentDate = GETDATE()

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	--Fetch data from the swipe access system database
	SELECT	DISTINCT 
			x.CostCenter,
			x.CostCenterName,
			x.EmpNo, 
			x.EmpName,
			x.Position,
			x.SupervisorNo,
			x.SupervisorName,
			x.EmpMobileNo,
			x.ShiftPatCode,
			x.Sched_Shift,
			x.DT,
			x.First_TimeIn,
			DATEDIFF(MINUTE, x.First_TimeIn, GETDATE()) AS CurrentWorkDuration,
			x.Last_SwipeType
	FROM
    (
		SELECT	b.BusinessUnit AS CostCenter,
				RTRIM(c.BUname) AS CostCenterName,
				a.EmpNo,
				CASE 
					WHEN ISNULL(b.EmpName, '') <> '' THEN RTRIM(b.EmpName)
					WHEN ISNULL(b.EmpName, '') = '' AND ISNULL(e.EmpNo, 0) > 0 THEN RTRIM(e.EmpName)
					WHEN ISNULL(b.EmpName, '') = '' AND ISNULL(f.VisitorCardNo, 0) > 0 THEN RTRIM(f.VisitorName)
				END AS EmpName,
				b.Position,
				b.SupervisorNo,
				RTRIM(d.EmpName) AS SupervisorName,
				tas.fnGetMobileNo(a.EmpNo) AS EmpMobileNo,
				a.ShiftPatCode,
				a.ShiftCode AS Sched_Shift,
				a.SwipeDate AS DT,
				tas.fnGetAllEmpFirstSwipeIn(a.EmpNo, a.SwipeDate) AS First_TimeIn,
				tas.fnGetLastSwipeType(a.EmpNo, a.SwipeDate) AS Last_SwipeType
		FROM tas.MainGateTodaySwipeLog a
			LEFT JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON RTRIM(b.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.Master_Employee_JDE_View_V2 d WITH (NOLOCK) ON b.SupervisorNo = d.EmpNo
			LEFT JOIN tas.Vw_EmpContractorIDBadgeInfo e WITH (NOLOCK) ON a.EmpNo = e.EmpNo
			LEFT JOIN tas.VisitorPassLog f WITH (NOLOCK) ON a.EmpNo = f.VisitorCardNo
	) x
	WHERE 
		x.DT = @processDate
		AND x.First_TimeIn IS NOT NULL 
		AND RTRIM(x.Last_SwipeType) <> 'OUT' 
		AND DATEDIFF(MINUTE, x.First_TimeIn, @currentDate) >= @longStayThreshold
		AND (x.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(x.CostCenter) = @costCenter OR @costCenter IS NULL)
		AND NOT EXISTS
        (
			SELECT 1 FROM tas.LongStayNotificationLog WITH (NOLOCK)
			WHERE EmpNo = x.EmpNo
				AND DT = x.DT
		)
	ORDER BY x.CostCenter, x.EmpNo, x.First_TimeIn

GO 


/*	Debug:

PARAMETERS:
	@processDate			DATETIME,
	@longStayThreshold		INT,
	@empNo					INT = 0,
	@costCenter				VARCHAR(12) = ''

	--Test server
	EXEC tas.Pr_GetEmployeeLongStay '03/31/2016', 600
	EXEC tas.Pr_GetEmployeeLongStay '03/31/2016', 600, 10001335
	EXEC tas.Pr_GetEmployeeLongStay '03/31/2016', 600, 0, '3240'

	--Production server
	EXEC tas.Pr_GetEmployeeLongStay '04/18/2019', 500


	SELECT * FROM tas.LongStayNotificationLog WITH (NOLOCK)

	BEGIN TRAN T1
	TRUNCATE TABLE tas.LongStayNotificationLog
	COMMIT TRAN T1

*/