/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetAssignedCostCenter
*	Description: This function is used to retrieve all cost centers wherein the employee is either the Superintendent or Manager
*
*	Date			Author		Rev. #		Comments:
*	12/03/2018		Ervin		1.0			Created
**********************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetAssignedCostCenter 
(
	@empNo	INT
)
RETURNS @rtnTable 
TABLE 
(
	CompanyCode			VARCHAR(5),
	CompanyName			VARCHAR(20),
	CostCenter			VARCHAR(12),
	CostCenterName		VARCHAR(30)
) 
AS
BEGIN

	--Populate data to the table
	INSERT INTO @rtnTable  
	SELECT	LTRIM(RTRIM(a.MCCO)),
			CASE WHEN LTRIM(RTRIM(a.MCCO)) = '00100' THEN 'GARMCO' 
					WHEN LTRIM(RTRIM(a.MCCO)) = '00600' THEN 'Foil Mill' 
					ELSE '' 
			END,
			LTRIM(RTRIM(a.MCMCU)),
			LTRIM(RTRIM(a.MCDL01)) 
	FROM tas.syJDE_F0006 a
	WHERE 
		a.MCSTYL IN ('*', ' ', 'BP') 
		AND LTRIM(RTRIM(a.MCCO))IN ('00000', '00100', '00333', '00777')
		AND (a.MCAN8 = @empNo OR a.MCANPA = @empNo)
	ORDER BY LTRIM(RTRIM(a.MCMCU))

	RETURN 

END

/*	Debug:

	SELECT * FROM tas.fnGetAssignedCostCenter(10003704)

*/
