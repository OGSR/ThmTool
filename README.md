# ThmTool

Simple tool to pack|unpack thm files into json format

Command line arguments:
-m <unpack|pack> - work mode, - default mode: unpack
-p <path> - set working directory, default - current directory

To unpack all *.thm to json format in current directory:
ThmTool.exe -m unpack

To pack all *.thm.json to thm format in current directory:
ThmTool.exe -m pack
