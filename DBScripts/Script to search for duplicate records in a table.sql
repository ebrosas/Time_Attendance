SELECT Desc1, COUNT(Desc1) 
FROM 
(
	SELECT DISTINCT
		FANUMB as AssetNo, 
		FAAPID as UnitNo, 
		LTRIM(FADL01) as Desc1,
		ltrim(rtrim(FAAPID)) + ' - ' + ltrim(rtrim(FADL01)) as Desc2
	FROM serviceuser.syJDE_F1201
) as a
GROUP BY Desc1
HAVING ( COUNT(Desc1) > 1 )
ORDER BY Desc1