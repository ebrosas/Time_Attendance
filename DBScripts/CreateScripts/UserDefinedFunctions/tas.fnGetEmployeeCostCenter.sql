/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetEmployeeCostCenter
*	Description: Get the employee's assigned cost center
*
*	Date			Author		Rev. #		Comments:
*	03/11/2016		Ervin		1.0			Created
*************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetEmployeeCostCenter
(
	@empNo	INT 
)
RETURNS VARCHAR(12)
AS
BEGIN      

	DECLARE @costCenter VARCHAR(12) 

	SELECT @costCenter = RTRIM(a.BusinessUnit) 
	FROM tas.Master_Employee_JDE_View_V2 a
	WHERE a.EmpNo = @empNo

	RETURN @costCenter 
END


/*	Debugging:

	SELECT tas.fnGetEmployeeCostCenter(10003632)

*/
