/**************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_SystemErrorLog_CRUD
*	Description: Performs insert, update, and delete operations against "tas.SystemErrorLog" table
*
*	Date:			Author:		Rev.#:		Comments:
*	04/10/2017		Ervin		1.0			Created
**************************************************************************************************************************************/


ALTER PROCEDURE tas.Pr_SystemErrorLog_CRUD
(
	@actionType			TINYINT,	
	@logID				INT,
	@requisitionNo		BIGINT,
	@errorCode			TINYINT,
	@errorDscription	VARCHAR(2000),
	@userEmpNo			INT,
	@userID				VARCHAR(50)	
)
AS	
	
	DECLARE @newID				int,
			@rowsAffected		int

	--Initialize variables
	SELECT	@newID			= 0,
			@rowsAffected	= 0

	IF @actionType = 1		--Insert new record
	BEGIN
		
		INSERT INTO tas.SystemErrorLog
		(
			RequisitionNo,
			ErrorCode,
			ErrorDscription,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID
		)
		VALUES
		(
			@requisitionNo,
			@errorCode,
			@errorDscription,
			GETDATE(), 
			@UserEmpNo,
			@UserID
		)
		
		--Get the new ID
		SET @newID = @@identity
	END

	ELSE IF @actionType = 2  --Update existing record
	BEGIN

		UPDATE tas.SystemErrorLog
		SET	RequisitionNo = @requisitionNo,
			ErrorCode = @errorCode,
			ErrorDscription = @errorDscription
		WHERE LogID = @logID

		SELECT @rowsAffected = @@rowcount 
	END

	ELSE IF @actionType = 3  --Delete record by unique ID
	BEGIN

		DELETE FROM tas.SystemErrorLog
		WHERE LogID = @logID

		SELECT @rowsAffected = @@rowcount
	END

	--Return the variables as resultset
	SELECT	@newID AS NewIdentityID,
			@rowsAffected AS RowsAffected
GO


/*	Debugging:

	EXEC secuser.Pr_GetLeaveRequisitionDetail 106748

	SELECT LeaveDuration, * FROM tas.SystemErrorLog ORDER BY LeaveReqDetailID
	
*/



