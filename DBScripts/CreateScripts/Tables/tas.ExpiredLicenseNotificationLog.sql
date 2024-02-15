/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ExpiredLicenseNotificationLog
*	Description: This table will store information about the expired license notifications
*
*	Date			Author		Rev.#		Comments
*	22/01/2023		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.ExpiredLicenseNotificationLog') IS NOT NULL
BEGIN	

	DROP TABLE tas.ExpiredLicenseNotificationLog
END

	CREATE TABLE tas.ExpiredLicenseNotificationLog
	(
		LogID INT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		SupervisorNo INT NOT NULL,
		CostCenter VARCHAR(12) NOT NULL,
		LicenseTypeCode VARCHAR(10) NOT NULL,
		LicenseTypeDesc VARCHAR(50) NULL,
		IssuedDate DATETIME NOT NULL,
		ExpiryDate DATETIME NOT NULL, 
		NotificationCounter TINYINT NULL,
		NotificationType TINYINT NULL,				--(Notes: 0 = Already expired license, 1 = About to expire in 21 days)
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_ExpiredLicenseNotificationLog] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			SupervisorNo,
			LicenseTypeCode,
			IssuedDate,
			ExpiryDate 
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
