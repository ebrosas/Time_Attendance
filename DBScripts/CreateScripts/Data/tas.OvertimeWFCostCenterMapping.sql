DECLARE	@isCommitTrans		BIT,
		@groupCode			VARCHAR(3),
		@costCenter			VARCHAR(12),
		@wfModuleCode		VARCHAR(10),
		@isWFByCostCenter	BIT,
		@isActive			BIT,
		@createdByEmpNo		INT,
		@createdByUserID	VARCHAR(50)

	SET		@isCommitTrans		= 1

	--SELECT	@groupCode			= 'PRD',
	--		@costCenter			= NULL,
	--		@wfModuleCode		= 'OTPRODUCTN',
	--		@isWFByCostCenter	= 0,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	--SELECT	@groupCode			= 'QC',
	--		@costCenter			= NULL,
	--		@wfModuleCode		= 'OTQCLAB',
	--		@isWFByCostCenter	= 0,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	--SELECT	@groupCode			= 'ENG',
	--		@costCenter			= NULL,
	--		@wfModuleCode		= 'OTENG',
	--		@isWFByCostCenter	= 0,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	--SELECT	@groupCode			= 'S&M',
	--		@costCenter			= NULL,
	--		@wfModuleCode		= 'OTS&M',
	--		@isWFByCostCenter	= 0,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	--SELECT	@groupCode			= 'ADM',
	--		@costCenter			= '7200',
	--		@wfModuleCode		= 'OTADM7200',
	--		@isWFByCostCenter	= 1,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	--SELECT	@groupCode			= 'ADM',
	--		@costCenter			= '7250',
	--		@wfModuleCode		= 'OTADM7250',
	--		@isWFByCostCenter	= 1,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	--SELECT	@groupCode			= 'ADM',
	--		@costCenter			= '7300',
	--		@wfModuleCode		= 'OTADM7300',
	--		@isWFByCostCenter	= 1,
	--		@isActive			= 1,
	--		@createdByEmpNo		= 10003632,
	--		@createdByUserID	= 'ervin'

	SELECT	@groupCode			= 'ADM',
			@costCenter			= '7560',
			@wfModuleCode		= 'OTADM7500',
			@isWFByCostCenter	= 1,
			@isActive			= 1,
			@createdByEmpNo		= 10003632,
			@createdByUserID	= 'ervin'
		
	IF NOT EXISTS
    (
		SELECT a.MappingID FROM tas.OvertimeWFCostCenterMapping a
		WHERE RTRIM(a.WFModuleCode) = @wfModuleCode
	)
	BEGIN 

		BEGIN TRAN T1

		INSERT INTO tas.OvertimeWFCostCenterMapping
		(
			GroupCode,
			CostCenter,
			WFModuleCode,
			IsWFByCostCenter,
			IsActive,
			CreatedDate,
			CreatedByEmpNo,
			CreatedByUserID
		)
		VALUES
		(
			@groupCode,
			@costCenter,
			@wfModuleCode,
			@isWFByCostCenter,
			@isActive,
			GETDATE(),
			@createdByEmpNo,
			@createdByUserID
		)

		--Get all the records
		SELECT * FROM tas.OvertimeWFCostCenterMapping a
		ORDER BY a.MappingID
	     
		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 
	ELSE
    BEGIN

		SELECT 'Record already exists!' AS ErrorDescription

		SELECT * FROM tas.OvertimeWFCostCenterMapping a
		WHERE RTRIM(a.GroupCode) = @groupCode
			AND RTRIM(a.WFModuleCode) = @wfModuleCode
    END 


