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
	1.0					Ervin			2015.08.17 14:25
	Created

	1.1					Ervin			2020.12.23 11:00
	Refactored the code to enhance performance

	1.2					Ervin			2021.01.27 11:23
	Cleaned up the code, removed join to redundant tables
************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetEmployeeSwipeRawData
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

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	--Populate records into the table
	INSERT INTO  @rtnTable  
	SELECT * FROM
	(
		--Get the swipe records at the Main Gate and workplace readers
		SELECT	a.EmpNo ,
				a.DT, 
				a.LocationCode, 
				a.ReaderNo, 
				a.EventCode,
				b.Direction,
				a.Source
		FROM tas.Tran_SwipeData_dsx1 a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
		WHERE a.LocationCode = b.LocationCode
			AND a.ReaderNo = b.ReaderNo
			AND RTRIM(b.UsedForTS) = 'y'
			AND a.EventCode = 8
			AND a.DT > @DT_SwipeLastProcessed
			AND a.DT <= @DT_SwipeNewProcess
		
		UNION
		
		--Get Manual Attendance records
		SELECT	CAST (a.EmpNo AS INT) EmpNo,
				a.DT, 
				a.LocationCode, 
				a.ReaderNo, 
				a.EventCode,
				b.Direction,
				a.[Source]
		FROM tas.Tran_SwipeDataManuaL2_SwipeFormat a WITH (NOLOCK)
			INNER JOIN tas.Master_AccessReaders b WITH (NOLOCK) ON a.LocationCode = b.LocationCode AND a.ReaderNo = b.ReaderNo
		WHERE a.LocationCode = b.LocationCode
			AND a.ReaderNo = b.ReaderNo
			AND RTRIM(b.UsedForTS) = 'y'
			AND a.EventCode = 8
			AND CAST(tas.fmtDate(a.DT) as datetime) + 1 > @DT_SwipeLastProcessed
			AND CAST(tas.fmtDate(a.DT) as datetime) + 1 <= @DT_SwipeNewProcess

		UNION
		
		-- this is a read from table Tran_ManualDsxInsert
		-- all filters should be applied to data in the table, view Tran_SwipeData_dsx2 will read all data only filtered by date
		SELECT	CAST (a.EmpNo AS INT) EmpNo,
				a.DT,
				a.LocationCode,
				a.ReaderNo,
				a.EventCode,
				a.Direction,
				a.[Source]
		FROM tas.Tran_ManualDsxInsert1 a WITH (NOLOCK)
		WHERE a.DT > @DT_SwipeLastProcessed
			AND a.DT <= @DT_SwipeNewProcess
	) a
	WHERE (EmpNo = @empNo OR @empNo IS NULL)
	ORDER BY EmpNo DESC, DT DESC, Direction DESC 

	RETURN 

END

/*	Debugging:

Parameters:
	@DT_SwipeLastProcessed	datetime,	
	@DT_SwipeNewProcess		datetime,
	@empNo					int

	SELECT * FROM tas.fnGetEmployeeSwipeRawData('2021-01-26 09:00:00.000', '2021-01-27 09:00:00.000', 10003631) ORDER BY EmpNo DESC, DT DESC

*/
GO


