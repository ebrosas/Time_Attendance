	---No Pay Hours
	SELECT '505 NoPay Hours' txt, autoid, tas.fmtdate(dt) dt, empno  
	FROM tmp_CHK_JDE_TS a 
		INNER JOIN tas.syJDE_F060116 b ON a.empno = b.YAAN8
	WHERE a.nph > 0
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618
		)

	---Absences
	SELECT '510 Absences' txt, a.autoid, tas.fmtdate(a.dt) dt, a.empno  
	FROM tmp_CHK_JDE_TS a 
		INNER JOIN tas.syJDE_F060116 b ON a.empno = b.YAAN8
	WHERE 
		(
			(UPPER(RTRIM(a.rem)) = 'A' AND a.grd <= 8 AND a.islastrow = 1)
			OR
			(UPPER(RTRIM(a.rem)) = 'A' AND a.grd >= 9 AND ISNULL(a.DW, 0) = 0 AND a.islastrow = 1)
		)
		AND ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)) THEN '0' ELSE b.YAPAST END) = 1
		AND a.autoid NOT IN
		(
			SELECT YTITM FROM tas.syJDE_F0618
		)

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
	ORDER BY CASE WHEN LTRIM(RTRIM(txt)) = '010 OT-' THEN '10-' WHEN LTRIM(RTRIM(txt)) = '010 OT+' THEN '10+' ELSE '10' END

	--JDE History
	SELECT * FROM tas.vu_JDE_mismatch_010		

/*

	EXEC tas.Pr_GetTASJDEComparisonReport  
	EXEC tas.Pr_GetTASJDETransHistory 010
	EXEC tas.Pr_GetTASJDETransHistory 010

	SELECT * FROM tas.tmp_CHK_JDE_summry a

*/