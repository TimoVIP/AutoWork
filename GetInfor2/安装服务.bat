@echo.��������......
@echo off
@sc create TelgramBotService_cn_2 binPath= "D:\Telegramǩ��\Debug_cn_2\TelgramBotService.exe"
@net start TelgramBotService_cn_2 
@sc config TelgramBotService_cn_2 start= AUTO
@echo off
@echo.������ϣ�
@pause