@echo off
::set "%0=%0 %*"

::#####预处理-检查参数中双引号的个数是否为奇数
:: break "%*"&goto :MAIN
:: echo 命令语法不正确。
:: exit /b 1
::#####预处理-检查参数中双引号的个数是否为奇数

:: ====

:: *************** start of 'main'----主函数
:MAIN

setlocal enabledelayedexpansion

RegExpIsMatch_ "/\?" "%*"
if 0==%errorlevel% call :USAGE&exit /b 0


copy /y F:\工作目录\EasyImage\ArtDeal\bin\Release\ArtDeal.dll f:\工作目录\EasyImage\EasyImage\Plugins\ArtDeal.dll
copy /y F:\工作目录\EasyImage\EasyDeal\bin\Release\EasyDeal.dll f:\工作目录\EasyImage\EasyImage\Plugins\EasyDeal.dll
copy /y F:\工作目录\EasyImage\Drawing\bin\Release\Drawing.dll f:\工作目录\EasyImage\EasyImage\Plugins\Drawing.dll
copy /y F:\工作目录\EasyImage\Beauty\bin\Release\Beauty.dll f:\工作目录\EasyImage\EasyImage\Plugins\Beauty.dll
copy /y F:\工作目录\EasyImage\Property\bin\Release\Property.dll f:\工作目录\EasyImage\EasyImage\Plugins\Property.dll



REM copy /y F:\工作目录\EasyImage\ArtDeal\bin\Release\ArtDeal.dll C:\Users\cheng\AppData\Local\Apps\2.0\Y38C8Z4A.G46\LD61W7T6.J3E\easy..tion_e673c794dde2cfee_0001.0000_466e6fad5bcf9763\Plugins\ArtDeal.dll
REM copy /y F:\工作目录\EasyImage\EasyDeal\bin\Release\EasyDeal.dll C:\Users\cheng\AppData\Local\Apps\2.0\Y38C8Z4A.G46\LD61W7T6.J3E\easy..tion_e673c794dde2cfee_0001.0000_466e6fad5bcf9763\Plugins\EasyDeal.dll
REM copy /y F:\工作目录\EasyImage\Drawing\bin\Release\Drawing.dll C:\Users\cheng\AppData\Local\Apps\2.0\Y38C8Z4A.G46\LD61W7T6.J3E\easy..tion_e673c794dde2cfee_0001.0000_466e6fad5bcf9763\Plugins\Drawing.dll
REM copy /y F:\工作目录\EasyImage\Beauty\bin\Release\Beauty.dll C:\Users\cheng\AppData\Local\Apps\2.0\Y38C8Z4A.G46\LD61W7T6.J3E\easy..tion_e673c794dde2cfee_0001.0000_466e6fad5bcf9763\Plugins\Beauty.dll
REM copy /y F:\工作目录\EasyImage\Property\bin\Release\Property.dll C:\Users\cheng\AppData\Local\Apps\2.0\Y38C8Z4A.G46\LD61W7T6.J3E\easy..tion_e673c794dde2cfee_0001.0000_466e6fad5bcf9763\Plugins\Property.dll


::endlocal
exit /b 0
:: *************** end of 'main'

:: *************** Functions begin here ****************************

:: *************** start of procedure USAGE----显示用法
:USAGE
	echo.
	echo 显示用法信息。
goto :EOF
:: *************** end of procedure USAGE