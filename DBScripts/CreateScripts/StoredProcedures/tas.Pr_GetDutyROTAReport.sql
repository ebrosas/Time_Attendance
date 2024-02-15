/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDutyROTAReport
*	Description: Retrieve duty ROTA report
*
*	Date			Author		Revision No.	Comments:
*	11/11/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDutyROTAReport
(   
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenterList		VARCHAR(500) = '',
	@empNo				INT = 0
)
AS

	--Validate parameters
	IF ISNULL(@costCenterList, '') = ''
		SET @costCenterList = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	a.AutoID,
			a.EmpNo,
			b.EmpName,
			b.Position,
			b.BusinessUnit,
			c.BusinessUnitName,
			a.EffectiveDate,
			a.EndingDate,
			a.DutyType,
			d.[Description] AS DutyDescription,
			d.DutyAllowance
	FROM tas.Tran_DutyRota a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(b.BusinessUnit) = RTRIM(c.BusinessUnit) 
		LEFT JOIN tas.Master_DutyType d ON a.DutyType = d.DutyType
	WHERE
		a.EffectiveDate >= @startDate
		AND a.EndingDate <= @endDate
		AND 
		(
			RTRIM(b.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))
			OR
            @costCenterList IS NULL
		)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY b.BusinessUnit, a.EmpNo, a.EffectiveDate DESC

GO 

/*	Debugging:

PARAMETERS:
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenterList		VARCHAR(500) = '',
	@empNo				INT = 0

	EXEC tas.Pr_GetDutyROTAReport '01/03/2016', '31/03/2016'
	EXEC tas.Pr_GetDutyROTAReport '16/02/2016', '15/03/2016', '7600, 5200, 5300'
	EXEC tas.Pr_GetDutyROTAReport '01/03/2016', '31/03/2016', '', 10003632

*/


