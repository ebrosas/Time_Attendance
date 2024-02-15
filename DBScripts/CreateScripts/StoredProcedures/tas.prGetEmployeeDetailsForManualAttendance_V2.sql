/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.prGetEmployeeDetailsForManualAttendance_V2
*	Description: Get the last swipe status of an employee or contractor
*
*	Date			Author		Revision No.	Comments:
*	31/08/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.prGetEmployeeDetailsForManualAttendance_V2
(  	             
	@empNo	INT
)
AS                
BEGIN  
	
	DECLARE	@autoID			INT,
			@dtIN			DATETIME,
			@checkInDate	DATETIME,
			@dtOUT			DATETIME,
			@checkOutDate	DATETIME,
			@timeIN			VARCHAR(5),
			@timeOut		VARCHAR(5),
			@swipeStatus	VARCHAR(20),
			@recordCount	INT,
			@currentDate	DATETIME 
	
	CREATE TABLE #Attendance
	(
		AutoID			INTEGER,
		EmpNo			INTEGER,
		CheckInDate		DATETIME,
		CheckOutDate	DATETIME
	)
		
	--Set the current date
	SET @currentDate = getdate()
	
	--Get the last IN datetime
	SELECT TOP 1 
		@dtIN = dtIN, 
		@timeIN = TimeIN, 
		@dtOUT = dtOUT, 
		@timeOut = [TimeOut], 
		@autoID = AutoID
	FROM tas.Tran_ManualAttendance a
	WHERE EmpNo = @empNo
	ORDER BY AutoID DESC
	
	IF @timeIN IS NOT NULL  --Swipe IN Date
	BEGIN
    
		SELECT	@checkInDate = DateAdd(hh, CONVERT(int, substring(@timeIN, 1, 2)), @dtIN),
				@checkInDate = DateAdd(minute, CONVERT(int, substring(@timeIN, 3, 2)), @checkInDate)
	End
	
	IF @timeOut IS NOT NULL --Swipe Out Date
	BEGIN
    
		SELECT	@checkOutDate = DateAdd(hh, CONVERT(INT, SUBSTRING(@timeOut, 1, 2)), @dtOUT),
				@checkOutDate = DateAdd(MINUTE, CONVERT(INT, SUBSTRING(@timeOut, 3, 2)), @checkOutDate)
	End
	
	--Add the record to the temp table
	Insert Into #Attendance 
	(
		AutoID, 
		EmpNo, 
		CheckInDate, 
		CheckOutDate
	) 
	VALUES 
	(
		@autoID,
		@empNo,
		@checkInDate,
		@checkOutDate
	)
	
	--Initialize
	SELECT	@autoID = 0,
			@dtIN = NULL,
			@dtOUT = NULL
	
	--Get the last activity within 24hrs
	SELECT	@autoID	= AutoID,
			@dtIN	= CheckInDate, 
			@dtOUT	= CheckOutDate
	FROM #Attendance
	WHERE EmpNo = @empNo
		AND 
		(
			CheckInDate BETWEEN DATEADD(hh, -24, @currentDate) AND @currentDate  
			OR 
			CheckOutDate BETWEEN DATEADD(hh, -24, @currentDate) AND @currentDate
		)
	
	--Check for the swipe status
	IF @autoID = 0 
		SET @swipeStatus = 'CheckIN'
	ELSE IF @dtOUT IS NOT NULL AND @autoID > 0
		SET @swipeStatus = 'CheckOUT'
	ELSE 
		SET @swipeStatus = 'CheckIN'
		
	--Get the employee information	
	--IF EXISTS
 --   (
	--	SELECT EmpNo FROM tas.Master_Employee_JDE_View
	--	WHERE EmpNo = @empNo
	--)
	--BEGIN
    
		SELECT	DISTINCT
				a.EmpNo,
				a.EmpName,
				a.BusinessUnit AS CostCenter,
				c.BusinessUnitName AS CostCenterName,
				b.JobTitle AS Position,
				@autoID AS AutoID,
				@dtIN AS DateIn,
				@dtOUT AS DateOut,
				@swipeStatus AS SwipeStatus
		FROM tas.Master_Employee_JDE_View a
			LEFT JOIN tas.Master_JobTitles_JDE b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
		WHERE a.EmpNo = @empNo
	--END
    
	--ELSE
 --   BEGIN

	--	SELECT	DISTINCT
	--			a.EmpNo,
	--			a.EmpName,
	--			a.CostCenter,
	--			a.CostCenterName,
	--			b.JobTitle AS Position,
	--			@autoID AS AutoID,
	--			@dtIN AS DateIn,
	--			@dtOUT AS DateOut,
	--			@swipeStatus AS SwipeStatus
	--	FROM tas.Vw_EmpContractorIDBadgeInfo a
	--		LEFT JOIN tas.Master_JobTitles_JDE b ON a.EmpNo = b.EmpNo
	--	WHERE a.EmpNo = @empNo
 --   END 
	
	
	DROP TABLE #Attendance
END
GO


/*	Debugging:

	EXEC tas.prGetEmployeeDetailsForManualAttendance_V2 10003632
	EXEC tas.prGetEmployeeDetailsForManualAttendance_V2 53599
	EXEC tas.prGetEmployeeDetailsForManualAttendance_V2 10002149


	SELECT * FROM tas.Tran_ManualAttendance a
	WHERE a.EmpNo = 10003632
	ORDER BY a.AutoID DESC	

	BEGIN TRAN T1
	
	DELETE FROM tas.Tran_ManualAttendance
	WHERE AutoID = 24821

	COMMIT TRAN T1

*/

