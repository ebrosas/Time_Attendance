/************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnOTDistributionGroupMembers
*	Description: Get the distribution group members
*
*	Date:			Author:		Rev.#:		Comments:
*	06/02/2017		Ervin		1.0			Created
**************************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnOTDistributionGroupMembers
(
	@otRequestNo			BIGINT,
	@workflowTransactionID	BIGINT	
)
RETURNS VARCHAR(1000)
AS
BEGIN

	DECLARE	@empNo		INT,
			@empName	VARCHAR(100),
			@result		VARCHAR(1000)

	--Initialize variables
	SELECT	@empNo		= 0,
			@empName	= '',
			@result		= ''

	--Validate parameters
	IF ISNULL(@workflowTransactionID, 0) = 0
		SET @workflowTransactionID = NULL

	DECLARE DistributionMemberCursor CURSOR READ_ONLY FOR
	SELECT DISTINCT 
		a.EmpNo, a.EmpName
	FROM tas.OvertimeDistributionMember a
	WHERE a.OTRequestNo = @otRequestNo
		AND (a.workflowTransactionID = @workflowTransactionID OR @workflowTransactionID IS NULL)

	OPEN DistributionMemberCursor
	FETCH NEXT FROM DistributionMemberCursor
	INTO @empNo, @empName

	WHILE @@FETCH_STATUS = 0
	BEGIN

		IF LEN(@result) = 0
			SET @result = @result + '(' + CONVERT(VARCHAR(8), @empNo) + ') ' + RTRIM(@empName)
		ELSE
			SET @result = @result + ', (' + CONVERT(VARCHAR(8), @empNo) + ') ' + RTRIM(@empName)

		--Retrieve next record
		FETCH NEXT FROM DistributionMemberCursor
		INTO @empNo, @empName
	END 

	--Close and deallocate
	CLOSE DistributionMemberCursor
	DEALLOCATE DistributionMemberCursor

	RETURN RTRIM(@result)

END

/*	Testing:

PARAMETERS:
	@otRequestNo			BIGINT,
	@workflowTransactionID	BIGINT	

	SELECT tas.fnOTDistributionGroupMembers(2, 8)

*/
