/**********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Vw_OTActualAmount
*	Description: Retrieves the overtime budget amount by fiscal year
*
*	Date:			Author:		Rev. #:		Comments:
*	07/03/2018		Ervin		1.0			Created
*	05/08/2018		Ervin		1.1			Added WITH (NOLOCK) clause
************************************************************************************************************************************************/

ALTER VIEW tas.Vw_OTActualAmount
AS		
	
	SELECT	tas.ConvertFromJulian(a.GBUPMJ) AS DateUpdated, 
			(a.GBCTRY * 100) + a.GBFY AS FiscalYear,
			LTRIM(RTRIM(a.GBMCU)) AS CostCenter,
			a.GBAN01 / 1000 AS January,
			a.GBAN02 / 1000 AS February,
			a.GBAN03 / 1000 AS March,
			a.GBAN04 / 1000 AS April,
			a.GBAN05 / 1000 AS May,
			a.GBAN06 / 1000 AS June,
			a.GBAN07 / 1000 AS July,
			a.GBAN08 / 1000 AS August,
			a.GBAN09 / 1000 AS September,
			a.GBAN10 / 1000 AS October,
			a.GBAN11 / 1000 AS November,
			a.GBAN12 / 1000 AS December
	FROM tas.sy_F0902 a WITH (NOLOCK)
	WHERE 
		LTRIM(RTRIM(a.GBOBJ)) = '533150'		--Object Account
		AND LTRIM(RTRIM(a.GBSUB)) = '1100'		--Subledger
		AND LTRIM(RTRIM(a.GBLT)) = 'AA'			--Ledger Type

GO

/* Testing:

	SELECT * FROM tas.Vw_OTActualAmount a
	ORDER BY FiscalYear DESC
	
*/
