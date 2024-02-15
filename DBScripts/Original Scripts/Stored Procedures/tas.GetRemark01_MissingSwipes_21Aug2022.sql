USE [tas2]
GO

/****** Object:  View [tas].[GetRemark01_MissingSwipes]    Script Date: 21/08/2022 09:11:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER    VIEW [tas].[GetRemark01_MissingSwipes] AS

/*
	--2Y 13N
	select * from GetRemark01_MissingSwipes where autoid in 
	(
	select autoid from tran_timesheet 
	where empno = 10001745
	and dt = '21-Aug-2004'
	)
	
	--5 N  0 Y
	select * from GetRemark01_MissingSwipes where autoid in 
	(
	select autoid from tran_timesheet 
	where empno = 10001902
	and dt = '2004-jan-26'
	)
*/

SELECT A.autoid , B.One FROM 

(SELECT autoid , empno , dt   FROM tran_timesheet) A

LEFT JOIN 

(SELECT DISTINCT empno , dt , 1 One
FROM tran_timesheet
WHERE (dtin IS NOT NULL AND dtout IS NOT NULL) OR (dtin IS NULL AND dtout IS NULL)
) B

ON A.empno = B.empno AND a.dt = B.dt



GO


