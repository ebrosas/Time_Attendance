/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.IDCardRegistry
*	Description: This table stores the contractor swipe data
*
*	Date			Author		Rev.#		Comments
*	07/10/2021		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.IDCardRegistry') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.IDCardRegistry
--END

	CREATE TABLE tas.IDCardRegistry
	(
		RegistryID INT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		EmpName VARCHAR(100) NULL,
		Position VARCHAR(50) NULL,
		CustomCostCenter VARCHAR(100) NULL,
		CPRNo VARCHAR(30) NULL,
		BloodGroup VARCHAR(10) NULL,
		IsContractor BIT NOT NULL,
		EmpPhoto VARBINARY(MAX) NULL,
		Base64Photo VARCHAR(MAX) NULL,
		ImageFileName VARCHAR(100) NULL,
		ImageFileExt VARCHAR(5) NULL,		
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NOT NULL,
		CreatedByUser VARCHAR(50) NULL,
		LastUpdatedDate DATETIME NULL,		
		LastUpdatedByEmpNo INT NULL,
		LastUpdatedByUser VARCHAR(50) NULL

		CONSTRAINT [PK_IDCardRegistry] PRIMARY KEY CLUSTERED 
		(
			EmpNo
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.IDCardRegistry a WITH (NOLOCK)

*/
