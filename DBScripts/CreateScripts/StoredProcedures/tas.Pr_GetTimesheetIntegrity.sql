/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTimesheetIntegrity
*	Description: This stored procedure is used to fetch data for the "Timesheet Integrity by Correction Code" form 
*
*	Date			Author		Revision No.	Comments:
*	05/07/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTimesheetIntegrity
(   	
	@actionCode		VARCHAR(12),
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

	IF @actionCode = 'TSOPT1'	--Add OT, but there is no OT
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				(
					RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'AO%'
					OR 
					RTRIM(ISNULL(a.CorrectionCode, '')) IN 
					(
						'ACS', 		--ADD OT Change Shift           
						'MA'  		--ADD OT Manager Approval       
					)
				)
			)
			AND (a.OTStartTime IS NULL OR a.OTEndTime IS NULL)			
	END 

	ELSE IF @actionCode = 'TSOPT2'	--Add NoPayHours, but there are no NoPayHour
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'AN%'
			)
			AND ISNULL(a.NoPayHours, 0) = 0
	END 

	ELSE IF @actionCode = 'TSOPT3'	--Add Shift Allowance, but there is no allowance
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'AS%'
			)
			AND ISNULL(a.ShiftAllowance, 0) = 0
	END 

	ELSE IF @actionCode = 'TSOPT4'	--Mark Absent, but not absent
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'MA%'
			)
			AND ISNULL(a.RemarkCode, '') <> 'A'
	END 

	ELSE IF @actionCode = 'TSOPT5'	--Mark DIL, but there is no DIL
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'MD%'
			)
			AND ISNULL(a.DIL_Entitlement, '') = ''
	END 

	ELSE IF @actionCode = 'TSOPT6'	--Remove OT, but still there is OT
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'RO%'
			)
			AND (a.OTStartTime IS NOT NULL AND a.OTEndTime IS NOT NULL)
	END 

	ELSE IF @actionCode = 'TSOPT7'	--Remove NoPayHour, but still there is NoPayHour
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'RN%'
			)
			AND ISNULL(a.NoPayHours, 0) > 0
	END 

	ELSE IF @actionCode = 'TSOPT8'	--Remove Shift Allowances, but it is not removed
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'RS%'
			)
			AND a.ShiftAllowance = 1
	END 

	ELSE IF @actionCode = 'TSOPT9'	--Remove Absence, but still there is Absence
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'RA%'
			)
			AND RTRIM(a.RemarkCode) = 'A'
	END 

	ELSE IF @actionCode = 'TSOPT10'		--Remove DIL, but still there is DIL
	BEGIN
    
		SELECT * FROM tas.Vw_TimesheetIntegrity a
		WHERE 
			(a.EmpNo = @empNo OR @empNo IS NULL)
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
				(@startDate IS NULL AND @endDate IS NULL)
			)
			AND 
			(
				a.CorrectionCode IS NOT NULL
				AND 
				RTRIM(ISNULL(a.CorrectionCode, '')) LIKE 'RD%'
			)
			AND RTRIM(a.DIL_Entitlement) <> ''
	END 

GO 

/*	Debugging:

PARAMETERS:
	@actionCode		VARCHAR(12),
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= ''

	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT1', '05/07/2015', '05/07/2016'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT2'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT3'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT4'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT5'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT6'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT7'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT8'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT8'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT9'
	EXEC tas.Pr_GetTimesheetIntegrity 'TSOPT10'

*/

/*	Data Testing:

	--Retrieve all Timesheet Correction Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-T0'
	ORDER BY LTRIM(RTRIM(DRKY))

	--Retrieve all leave type codes
	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'VC'
	ORDER BY LTRIM(RTRIM(DRDL01))

	--Retrieve Leave Absence Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRSY)) = '58' AND LTRIM(RTRIM(DRRT)) = 'WC'
	ORDER BY LTRIM(RTRIM(DRDL01))

	SELECT * FROM tas.syJDE_F0005
	WHERE LTRIM(RTRIM(DRKY)) = 'RWLC'		

	--Retrieve all Absent Reason Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-RA'
	ORDER BY DRDL01

	--Retrieve all Leave Types
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '55-LV'
	ORDER BY DRDL01

	--Retrieve all Absent Codes
	SELECT * FROM tas.syJDE_F0005
	WHERE ltrim(rtrim(DRSY)) + '-' + ltrim(rtrim(DRRT)) = '00-TD'
	ORDER BY DRDL01

*/


