USE [tas2]
GO

/****** Object:  View [tas].[vuEmployeeAttendance]    Script Date: 06/04/2022 14:39:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetMainGateSwipe
*	Description: Retrieve the the swipe time in/out based on specific date
*
*	Date			Author		Revision No.	Comments:
*	30/01/2011		Zaharan		1.0				Created
*	02/07/2020		Ervin		1.1				Refactored the code 
*************************************************************************************************************************************************/

ALTER VIEW [tas].[vuEmployeeAttendance] 
AS 

	--SELECT empno, DT, LocationCode, ReaderNo, EventCode, Source FROM tas.Tran_SwipeData_dsx1
	SELECT a.EmpNo, a.TimeDate AS DT, a.LocationCode, a.ReaderNo, a.EventCode, 'A' AS 'Source' 
	FROM tas.Vw_MainGateSwipeRawData a WITH (NOLOCK)

	UNION

	SELECT EmpNo, DATEADD(MINUTE,CONVERT(INT, SUBSTRING(timeIN,3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING(timeIN,1,2)), dtIN)) AS 'DT', 
	-1 AS 'LocationCode', -1 AS 'ReaderNo', '' AS 'EventCode', '' AS 'Source' 
	 FROM tas.Tran_ManualAttendance WITH (NOLOCK)

	UNION

	SELECT EmpNo, DATEADD(MINUTE,CONVERT(INT, SUBSTRING([timeOUT],3,2)), DATEADD(hh,CONVERT(INT, SUBSTRING([timeOUT],1,2)), dtOut)) AS 'DT', 
	-1 AS 'LocationCode', -2 AS 'ReaderNo', '' AS 'EventCode', '' AS 'Source' 
	FROM tas.Tran_ManualAttendance WITH (NOLOCK)

GO


