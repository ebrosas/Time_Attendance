USE [tas2]
GO

/****** Object:  View [tas].[vuEmployeeAttendance]    Script Date: 02/07/2020 09:58:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:      Zaharan Haleed
-- Create date:	30th January 2011
-- Description: Gets employee attendance record from swipe and manual records
-- =============================================
ALTER VIEW [tas].[vuEmployeeAttendance] 
AS 

   
SELECT empno, DT, LocationCode, ReaderNo, EventCode, Source FROM tas.Tran_SwipeData_dsx1

UNION

SELECT EmpNo, DATEADD(MINUTE,CONVERT(INT, SUBSTRING(timeIN,3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING(timeIN,1,2)), dtIN)) AS 'DT', 
-1 AS 'LocationCode', -1 AS 'ReaderNo', '' AS 'EventCode', '' AS 'Source' 
 FROM tas.Tran_ManualAttendance

UNION

SELECT EmpNo, DATEADD(MINUTE,CONVERT(INT, SUBSTRING([timeOUT],3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING([timeOUT],1,2)), dtOut)) AS 'DT', 
-1 AS 'LocationCode', -2 AS 'ReaderNo', '' AS 'EventCode', '' AS 'Source' 
  FROM tas.Tran_ManualAttendance





GO


