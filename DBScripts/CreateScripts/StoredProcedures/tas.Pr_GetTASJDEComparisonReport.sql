/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTASJDEComparisonReport 
*	Description: Get data for the TAS and JDE Comparison Report 
*
*	Date:			Author:		Revision #:		Comments:
*	01/01/2017		Ervin		1.0				Created
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTASJDEComparisonReport 
AS	

	EXEC tas.Payroll_Validation_ControlTotals_for_JDE  

	SELECT	a.PDBA,
			a.PDBA_Name,
			a.TAS_cnt,
			a.Diff_TAS,
			a.JDE_cnt,
			a.Diff_JDE,
			a.Diff
	FROM tas.vu_tmp_CHK_JDE_summry a

GO


/*	Debug:

	EXEC tas.Pr_GetTASJDEComparisonReport  

*/

