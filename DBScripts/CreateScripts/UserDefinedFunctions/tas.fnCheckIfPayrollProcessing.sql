/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfPayrollProcessing
*	Description: This function is used to check whether payroll processing is in progress
*
*	Date			Author		Rev. #		Comments:
*	23/01/2018		Ervin		1.0			Created
**********************************************************************************************************************************************/

CREATE FUNCTION tas.fnCheckIfPayrollProcessing 
(
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
    
	DECLARE	@isPayrollProcessing BIT 
	SET @isPayrollProcessing = 0

	IF EXISTS
    (
		SELECT TOP 1 a.Y0AN8
		FROM tas.sy_F07350 AS a
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

	SELECT * FROM tas.fnCheckIfPayrollProcessing()	

*/