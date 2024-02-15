USE [tas2]
GO

/****** Object:  View [tas].[TimeSheetUploaded_JDE]    Script Date: 6/23/2016 9:29:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

ALTER    VIEW [tas].[TimeSheetUploaded_JDE]
-- select top 10 * from TimeSheetUploaded_JDE
--
-- this view gives autoid of timesheet and pdba code.
-- this can be used to compare rows with output of SP Payroll_Validation_ControlTotals_for_JDE
AS

SELECT 	CAST(ytpdba AS INT) PDBA,
	CAST(ytan8 AS INT) EmpNo, 
	CAST(ytitm AS INT) AutoID ,
	ytphrw/100 'Hrs' 
FROM External_JDE_F06116 J , System_Values S
WHERE yticu = s.Upload_ID
--and ytpdba = 10

GO


