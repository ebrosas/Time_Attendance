/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.VisitorSwipeLog
*	Description: This table will store the manual swipe reords of Visitors
*
*	Date			Author		Rev.#		Comments
*	08/08/2016		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

IF OBJECT_ID ('tas.VisitorSwipeLog') IS NOT NULL
BEGIN	

	DROP TABLE tas.VisitorSwipeLog
END

	CREATE TABLE tas.VisitorSwipeLog
	(
		SwipeID BIGINT IDENTITY(1,1) NOT NULL,	
		LogID BIGINT NOT NULL,
		SwipeDate DATETIME NOT NULL,
		SwipeTime DATETIME NOT NULL,
		SwipeTypeCode VARCHAR(20) NOT NULL, 
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
		
		CONSTRAINT [PK_VisitorSwipeLog] PRIMARY KEY CLUSTERED 
		(
			LogID,
			SwipeDate,
			SwipeTime
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
