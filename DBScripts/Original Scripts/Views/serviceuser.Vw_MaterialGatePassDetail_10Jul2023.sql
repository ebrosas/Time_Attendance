USE [ServiceMgmt]
GO

/****** Object:  View [serviceuser].[Vw_MaterialGatePassDetail]    Script Date: 10/07/2023 11:05:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/**********************************************************************************************************************************************
*	Revision History
*
*	Name: serviceuser.Vw_MaterialGatePassDetail
*	Description: Get detailed information about the Material Gate Pass requisition
*
*	Date:			Author:		Rev. #:		Comments:
*	20/11/2017		Ervin		1.0			Created
*	04/12/2017		Ervin		1.1			Added filter condition to exclude draft, cancelled and rejected gate pass requisitions
*	05/12/2017		Ervin		1.2			Added the following filter fields: ServiceModuleCode, ServiceTypeCode
*	07/12/2017		Ervin		1.3			Added filter condition that exclude records where TMS_RefNo is null
*	22/03/2018		Ervin		1.4			Refactored the logic to include the scrap items in the view
*	30/09/2020		Ervin		1.5			Added "Quantity" in the recordset to be used for Washcast gate pass
************************************************************************************************************************************************/

ALTER VIEW [serviceuser].[Vw_MaterialGatePassDetail]
AS	
	
	SELECT	CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN 'Waste' 
				ELSE 'Scrap' 
			END AS GatePassType,
			CASE WHEN ISNUMERIC(b.Field18Value) = 1 AND NOT RTRIM(b.Field18Value) IN ('.', '+', '-', ',', '$')
				THEN CAST(b.Field18Value AS INT)
				ELSE NULL
			END AS TMS_RefNo,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN RTRIM(b.Field28Value)
				ELSE RTRIM(b.Field17Value)
			END AS Transporter,
			RTRIM(b.Field19Value) AS VehicleNo,
			a.ServiceRequestNo,
			a.Title,
			a.ServiceTypeCode,
			a.CategoryCode,
			CAST(ISNULL(b.Flag2Value, '') AS BIT) AS IsItemForSale,
			a.CreatedByUserEmpNo,
			a.CreatedByUserEmpName,
			a.CreatedByUserEmail,
			RTRIM(b.Field1Value) AS IssuedTo,
			RTRIM(b.Field3Value) AS IssuedDate,
			RTRIM(b.Field4Value) AS IssuedByEmpNo,
			RTRIM(b.Field6Value) AS IssuedByEmpName,
			RTRIM(b.Field7Value) AS Position,
			RTRIM(b.Field11Value) AS CostCenter,
			RTRIM(b.Field8Value) AS ImmediateSupervisor,
			RTRIM(b.Field8Value) AS CostCenterManager,
			
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN RTRIM(b.Field24Value)
				ELSE NULL
			END AS FormOfWaste,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN RTRIM(b.Field25Value) 
				ELSE NULL 
			END AS TypeOfWaste,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN RTRIM(b.Field15Value)
				ELSE NULL
			END AS OtherWaste,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN RTRIM(b.Field26Value) 
				ELSE NULL
			END AS ContainerType,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPWASTE' 
				THEN RTRIM(b.Field27Value)
				ELSE NULL
			END AS Destination,			
			
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPSCRAP' 
				THEN RTRIM(b.Field13Value) 
				ELSE NULL 
			END AS ScrapTypeCode,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPSCRAP' 
				THEN RTRIM(b.Field14Value) 
				ELSE NULL 
			END AS TypeOfScrap,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPSCRAP' 
				THEN RTRIM(b.Field25Value) 
				ELSE NULL 
			END AS UnitMeasure,
			CASE WHEN RTRIM(a.ServiceTypeCode) = 'MGPSCRAP' 
				THEN CASE WHEN RTRIM(b.Field36Value) = '1586772' AND ISNUMERIC(b.Field45Value) = 1 THEN CAST(b.Field45Value AS DECIMAL(10,2)) ELSE 0 END	--Rev. #1.5
				ELSE NULL 
			END AS Quantity,

			RTRIM(b.FieldMax1Value) AS DetailedDescription,						
			b.Field22Value AS SecurityShiftSchedule,
			CAST(CASE WHEN RTRIM(b.Field20Value) = 'valYes' THEN 1 ELSE 0 END AS BIT) AS IsReturn,
			CASE WHEN RTRIM(b.Field20Value) = 'valYes'
				THEN b.Field21Value 
				ELSE NULL  
			END AS ReturnDate,

			CAST(RTRIM(ISNULL(b.Flag3Value, '')) AS BIT) AS IsApprovalCompleted,
			CASE WHEN CAST(RTRIM(ISNULL(b.Flag3Value, '')) AS BIT) = 1
				THEN g.AppCreatedBy 
				ELSE NULL
			END AS ApprovedByEmpNo,
			CASE WHEN CAST(RTRIM(ISNULL(b.Flag3Value, '')) AS BIT) = 1
				THEN g.AppCreatedName
				ELSE NULL
			END AS ApprovedByEmpName,
			CASE WHEN CAST(RTRIM(ISNULL(b.Flag3Value, '')) AS BIT) = 1
				THEN g.AppCreatedDate 
				ELSE NULL
			END AS ApprovedDate,
			CAST(RTRIM(ISNULL(b.Flag4Value, '')) AS BIT) AS IsClosedBySecurity,
			CASE WHEN CAST(RTRIM(ISNULL(b.Flag4Value, '')) AS BIT) = 1
				THEN 
					CASE WHEN RTRIM(d.ActionCode) IN ('WASTE_SP_REJECT', 'SCRAP_SP_REJECT') 
							THEN d.CreatedByUserEmpNo
						ELSE 
							CASE WHEN ISNULL(e.ControlText, '') <> '' 
								THEN CAST(e.ControlText AS INT) 
								ELSE NULL 
							END
					END 
				ELSE NULL 
			END AS ClosedBySecurityEmpNo,
			CASE WHEN CAST(RTRIM(ISNULL(b.Flag4Value, '')) AS BIT) = 1
				THEN 
					CASE WHEN RTRIM(d.ActionCode) IN ('WASTE_SP_REJECT', 'SCRAP_SP_REJECT') 
						THEN d.CreatedDate
						ELSE 
							CASE WHEN ISDATE(f.ControlValue) = 1 THEN CONVERT(DATETIME, f.ControlValue) 
								WHEN ISDATE(f.ControlText) = 1 THEN CONVERT(DATETIME, f.ControlText) 
								ELSE NULL 
							END
					END 
				ELSE NULL
			END AS ClosedDate
	FROM serviceuser.ServiceRequest a WITH (NOLOCK)
		INNER JOIN serviceuser.ServiceRequestDetail b WITH (NOLOCK) ON a.ServiceRequestNo = b.ServiceRequestNo
		--OUTER APPLY	
		--(
		--	SELECT IsCurrent, IsCompleted
		--	FROM serviceuser.sy_WorkflowTransactionActivity 
		--	WHERE ServiceRequestNo = a.ServiceRequestNo
		--		AND RTRIM(ActivityCode) = 'WASTE_NTFY_ORIG'
		--) c
		OUTER APPLY
		(
			SELECT TOP 1 AutoID, ActionCode, CreatedDate, CreatedByUserEmpNo 
			FROM serviceuser.ServiceResolution WITH (NOLOCK) 
			WHERE ServiceRequestNo = a.ServiceRequestNo 
				AND RTRIM(ActionCode) IN 
				(
					'WASTE_SP_LOGEXIT', 'WASTE_SP_CLEARCLOSE', 'WASTE_SP_REJECT',	--Used in Material Gate Pass for waste items
					'SCRAP_SP_LOGEXIT', 'SCRAP_SP_CLEARCLOSE', 'SCRAP_SP_REJECT'	--Used in Material Gate Pass for scrap items
				)
			ORDER BY AutoID DESC
		) d
		OUTER APPLY
		(
			SELECT TOP 1 * FROM 
			(
				SELECT AutoID, ControlText FROM serviceuser.ServiceResolutionDetail WITH (NOLOCK) 
				WHERE SolutionID = d.AutoID
					AND ControlID IN (SELECT ControlID FROM serviceuser.ActionControlFields WITH (NOLOCK) WHERE RTRIM(ActionCode) IN ('WASTE_SP_LOGEXIT', 'SCRAP_SP_LOGEXIT') AND RTRIM(ServiceRefCode) = 'MGP_EMPNO')

				UNION
            
				SELECT AutoID, ControlText FROM serviceuser.ServiceResolutionDetail WITH (NOLOCK) 
				WHERE SolutionID = d.AutoID
					AND ControlID IN (SELECT ControlID FROM serviceuser.ActionControlFields WITH (NOLOCK) WHERE RTRIM(ActionCode) IN ('WASTE_SP_CLEARCLOSE', 'SCRAP_SP_CLEARCLOSE') AND RTRIM(ServiceRefCode) = 'MGP_EMPNO')
			) x
			ORDER BY x.AutoID DESC
		) e
		OUTER APPLY
		(
			SELECT TOP 1 * FROM 
			(
				SELECT AutoID, ControlValue, ControlText FROM serviceuser.ServiceResolutionDetail WITH (NOLOCK) 
				WHERE SolutionID = d.AutoID
					AND ControlID IN (SELECT ControlID FROM serviceuser.ActionControlFields WITH (NOLOCK) WHERE RTRIM(ActionCode) IN ('WASTE_SP_LOGEXIT', 'SCRAP_SP_LOGEXIT') AND RTRIM(ServiceRefCode) = 'MGP_EXIT_DATE')

				UNION
            
				SELECT AutoID, ControlValue, ControlText FROM serviceuser.ServiceResolutionDetail WITH (NOLOCK) 
				WHERE SolutionID = d.AutoID
					AND ControlID IN (SELECT ControlID FROM serviceuser.ActionControlFields WITH (NOLOCK) WHERE RTRIM(ActionCode) IN ('WASTE_SP_CLEARCLOSE', 'SCRAP_SP_CLEARCLOSE') AND RTRIM(ServiceRefCode) = 'MGP_EXIT_DATE')
			) x
			ORDER BY x.AutoID DESC
		) f
		OUTER APPLY
		(
			SELECT TOP 1 AppCreatedBy, AppCreatedName, AppCreatedDate 
			FROM serviceuser.Approval WITH (NOLOCK)
			WHERE AppReqTypeNo = a.ServiceRequestNo
			ORDER BY AutoID DESC	
		) g
	WHERE 
		ISNULL(a.IsDraft, 0) = 0
		AND RTRIM(a.StatusHandlingCode) NOT IN ('Rejected', 'Cancelled')
		AND RTRIM(a.ServiceModuleCode) = 'MODMGP'
		AND RTRIM(a.ServiceTypeCode) IN ('MGPWASTE', 'MGPSCRAP')
		AND (CASE WHEN ISNUMERIC(b.Field18Value) = 1 AND NOT RTRIM(b.Field18Value) IN ('.', '+', '-', ',', '$')		--Rev. #1.3
				THEN CAST(b.Field18Value AS INT)
				ELSE NULL
			END) > 0

GO


