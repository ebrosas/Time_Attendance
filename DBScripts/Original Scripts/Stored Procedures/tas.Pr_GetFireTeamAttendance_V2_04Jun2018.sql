USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetFireTeamAttendance_V2]    Script Date: 04/06/2018 08:39:53 ******/
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

	DECLARE	@CONST_CUTOFF_TIME TIME 
	SET @CONST_CUTOFF_TIME = CONVERT(TIME, '07:00:00')	--(Note: Refers to the end time of the Night Shift schedule)

	--Validate the parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF @loadType = 1	--Get All Fire Team members
	BEGIN
	
		IF CONVERT(TIME, GETDATE()) < @CONST_CUTOFF_TIME
		BEGIN

			--The current system date/time is less than the required time out for Night Shift
			SELECT	DISTINCT
					a.EmpNo, 
					a.EmpName,
					a.Position,
					a.GradeCode,
					a.CostCenter,
					a.CostCenterName,
					a.SupervisorEmpNo,
					a.SupervisorEmpName,			
					a.Extension,
					a.MobileNo,
					a.Notes,
					d.Effective_ShiftPatCode AS ShiftPatCode,
					d.Effective_ShiftPointer AS ShiftPointer,
					d.Effective_ShiftCode AS ShiftCode,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) AS SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) AS SwipeTime,
					RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType
			FROM tas.Vw_FireTeamMembers a
				LEFT JOIN tas.sy_EvnLog b ON a.EmpNo = CAST(b.FName AS INT) + 10000000  
					AND 
					(
						(b.Loc = 1 AND b.Dev IN (4, 6, 5, 7))
						OR (b.Loc = 2 AND b.Dev IN (0, 1, 2, 3)) 
					) 
					AND b.[Event] = 8
					AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) BETWEEN DATEADD(DAY, -1, @processDate) AND @processDate	
				LEFT JOIN tas.Master_AccessReaders c ON b.Loc = c.LocationCode AND b.Dev = c.ReaderNo
				LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = d.DateX
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.CostCenter = @costCenter OR @costCenter IS NULL)			
			ORDER BY a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) DESC
        END 

		ELSE
        BEGIN
        
			SELECT	DISTINCT
					a.EmpNo, 
					a.EmpName,
					a.Position,
					a.GradeCode,
					a.CostCenter,
					a.CostCenterName,
					a.SupervisorEmpNo,
					a.SupervisorEmpName,			
					a.Extension,
					a.MobileNo,
					a.Notes,
					d.Effective_ShiftPatCode AS ShiftPatCode,
					d.Effective_ShiftPointer AS ShiftPointer,
					d.Effective_ShiftCode AS ShiftCode,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) AS SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) AS SwipeTime,
					RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType
			FROM tas.Vw_FireTeamMembers a
				LEFT JOIN tas.sy_EvnLog b ON a.EmpNo = CAST(b.FName AS INT) + 10000000  
					AND 
					(
						(b.Loc = 1 AND b.Dev IN (4, 6, 5, 7))
						OR (b.Loc = 2 AND b.Dev IN (0, 1, 2, 3)) 
					) 
					AND b.[Event] = 8
					AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = @processDate	
				LEFT JOIN tas.Master_AccessReaders c ON b.Loc = c.LocationCode AND b.Dev = c.ReaderNo
				LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = d.DateX
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.CostCenter = @costCenter OR @costCenter IS NULL)			
			ORDER BY a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) DESC
		END 
	END

	IF @loadType = 2	--Get All Fire Watch members
	BEGIN
	
		IF CONVERT(TIME, GETDATE()) < @CONST_CUTOFF_TIME
		BEGIN

			--The current system date/time is less than the required time out for Night Shift
			SELECT	DISTINCT
					a.EmpNo, 
					a.EmpName,
					a.Position,
					a.GradeCode,
					a.CostCenter,
					a.CostCenterName,
					a.SupervisorEmpNo,
					a.SupervisorEmpName,			
					a.Extension,
					a.MobileNo,
					a.Notes,
					d.Effective_ShiftPatCode AS ShiftPatCode,
					d.Effective_ShiftPointer AS ShiftPointer,
					d.Effective_ShiftCode AS ShiftCode,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) AS SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) AS SwipeTime,
					RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType
			FROM tas.Vw_FireWatchMembers a
				LEFT JOIN tas.sy_EvnLog b ON a.EmpNo = CAST(b.FName AS INT) + 10000000  
					AND 
					(
						(b.Loc = 1 AND b.Dev IN (4, 6, 5, 7))
						OR (b.Loc = 2 AND b.Dev IN (0, 1, 2, 3)) 
					) 
					AND b.[Event] = 8
					AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) BETWEEN DATEADD(DAY, -1, @processDate) AND @processDate	
				LEFT JOIN tas.Master_AccessReaders c ON b.Loc = c.LocationCode AND b.Dev = c.ReaderNo
				LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = d.DateX
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.CostCenter = @costCenter OR @costCenter IS NULL)			
			ORDER BY a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) DESC
        END
        
		ELSE
        BEGIN
        
			SELECT	DISTINCT
					a.EmpNo, 
					a.EmpName,
					a.Position,
					a.GradeCode,
					a.CostCenter,
					a.CostCenterName,
					a.SupervisorEmpNo,
					a.SupervisorEmpName,			
					a.Extension,
					a.MobileNo,
					a.Notes,
					d.Effective_ShiftPatCode AS ShiftPatCode,
					d.Effective_ShiftPointer AS ShiftPointer,
					d.Effective_ShiftCode AS ShiftCode,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) AS SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) AS SwipeTime,
					RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType
			FROM tas.Vw_FireWatchMembers a
				LEFT JOIN tas.sy_EvnLog b ON a.EmpNo = CAST(b.FName AS INT) + 10000000  
					AND 
					(
						(b.Loc = 1 AND b.Dev IN (4, 6, 5, 7))
						OR (b.Loc = 2 AND b.Dev IN (0, 1, 2, 3)) 
					) 
					AND b.[Event] = 8
					AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = @processDate	
				LEFT JOIN tas.Master_AccessReaders c ON b.Loc = c.LocationCode AND b.Dev = c.ReaderNo
				LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = d.DateX
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.CostCenter = @costCenter OR @costCenter IS NULL)			
			ORDER BY a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) DESC
		END 
	END

	ELSE IF @loadType = 3	--Get All Fire Watch and Fire Watch members
	BEGIN
	
		IF CONVERT(TIME, GETDATE()) < @CONST_CUTOFF_TIME
		BEGIN

			--The current system date/time is less than the required time out for Night Shift
			SELECT	DISTINCT
					a.EmpNo, 
					a.EmpName,
					a.Position,
					a.GradeCode,
					a.CostCenter,
					a.CostCenterName,
					a.SupervisorEmpNo,
					a.SupervisorEmpName,			
					a.Extension,
					a.MobileNo,
					a.Notes,
					d.Effective_ShiftPatCode AS ShiftPatCode,
					d.Effective_ShiftPointer AS ShiftPointer,
					d.Effective_ShiftCode AS ShiftCode,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) AS SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) AS SwipeTime,
					RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType
			FROM tas.Vw_FireTeamFireWatchMembers a
				LEFT JOIN tas.sy_EvnLog b ON a.EmpNo = CAST(b.FName AS INT) + 10000000  
					AND 
					(
						(b.Loc = 1 AND b.Dev IN (4, 6, 5, 7))
						OR (b.Loc = 2 AND b.Dev IN (0, 1, 2, 3)) 
					) 
					AND b.[Event] = 8
					AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) BETWEEN DATEADD(DAY, -1, @processDate) AND @processDate	
				LEFT JOIN tas.Master_AccessReaders c ON b.Loc = c.LocationCode AND b.Dev = c.ReaderNo
				LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = d.DateX
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.CostCenter = @costCenter OR @costCenter IS NULL)			
			ORDER BY a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) DESC
        END
        
		ELSE
        BEGIN
        
			SELECT	DISTINCT
					a.EmpNo, 
					a.EmpName,
					a.Position,
					a.GradeCode,
					a.CostCenter,
					a.CostCenterName,
					a.SupervisorEmpNo,
					a.SupervisorEmpName,			
					a.Extension,
					a.MobileNo,
					a.Notes,
					d.Effective_ShiftPatCode AS ShiftPatCode,
					d.Effective_ShiftPointer AS ShiftPointer,
					d.Effective_ShiftCode AS ShiftCode,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) AS SwipeDate,
					CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) AS SwipeTime,
					RTRIM(c.LocationName)  + ' - ' + RTRIM(c.ReaderName) AS SwipeLocation,
					(
						CASE	WHEN UPPER(RTRIM(c.Direction)) = 'I' THEN 'IN' 
								WHEN UPPER(RTRIM(c.Direction)) = 'O' THEN 'OUT' 
								ELSE '' END
					) AS SwipeType
			FROM tas.Vw_FireTeamFireWatchMembers a
				LEFT JOIN tas.sy_EvnLog b ON a.EmpNo = CAST(b.FName AS INT) + 10000000  
					AND 
					(
						(b.Loc = 1 AND b.Dev IN (4, 6, 5, 7))
						OR (b.Loc = 2 AND b.Dev IN (0, 1, 2, 3)) 
					) 
					AND b.[Event] = 8
					AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = @processDate	
				LEFT JOIN tas.Master_AccessReaders c ON b.Loc = c.LocationCode AND b.Dev = c.ReaderNo
				LEFT JOIN tas.Tran_ShiftPatternUpdates d ON a.EmpNo = d.EmpNo AND CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 12)) = d.DateX
			WHERE 
				(a.EmpNo = @empNo OR @empNo IS NULL)
				AND (a.CostCenter = @costCenter OR @costCenter IS NULL)			
			ORDER BY a.EmpNo, CONVERT(DATETIME, CONVERT(VARCHAR, b.TimeDate, 126)) DESC
		END 
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
	EXEC tas.Pr_GetFireTeamAttendance_V2 1, '03/20/2018'		--Get all fire team members
	EXEC tas.Pr_GetFireTeamAttendance_V2 2, '03/20/2018'		--Get all fire watch Members
	EXEC tas.Pr_GetFireTeamAttendance_V2 3, '03/20/2018'		--Get all fire team / fire watch members)

*/