USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetFireTeamAttendance_V2]    Script Date: 11/02/2018 15:58:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetFireTeamAttendance_V2
*	Description: This stored procedure is used to fetch attendance records of Fire Team and Fire Wacth members
*
*	Date:			Author:		Rev. #:		Comments:
*	04/02/2018		Ervin		1.0			Created
*****************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetFireTeamAttendance_V2]
(	
	@loadType			TINYINT,	--(Note: 1 => Get all fire team members; 2 => Get all fire watch Members; 3 => Get all fire team / fire watch members)
	@processDate		DATETIME,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = NULL
)
AS

	--Validate the parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @loadType = 1	--Get All Fire Team members
	BEGIN
	
		SELECT	DISTINCT
				a.SwipeDate,
				a.SwipeTime,
				a.SwipeLocation,
				a.SwipeType,
				a.Notes, 
				a.EmpNo, 
				b.EmpName,
				b.Position,
				b.GradeCode,
				RTRIM(b.BusinessUnit) AS CostCenter,
				RTRIM(e.BUname) AS CostCenterName,
				ISNULL(CONVERT(VARCHAR(20), c.WPPH1),'') AS Extension,
				ISNULL(LTRIM(RTRIM(d.WPPH1)), '') AS MobileNo,
				f.Effective_ShiftPatCode AS ShiftPatCode,
				f.Effective_ShiftPointer AS ShiftPointer,
				f.Effective_ShiftCode AS ShiftCode,
				b.SupervisorNo AS SupervisorEmpNo,
				RTRIM(g.EmpName) AS SupervisorEmpName
		FROM tas.Vw_AllFireTeamMembers a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.syJDE_F0115 c on a.EmpNo = c.WPAN8 AND UPPER(LTRIM(RTRIM(c.WPPHTP))) = 'EXT'
			LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 AND UPPER(LTRIM(RTRIM(d.WPPHTP))) = 'MOBS'
			LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(b.BusinessUnit) = RTRIM(e.BU)
			LEFT JOIN tas.Tran_ShiftPatternUpdates f ON a.EmpNo = f.EmpNo AND a.SwipeDate = f.DateX
			LEFT JOIN tas.Master_Employee_JDE_View g ON b.SupervisorNo = g.EmpNo
		WHERE 
			a.SwipeDate = @processDate
			AND ISNUMERIC(b.PayStatus) = 1
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)			
		ORDER BY a.EmpNo, a.SwipeTime DESC
	END

	IF @loadType = 2	--Get All Fire Watch members
	BEGIN
	
		SELECT	DISTINCT
				a.SwipeDate,
				a.SwipeTime,
				a.SwipeLocation,
				a.SwipeType,
				a.Notes, 
				a.EmpNo, 
				b.EmpName,
				b.Position,
				b.GradeCode,
				RTRIM(b.BusinessUnit) AS CostCenter,
				RTRIM(e.BUname) AS CostCenterName,
				ISNULL(CONVERT(VARCHAR(20), c.WPPH1),'') AS Extension,
				ISNULL(LTRIM(RTRIM(d.WPPH1)), '') AS MobileNo,
				f.Effective_ShiftPatCode AS ShiftPatCode,
				f.Effective_ShiftPointer AS ShiftPointer,
				f.Effective_ShiftCode AS ShiftCode,
				b.SupervisorNo AS SupervisorEmpNo,
				RTRIM(g.EmpName) AS SupervisorEmpName
		FROM tas.Vw_AllFireWatchMembers a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.syJDE_F0115 c on a.EmpNo = c.WPAN8 AND UPPER(LTRIM(RTRIM(c.WPPHTP))) = 'EXT'
			LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 AND UPPER(LTRIM(RTRIM(d.WPPHTP))) = 'MOBS'
			LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(b.BusinessUnit) = RTRIM(e.BU)
			LEFT JOIN tas.Tran_ShiftPatternUpdates f ON a.EmpNo = f.EmpNo AND a.SwipeDate = f.DateX
			LEFT JOIN tas.Master_Employee_JDE_View g ON b.SupervisorNo = g.EmpNo
		WHERE 
			a.SwipeDate = @processDate
			AND ISNUMERIC(b.PayStatus) = 1
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)			
		ORDER BY a.EmpNo, a.SwipeTime DESC			
	END

	ELSE IF @loadType = 3	--Get All Fire Watch and Fire Watch members
	BEGIN
	
		SELECT	DISTINCT
				a.SwipeDate,
				a.SwipeTime,
				a.SwipeLocation,
				a.SwipeType,
				a.Notes, 
				a.EmpNo, 
				b.EmpName,
				b.Position,
				b.GradeCode,
				RTRIM(b.BusinessUnit) AS CostCenter,
				RTRIM(e.BUname) AS CostCenterName,
				ISNULL(CONVERT(VARCHAR(20), c.WPPH1),'') AS Extension,
				ISNULL(LTRIM(RTRIM(d.WPPH1)), '') AS MobileNo,
				f.Effective_ShiftPatCode AS ShiftPatCode,
				f.Effective_ShiftPointer AS ShiftPointer,
				f.Effective_ShiftCode AS ShiftCode,
				b.SupervisorNo AS SupervisorEmpNo,
				RTRIM(g.EmpName) AS SupervisorEmpName,
				a.GroupType
		FROM tas.Vw_FireTeamAndFireWatchMember a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.syJDE_F0115 c on a.EmpNo = c.WPAN8 AND UPPER(LTRIM(RTRIM(c.WPPHTP))) = 'EXT'
			LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 AND UPPER(LTRIM(RTRIM(d.WPPHTP))) = 'MOBS'
			LEFT JOIN tas.Master_BusinessUnit_JDE_view e ON RTRIM(b.BusinessUnit) = RTRIM(e.BU)
			LEFT JOIN tas.Tran_ShiftPatternUpdates f ON a.EmpNo = f.EmpNo AND a.SwipeDate = f.DateX
			LEFT JOIN tas.Master_Employee_JDE_View g ON b.SupervisorNo = g.EmpNo
		WHERE 
			a.SwipeDate = @processDate
			AND ISNUMERIC(b.PayStatus) = 1
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)			
		ORDER BY a.EmpNo, a.SwipeTime DESC			
	END

/*	Debugging:
	
PARAMETERS:
	@loadType			TINYINT,		--(Note: 1 => Get all fire team members; 2 => Get all fire watch Members; 3 => Get all fire team / fire watch members)
	@processDate		DATETIME,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = NULL	

	--Test database
	EXEC tas.Pr_GetFireTeamAttendance_V2 1, '01/03/2016'		--Currently Available Fire Team
	EXEC tas.Pr_GetFireTeamAttendance_V2 2, '31/03/2016'		--Available Fire Team

	--Live database
	EXEC tas.Pr_GetFireTeamAttendance_V2 1, '02/08/2018'		--Get all fire team members
	EXEC tas.Pr_GetFireTeamAttendance_V2 2, '02/08/2018'		--Get all fire watch Members
	EXEC tas.Pr_GetFireTeamAttendance_V2 3, '02/08/2018'		--Get all fire team / fire watch members)

*/