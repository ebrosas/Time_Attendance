/*************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.LeaveUnentitledDayoffLog
*	Description: This table stores all incorrect attendance records wherein the dayoff were flagged as absent
*
*	Date			Author		Rev.#		Comments
*	14/09/2020		Ervin		1.0			Created
************************************************************************************************************************************************************************************/

--IF OBJECT_ID ('tas.LeaveUnentitledDayoffLog') IS NOT NULL
--BEGIN	

--	DROP TABLE tas.LeaveUnentitledDayoffLog
--END

	CREATE TABLE tas.LeaveUnentitledDayoffLog
	(
		LogID BIGINT IDENTITY(1,1) NOT NULL,	
		CostCenter VARCHAR(12) NOT NULL,
		EmpNo INT NOT NULL,
		DT DATETIME NOT NULL,
		LeaveNo FLOAT NOT NULL,
		LeaveStartDate DATETIME NOT NULL,
		LeaveEndDate DATETIME NOT NULL,	
		LeaveResumeDate DATETIME NOT NULL,
		LeaveDuration FLOAT NULL,
		NoOfWeekends FLOAT NULL,
		LeaveBalance FLOAT NULL,
		ApprovalFlag VARCHAR(10) NULL,
		LeaveStatus VARCHAR(100) NULL,
		DayBeforeLeaveStartDateDesc VARCHAR(100) NULL,
		PrevStartDateShiftCode VARCHAR(10),
		HolidayDate DATETIME NULL,
		HolidayCode VARCHAR(10) NULL,
		IsProcessed BIT NULL,
		CoverLeaveNo FLOAT NULL,
		CoverLeaveDuration FLOAT NULL,
		CoverLeaveStartDate DATETIME NULL,
		CoverLeaveResumeDate DATETIME NULL,
		CreatedDate DATETIME DEFAULT GETDATE(),		
		CreatedByEmpNo INT NULL,
		CreatedByUserID VARCHAR(50) NULL

		CONSTRAINT [PK_LeaveUnentitledDayoffLog] PRIMARY KEY CLUSTERED 
		(
			EmpNo,
			DT
		)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

GO

/*	Debug:

	SELECT * FROM tas.LeaveUnentitledDayoffLog

*/
