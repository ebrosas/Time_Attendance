/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTotalManhour
*	Description: This stored procedure is used to calculate the total manhours consumed by all employees and contractors based on specific period 
*
*	Date			Author		Rev. #		Comments:
*	15/06/2016		Ervin		1.0			Created
*	02/08/2016		Ervin		1.1			Refactored the code to check for LTI record when calculating the total man-hour
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTotalManhour
(
	@actionType	INT
)
AS

	IF @actionType = 0			--Get Man-hour grand total
	BEGIN
    
		DECLARE	@timesheetLastProcessed DATETIME
		SELECT @timesheetLastProcessed = DT_SwipeLastProcessed FROM tas.System_Values

		--Check for the last LTI record
		IF EXISTS
        (
			SELECT a.LogID FROM tas.ManhourHistory a
			WHERE a.IsLTI = 1
		)
		BEGIN

			IF EXISTS
            (
				SELECT	a.LogID
				FROM tas.ManhourHistory a
				WHERE ISNULL(a.IsProcessed , 0) = 0
					AND a.LogID > (SELECT TOP 1 LogID FROM tas.ManhourHistory WHERE IsLTI = 1 ORDER BY LogID DESC)
			)
			BEGIN
            
				SELECT	SUM(a.CurrentTotalHour) AS TotalManHours,
						@timesheetLastProcessed AS TimesheetLastProcessed
				FROM tas.ManhourHistory a
				WHERE ISNULL(a.IsProcessed , 0) = 0
					AND a.LogID > (SELECT TOP 1 LogID FROM tas.ManhourHistory WHERE IsLTI = 1 ORDER BY LogID DESC)
			END
            ELSE
            BEGIN
				
				SELECT	0 AS TotalManHours,
						@timesheetLastProcessed AS TimesheetLastProcessed
            END 
		END 
		
		ELSE
        BEGIN
        
			SELECT	SUM(a.CurrentTotalHour) AS TotalManHours,
					@timesheetLastProcessed AS TimesheetLastProcessed
			FROM tas.ManhourHistory a
			WHERE ISNULL(a.IsProcessed , 0) = 0
		END 
	END 

	ELSE IF @actionType = 1		--Get Man-hour history
	BEGIN
    
		SELECT	LogID,
				StartDate,
				EndDate,
				CurrentTotalHour AS TotalHourByPeriod,
				CASE WHEN a.IsLTI = 1 THEN 0 ELSE CurrentTotalHour + PreviousTotalHour END AS GrandTotal
		FROM tas.ManhourHistory a
		ORDER BY a.LogID DESC	--a.EndDate DESC 
	END 


GO

/*	Testing:

	EXEC tas.Pr_GetTotalManhour 0
	EXEC tas.Pr_GetTotalManhour 1

*/
	

	