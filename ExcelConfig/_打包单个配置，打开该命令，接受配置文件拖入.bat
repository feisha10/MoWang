cd ..
@echo off

:StartBuild
set /p a=��һ�������ļ��Ͻ�������Enter����

@tool\CfgExportor\CfgExportor\CfgExportor\bin\Debug\CfgExportor client/game/Assets/Resources/Config/ server/config/ 1 %a%

goto StartBuild

