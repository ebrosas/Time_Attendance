/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.IDCardHistory
*	Description: This table stores the ID card history
*
*	Date			Author		Rev.#		Comments
*	11/10/2021		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.IDCardHistory') IS NOT NULL
BEGIN	

	DROP TABLE tas.IDCardHistory
END

	CREATE TABLE tas.IDCardHistory
	(
		HistoryID INT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		IsContractor BIT NOT NULL,
		CardRefNo VARCHAR(20) NOT NULL,
		Remarks VARCHAR(300) NULL,
		CardGUID VARCHAR(50) NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NOT NULL,
		CreatedByUser VARCHAR(50) NULL,
		LastUpdatedDate DATETIME NULL,		
		LastUpdatedByEmpNo INT NULL,
		LastUpdatedByUser VARCHAR(50) NULL

		CONSTRAINT [PK_IDCardHistory] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			CardRefNo
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.IDCardHistory a WITH (NOLOCK)

*/
