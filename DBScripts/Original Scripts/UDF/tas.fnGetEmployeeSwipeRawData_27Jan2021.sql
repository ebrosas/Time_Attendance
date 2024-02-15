USE [tas2]
GO

/****** Object:  UserDefinedFunction [tas].[fnGetEmployeeSwipeRawData]    Script Date: 27/01/2021 11:12:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/********************************************************************************************************************************
User-defined Function Name		:	tas.fnGetEmployeeSwipeRawData
Description						:	This tabled-view function is used to fetch swipe raw data from the main gate readers
Created By						:	Ervin O. Brosas
Date Created					:	14 July 2015

Parameters
	@DT_SwipeLastProcessed		:	Refers to the begin date of the Timesheet process 
	@DT_SwipeNewProcess			:	Refers to the end date of the Timesheet process 
	@empNo						:	The Employee No. to filter the record

Revision History:
	1.0					EOB			2015.08.17 14:25
	Created

	1.1					EOB			2020.12.23 11:00
	Refactored the code to enhance performance
************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetEmployeeSwipeRawData]
(
	@DT_SwipeLastProcessed	DATETIME,	
	@DT_SwipeNewProcess		DATETIME,
	@empNo					INT
)
RETURNS @rtnTable 
TABLE 
(
	EmpNo INT NOT NULL,
	DT DATETIME,
	LocationCode INT NOT NULL,
	ReaderNo SMALLINT NOT NULL,
	EventCode SMALLINT NOT NULL,
	Direction CHAR(1) NULL,
	SwipeSource CHAR(1) NULL
) 
AS 
BEGIN

	DECLARE @myTable TABLE 
	(
		EmpNo int NOT NULL,
		DT datetime,
		LocationCode int NOT NULL,
		ReaderNo smallint NOT NULL,
		EventCode smallint NOT NULL,
		Direction char(1) NULL,
		SwipeSource char(1) NULL
	) 

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	--Populate records into the table
	INSERT INTO @myTable  
	SELECT * FROM
	(
		SELECT CAST (T.EmpNo AS INT) EmpNo  ,
			DT, 
			T.LocationCode, 
			T.ReaderNo, 
			EventCode,
			A.Direction,
			Source

		FROM tas.Tran_SwipeData_dsx1 T WITH (NOLOCK),
			tas.Master_AccessReaders A WITH (NOLOCK),
			tas.System_Values V WITH (NOLOCK)

		WHERE T.LocationCode = A.LocationCode
		AND   T.ReaderNo= A.ReaderNo
		AND   A.UsedForTS='y'
		AND   T.EventCode=8
		AND   T.DT > @DT_SwipeLastProcessed
		AND   T.DT <= @DT_SwipeNewProcess
		-----------------------------------------------------------
				UNION
		-----------------------------------------------------------
		SELECT  CAST (T.EmpNo AS INT) EmpNo  ,
			DT, 
			T.LocationCode, 
			T.ReaderNo, 
			EventCode,
			A.Direction,
			Source

		FROM tas.Tran_SwipeDataManuaL2_SwipeFormat T WITH (NOLOCK),
			tas.Master_AccessReaders A WITH (NOLOCK),
			tas.System_Values V WITH (NOLOCK)
		WHERE T.LocationCode = A.LocationCode
		AND   T.ReaderNo= A.ReaderNo
		AND   A.UsedForTS='y'
		AND   T.EventCode=8
		AND   CAST(tas.fmtDate(T.DT) AS DATETIME)+1 > @DT_SwipeLastProcessed
		AND   CAST(tas.fmtDate(T.DT) AS DATETIME)+1 <= @DT_SwipeNewProcess
		-----------------------------------------------------------
				UNION
		-----------------------------------------------------------
		-- this is a read from table Tran_ManualDsxInsert
		-- all filters should be applied to data in the table, view Tran_SwipeData_dsx2 will read all data only filtered by date
		SELECT
			CAST (T.EmpNo AS INT) EmpNo ,
			DT,
			LocationCode,
			ReaderNo,
			EventCode,
			Direction,
			Source
		FROM tas.Tran_ManualDsxInsert1 T WITH (NOLOCK),
			tas.System_Values S WITH (NOLOCK)
		WHERE   1=1 
		AND T.DT > @DT_SwipeLastProcessed
		AND   T.DT <= @DT_SwipeNewProcess
	) a
	WHERE (EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY EmpNo DESC, DT DESC, Direction DESC 

	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 

	RETURN 

END

/*	Debugging:

Parameters:
	@DT_SwipeLastProcessed	datetime,	
	@DT_SwipeNewProcess		datetime,
	@empNo					int

	SELECT * FROM tas.fnGetEmployeeSwipeRawData('2020-12-22 09:00:00.000', '2020-12-23 09:00:00.000', 10003631) ORDER BY EmpNo DESC, DT DESC

*/
GO


