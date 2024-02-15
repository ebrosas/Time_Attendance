/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_PurchaseOrder
*	Description: Get PO details
*
*	Date:			Author:		Rev. #:		Comments:
*	05/12/2021		Ervin		1.0			Created
************************************************************************************************************************************************/

CREATE VIEW tas.Vw_PurchaseOrder
AS

	SELECT	a.PRDocNo AS PONumber,
			a.PRCreatedDate AS PROrderDate,
			a.PRReqTypeID,
			a.PRReqTypeName,
			a.PROrderType,
			a.PRStockType,
			a.PRItemType,
			RTRIM(e.ItemDesc) AS PRItemDesc,
			ISNULL(b.PHAN8, 0) AS SupplierNo, 
			LTRIM(RTRIM(ISNULL(d.ABALPH, ''))) AS SupplierName,
			a.PREmpNo AS OriginatorNo,
			a.PREmpName AS OriginatorName,
			a.PRCostCenter,
			a.PRChargeCostCenter,
			a.PRBuyerEmpNo,
			a.PRBuyerEmpName,
			a.PRIsBuyerAssigned,
			a.PROriginalPRNo,
			a.PRReqStatusCode AS StatusCode,
			c.UDCDesc1 AS StatusDesc,
			c.UDCSpecialHandlingCode AS StatusHandlingCode
	FROM tas.PurchaseRequisitionWF a WITH (NOLOCK)
		INNER JOIN tas.F4301 b WITH (NOLOCK) ON a.PRDocNo = b.PHDOCO 
		INNER JOIN tas.sy_UserDefinedCode c WITH (NOLOCK) ON a.PRReqStatusID = c.UDCID 
		LEFT JOIN tas.F0101 d  WITH (NOLOCK) ON b.PHAN8 = d.ABAN8 AND d.ABAT1 = 'V' 
		OUTER APPLY	
		(
			SELECT y.UDCCode AS ItemType, RTRIM(y.UDCDesc1) AS ItemDesc 
			FROM tas.sy_UserDefinedCodeGroup x WITH (NOLOCK)
				INNER JOIN tas.sy_UserDefinedCode y WITH (NOLOCK) ON x.UDCGID = y.UDCUDCGID
			WHERE RTRIM(x.UDCGCode) = 'ITEMTYPE'	
				AND RTRIM(y.UDCCode) = RTRIM(a.PRItemType)
		) e
	WHERE RTRIM(a.PROrderType) = 'OP'
		AND RTRIM(c.UDCSpecialHandlingCode) NOT IN ('Cancelled', 'Rejected')
		AND ISNULL(a.PRDraft, 0) = 0

GO 

/*	Debug:

	SELECT * FROM tas.Vw_PurchaseOrder a
	WHERE a.PONumber = 60184059

	SELECT * FROM tas.Vw_PurchaseOrder a
	WHERE a.SupplierNo = 10411

	SELECT * FROM tas.Vw_PurchaseOrder a
	WHERE RTRIM(a.StatusHandlingCode) IN ('Cancelled', 'Rejected')
	ORDER BY a.StatusHandlingCode
	
*/
