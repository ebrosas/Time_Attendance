DECLARE	@actionType			TINYINT = 0,	--(Note: 0 = Check existing records; 1 = Insert new record; 2 = Update record; 3 = Delete record)
		@isCommitTrans		BIT, 
		@AutoID				INT,
		@LocationCode		INT = 0,		--(Notes: 1 = Main Mill, 2 = Foil Mill)
		@ReaderNo			INT = 0,
		@LocationName		CHAR(40),
		@ReaderName			CHAR(40),
		@Direction			CHAR(40),		--(Note: I = In; O = Out; IO = In/Out)
		@UsedForTS			CHAR(1),
		@MinTranNormalDay	INT,
		@MinTranHoliday		INT,
		@Tmp_Counts			INT,
		@WValue				INT,
		@HValue				INT,
		@SourceID			TINYINT,		--(Note: 0 or NULL = Access System Readers; 1 = unis_tenter readers)
		@LastUpdateUser		VARCHAR(20),
		@LastUpdateTime		DATETIME

	IF ISNULL(@LocationCode, 0) = 0
		SET @LocationCode = NULL

	IF ISNULL(@ReaderNo, 0) = 0
		SET @ReaderNo = NULL

	/*	Add Reader #13 => Main Gate - Turnstile (In)  - Added on 29-Aug-2021
	SELECT	@actionType			= 0,	
			@isCommitTrans		= 0, 
			@LocationCode		= 1,
			@ReaderNo			= 13,
			@LocationName		= 'Main Gate',
			@ReaderName			= 'Turnstile (In)',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 494,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 150,
			@WValue				= 300,
			@HValue				= 200,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #14 => Main Gate - Turnstile (Out)	- Added on 29-Aug-2021 
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 1,
			@ReaderNo			= 14,
			@LocationName		= 'Main Gate',
			@ReaderName			= 'Turnstile (Out)',
			@Direction			= 'O',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 406,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 115,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #15 => Main Gate - Barrier (In)	- Added on 29-Aug-2021 
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 1,
			@ReaderNo			= 15,
			@LocationName		= 'Main Gate',
			@ReaderName			= 'Barrier (In)',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 179,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 73,
			@WValue				= 60,
			@HValue				= 40,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #16 => Main Gate - Barrier (Out)	- Added on 29-Aug-2021 
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 1,
			@ReaderNo			= 16,
			@LocationName		= 'Main Gate',
			@ReaderName			= 'Barrier (Out)',
			@Direction			= 'O',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 177,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 13,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #31 => Foil Mill - Turnstile (In) 
	SELECT	@actionType			= 0,	
			@isCommitTrans		= 0, 
			@LocationCode		= 2,
			@ReaderNo			= 31,
			@LocationName		= 'Foil Mill',
			@ReaderName			= 'Turnstile (In)',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 20,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 236,
			@WValue				= 80,
			@HValue				= 70,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #32 => Foil Mill - Turnstile (Out) 
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 2,
			@ReaderNo			= 32,
			@LocationName		= 'Foil Mill',
			@ReaderName			= 'Turnstile (Out)',
			@Direction			= 'O',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 22,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 220,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #33 => Foil Mill - Barrier (In) - Added on 12/08/2021
	SELECT	@actionType			= 0,	
			@isCommitTrans		= 0, 
			@LocationCode		= 2,
			@ReaderNo			= 33,
			@LocationName		= 'Foil Mill',
			@ReaderName			= 'Barrier (In)',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 74,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 20,
			@WValue				= 2,
			@HValue				= 10,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #34 => Foil Mill - Barrier (Out) - Added on 12/08/2021 
	SELECT	@actionType			= 0,	
			@isCommitTrans		= 0, 
			@LocationCode		= 2,
			@ReaderNo			= 34,
			@LocationName		= 'Foil Mill',
			@ReaderName			= 'Barrier (Out)',
			@Direction			= 'O',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 37,
			@MinTranHoliday		= NULL,
			@Tmp_Counts			= 19,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= 1,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #41 => Annealing 123
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 41,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Annealling 123',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #42 => Annealing 456
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 42,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Annealling 456',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #43 => Roll Grinder 1
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 43,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Roll Grinder 1',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #44 => EMD Workshop
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 44,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'EMD Workshop',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #47 => MMD Workshop
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 47,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'MMD Workshop',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #46 => Remelt Control Room2
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 46,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Remelt Control Room2',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #45 => Remelt 2
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 45,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Remelt 2',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #48 => CM1 Floor Intercom 
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 48,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'CM1 Floor Intercom ',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #49 => CM2 Floor Intercom 
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 49,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'CM2 Floor Intercom ',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #50 => Water Treatment
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 50,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Water Treatment',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #51 => SL2 Operator Cabin
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 51,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'SL2 Operator Cabin',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #52 => CTL Operator SFDC
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 52,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'CTL Operator SFDC',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #53 => Packing Office
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 53,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Packing Office',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #53 => Packing Office
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 53,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'Packing Office',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #54 => SL1 Operator SFDC
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 54,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'SL1 Operator SFDC',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #55 => TLL1 Operator SFDC
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 55,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'TLL1 Operator SFDC',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #56 => TLL2 Operator Cabin
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 56,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'TLL2 Operator Cabin',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #57 => TLL3 Operator Cabin
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 57,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'TLL3 Operator Cabin',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #58 => HM Floor Cabin
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 58,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'HM Floor Cabin',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/

	/*	Add Reader #59 => SHF Control Room
	SELECT	@actionType			= 1,	
			@isCommitTrans		= 1, 
			@LocationCode		= 8,	--(Note: 8 means workplace reader)
			@ReaderNo			= 59,
			@LocationName		= 'GARMCO',
			@ReaderName			= 'SHF Control Room',
			@Direction			= 'I',	--(Note: I = In; O = Out; IO = In/Out)
			@UsedForTS			= 'y',
			@MinTranNormalDay	= 0,
			@MinTranHoliday		= 0,
			@Tmp_Counts			= 0,
			@WValue				= 0,
			@HValue				= 0,
			@SourceID			= NULL,
			@LastUpdateUser		= 'System Admin',
			@LastUpdateTime		= GETDATE()
	*/
		

	IF @actionType = 0
	BEGIN 

		--Get specific reader
		SELECT * FROM tas.Master_AccessReaders a WITH (NOLOCK)
		WHERE a.ReaderNo = @ReaderNo
			AND a.LocationCode = @LocationCode

		--Get all readers
		SELECT * FROM tas.Master_AccessReaders a WITH (NOLOCK)
		WHERE (a.LocationCode = @LocationCode OR @LocationCode IS NULL)
			AND (a.ReaderNo = @ReaderNo OR @ReaderNo IS NULL)
		ORDER BY a.LocationCode, a.ReaderNo
	END 

	ELSE IF @actionType = 1
	BEGIN
    
		BEGIN TRAN T1

		INSERT INTO tas.Master_AccessReaders
		(
			LocationCode,
			ReaderNo,
			LocationName,
			ReaderName,
			Direction,
			UsedForTS,
			MinTranNormalDay,
			MinTranHoliday,
			Tmp_Counts,
			WValue,
			HValue,
			LastUpdateUser,
			LastUpdateTime,
			SourceID
		)
		SELECT	@LocationCode,
				@ReaderNo,
				@LocationName,
				@ReaderName,
				@Direction,
				@UsedForTS,
				@MinTranNormalDay,
				@MinTranHoliday,
				@Tmp_Counts,
				@WValue,
				@HValue,	
				@LastUpdateUser,
				@LastUpdateTime,
				@SourceID	

		--Check inserted records
		SELECT * FROM tas.Master_AccessReaders a WITH (NOLOCK)
		WHERE a.ReaderNo = @ReaderNo
			AND a.LocationCode = @LocationCode

		IF @isCommitTrans = 1
			COMMIT TRAN T1
		ELSE
			ROLLBACK TRAN T1
	END 

/*	Debug:

	--Get old Foil Mill readers
	SELECT * FROM tas.Master_AccessReaders a WITH (NOLOCK)
	WHERE a.SourceID = 1

	--Get new Foil Mill readers
	SELECT * FROM tas.Master_AccessReaders a WITH (NOLOCK)
	WHERE a.ReaderNo IN 
		(
			0,	--Foil Mill Barrier (In)    
			1,	--Foil Mill Barrier (Out)
			2,	--Foil Mill Turnstile (Out)    
			3	--Foil Mill Turnstile (In)    
		)
		AND a.LocationCode = 2

	BEGIN TRAN T1

	--Foil Mill Turnstile
	UPDATE tas.Master_AccessReaders
	SET SourceID = 1
	WHERE ReaderNo IN (31, 32)

	--Foil Mill Barrier
	UPDATE tas.Master_AccessReaders
	SET SourceID = 1
	WHERE ReaderNo IN (33, 34)

	--Foil Mill Turnstile (In)   
	UPDATE tas.Master_AccessReaders
	SET Tmp_Counts = 313
	WHERE ReaderNo = 31

	--Foil Mill Turnstile (Out)                          
	UPDATE tas.Master_AccessReaders
	SET Tmp_Counts = 300
	WHERE ReaderNo = 32

	COMMIT TRAN T1
	ROLLBACK TRAN T1

*/


