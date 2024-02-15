USE [tas2]
GO

/****** Object:  View [tas].[Tran_SwipeDataManual1_TransfromDT]    Script Date: 27/01/2021 10:49:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER  VIEW [tas].[Tran_SwipeDataManual1_TransfromDT] AS
SELECT 
	autoid,
	empno, 
	tas.add_HHMM_TO_date(dtIN , timeIN ) dtIN,
	tas.add_HHMM_TO_date(dtOUT , timeOUT ) dtOUT
FROM Tran_ManualAttendance



GO


