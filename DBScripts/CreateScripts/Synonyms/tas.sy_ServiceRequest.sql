/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_ServiceRequest
*	Description: Retrieves data from serviceuser.ServiceRequest table
*
*	Date:			Author:		Rev. #:		Comments:
*	05/03/2017		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_ServiceRequest') IS NOT NULL
--	DROP SYNONYM tas.sy_ServiceRequest
--GO

--CREATE SYNONYM tas.sy_ServiceRequest FOR ServiceMgmtTest.serviceuser.ServiceRequest		--Test server
CREATE SYNONYM tas.sy_ServiceRequest FOR ServiceMgmt.serviceuser.ServiceRequest			--Production server

GO


/*	Debugging:

	SELECT * FROM tas.sy_ServiceRequest

*/

