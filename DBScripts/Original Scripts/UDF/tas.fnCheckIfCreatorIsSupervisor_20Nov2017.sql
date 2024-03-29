USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnCheckIfCreatorIsSupervisor]    Script Date: 20/11/2017 22:26:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*******************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnCheckIfCreatorIsSupervisor
*	Description: This function is used to check if the creator of the overtime request is a Shift Supervisor
*
*	Date			Author		Rev. #		Comments:
*	12/09/2017		Ervin		1.0			Created
**********************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnCheckIfCreatorIsSupervisor] 
(
	@originatorEmpNo	INT,
	@creatorEmpNo		INT
)
RETURNS BIT 
AS
BEGIN

    DECLARE	@result BIT

	SET @result = 0
    
	IF EXISTS
    (
		SELECT a.EmpNo FROM tas.Vw_ShiftSupervisor a
		WHERE a.EmpNo = @creatorEmpNo
	)
	SET @result = 1

	RETURN @result
END


/*	Debugging:

PARAMETERS:
	@originatorEmpNo	INT,
	@creatorEmpNo		INT

	SELECT tas.fnCheckIfCreatorIsSupervisor(10003631, 10001405) 

*/