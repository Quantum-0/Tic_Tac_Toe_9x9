:WaitForClosing
>nul ping.exe -n  2 127.0.0.1
tasklist|find /i %1>nul && goto WaitForClosing

del %1
ren "Upd.tmp" %1
start %1