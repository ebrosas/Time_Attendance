USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetLeaveInformation]    Script Date: 14/04/2020 15:53:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetLeaveInformation
*	Description: Used to retrieve the employee's annual leave information
*
*	Date			Author		Revision No.	Comments:
*	05/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetLeaveInformation]
(   
	@empNo	INT
)
AS
	
	DECLARE	@leaveType					VARCHAR(5),	
			@leaveEmpServiceDate		DATETIME,
			@leaveAsOfDate				SMALLDATETIME,
			@leaveOpeningDate			SMALLDATETIME,
			@leaveEndDate				SMALLDATETIME,
			@leaveEndYear				SMALLDATETIME,
			@leaveEntitlement			FLOAT,
			@leaveCompany				CHAR(5),
			@leaveTotalService			FLOAT,
			@leaveTakenAsOfDate			FLOAT,
			@leaveTakenCurrentYear		FLOAT,
			@leaveCurrentBal			FLOAT

	--Initialize variables
	SELECT	@leaveType					= 'AL',	
			@leaveEmpServiceDate		= NULL,
			@leaveAsOfDate				= NULL,
			@leaveOpeningDate			= NULL,
			@leaveEndDate				= NULL,
			@leaveEndYear				= NULL,
			@leaveEntitlement			= 0,
			@leaveCompany				= '',
			@leaveTotalService			= 0,
			@leaveTakenAsOfDate			= 0,
			@leaveTakenCurrentYear		= 0,
			@leaveCurrentBal			= 0
		
	--Set the opening date based on the current year
	SELECT @leaveOpeningDate = CONVERT(smalldatetime, '1/1/' +
		CONVERT(char(4), DATEPART(yyyy, GETDATE())))

	--Set the ending date based on the current year
	SELECT @leaveAsOfDate = CONVERT(DATETIME, SUBSTRING(CONVERT(VARCHAR(4), DATEPART(YEAR, GETDATE())), 3, 2) + '1231')

	--Get the total leaves taken from joining date till today
	SELECT	@leaveTakenAsOfDate = ABS(SUM(ISNULL(a.LeaveDuration, 0)))			
	FROM tas.sy_LeaveRequisition AS a
	WHERE a.EmpNo = @empNo 
		AND a.LeaveType = @leaveType 
		AND a.ApprovalFlag IN ('A', 'N', 'W') 

	-- Retrieve the number of leaves taken based on as of date -------------------------------
	-- Set the end date and end year date
	SELECT @leaveEndDate = DATEADD(dd, 1, @leaveAsOfDate)
	SELECT @leaveEndYear = DATEADD(yyyy, 1, @leaveOpeningDate)

	--Get the total leaves taken on current year
	SELECT	@leaveTakenCurrentYear = ABS(SUM(ISNULL(a.LeaveDuration, 0)))			
	FROM tas.sy_LeaveRequisition AS a
	WHERE a.EmpNo = @empNo 
		AND a.LeaveType = @leaveType 
		AND a.ApprovalFlag IN ('A', 'N', 'W') 
		AND a.LeaveStartDate BETWEEN @leaveOpeningDate AND @leaveAsOfDate
			 
	--Retrieve employment history
	SELECT @leaveTotalService = 0
	SELECT @leaveTotalService = ISNULL(SUM(DATEDIFF(mm, tas.ConvertFromJulian(T3EFT), tas.ConvertFromJulian(T3EFTE))), 0)
	FROM tas.syJDE_F00092 AS a
	WHERE a.T3SBN1 = @empNo 
		AND a.T3SDB = 'E' 
		AND a.T3TYDT = 'WH'

	--Retrieve the Service Date and Leave Entitlement
	SELECT TOP 1 
		@leaveEmpServiceDate = tas.ConvertFromJulian(a.YADSI),
		@leaveEntitlement = (ISNULL(b.LVY58VCVDR, 0) / 10000),
		@leaveCompany = LTRIM(RTRIM(a.YAHMCO))
	FROM tas.syJDE_F060116 AS a 
		LEFT JOIN tas.sy_F58LV46 AS b ON a.YAHMCO = b.LVCO AND LVY58VCVCD = @leaveType AND LTRIM(RTRIM(a.YAPGRD)) = LTRIM(RTRIM(b.LVJBCD)) 
			AND (((DATEDIFF(mm, tas.ConvertFromJulian(a.YADSI), GETDATE()) + @leaveTotalService) / 12) * 1000) >= b.LVY57EPRSD
	WHERE a.YAAN8 = @empNo
	ORDER BY b.LVY58VCVDR DESC

	-- Retrieve Current Balance
	EXEC tas.pr_GetLeaveBalance @empNo, @leaveType, @leaveAsOfDate, @leaveCurrentBal output

	SELECT	@empNo					AS LeaveEmpNo,
			@leaveOpeningDate		AS LeaveOpeningDate,
			@leaveAsOfDate			AS LeaveEndingDate,
			@leaveEmpServiceDate	AS LeaveEmpServiceDate,
			RTRIM(CONVERT(VARCHAR, ISNULL(@leaveEntitlement, 0))) + ' day(s) per year' AS LeaveEntitlement,
			RTRIM(CONVERT(VARCHAR, ISNULL(@leaveTakenAsOfDate, 0))) + ' day(s)' AS LeaveTakenAsOfDate,
			RTRIM(CONVERT(VARCHAR, ISNULL(@leaveTakenCurrentYear, 0))) + ' day(s)'	AS LeaveTakenCurrentYear,
			RTRIM(CONVERT(VARCHAR, ISNULL(@leaveCurrentBal, 0))) + ' day(s)' AS LeaveCurrentBal

