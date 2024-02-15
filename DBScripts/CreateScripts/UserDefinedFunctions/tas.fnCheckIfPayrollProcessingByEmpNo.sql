/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfPayrollProcessingByEmpNo
*	Description: This function is used to check whether payroll processing is in progress based on the employee no.
*
*	Date			Author		Rev. #		Comments:
*	06/08/2018		Ervin		1.0			Created
**********************************************************************************************************************************************/

CREATE FUNCTION tas.fnCheckIfPayrollProcessingByEmpNo 
(
	@empNo	INT 
)
RETURNS @rtnTable 
TABLE 
(
	IsPayrollProcessing	BIT
) 
AS
BEGIN

    DECLARE @myTable TABLE 
	(
		IsPayrollProcessing	BIT
	)
    
	--Declare constants

	--Declare variables
	DECLARE	@isPayrollProcessing	BIT,
			@companyCode			VARCHAR(5) 

	SET @isPayrollProcessing = 0

	--Get the employee's company code
	SELECT @companyCode = LTRIM(RTRIM(a.YAHMCO))
	FROM tas.syJDE_F060116 a
	WHERE a.YAAN8 = @empNo

	IF EXISTS
    (
		SELECT 1 FROM tas.sy_F07350 AS a
		WHERE LTRIM(RTRIM(a.Y0HMCO)) = @companyCode
	)
	SET @isPayrollProcessing = 1

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	@isPayrollProcessing
	
	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 
END


/*	Debugging:

	SELECT * FROM tas.fnCheckIfPayrollProcessingByEmpNo(10003632)	

*/