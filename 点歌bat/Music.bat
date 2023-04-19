
@echo off

rem 需要reg.exe的支持
rem 无法保证在中、英之外的其它语言的操作系统上得到正确结果
for /f "skip=4 delims= " %%a in ('reg query "HKEY_CURRENT_USER\Control Panel\International" /v sShortDate') do set DateFormat=%%a
set DateFormat=%DateFormat:yyyy/M/d%
reg add "HKEY_CURRENT_USER\Control Panel\International" /v sShortDate /t REG_SZ /d yyyy-M-d /f>nul
set Today=%date: =%
reg add "HKEY_CURRENT_USER\Control Panel\International" /v sShortDate /t REG_SZ /d %DateFormat% /f>nul
set "Week=Mon Tue Wed Thu Fri Sat Sun 星期一 星期二 星期三 星期四 星期五 星期六 星期日"
for %%a in (%Week%) do call set "Today=%%Today:%%a=%%"


::获取时间中的小时 将格式设置为：24小时制
set timevar=%time:~0,2%
if /i %timevar% LSS 10 (
set timevar=0%time:~1,1%
)


::获取时间中的分、秒 将格式设置为：3220 ，表示 32分20秒
::set timevar=%timevar%%time:~3,2%%time:~6,2%
@echo %Today%--%time%%time:~6,2% >>"f:\play\播放历史.txt"



@echo off
	 set ProcessName1=CloudMusic.exe
	 set processName=wmplayer.exe
 	 set ProcessName2=DoubanRadio.exe
	 set processName0=cmd.exe
	 

 :panduang

::遍历mp3文件
::	初始化co

	set co = 1
	for /r "Y:\music" %%a in (*.mp3) do (
		set fn=%%~na
		if "!fn:~0,1!" neq "0" (
		set /a co += 1
		echo %%~a>>1.txt
               
		)
	)      


	
	for /r "Y:\music" %%a in (*.mp3.td*) do (
		set fn=%%~na
		if "!fn:~0,1!" neq "0" (
		set /a cc += 1
		del  /q /s "%%~a"
               
		)
	)   

	for /r "Y:\music" %%a in (*.td) do (
		set fn=%%~na
		if "!fn:~0,1!" neq "0" (
		set /a ce += 1
		del  /q /s "%%~a"
               
		)
	)  


::这里如果%co%<3运行DoubanRadio.exe"豆瓣,否则运行WMP

::	echo  %co% 
	
::if %co% lss 3 goto :kugou 
	if defined co (echo 变量str已经被赋值，其值为%co%) else (goto  :kugou ) 
	if %co% LSS 3  goto :kugou 
	if !co! GEQ 3  goto :wmplayer
	
	::echo "%te%"
	echo wmp f:\play\播放历史.txt
	goto :wmplayer

	echo 统计不成功
	ping -n 20 127.1 >nul 
	goto :exit
 
:kugou

taskkill /f /im "%ProcessName%"
taskkill /f /im "%ProcessName1%"

echo %ProcessName1%>>f:\play\播放历史.txt
ping -n 15 127.1 >nul 
	start ""  "E:\Program Files\Netease\CloudMusic\cloudmusic.exe"

::下面这句这里设置播放时间
	ping -n 900 127.1 >nul
	echo no有
	goto :exit

:wmplayer
   
taskkill /f /im "%ProcessName%"
taskkill /f /im "%ProcessName1%"

::这里扫描媒体文件
  
	for /f "tokens=1* delims=:" %%i in ('findstr /n ".*" 1.txt') do (
      if %%i==1   copy /y "%%j" "f:\play"  && del /q /s "%%j"  &&  echo %%j>>f:\play\播放历史.txt
      if %%i==2   copy /y "%%j" "f:\play"  && del /q /s "%%j"  &&  echo %%j>>f:\play\播放历史.txt
      if %%i==3   copy  /y "%%j" "f:\play" && del /q /s "%%j"  &&  echo %%j>>f:\play\播放历史.txt
	    if %%i==4   copy  /y "%%j" "f:\play" && del /q /s "%%j"  &&  echo %%j>>f:\play\播放历史.txt

	)

	start /min ""  "D:\Program Files\Windows Media Player\wmplayer.exe"
	 ping -n 22 127.1 >nul 
	 
	 
	  echo 正在启动点歌系统
	 start /max ""  "f:\musicbak\Playlists\自动播放列表.wpl"

 ::下面这句这里设置播放时间
	ping -n 900 127.1 >nul
	
::这里先关播放器，否则不能删除
	
	taskkill /f /im "%ProcessName%"
	taskkill /f /im "%ProcessName1%"
::这里删除前三个已播放过的文件
         for /f "delims=" %%i in ('dir "f:\play\*.mp3"  /s /b') do copy /y "%%i" "f:\mp3样本\"


	    
 
		
		ping -n 3 127.1 >nul

::taskkill /f /im "%ProcessName2%" 这里不能先关CMD


goto :exit

:exit
del /q /s 1.txt
del /q /s "f:\play\*.mp3"
del /q /s "f:\play\*.jpg"
del /q /s "Y:\music\*.jpg"
del /q /s "Y:\music\*.txt"

del /q /s 1.txt
del /q /s "f:\play\*.mp3"
del /q /s "f:\music\*.jpg"
del /q /s "f:\music\*.txt"

taskkill /f /im "%ProcessName%"
taskkill /f /im "%ProcessName1%"
taskkill /f /im "%ProcessName2%"
taskkill /f /im "%ProcessName3%"
taskkill /f /im "%ProcessName0%"


 exit