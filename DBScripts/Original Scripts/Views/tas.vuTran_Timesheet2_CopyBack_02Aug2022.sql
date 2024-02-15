USE [tas2]
GO

/****** Object:  View [tas].[vuTran_Timesheet2_CopyBack]    Script Date: 02/08/2022 14:02:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- VIEWS
ALTER VIEW [tas].[vuTran_Timesheet2_CopyBack] 
AS 
SELECT T.* , (CASE WHEN T.dtIN IS NOT NULL THEN T.dtIN    WHEN T.dtOUT IS NOT NULL THEN T.dtOUT    ELSE T.DT END    )  dtSORT
FROM tas.tmp_Tran_Timesheet T


GO


