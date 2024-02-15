/*************************************************************************************************************************
*	Revision History
*
*	Name: tas.sy_MIRRequest
*	Description: Retrieves data from serviceuser.MIRRequest table
*
*	Date:			Author:		Rev. #:		Comments:
*	02/08/2016		Ervin		1.0			Created
**************************************************************************************************************************/

--IF OBJECT_ID ('tas.sy_MIRRequest') IS NOT NULL
--	DROP SYNONYM tas.sy_MIRRequest
--GO

--CREATE SYNONYM tas.sy_MIRRequest FOR ServiceMgmtTest.serviceuser.MIRRequest		--Test server
CREATE SYNONYM tas.sy_MIRRequest FOR ServiceMgmt.serviceuser.MIRRequest			--Production server

GO


/*	Debugging:

	SELECT * FROM tas.sy_MIRRequest

*/

