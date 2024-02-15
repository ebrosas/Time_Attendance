	/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAttendanceWithOT
*	Description: Retrieve the employee attendance with overtime
*
*	Date			Author		Revision No.	Comments:
*	26/10/2016		Ervin		1.0				Created
*	14/11/2016		Ervin		1.1				Commented filter condition "RTRIM(b.OTType) = @code_OTType_Regular"
*	30/11/2016		Ervin		1.2				Return zero OT duration if it is not approved
*	16/12/2016		Ervin		1.3				Modified the Order By clause
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAttendanceWithOT
(   
	@startDate		DATETIME,
	@endDate		DATETIME = NULL,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0
)
AS

	DECLARE	@minutes_MinOT_NSS		INT,
			@code_OTType_Regular	VARCHAR(10)	--OT Code for regular working day

	--Get the minimum OT allowed
	SELECT	@minutes_MinOT_NSS		= Minutes_MinOT_NSS,
			@code_OTType_Regular	= RTRIM(Code_OTtype_Regular)
	FROM tas.System_Values

	--Validate parameters
	IF ISNULL(@endDate, '') = ''
		SET @endDate = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	a.DT,
			a.BusinessUnit,
			d.BusinessUnitName,
			a.EmpNo,
			c.EmpName,
			c.GradeCode,
			a.dtIN,
			a.dtOUT,
			b.OTstartTime,
			b.OTendTime,
			b.OTtype,
			CASE WHEN b.Approved = 1
				THEN DATEDIFF(n, a.OTStartTime, a.OTEndTime)
				ELSE --DATEDIFF(n, b.OTStartTime, b.OTEndTime)
					CASE WHEN RTRIM(b.OTApproved) = 'N' THEN 0 ELSE DATEDIFF(n, b.OTStartTime, b.OTEndTime) END	--Rev. #1.2
			END AS OTDurationMinute,
			CASE WHEN b.Approved = 1 
				THEN tas.fmtMIN_HHmm(DATEDIFF(n, a.OTStartTime, a.OTEndTime)) 
				ELSE --tas.fmtMIN_HHmm(DATEDIFF(n, b.OTStartTime, b.OTEndTime)) 
					CASE WHEN RTRIM(b.OTApproved) = 'N' THEN '' ELSE tas.fmtMIN_HHmm(DATEDIFF(n, b.OTStartTime, b.OTEndTime)) END	--Rev. #1.2
			END AS OTDurationHour,
			b.Approved,
			CASE WHEN b.OTApproved = '0'
				THEN NULL
				ELSE a.MealVoucherEligibility
			END AS MealVoucherEligibility, 
			b.Comment,
			b.OTApproved,
			b.OTReason AS OTReasonCode,
			e.[DESCRIPTION] AS OTReason,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.AutoID,
			b.XID_AutoID,
			a.Processed
	From tas.Tran_Timesheet a
		INNER JOIN tas.Tran_Timesheet_Extra b ON a.AutoID = b.XID_AutoID	
		INNER JOIN tas.Master_Employee_JDE_View_V2 c on a.EmpNo = c.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE d ON RTRIM(a.BusinessUnit) = RTRIM(d.BusinessUnit)
		LEFT JOIN tas.Master_OTReasons_JDE e ON RTRIM(b.OTReason) = RTRIM(e.CODE)
	WHERE 		
		b.OTstartTime IS NOT NULL
		AND b.OTendTime IS NOT NULL
		AND a.IsLastRow = 1
		AND 
		(
			(
				DATEDIFF(n, b.OTstartTime, b.OTendTime) > @minutes_MinOT_NSS 
				--AND RTRIM(b.OTType) = @code_OTType_Regular	
				AND RTRIM(b.OTType) IN ('R', 'O', 'D')	--(Note: R - Regular OT; O - Dayoff OT; D - DIL OT)
			)
		)
		AND 
		(
			(
				a.DT BETWEEN @startDate AND @endDate 
				AND 
				(@startDate IS NOT NULL AND @endDate IS NOT NULL)
				AND 
				@startDate < @endDate
			)
			OR
            (
				a.DT = @startDate 
				AND 
				(@endDate IS NULL OR @endDate = @startDate)
			)
		)
		AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY a.DT, a.BusinessUnit, a.EmpNo

GO 

/*	Debugging:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME = NULL,
	@costCenter		VARCHAR(12) = ''
	@empNo			INT = 0

	EXEC tas.Pr_GetAttendanceWithOT '14/03/2016', '15/03/2016', '3230'
	EXEC tas.Pr_GetAttendanceWithOT '16/02/2016', '15/03/2016', '2110'
	EXEC tas.Pr_GetAttendanceWithOT '11/01/2016', '11/01/2016', '', 10002160

	

*/


