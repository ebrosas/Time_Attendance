/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetWorkingCostCenter
*	Description: Get the list of employees with different working cost center
*
*	Date			Author		Rev.#		Comments:
*	20/06/2016		Ervin		1.0			Created
*	28/12/2017		Ervin		1.1			Added "CatgEffectiveDate" and "CatgEndingDate" fields in the returned dataset	
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetWorkingCostCenter
(   
	@autoID				INT = 0,
	@empNo				INT = 0,
	@costCenter			VARCHAR(12) = '', 
	@specialJobCatg		varchar(10) = ''
)
AS

	--Validate parameters
	IF ISNULL(@autoID, 0) = 0
		SET @autoID = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@specialJobCatg, '') = ''
		SET @specialJobCatg = NULL
		 
	SELECT	a.AutoID,
			a.EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,
			LTRIM(RTRIM(ISNULL(g.JMDL01, ''))) AS Position,
			CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
				WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
			END AS CostCenter,
			d.BusinessUnitName AS CostCenterName,
			a.ShiftPatCode,
			a.ShiftPointer,
			a.WorkingBusinessUnit,
			e.BusinessUnitName AS WorkingBusinessUnitName,
			a.SpecialJobCatg,
			f.[Description] AS SpecialJobCatgDesc,
			a.LastUpdateUser,
			a.LastUpdateTime,
			a.CatgEffectiveDate,
			a.CatgEndingDate
	FROM tas.Master_EmployeeAdditional a 
		INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		LEFT JOIN tas.syJDE_F0101 c ON b.YAAN8 = c.ABAN8
		LEFT JOIN tas.Master_BusinessUnit_JDE d ON 
		(
			CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
				WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
			END
		) = LTRIM(RTRIM(d.BusinessUnit))
		LEFT JOIN tas.Master_BusinessUnit_JDE e ON RTRIM(a.WorkingBusinessUnit) = LTRIM(RTRIM(e.BusinessUnit))
		LEFT JOIN tas.Master_UDCValues f ON RTRIM(a.SpecialJobCatg) = RTRIM(f.Code) AND f.UDCKey = '55-SJ' 
		LEFT JOIN tas.syJDE_F08001 g on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(g.JMJBCD))
	WHERE 
		ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END) = 1
		AND (a.AutoID = @autoID OR @autoID IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND 
		(
			(
				CASE WHEN LTRIM(RTRIM(c.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
					WHEN LTRIM(RTRIM(c.ABAT1)) = 'UG' THEN LTRIM(RTRIM(c.ABMCU)) 
				END
			) = @costCenter
			OR
            @costCenter IS NULL
		)
		AND (RTRIM(a.SpecialJobCatg) = RTRIM(@specialJobCatg) OR @specialJobCatg IS NULL)
	ORDER BY a.LastUpdateTime DESC, a.EmpNo

GO 

/*	Debugging:

	EXEC tas.Pr_GetWorkingCostCenter
	EXEC tas.Pr_GetWorkingCostCenter 2650
	EXEC tas.Pr_GetWorkingCostCenter 0, '5300'
	EXEC tas.Pr_GetWorkingCostCenter 0, '', 'D'

*/


