/******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.ManhourHistory
*	Description: This table stores information about the total man-hours of all employees and contractors within specific period
*
*	Date			Author		Rev.#		Comments
*	15/06/2016		Ervin		1.0			Created
*****************************************************************************************************************************************/

IF OBJECT_ID ('tas.ManhourHistory') IS NOT NULL
BEGIN	

	DROP TABLE tas.ManhourHistory
END

	CREATE TABLE tas.ManhourHistory
	(
		LogID bigint IDENTITY(1,1) NOT NULL,
		StartDate DATETIME NOT NULL,
		EndDate DATETIME NOT NULL,
		CurrentTotalHour FLOAT NOT NULL,
		PreviousTotalHour FLOAT NOT NULL,
		IsProcessed BIT NULL,
		CreatedDate datetime DEFAULT GETDATE(),		
		CreatedByEmpNo int NOT NULL,
		CreatedByUser varchar(50) NULL,
		LastUpdateTime datetime NULL,
		LastUpdateEmpNo int NULL,
		LastUpdateUser varchar(50) NULL
		
		CONSTRAINT [PK_ManhourHistory] PRIMARY KEY CLUSTERED 
		(
			StartDate, 
			EndDate
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]


GO
