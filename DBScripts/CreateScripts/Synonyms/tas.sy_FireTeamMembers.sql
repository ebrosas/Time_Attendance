/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_FireTeamMembers
*	Description: Retrieves data from GRMACC.AcsData.dbo.FireTeamMembers table
*
*	Date:			Author:		Rev. #:		Comments:
*	15/06/2016		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_FireTeamMembers') IS NOT NULL
--	DROP SYNONYM tas.sy_FireTeamMembers
--GO

CREATE SYNONYM tas.sy_FireTeamMembers FOR GRMACC.AcsData.dbo.FireTeamMembers 
--CREATE SYNONYM tas.sy_FireTeamMembers FOR SWIPELNK.AcsData.dbo.NAMES

GO



/*	Debugging:

	SELECT * FROM tas.sy_FireTeamMembers

*/

