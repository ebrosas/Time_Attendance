USE [tas2]
GO

/****** Object:  UserDefinedFunction [tas].[isDateOnPublicHoliday]    Script Date: 09/08/2019 14:25:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

ALTER  FUNCTION [tas].[isDateOnPublicHoliday](@DT AS DATETIME) RETURNS BIT
-- select tas.isDateOnPublicHoliday('02-aug-2005')
AS
BEGIN
	DECLARE @ret BIT
	DECLARE @cnt INTEGER

	IF EXISTS
	(
		SELECT  TOP 1 1 
		FROM 	Master_Calendar C,
			System_Values S

		WHERE HolidayDate BETWEEN tas.fmtDate2(@DT,'00:00:00') AND tas.fmtDate2(@DT,'23:59:59') 
		AND   C.HolidayType=S.Code_PublicHolidy
	)
		SET @ret = 1
	ELSE
		SET @ret = 0

	RETURN @ret
END
GO


