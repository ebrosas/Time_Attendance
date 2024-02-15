/*********************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetTASJDETransHistory 
*	Description: Get data for the TAS and JDE Comparison Report 
*
*	Date:			Author:		Revision #:		Comments:
*	02/01/2017		Ervin		1.0				Created
*	23/03/2017		Ervin		1.1				Added filter for overtime
**********************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetTASJDETransHistory 
(
	@PDBA	VARCHAR(10)
)
AS	

	IF RTRIM(@PDBA) = '010'				--Overtime
	BEGIN

		--TAS History	
		SELECT TOP 100 PERCENT
			CASE WHEN LTRIM(RTRIM(txt)) = '010 OT-' THEN '10-'
				WHEN LTRIM(RTRIM(txt)) = '010 OT+' THEN '10+'
				ELSE '10' 
			END AS PDBA,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo,
			a.ot1 AS OTFrom,
			a.ot2 AS OTTo
		FROM tas.tmp_CHK_JDE_pdba010 a
		WHERE a.txt <> '010 OT-'
		ORDER BY CASE WHEN LTRIM(RTRIM(txt)) = '010 OT-' THEN '10-' WHEN LTRIM(RTRIM(txt)) = '010 OT+' THEN '10+' ELSE '10' END

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_010			
    END 

	ELSE IF RTRIM(@PDBA) = '020'		--Shift Allowance Night
	BEGIN

		--TAS History	
		SELECT 
			'20' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba020 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_020			
    END 

	ELSE IF RTRIM(@PDBA) = '030'		--Shift Allowance Evening
	BEGIN

		--TAS History	
		SELECT 
			'30' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba030 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_030			
    END 

	ELSE IF RTRIM(@PDBA) = '095'		--Extra pay (Absences Removed)
	BEGIN

		--TAS History	
		SELECT 
			'30' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba095 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_095			
    END 

	ELSE IF RTRIM(@PDBA) = '100(A)'		--Extra pay (holiday) 
	BEGIN

		--TAS History	
		SELECT 
			'100A' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba100A a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_100A			
    END 

	ELSE IF RTRIM(@PDBA) = '100(B)'		--Extra pay (nonsal DW DilDW Sh=O)
	BEGIN

		--TAS History	
		SELECT 
			'100B' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba100B a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_100B			
    END 

	ELSE IF RTRIM(@PDBA) = '505'		--NoPay Hours
	BEGIN

		--TAS History	
		SELECT 
			'505' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba505 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_505				
    END 

	ELSE IF RTRIM(@PDBA) = '506'		--NoPay Hours adjustment
	BEGIN

		--TAS History	
		SELECT 
			'506' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba506 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_506				
    END 

	ELSE IF RTRIM(@PDBA) = '510'		--Absences
	BEGIN

		--TAS History	
		SELECT 
			'510' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba510 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_510		
    END 

	ELSE IF RTRIM(@PDBA) = '511'		--Absences Adjustment
	BEGIN

		--TAS History	
		SELECT 
			'511' AS txt,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo
		FROM tas.tmp_CHK_JDE_pdba511 a

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_511			
    END 

	ELSE IF RTRIM(@PDBA) = '-1'			--OneWorld Batch No.
	BEGIN

		--TAS History	
		SELECT TOP 100 PERCENT
			CASE WHEN LTRIM(RTRIM(txt)) = '010 OT-' THEN '10-'
				WHEN LTRIM(RTRIM(txt)) = '010 OT+' THEN '10+'
				ELSE '10' 
			END AS PDBA,
			a.autoid AS AutoID,
			a.dt AS DT,
			a.empno AS EmpNo,
			a.ot1 AS OTFrom,
			a.ot2 AS OTTo
		FROM tas.tmp_CHK_JDE_pdba010 a
		WHERE 1=2
		ORDER BY CASE WHEN LTRIM(RTRIM(txt)) = '010 OT-' THEN '10-' WHEN LTRIM(RTRIM(txt)) = '010 OT+' THEN '10+' ELSE '10' END

		--SELECT * FROM tas.vu_tmp_CHK_JDE_pdba010 WHERE 1=2		--TAS History	

		--JDE History
		SELECT * FROM tas.vu_JDE_mismatch_010 WHERE 1=2			
    END 

GO


/*	Debug:

	EXEC tas.Pr_GetTASJDETransHistory '010'		--OT
	EXEC tas.Pr_GetTASJDETransHistory '020'		--Shift Allowance Night
	EXEC tas.Pr_GetTASJDETransHistory '505'		--NoPay Hours

*/

