/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnIsSpecialCatalogMedicalCondition
*	Description: This function is used to check if an employee is setup as "Medical Condition" using the Special Catalog form in TAS
*
*	Date			Author		Rev. #		Comments:
*	16/10/2017		Ervin		1.0			Created
*	07/12/2017		Ervin		1.1			Added "Child Care Period" in the filter condition as per Helpdesk #85629
*	31/01/2018		Ervin		1.2			Added "Excused - Job Requirement" in the filter condition as per Helpdesk #86099
*	25/02/2018		Ervin		1.3			Added validation that checks the effectivity date
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnIsSpecialCatalogMedicalCondition 
(
	@empNo	INT
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result BIT
	SET @result = 0
    
	IF EXISTS
    (
		SELECT AutoID FROM tas.Master_EmployeeAdditional a
		WHERE a.EmpNo = @empNo
			AND RTRIM(a.SpecialJobCatg) IN
			(
				'C',	--Child Care Period	(Rev. #1.1)
				'M',	--Medical Condition
				'E' 	--Excused - Job Requirement	(Rev. #1.2)     
			)
			AND	--Rev. #1.3
			(
				CONVERT(VARCHAR, GETDATE(), 12) BETWEEN a.CatgEffectiveDate AND a.CatgEndingDate
				OR (a.CatgEffectiveDate IS NULL and a.CatgEndingDate IS NULL)
			)
	)
	SET @result = 1

	RETURN @result
END


/*	Debugging:

	--Test database
	SELECT tas.fnIsSpecialCatalogMedicalCondition(10003632) 

	--Live database
	SELECT tas.fnIsSpecialCatalogMedicalCondition(10003381) 

*/