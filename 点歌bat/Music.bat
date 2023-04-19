
@echo off
setlocal enabledelayedexpansion

::检查当前系统时间格式是否符合 YYYY/M/D 或 YYYY/M/DD 格式，如果不是则修改为标准格式 YYYY-MM-DD 并记录原格式
for /f "skip=4 delims= " %%a in ('reg query "HKEY_CURRENT_USER\Control Panel\International" /v sShortDate') do set DateFormat=%%a
set DateFormat=%DateFormat:/M/=M%
if not "%DateFormat%"=="yyyy-M-d" if not "%DateFormat%"=="yyyy-M-dd" (
    set Today=%date:~0,10%
    reg add "HKEY_CURRENT_USER\Control Panel\International" /v sShortDate /t REG_SZ /d yyyy-M-d /f>nul
    for %%a in (%Week%) do set "Today=!Today:%%a=!"
)

::获取当前时间中的小时（24小时制）和分钟，并写入播放历史日志
for /f "tokens=1-4 delims=:." %%a in ("%time%") do set /a "hour=%%a", "min=%%b", "sec=%%c"
set timevar=%time:~0,2%
if %timevar% LSS 10 set timevar=0%time:~1,1%
set PlayLog=f:\play\播放历史.txt
echo [%date% %timevar%:%time:~3,2%]>>"%PlayLog%"

:: 遍历音乐目录下的 MP3 文件
set Count=0
for /r "Y:\music" %%A in (*.mp3) do (
    set /a Count+=1
    echo %%A>>1.txt
)

::如果从音乐目录下找到的 MP3 文件数量小于 3，则将播放器设置为使用豆瓣电台播放，否则使用 Windows Media Player 
if %Count% lss 3 (
    echo 使用豆瓣电台
    taskkill /F /IM CloudMusic.exe >nul 2>nul
    ping -n 15 127.0.0.1 >nul 2>nul
    start "" "E:\Program Files\Netease\CloudMusic\cloudmusic.exe"
    ping -n 900 127.0.0.1 >nul 2>nul
) else (
    echo 使用 Windows Media Player
    taskkill /F /IM wmplayer.exe >nul 2>nul
    for /f "tokens=2 delims=:" %%A in ('tasklist /FI "IMAGENAME eq wmplayer.exe" /NH /FO CSV') do (
        taskkill /F /PID %%A >nul 2>nul
    )
    ping -n 22 127.0.0.1 >nul 2>nul

    :: 找到音乐目录下最老的三个 MP3 文件并移至播放目录
    dir /B /A:-D /O:D "Y:\music\*.mp3" | set /P oldest_files=
    type nul >temp.txt
    for %%A in (%oldest_files%) do (
        echo copying "Y:\music\%%A" to "f:\play\%%A">>temp.txt
        copy /Y "Y:\music\%%A" "f:\play\%%A" >>temp.txt
    )
    type temp.txt>>"%PlayLog%"
    del temp.txt >nul 2>nul

    start /min "" "D:\Program Files\Windows Media Player\wmplayer.exe"
    ping -n 5 127.0.0.1 >nul 2>nul
    echo 开始自动播放
    start /max "" "f:\musicbak\Playlists\自动播放列表.wpl"
    ping -n 900 127.0.0.1 >nul 2>nul

    :: 停止 Windows Media Player 的运行，并删除已经播放过的文件
    taskkill /F /IM wmplayer.exe >nul 2>nul
    for /f "tokens=2 delims=:" %%A in ('tasklist /FI "IMAGENAME eq wmplayer.exe" /NH /FO CSV') do (
        taskkill /F /PID %%A >nul 2>nul
    )
    dir /B /A:-D /O:D "f:\play\*.mp3" | findstr /I "mp3" >temp.txt
    set fileCount=0
    for /f "tokens=* delims=" %%A in (temp.txt) do (
        if !fileCount! LSS 3 (
            type nul >nul 2>nul "f:\mp3样本\%%A"
            echo 删除已播放过的文件 "f:\play\%%A">>"%PlayLog%"
        ) else (
            del /Q /F "f:\play\%%A" >nul 2>nul
            echo 删除已播放过的文件 "f:\play\%%A">>"%PlayLog%"
        )
        set /a fileCount+=1
    )
    del temp.txt >nul 2>nul
)

::清理临时文件
del /Q /S /F Y:\music\*.td* >nul 2>nul
del /Q /S /F f:\play\*.txt >nul 2>nul

exit
