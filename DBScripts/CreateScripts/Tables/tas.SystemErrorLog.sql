/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.SystemErrorLog
*	Description: This table stores system error logs
*
*	Date			Author		Rev.#		Comments
*	04/10/2017		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.SystemErrorLog') IS NOT NULL
BEGIN	

	DROP TABLE tas.SystemErrorLog
END

	CREATE TABLE tas.SystemErrorLog
	(
		LogID INT IDENTITY(1,1) NOT NULL,	
		RequisitionNo BIGINT NULL,
		ErrorCode TINYINT NOT NULL,
		ErrorDscription VARCHAR(2000) NOT NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUserID VARCHAR(50) NULL
		
		CONSTRAINT [PK_SystemErrorLog] PRIMARY KEY CLUSTERED 
		(
			LogID,
			ErrorCode,
			CreatedDate
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
