/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ContractorRegistry
*	Description: This table stores the contractor swipe data
*
*	Date			Author		Rev.#		Comments
*	05/09/2021		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.ContractorRegistry') IS NOT NULL
BEGIN	

	DROP TABLE tas.ContractorRegistry
END

	CREATE TABLE tas.ContractorRegistry
	(
		RegistryID INT IDENTITY(1,1) NOT NULL,	
		ContractorNo INT NOT NULL,
		RegistrationDate DATETIME DEFAULT GETDATE(),	
		IDNumber VARCHAR(20) NOT NULL,
		IDType TINYINT NOT NULL,	--(Notes: 0 = CPR, 1 = Passport)	
		FirstName VARCHAR(30) NOT NULL,
		LastName VARCHAR(30) NOT NULL,
		CompanyName VARCHAR(50) NOT NULL,
		CompanyID INT NULL,
		CompanyCRNo VARCHAR(20) NULL,
		PurchaseOrderNo FLOAT NULL,
		JobTitle VARCHAR(50) NOT NULL,
		MobileNo VARCHAR(20) NULL,
		VisitedCostCenter VARCHAR(12) NOT NULL,
		SupervisorEmpNo INT NOT NULL,
		SupervisorEmpName VARCHAR(100) NULL,
		PurposeOfVisit VARCHAR(300) NULL,
		ContractStartDate DATETIME NOT NULL,
		ContractEndDate DATETIME NOT NULL,
		BloodGroup VARCHAR(10) NULL,
		Remarks VARCHAR(500) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NOT NULL,
		CreatedByUser VARCHAR(50) NULL,
		LastUpdatedDate DATETIME NULL,		
		LastUpdatedByEmpNo INT NULL,
		LastUpdatedByUser VARCHAR(50) NULL

		CONSTRAINT [PK_ContractorRegistry] PRIMARY KEY CLUSTERED 
		(
			IDNumber,
			ContractStartDate,
			ContractEndDate
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.ContractorRegistry a WITH (NOLOCK)

*/
