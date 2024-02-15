/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAnnualLeaveButSwiped
*	Description: Get the list of employees who came to work while on leave
*
*	Date			Author		Revision No.	Comments:
*	27/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAnnualLeaveButSwiped
(   	
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= ''
)
AS

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDate, '') = CONVERT(DATETIME, '')
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = CONVERT(DATETIME, '')
		SET @endDate = NULL

	SELECT	a.AutoID,
			a.EmpNo,
			b.EmpName,
			--LTRIM(RTRIM(b.YAALPH)) AS EmpName,			
			a.BusinessUnit AS CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			a.DT,
			a.dtIN,
			a.dtOUT,			
			CASE WHEN DATEDIFF(n, a.dtIN, a.dtOUT) < 0
				THEN DATEDIFF(n, a.dtIN, a.dtOUT) + (24 * 60)
				ELSE DATEDIFF(n, a.dtIN, a.dtOUT)
			END AS Duration,
			CASE WHEN 
			(
				SELECT COUNT(*) 
				FROM tas.Tran_Timesheet 
				WHERE EmpNo = a.EmpNo 
					AND DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL	
			) > 1 THEN 1 ELSE 0 END AS HasMultipleSwipe,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.LeaveType,
			LTRIM(RTRIM(d.DRDL01)) AS LeaveTypeDesc
			--a.Duration_Worked,
			--a.Duration_Worked_Cumulative,
			--a.NetMinutes
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		--INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.BusinessUnit) = RTRIM(c.BusinessUnit)
		LEFT JOIN tas.syJDE_F0005 d ON RTRIM(a.LeaveType) = LTRIM(RTRIM(d.DRKY)) AND LTRIM(RTRIM(d.DRSY)) = '58' AND LTRIM(RTRIM(d.DRRT)) = 'VC'
	WHERE 
		a.EmpNo > 10000000
		AND ISNULL(a.LeaveType, '') IN ('AL')
		AND (a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL)	
		AND ISNUMERIC(b.PayStatus) = 1
		--AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END) = 1
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND 
		(
			a.DT BETWEEN @startDate AND @endDate
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
	ORDER BY a.DT DESC, a.BusinessUnit, a.EmpNo

GO 

/*	Debugging:

PARAMETERS:
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= ''

	EXEC tas.Pr_GetAnnualLeaveButSwiped
	EXEC tas.Pr_GetAnnualLeaveButSwiped '01/01/2016', '30/04/2016'
	EXEC tas.Pr_GetAnnualLeaveButSwiped '', '', 0, '7600'

*/


