/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ContractorSwipeLog
*	Description: This table stores the contractor swipe data
*
*	Date			Author		Rev.#		Comments
*	27/06/2021		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.ContractorSwipeLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.ContractorSwipeLog
--END

	CREATE TABLE tas.ContractorSwipeLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		EmpNo INT NOT NULL,
		SwipeDate DATETIME NOT NULL,
		SwipeTime DATETIME NOT NULL,
		LocationName VARCHAR(50) NULL,
		ReaderName VARCHAR(50) NULL,
		SwipeType VARCHAR(5) NULL,
		LocationCode INT NULL,
		ReaderNo INT NULL,
		ContractorName VARCHAR(200),
		EventCode INT NULL, 
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL

		CONSTRAINT [PK_ContractorSwipeLog] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			SwipeDate,
			SwipeTime
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.ContractorSwipeLog a WITH (NOLOCK)
	ORDER BY a.SwipeDate DESC, a.SwipeTime

*/
