	--exec Payroll_Validation_ControlTotals_for_JDE 
	select * from vu_tmp_CHK_JDE_summry

	SELECT COUNT(*) AS TAS_cnt FROM tas.tmp_CHK_JDE_pdba020
	
	--Shift Allowance Night
	SELECT a.*
	FROM tmp_CHK_JDE_TS a 
		INNER JOIN tas.syJDE_F060116 b ON a.empno = b.YAAN8
	WHERE a.N >= 240
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618
		)
	ORDER BY a.dt  DESC 

	--Shift Allowance Evening
	SELECT a.*
	FROM tmp_CHK_JDE_TS a 
		INNER JOIN tas.syJDE_F060116 b ON a.empno = b.YAAN8
	WHERE a.E >= 240
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618
		)

/*

	select Upload_ID from System_Values 

*/	
	