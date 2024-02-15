/*********************************************************************************
*	Revision History
*
*	Name: tas.PurchaseRequisitionWF
*	Description: Retrieves data from "PurchaseRequisitionWF" table
*
*	Date:			Author:		Rev.#:		Comments:
*	05/12/2021		Ervin		1.0			Created
**********************************************************************************/

	--IF OBJECT_ID ('tas.PurchaseRequisitionWF') IS NOT NULL
	--	DROP SYNONYM tas.PurchaseRequisitionWF
	--GO

	CREATE SYNONYM tas.PurchaseRequisitionWF FOR JDE_CRP.secuser.PurchaseRequisitionWF					--Test server
	--CREATE SYNONYM tas.PurchaseRequisitionWF FOR JDE_PRODUCTION.secuser.PurchaseRequisitionWF			--Production server

GO


/*	Testing

	SELECT * FROM tas.PurchaseRequisitionWF

*/

