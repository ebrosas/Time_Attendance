/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_TimesheetArchive
*	Description: Get the Timesheet archive data from year 2011 and below
*
*	Date:			Author:		Rev. #:		Comments:
*	08/11/2016		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_TimesheetArchive') IS NOT NULL
--	DROP SYNONYM tas.sy_TimesheetArchive
--GO

CREATE SYNONYM tas.sy_TimesheetArchive FOR Archive.dbo.tas2_Tran_Timesheet_before_2011
GO

/*	Debugging:

	SELECT * FROM tas.sy_TimesheetArchive

*/

