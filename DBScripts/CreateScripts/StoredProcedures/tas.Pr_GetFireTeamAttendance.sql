/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetFireTeamAttendance
*	Description: This stored procedure is used to fetch attendance records of the Fire Team Members
*
*	Date:			Author:		Rev. #:		Comments:
*	18/01/2015		Ervin		1.0			Created
*****************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetFireTeamAttendance
(	
	@actionType		INT,		--(Note: 0 => All Fire Team Members; 1 => Only Present Fire Team Members)
	@processDate	DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = NULL	
)
AS

	--Validate the parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @actionType = 0
	BEGIN
	
		--Get all Fire Team members
		SELECT  SwipeDate,
			SwipeTime,
			a.EmpNo, 
			b.EmpName,
			b.Position,
			ISNULL(CONVERT(VARCHAR(20), e.WPPH1),'') AS Extension,
			ISNULL(LTRIM(RTRIM(f.WPPH1)), '') AS MobileNo, 
			b.GradeCode,
			b.PayStatus,
			RTRIM(b.BusinessUnit) AS CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			b.SupervisorNo AS SupervisorEmpNo,
			RTRIM(i.EmpName) AS SupervisorEmpName,
			c.Superintendent AS SuperintendentEmpNo,
			RTRIM(d.EmpName) AS SuperintendentEmpName,
			h.Effective_ShiftPatCode AS ShiftPatCode,
			h.Effective_ShiftPointer AS ShiftPointer,
			h.Effective_ShiftCode AS ShiftCode,
			SwipeLocation,
			SwipeType,
			LocationCode,
			ReaderNo,
			SwipeCode,
			LocationName,
			ReaderName,
			[Event],
			Notes
		FROM
		(
			SELECT CASE WHEN ISNUMERIC(a.FName) = 1 
						THEN 
							CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
							THEN 
								CONVERT(INT, a.FName)
							ELSE 
								CONVERT(INT, a.FName) + 10000000 
							END
						ELSE 0 
						END AS EmpNo,
				CONVERT(DATETIME, CONVERT(VARCHAR, c.TimeDate, 12)) as SwipeDate,
				CONVERT(DATETIME, CONVERT(VARCHAR, c.TimeDate, 126)) as SwipeTime,
				RTRIM(d.LocationName)  + ' - ' + RTRIM(d.ReaderName) AS SwipeLocation,
				(
					CASE	WHEN UPPER(RTRIM(d.Direction)) = 'I' THEN 'IN' 
							WHEN UPPER(RTRIM(d.Direction)) = 'O' THEN 'OUT' 
							ELSE '' END
				) AS SwipeType, 		
				d.LocationCode,
				d.ReaderNo,
				'MAINGATE' AS SwipeCode,
				d.LocationName,
				d.ReaderName,		
				c.[Event],
				RTRIM(CONVERT(VARCHAR(500), a.Notes)) AS Notes	
			FROM tas.sy_NAMES a 
				INNER JOIN tas.sy_UDF b ON a.ID = b.NameID	
				LEFT JOIN tas.sy_EvnLog c ON LTRIM(RTRIM(a.FName)) = LTRIM(RTRIM(c.FName)) 
					AND CONVERT(VARCHAR, c.TimeDate, 12) = @processDate 
					AND 
					(
						(c.Loc = 1 AND c.Dev IN (4, 6, 5, 7))
						OR (c.Loc = 2 AND c.Dev IN (0, 1, 2, 3)) 
					) 
					AND c.[Event] = 8
				LEFT JOIN tas.Master_AccessReaders d ON c.Loc = d.LocationCode AND c.Dev = d.ReaderNo
			WHERE b.UdfNum = 11 
				AND UPPER(LTRIM(RTRIM(b.UdfText))) = 'Y' 
		) a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)
		LEFT JOIN tas.Master_Employee_JDE_View d ON c.Superintendent = d.EmpNo
		LEFT JOIN tas.syJDE_F0115 e on a.EmpNo = e.WPAN8 AND UPPER(LTRIM(RTRIM(e.WPPHTP))) = 'EXT'
		LEFT JOIN tas.syJDE_F0115 f on a.EmpNo = f.WPAN8 AND UPPER(LTRIM(RTRIM(f.WPPHTP))) = 'MOBS'
		LEFT JOIN tas.Tran_ShiftPatternUpdates h ON a.EmpNo = h.EmpNo AND a.SwipeDate = h.DateX
		LEFT JOIN tas.Master_Employee_JDE_View i ON b.SupervisorNo = i.EmpNo
		WHERE 
			ISNUMERIC(b.PayStatus) = 1
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		ORDER BY EmpNo, SwipeTime DESC 
	END

	ELSE
	BEGIN

		--Get Fire Team members who are present in the company
		SELECT DISTINCT
			SwipeDate,
			SwipeTime,
			a.EmpNo, 
			b.EmpName,
			b.Position,
			ISNULL(CONVERT(VARCHAR(20), e.WPPH1),'') AS Extension,
			ISNULL(LTRIM(RTRIM(f.WPPH1)), '') AS MobileNo, 
			b.GradeCode,
			b.PayStatus,
			RTRIM(b.BusinessUnit) AS CostCenter,
			RTRIM(c.BusinessUnitName) AS CostCenterName,
			b.SupervisorNo AS SupervisorEmpNo,
			RTRIM(i.EmpName) AS SupervisorEmpName,
			c.Superintendent AS SuperintendentEmpNo,
			RTRIM(d.EmpName) AS SuperintendentEmpName,
			h.Effective_ShiftPatCode AS ShiftPatCode,
			h.Effective_ShiftPointer AS ShiftPointer,
			h.Effective_ShiftCode AS ShiftCode,
			SwipeLocation,
			SwipeType,
			LocationCode,
			ReaderNo,
			SwipeCode,
			LocationName,
			ReaderName,
			[Event],
			Notes
		FROM
		(
			SELECT	CASE WHEN ISNUMERIC(a.FName) = 1 
					THEN 
						CASE WHEN ((CONVERT(INT, a.FName) >= 10000 OR CONVERT(INT, a.FName) >= 50000) AND CONVERT(INT, a.FName) < 10000000)
						THEN 
							CONVERT(INT, a.FName)
						ELSE 
							CONVERT(INT, a.FName) + 10000000 
						END
					ELSE 0 
					END AS EmpNo,
					CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) as SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 126)) as SwipeTime,
					RTRIM(d.LocationName)  + ' - ' + RTRIM(d.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(d.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(d.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType, 		
					d.LocationCode,
					d.ReaderNo,
					'MAINGATE' AS SwipeCode,
					d.LocationName,
					d.ReaderName,		
					a.[Event],
					CONVERT(VARCHAR(500), b.Notes) AS Notes
			FROM tas.sy_EvnLog a 
				INNER JOIN tas.sy_NAMES b ON LTRIM(RTRIM(a.FName)) = LTRIM(RTRIM(b.FName)) 
				INNER JOIN tas.sy_UDF c ON b.ID = c.NameID
				INNER JOIN tas.Master_AccessReaders d ON a.Loc = d.LocationCode AND a.Dev = d.ReaderNo
			WHERE 
				a.[Event] = 8	--(Note: 8 means successful swipe)
				AND c.UdfNum = 11 
				AND UPPER(LTRIM(RTRIM(c.UdfText))) = 'Y' 
				AND CONVERT(VARCHAR, a.TimeDate, 12) = @processDate
				AND 
				(
					(a.Loc = 1 AND a.Dev IN (4, 6, 5, 7))
					OR (a.Loc = 2 AND a.Dev IN (0, 1, 2, 3)) 
				) 
		) a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit)
		LEFT JOIN tas.Master_Employee_JDE_View d ON c.Superintendent = d.EmpNo
		LEFT JOIN tas.syJDE_F0115 e on a.EmpNo = e.WPAN8 AND UPPER(LTRIM(RTRIM(e.WPPHTP))) = 'EXT'
		LEFT JOIN tas.syJDE_F0115 f on a.EmpNo = f.WPAN8 AND UPPER(LTRIM(RTRIM(f.WPPHTP))) = 'MOBS'
		LEFT JOIN tas.Tran_ShiftPatternUpdates h ON a.EmpNo = h.EmpNo AND a.SwipeDate = h.DateX
		LEFT JOIN tas.Master_Employee_JDE_View i ON b.SupervisorNo = i.EmpNo
		WHERE 
			ISNUMERIC(b.PayStatus) = 1
			AND (a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(b.BusinessUnit) = @costCenter OR @costCenter IS NULL)
		ORDER BY EmpNo, SwipeTime DESC
	END


/*	Debugging:
	
PARAMETERS:
	@actionType		INT,		--(Note: 0 => All Fire Team Members; 1 => Only Present Fire Team Members)
	@processDate	DATETIME,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12) = NULL	

	--Test database
	EXEC tas.Pr_GetFireTeamAttendance 0, '01/03/2016'	--All Fire Team
	EXEC tas.Pr_GetFireTeamAttendance 1, '01/03/2016'	--Available Fire Team

	--Live database
	EXEC tas.Pr_GetFireTeamAttendance 0, '02/04/2018'	--All Fire Team
	EXEC tas.Pr_GetFireTeamAttendance 1, '02/04/2018'	--Available Fire Team

*/