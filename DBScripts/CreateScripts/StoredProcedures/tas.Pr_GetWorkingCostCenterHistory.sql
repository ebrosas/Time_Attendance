/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetWorkingCostCenterHistory
*	Description: Get the journal entries for all database transactions against "Master_EmployeeAdditional" table
*
*	Date			Author		Rev.#		Comments:
*	16/05/2018		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetWorkingCostCenterHistory
(   
	@empNo INT 
)
AS

	IF EXISTS
    (
		SELECT a.AutoID FROM tas.Master_EmployeeAdditional_JN a
		WHERE a.OLD_EmpNo = @empNo
			AND
			(
				ISNULL(a.OLD_SpecialJobCatg, '') <> '' 
				OR ISNULL(a.NEW_SpecialJobCatg, '') <> ''
				OR ISNULL(a.OLD_WorkingBusinessUnit, '') <> '' 
				OR ISNULL(a.NEW_WorkingBusinessUnit, '') <> ''
			)
	)
	BEGIN 
	
		SELECT DISTINCT 
			a.AutoID,
			a.OLD_EmpNo AS EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,
			a.OLD_ShiftPatCode AS ShiftPatCode,
			--a.OLD_ShiftPointer AS ShiftPointer,
			a.NEW_SpecialJobCatg AS SpecialJobCatg,
			d.[Description] AS SpecialJobCatgDesc,		
			a.NEW_WorkingBusinessUnit AS WorkingBusinessUnit,
			c.BusinessUnitName AS WorkingBusinessUnitName,
			a.NEW_CatgEffectiveDate AS CatgEffectiveDate,
			a.NEW_CatgEndingDate AS CatgEndingDate,
			a.NEW_LastUpdateUser AS LastUpdateUser,
			a.NEW_LastUpdateTime AS LastUpdateTime
		FROM tas.Master_EmployeeAdditional_JN a
			LEFT JOIN tas.syJDE_F060116 b ON a.OLD_EmpNo = b.YAAN8
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.NEW_WorkingBusinessUnit) = LTRIM(RTRIM(c.BusinessUnit))
			LEFT JOIN tas.Master_UDCValues d ON RTRIM(a.NEW_SpecialJobCatg) = RTRIM(d.Code) AND RTRIM(d.UDCKey) = '55-SJ' 
		WHERE 
			a.OLD_EmpNo = @empNo 
			AND
			(
				ISNULL(a.OLD_SpecialJobCatg, '') <> '' 
				OR ISNULL(a.NEW_SpecialJobCatg, '') <> ''
				OR ISNULL(a.OLD_WorkingBusinessUnit, '') <> '' 
				OR ISNULL(a.NEW_WorkingBusinessUnit, '') <> ''
			)
		ORDER BY a.NEW_LastUpdateTime DESC
	END 

	ELSE
    BEGIN

		SELECT DISTINCT 
			a.AutoID,
			a.EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,
			a.ShiftPatCode,
			--a.ShiftPointer,
			a.SpecialJobCatg,
			d.[Description] AS SpecialJobCatgDesc,		
			a.WorkingBusinessUnit,
			c.BusinessUnitName AS WorkingBusinessUnitName,
			a.CatgEffectiveDate,
			a.CatgEndingDate,
			a.LastUpdateUser,
			a.LastUpdateTime
		FROM tas.Master_EmployeeAdditional a
			INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON RTRIM(a.WorkingBusinessUnit) = LTRIM(RTRIM(c.BusinessUnit))
			LEFT JOIN tas.Master_UDCValues d ON RTRIM(a.SpecialJobCatg) = RTRIM(d.Code) AND RTRIM(d.UDCKey) = '55-SJ' 
		WHERE 
			a.EmpNo = @empNo
    END 

GO 

/*	Debugging:

	EXEC tas.Pr_GetWorkingCostCenterHistory 10003195
	EXEC tas.Pr_GetWorkingCostCenterHistory 10003813
	
*/


