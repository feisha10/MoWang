cd ..
@echo off

:StartBuild
set /p a=把一个配置文件拖进来，按Enter键：

@tool\CfgExportor\CfgExportor\CfgExportor\bin\Debug\CfgExportor client/game/Assets/Resources/Config/ server/config/ 1 %a%

goto StartBuild

