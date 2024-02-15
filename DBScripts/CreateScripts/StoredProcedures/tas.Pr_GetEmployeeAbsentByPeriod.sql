/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmployeeAbsentByPeriod
*	Description: Retrieves the employee absence information based on the specified date 
*
*	Date:			Author:		Rev#		Comments:
*	08/04/2019		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetEmployeeAbsentByPeriod
(
	@loadType			TINYINT,	--(Note: 0 = Get all employee absences; 1 = Get absences for specific employee)
	@processDate		DATETIME,
	@costCenterArray	VARCHAR(200) = '',
	@empNo				INT = 0
)
AS
	
	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 
		
	--Validate parameters
	IF ISNULL(@costCenterArray, '') = ''
		SET @costCenterArray = NULL

	IF @loadType = 0		--Get all employee absences			
	BEGIN
    
		SELECT	a.EmpNo,
				b.EmpName,
				b.Position,
				b.GradeCode,
				b.EmpEmail,
				a.BusinessUnit AS CostCenter,
				RTRIM(e.BusinessUnitName) AS CostCenterName,
				a.ShiftPatCode,
				a.ShiftCode,
				a.DT,
				a.RemarkCode,			
				b.SupervisorNo,
				RTRIM(f.EmpName) AS SupervisorName,
				RTRIM(f.EmpEmail) AS SupervisorEmail,
				e.CostCenterManager AS ManagerNo,
				RTRIM(g.EmpName) AS ManagerName,
				RTRIM(g.EmpEmail) AS ManagerEmail
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			OUTER APPLY
			(
				SELECT x.AutoID 
				FROM tas.Tran_Absence_DIL x WITH (NOLOCK)
					INNER JOIN tas.Tran_DIL_Consumption y WITH (NOLOCK) ON x.AutoID = y.RequisitionNo
				WHERE x.EmpNo = a.EmpNo
					AND y.AppliedDate = a.DT
					AND x.DILConsumptionType = 2	--AbsencesOnly
					AND x.DILRequestType = 1		--DIL Utilization Request
			) c
			OUTER APPLY tas.fnGetLeaveToOffsetAbsent(a.EmpNo, a.DT) d
			INNER JOIN tas.Master_BusinessUnit_JDE_V2 e WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(e.BusinessUnit)
			LEFT JOIN tas.Master_Employee_JDE_View_V2 f WITH (NOLOCK) ON b.SupervisorNo = f.EmpNo
			LEFT JOIN tas.Master_Employee_JDE_View_V2 g WITH (NOLOCK) ON e.Superintendent = g.EmpNo
		WHERE 
			ISNUMERIC(b.PayStatus) = 1
			AND b.DateResigned IS NULL
			AND RTRIM(b.Company) = '00100'
			AND RTRIM(a.RemarkCode) = 'A'
			AND a.IsLastRow = 1		
			AND c.AutoID IS NULL
			AND ISNULL(d.RequisitionNo, 0) = 0
			AND 
			(
				RTRIM(a.BusinessUnit) IN (SELECT RTRIM(GenericStringField) FROM tas.fnParseStringArrayToString(@costCenterArray, ','))
				OR @costCenterArray IS NULL
			)
			AND NOT a.EmpNo BETWEEN 10005000 AND  10005999
			AND NOT (a.EmpNo BETWEEN 10002000 AND  10002999 AND RTRIM(b.ActualCostCenter) = '7600')
			AND a.DT = @processDate
		ORDER BY a.BusinessUnit, a.EmpNo
	END 

	ELSE IF @loadType = 1		--Get absences for specific employee
	BEGIN
    
		SELECT	a.EmpNo,
				b.EmpName,
				b.Position,
				b.GradeCode,
				b.EmpEmail,
				a.BusinessUnit AS CostCenter,
				RTRIM(e.BusinessUnitName) AS CostCenterName,
				a.ShiftPatCode,
				a.ShiftCode,
				a.DT,
				a.RemarkCode			
		FROM tas.Tran_Timesheet a WITH (NOLOCK)
			INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
			OUTER APPLY
			(
				SELECT x.AutoID 
				FROM tas.Tran_Absence_DIL x WITH (NOLOCK)
					INNER JOIN tas.Tran_DIL_Consumption y WITH (NOLOCK) ON x.AutoID = y.RequisitionNo
				WHERE x.EmpNo = a.EmpNo
					AND y.AppliedDate = a.DT
					AND x.DILConsumptionType = 2	--AbsencesOnly
					AND x.DILRequestType = 1		--DIL Utilization Request
			) c
			OUTER APPLY tas.fnGetLeaveToOffsetAbsent(a.EmpNo, a.DT) d
			INNER JOIN tas.Master_BusinessUnit_JDE_V2 e WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(e.BusinessUnit)
		WHERE 
			ISNUMERIC(b.PayStatus) = 1
			AND b.DateResigned IS NULL
			AND RTRIM(b.Company) = '00100'
			AND RTRIM(a.RemarkCode) = 'A'
			AND a.IsLastRow = 1		
			AND c.AutoID IS NULL
			AND ISNULL(d.RequisitionNo, 0) = 0
			AND a.EmpNo = @empNo
		ORDER BY a.DT DESC, a.BusinessUnit, a.EmpNo
	END
    
GO 

/*	Debug:

PARAMETERS:
	@loadType			TINYINT,	--(Note: 0 = Get all employee absences; 1 = Get absences for specific employee)
	@processDate		DATETIME,
	@costCenterArray	VARCHAR(200) = '',
	@empNo				INT = 0

	EXEC tas.Pr_GetEmployeeAbsentByPeriod 0, '04/08/2019', '7600'
	EXEC tas.Pr_GetEmployeeAbsentByPeriod 1, NULL, '', 10003633

	EXEC tas.Pr_GetEmployeeAbsentByPeriod 0, '03/20/2019', '7600'
	EXEC tas.Pr_GetEmployeeAbsentByPeriod 1, NULL, '', 10003154

*/
