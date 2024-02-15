/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.OvertimeRequest
*	Description: This table stores information about the overtime approval request
*
*	Date			Author		Rev.#		Comments
*	24/07/2017		Ervin		1.0			Created
*	06/09/2017		Ervin		1.1			Set "SubmittedDate" as mandatory and added it in the primary keys. Added "OTStartTime_Orig" and "OTEndTime_Orig" fields.
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.OvertimeRequest') IS NOT NULL
BEGIN	

	DROP TABLE tas.OvertimeRequest
END

	CREATE TABLE tas.OvertimeRequest
	(
		OTRequestNo BIGINT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		TS_AutoID INT NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		OTStartTime DATETIME NULL,
		OTEndTime DATETIME NULL,
		OTType VARCHAR(10) NULL,
		CorrectionCode VARCHAR(10) NULL,
		MealVoucherEligibility VARCHAR(10) NULL,
		StatusID INT NULL,
		StatusCode VARCHAR(10) NULL,
		StatusDesc VARCHAR(50) NULL,
		StatusHandlingCode VARCHAR(50) NULL,
		IsClosed BIT NULL,
		ClosedDate DATETIME NULL,
		IsSubmittedForApproval BIT NULL,
		SubmittedDate DATETIME NOT NULL,
		CurrentlyAssignedEmpNo INT NULL,
		CurrentlyAssignedEmpName VARCHAR(50) NULL,
		CurrentlyAssignedEmpEmail VARCHAR(50) NULL,
		ServiceProviderTypeCode VARCHAR(10) NULL,
		DistListCode VARCHAR(10) NULL,	
		IsModifiedByHR BIT NULL,	
		OTApproved VARCHAR(1) NULL,
		OTReason VARCHAR(10) NULL,
		OTComment VARCHAR(1000) NULL,
		OTStartTime_Orig DATETIME NULL,
		OTEndTime_Orig DATETIME NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NOT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		CreatedByEmpName varchar(100) NULL,
		CreatedByEmail VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL,
		LastUpdateEmpName varchar(100) NULL
		
		CONSTRAINT [PK_OvertimeRequest] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			DT,
			TS_AutoID,
			SubmittedDate
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
