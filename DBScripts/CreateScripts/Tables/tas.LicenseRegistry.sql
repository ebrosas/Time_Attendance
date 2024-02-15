/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.LicenseRegistry
*	Description: This table stores the contractor swipe data
*
*	Date			Author		Rev.#		Comments
*	13/09/2021		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.LicenseRegistry') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.LicenseRegistry
--END

	CREATE TABLE tas.LicenseRegistry
	(
		RegistryID INT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		LicenseNo VARCHAR(20) NOT NULL,
		LicenseTypeCode VARCHAR(10) NOT NULL,
		LicenseTypeDesc VARCHAR(50) NOT NULL,
		IssuingAuthority VARCHAR(200) NULL,
		IssuedDate DATETIME NOT NULL,
		ExpiryDate DATETIME NOT NULL,
		Remarks VARCHAR(300) NULL,
		LicenseGUID VARCHAR(50) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NOT NULL,
		CreatedByEmpName VARCHAR(100) NULL,
		CreatedByUser VARCHAR(50) NULL,
		LastUpdatedDate DATETIME NULL,		
		LastUpdatedByEmpNo INT NULL,
		LastUpdatedByEmpName VARCHAR(100) NULL,
		LastUpdatedByUser VARCHAR(50) NULL

		CONSTRAINT [PK_LicenseRegistry] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			LicenseNo,
			LicenseTypeCode,
			IssuedDate,
			ExpiryDate
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.LicenseRegistry a WITH (NOLOCK)

*/
