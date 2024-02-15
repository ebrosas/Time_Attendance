/***********************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.External_DSX_evnlog
*	Description: Get the swipe data from the swipe system
*
*	Date			Author		Revision No.		Comments
*	18/07/2006		Khuzema		1.0					Created
*	21/12/2016		Ervin		1.1					Refactored the code
*************************************************************************************************************************************************************************/

ALTER  VIEW tas.vu_tmp_CHK_JDE_summry 
AS

	SELECT	a.PDBA,
			a.PDBA_Name, 
			ISNULL(a.TAS_cnt, 0) AS TAS_cnt,
			ISNULL(Diff_TAS, 0) AS Diff_TAS,
			ISNULL(JDE_cnt, 0) AS JDE_cnt,
			ISNULL(Diff_JDE, 0) AS Diff_JDE,
			ISNULL(Diff, 0) AS Diff 
	FROM tas.tmp_CHK_JDE_summry a

GO


