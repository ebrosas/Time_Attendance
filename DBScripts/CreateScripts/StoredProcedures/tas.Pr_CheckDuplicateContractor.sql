/******************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ContractorRegistry_CRUD
*	Description: This stored procedure is used to perform CRUD operations in "ContractorRegistry" table
*
*	Date			Author		Revision No.	Comments:
*	21/09/2021		Ervin		1.0				Created
*******************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_CheckDuplicateContractor
(	
	@idNumber			VARCHAR(20),
	@contractStartDate	DATETIME,
	@contractEndDate	DATETIME
)
AS	
BEGIN

	SET NOCOUNT ON 
		
	--Check existing records
	SELECT a.ContractorNo, a.IDNumber, a.ContractStartDate, a.ContractEndDate, a.FirstName, a.LastName
	FROM tas.ContractorRegistry a WITH (NOLOCK)
	WHERE UPPER(RTRIM(IDNumber)) = @idNumber
		AND 
		(
			@contractStartDate BETWEEN a.ContractStartDate AND a.ContractEndDate
			OR @contractEndDate BETWEEN a.ContractStartDate AND a.ContractEndDate
		)
	ORDER BY a.ContractorNo

END 

/*	Debug:

	EXEC tas.Pr_CheckDuplicateContractor '781202647', '09/01/2021', '12/31/2021'

*/