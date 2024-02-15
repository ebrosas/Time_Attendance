/*********************************************************************************
*	Revision History
*
*	Name: tas.sy_LeaveRequisition2
*	Description: Retrieves data from "LeaveRequisition2" table
*
*	Date:			Author:		Rev.#:		Comments:
*	27/11/2019		Ervin		1.0			Created
**********************************************************************************/

	IF OBJECT_ID ('tas.sy_LeaveRequisition2') IS NOT NULL
		DROP SYNONYM tas.sy_LeaveRequisition2
	GO

	CREATE SYNONYM tas.sy_LeaveRequisition2 FOR JDE_CRP.secuser.LeaveRequisition2				--Test server
	--CREATE SYNONYM tas.sy_LeaveRequisition2 FOR JDE_PRODUCTION.secuser.LeaveRequisition2		--Production server

GO


/*	Testing

	SELECT * FROM tas.sy_LeaveRequisition2

*/

