	SELECT CODE from master_UDCValues_JDE where UDCKey='55  -' + ( select TSCorrectionSet from master_businessUnit_JDE where ltrim(BusinessUnit) = '5300')

	exec pTASTran_TimesheetDrillDown @p_select_str=N'Distinct [Tran_Timesheet_].[OTType]',@p_join_str=NULL,@p_where_str=N'(([Tran_Timesheet_].[OTType] IS NOT NULL) OR ([Tran_Timesheet_].[OTType] <> ''''))',@p_order_by_str=N'[Tran_Timesheet_].[OTType] Asc'
	exec pTASTran_TimesheetDrillDown @p_select_str=N'Distinct [Tran_Timesheet_].[CorrectionCode]',@p_join_str=NULL,@p_where_str=N'(([Tran_Timesheet_].[CorrectionCode] IS NOT NULL) OR ([Tran_Timesheet_].[CorrectionCode] <> ''''))',@p_order_by_str=N'[Tran_Timesheet_].[CorrectionCode] Asc'

	--Correction Code combo
	select CODE,Description from master_UDCValues_JDE where UDCKey='55  -T0'
		and CODE not Like 'AO%' 
		and CODE not Like 'RN%' 
		and CODE not Like 'ASNS' 
		and CODE not Like 'RSES' 
		and CODE not Like 'MA%' 
		and CODE not Like 'RD%' 
		and CODE not Like 'MO%' 

	--DIL Entitlement
	exec pTASTran_TimesheetDrillDown @p_select_str=N'Distinct [Tran_Timesheet_].[DIL_Entitlement]',@p_join_str=NULL,@p_where_str=N'(([Tran_Timesheet_].[DIL_Entitlement] IS NOT NULL) OR ([Tran_Timesheet_].[DIL_Entitlement] <> ''''))',@p_order_by_str=N'[Tran_Timesheet_].[DIL_Entitlement] Asc'
	select CODE,Description from master_UDCValues_JDE where UDCKey='55  -1' and CODE like 'E%' 

	--Remark Code
	exec pTASTran_TimesheetDrillDown @p_select_str=N'Distinct [Tran_Timesheet_].[RemarkCode]',@p_join_str=NULL,@p_where_str=N'(([Tran_Timesheet_].[RemarkCode] IS NOT NULL) OR ([Tran_Timesheet_].[RemarkCode] <> ''''))',@p_order_by_str=N'[Tran_Timesheet_].[RemarkCode] Asc'

	--OT Types
	select  Code,Description from Master_udcValues_JDE where udckey='55  -OT'
