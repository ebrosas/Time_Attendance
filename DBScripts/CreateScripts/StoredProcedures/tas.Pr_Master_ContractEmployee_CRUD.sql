/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_Master_ContractEmployee_CRUD
*	Description: Performs insert, update, and delete operations against "tas.Master_ContractEmployee" table
*
*	Date:			Author:		Rev.#:		Comments:
*	04/01/2017		Ervin		1.0			Created
**************************************************************************************************************************************/


ALTER PROCEDURE tas.Pr_Master_ContractEmployee_CRUD
(
	@actionType			INT,	
	@autoID				INT,
	@empNo				INT,
	@contractorEmpName	VARCHAR(40),
	@groupCode			VARCHAR(10),
	@contractorNumber	FLOAT,
	@dateJoined			DATETIME,
	@dateResigned		DATETIME,
	@shiftPatCode		VARCHAR(2),
	@shiftPointer		INT,
	@religionCode		VARCHAR(10),
	@lastUpdateUser		VARCHAR(50)
)
AS	
	
	DECLARE @newID				int,
			@rowsAffected		int

	--Initialize variables
	SELECT	@newID			= 0,
			@rowsAffected	= 0

	IF @actionType = 1		--Insert new record
	BEGIN
		
		INSERT INTO tas.Master_ContractEmployee
		(
			EmpNo,
			ContractorEmpName,
			GroupCode,
			ContractorNumber,
			DateJoined,
			DateResigned,
			ShiftPatCode,
			ShiftPointer,
			ReligionCode,
			LastUpdateUser,
			LastUpdateTime
		)
		VALUES
		(
			@empNo,
			@contractorEmpName,
			@groupCode,
			@contractorNumber,
			@dateJoined,
			@dateResigned,
			@shiftPatCode,
			@shiftPointer,
			@religionCode,
			@lastUpdateUser,
			GETDATE()
		)
		
		--Get the new ID
		SET @newID = @@identity
	END

	ELSE IF @actionType = 2		--Update existing record
	BEGIN

		UPDATE tas.Master_ContractEmployee
		SET	ContractorEmpName = @contractorEmpName,
			GroupCode = @groupCode,
			ContractorNumber = @contractorNumber,
			DateJoined = @dateJoined,
			DateResigned = @dateResigned,
			ShiftPatCode = @shiftPatCode,
			ShiftPointer = @shiftPointer,
			ReligionCode = @religionCode,
			LastUpdateUser = @lastUpdateUser,
			LastUpdateTime = GETDATE()
		WHERE AutoID = @autoID

		SELECT @rowsAffected = @@rowcount 
	END

	ELSE IF @actionType = 3		--Delete existing record 
	BEGIN

		DELETE FROM tas.Master_ContractEmployee
		WHERE AutoID = @autoID

		SELECT @rowsAffected = @@rowcount
	END

	ELSE IF (@actionType = 4)  --Delete record by Emp. No.
	BEGIN

		DELETE FROM tas.Master_ContractEmployee
		WHERE EmpNo = @empNo

		SELECT @rowsAffected = @@rowcount
	END

	--Return the variables as resultset
	SELECT	@newID AS NewIdentityID,
			@rowsAffected AS RowsAffected
GO


/*	Debugging:

	EXEC tas.Pr_GetContractorShiftPattern
	
*/



