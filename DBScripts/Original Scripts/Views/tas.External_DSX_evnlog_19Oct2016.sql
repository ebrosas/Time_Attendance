USE [tas2]
GO

/****** Object:  View [tas].[External_DSX_evnlog]    Script Date: 19/10/2016 11:44:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER VIEW [tas].[External_DSX_evnlog]
-- select top 10 * from External_DSX_evnlog
AS
-- select * from dbo.evnlog
SELECT * FROM grmacc.acslog.dbo.evnlog


GO


