@echo.服务删除
@echo off
@net stop TelgramBotService_cn_2
@echo.关闭结束！
@sc delete TelgramBotService_cn_2
@echo off
@echo.删除结束！
@pause