/***********************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.External_DSX_evnlog
*	Description: Get the swipe data from the swipe system
*
*	Date			Author		Revision No.		Comments
*	18/07/2006		Khuzema		1.0					Created
*	19/10/2016		EBrosas		1.1					Added filter condition to exclude records wherein the value of Dev is either 8 or 9
*************************************************************************************************************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW tas.External_DSX_evnlog
AS

	SELECT * FROM GRMACC.AcsLog.dbo.EvnLog a
	WHERE a.Dev NOT IN (8, 9)	--(Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)                 

GO

/*	Debug:

	SELECT * FROM GRMACC.AcsLog.dbo.EvnLog a
	WHERE a.Dev IN (8, 9)
	ORDER BY TimeDate DESC

	SELECT * FROM tas.External_DSX_evnlog a
	WHERE a.FName LIKE '3409%'
		AND CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) = '09/15/2016'
*/


