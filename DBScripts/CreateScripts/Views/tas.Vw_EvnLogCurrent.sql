/********************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_EvnLogCurrent
*	Description: This view fetches the swipe records of all people in GARMCO per given day
*
*	Date:			Author:		Rev.#:		Comments:
*	17/04/2019		Ervin		1.0			Created
**********************************************************************************************************************************************************************************************/

ALTER VIEW tas.Vw_EvnLogCurrent
AS
	
	SELECT * FROM tas.sy_EvnLog a WITH (NOLOCK)
	WHERE CONVERT(DATETIME, CONVERT(VARCHAR, a.TimeDate, 12)) BETWEEN DATEADD(DAY, -1, CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))) AND CONVERT(DATETIME, CONVERT(VARCHAR, GETDATE(), 12))

GO


/*	Debug:

	SELECT * FROM tas.Vw_EvnLogCurrent a
	WHERE a.[Event] = 8				--(Note: 8 means successful swipe)
		AND a.Dev NOT IN (8, 9)		--Rev. #1.5 (Note: 8 = GARMCO Main gate ALT-Turnstile; 9 = GARMCO Main gate ALT-Turnstile)   

*/


