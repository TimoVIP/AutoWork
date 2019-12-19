@echo.服务启动......
@echo off
@sc create TelgramBotService_cn_2 binPath= "D:\Telegram签到\Debug_cn_2\TelgramBotService.exe"
@net start TelgramBotService_cn_2 
@sc config TelgramBotService_cn_2 start= AUTO
@echo off
@echo.启动完毕！
@pause