/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.VisitorPassLog
*	Description: This table stores the visitor's pass log history
*
*	Date			Author		Rev.#		Comments
*	16/03/2015		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.VisitorPassLog') IS NOT NULL
BEGIN	

	DROP TABLE tas.VisitorPassLog
END

	CREATE TABLE tas.VisitorPassLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		VisitorName VARCHAR(100) NOT NULL,
		IDNumber VARCHAR(50) NOT NULL,
		VisitorCardNo VARCHAR(20) NOT NULL,
		VisitEmpNo INT NOT NULL, 	 
		VisitDate DATETIME NOT NULL,
		VisitTimeIn DATETIME NULL,
		VisitTimeOut DATETIME NULL,
		Remarks VARCHAR(3000) NULL,
		IsBlock BIT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUserID VARCHAR(50) NULL,
		CreatedByEmpName VARCHAR(100) NULL,
		CreatedByEmpEmail VARCHAR(50) NULL,
		LastUpdateTime DATETIME NULL,
		LastUpdateEmpNo INT NULL,
		LastUpdateUserID VARCHAR(50) NULL,
		LastUpdateEmpName VARCHAR(100) NULL,
		LastUpdateEmpEmail VARCHAR(50) NULL
		
		CONSTRAINT [PK_VisitorPassLog] PRIMARY KEY CLUSTERED 
		(
			IDNumber,
			VisitorCardNo,
			VisitDate
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
