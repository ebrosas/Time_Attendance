USE [tas2]
GO

/****** Object:  View [tas].[vu_tmp_CHK_JDE_summry]    Script Date: 21/12/2016 15:05:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER  VIEW [tas].[vu_tmp_CHK_JDE_summry] AS
SELECT
	PDBA ,
	PDBA_Name, 
	TAS_cnt ,
	Diff_TAS ,
	JDE_cnt ,
	Diff_JDE ,
	Diff 
FROM tmp_CHK_JDE_summry


GO


