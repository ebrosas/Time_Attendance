@echo off
SET DBSERVER=ERVIN\SQL2008	
SET DATABASE=ServiceMgmt
SET USERNAME=wfcuser
SET PASSWORD=wfcpwd


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

echo Running Synonyms scripts...
for %%i in ("Synonyms\*.sql") do (
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


goto SUCCEED

:SUCCEED
echo Success!
exit 0

:END
pause
exit %ERRORLEVEL%