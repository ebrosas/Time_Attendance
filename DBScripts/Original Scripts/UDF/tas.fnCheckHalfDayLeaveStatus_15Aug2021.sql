USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnCheckHalfDayLeaveStatus]    Script Date: 15/08/2021 08:37:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckHalfDayLeaveStatus
*	Description: Check if the employee is on half day leave
*
*	Date			Author		Rev. #		Comments:
*	15/04/2016		Ervin		1.0			Created
*	27/11/2019		Ervin		1.1			Refactored the code to enhance data retrieval performance
*************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnCheckHalfDayLeaveStatus] 
(
      @empNo		INT,
      @inOutStatus	VARCHAR(2),
	  @processDate	DATETIME
)
RETURNS varchar(100)
AS
BEGIN

	--Declare variables
	DECLARE @remarks			VARCHAR(100),
			@haldDayLeaveFlag	CHAR(1),
			@leaveResumeDate	DATETIME,
			@empArrival			DATETIME

	--Initialize variables
	SELECT	@remarks			= 'NotHD',
			@haldDayLeaveFlag	= '',
			@leaveResumeDate	= NULL 

	--Validate parameters
	IF @processDate IS NULL OR @processDate = CONVERT(DATETIME, '') 
		SET @processDate = GETDATE()

	--Get the employee arrival time
	SELECT @empArrival = CONVERT(TIME, MIN(a.AttendanceDate))  
	FROM tas.Master_EmployeeAttendance a WITH (NOLOCK)
	WHERE a.LocationCode IN (1, 2)
		AND a.ReaderNo IN (0, 3, 4, 6, 8)
		AND a.EmployeeNo = @empNo
    
	--Get the half day leave flag
	SELECT @haldDayLeaveFlag = RTRIM(a.HalfDayLeaveFlag )
	FROM tas.Vw_LeaveRequisition a WITH (NOLOCK)
	WHERE a.EmpNo = @empNo
		AND a.LeaveResumeDate = CONVERT(DATETIME, CONVERT(VARCHAR, @processDate, 12))
		AND a.HalfDayLeave = 'F'
		AND LTRIM(RTRIM(a.ApprovalFlag)) IN ('A', 'N')

	IF @haldDayLeaveFlag = '2'	--Half Day on Leave Resume Date 
	BEGIN

		--Get the resume date
		SELECT @leaveResumeDate = b.LeaveResumeDate
		FROM tas.Vw_LeaveRequisition a WITH (NOLOCK)
			INNER JOIN tas.sy_LeaveRequisitionDetail b WITH (NOLOCK) ON a.LeaveNo = b.LeaveNo
		WHERE a.EmpNo = @empNo
			AND a.LeaveResumeDate = CONVERT(DATETIME, CONVERT(VARCHAR, @processDate, 12))
			AND a.HalfDayLeave = 'F'
			AND LTRIM(RTRIM(a.ApprovalFlag)) IN ('A', 'N')

		IF @leaveResumeDate IS NOT NULL 
		BEGIN

			--IF CONVERT(TIME, @empArrival) > CONVERT(TIME, @leaveResumeDate)
			IF DATEDIFF(MINUTE, CONVERT(TIME, @leaveResumeDate),  CONVERT(TIME, @empArrival)) > 1	--Give 1 minute grace time
				SET @remarks = 'Late'
        END 
    END 

	RETURN @remarks

END


/*	Debugging:

	SELECT tas.fnCheckHalfDayLeaveStatus (10003632, 'i', '02/11/2016')
	SELECT tas.fnCheckHalfDayLeaveStatus (10003632, 'o', '04/09/2014')

*/

