/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDILDueToLateEntryDutyROTA
*	Description: Get Duty ROTA late entry records that causes DIL to be given to employees by the Timesheet Process
*
*	Date			Author		Rev. #		Comments:
*	21/11/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDILDueToLateEntryDutyROTA
(   	
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0	
)
AS

	--Validate parameters
	IF ISNULL(@startDate, '') = '' OR @startDate = CONVERT(DATETIME, '')  
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = '' OR @endDate = CONVERT(DATETIME, '')  
		SET @endDate = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL		
			
	SELECT	a.AutoID,
			a.BusinessUnit,
			a.EmpNo,
			LTRIM(RTRIM(d.YAALPH)) AS EmpName,
			a.DT,
			a.DIL_Entitlement,
			c.[DESCRIPTION] AS DIL_Desc,			
			a.IsPublicHoliday,
			a.IsHedger,
			b.EffectiveDate,
			b.EndingDate, 
			b.LastUpdateTime,
			b.LastUpdateUser,
			DATEDIFF(DAY, a.DT, b.LastUpdateTime) AS EntryAfterDays
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Tran_DutyRota b ON a.EmpNo = b.EmpNo AND a.DT BETWEEN b.EffectiveDate AND b.EndingDate
		LEFT JOIN tas.Master_UDCValues_JDE c ON RTRIM(a.DIL_Entitlement) = LTRIM(RTRIM(c.CODE)) AND RTRIM(c.UDCKey) = '55  -1'
		INNER JOIN tas.syJDE_F060116 d ON a.EmpNo = CAST(d.YAAN8 AS INT) 
	WHERE
		ISNULL(a.DIL_Entitlement, '') <> ''
		AND RTRIM(a.DIL_Entitlement) <> 'EA'	--(Notes: Day in lieu given to sales guys should not be checked because the condition is (1) publicholiday  (2) DayOff  (3) not Thu/Fri.)
		AND ISNULL(a.IsPublicHoliday, 0) = 0
		AND a.DT <> CONVERT(DATETIME, CONVERT(VARCHAR, '041104', 12))	--(Notes: DIL manually given to employess due to Shiekh zaid Death  -- this line doesnt make diff for ouput but included for documentation.)
		AND a.AutoID NOT IN 
        (
			SELECT XID_TS_DIL_ENT FROM tas.Tran_Absence 
			WHERE RTRIM(DIL_ENT_CODE) IN ('Admin', 'EA')
		)
		AND b.LastUpdateTime > a.DT
		AND
        (
			(a.DT BETWEEN @startDate AND @endDate AND @startDate IS NOT NULL AND @endDate IS NOT NULL) 
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
		--AND (a.EmpNo = @empNo OR @empNo IS NULL)
		--AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
	ORDER BY a.DT DESC, a.EmpNo

GO 


/*	Debug:

PARAMETERS:
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0	

	EXEC tas.Pr_GetDILDueToLateEntryDutyROTA
	EXEC tas.Pr_GetDILDueToLateEntryDutyROTA '01/01/2016', '12/31/2016'
	EXEC tas.Pr_GetDILDueToLateEntryDutyROTA '', '', '7600'
	EXEC tas.Pr_GetDILDueToLateEntryDutyROTA '', '', '', 10003154

*/
		