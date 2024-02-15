@echo off
SET DBSERVER=GRMDBT02	
SET DATABASE=ServiceMgmt
SET USERNAME=serviceuser
SET PASSWORD=servicepwd


echo Running Drop Constraints scripts...
for %%i in ("DropConstraints\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i 
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running Table scripts...
for %%i in ("Tables\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i 
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running Data scripts...
for %%i in ("Data\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i 
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running Constraints scripts...
for %%i in ("Constraints\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i 
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running Views scripts...
for %%i in ("Views\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i  
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running User-Defined Functions scripts...
for %%i in ("UserDefinedFunctions\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i 
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running Stored Procedures scripts...
for %%i in ("StoredProcedures\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i 
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)

echo Running Triggers scripts...
for %%i in ("Triggers\*.sql") do (
	sqlcmd -b -S %DBSERVER% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i %%i  
	if ERRORLEVEL 1 (
		echo %%i 
		goto END
	)
)


Echo		--------------------------------------------------- 
Echo		Adjust Database compatibility level to SQL2005/90
Echo		---------------------------------------------------
osql -S %DBSERVER% -U %USERNAME% -P %PASSWORD% -n  -Q"USE %DATABASE%  EXECUTE sys.sp_dbcmptlevel %DATABASE%, 90"

goto SUCCEED

:SUCCEED
echo Success!
exit 0

:END
pause
exit %ERRORLEVEL%