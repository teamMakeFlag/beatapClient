reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Beatap" /v "URL Protocol" /t "REG_SZ" /d "" /f
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Beatap" /v "EditFlags" /t "REG_DWORD" /d "2" /f
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Beatap" /ve /t "REG_SZ" /d "URL:beatap Protocol" /f
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Beatap\shell\open\command" /ve /t "REG_SZ" /d "C:\Users\fazerog\git\beatapClient\Beatap\Danger!!!!!!!!!!\launcher_wrapper.bat %%1" /f