# ThmTool

Simple tool to pack|unpack thm files into json format

Command line arguments:

`-unpack` - unpack mode, that is default work mode. Will unpack all files by mask *.thm

`-cop2soc` - work only in `-unpack` mode, will convert COP format to SHOC on fly

`-pack` - pack mode. Will pack all files by mask *.thm.json

`-overwrite` - work only in `-pack` mode, will override existing thm files

`-source <path>` - set working directory, default: current directory

`-m <0|1>` - format for flag values in json. 0 - use actual values (default mode), 1 - use names

To unpack all *.thm to json format in current directory:

`ThmTool.exe -unpack`

To pack all *.thm.json to thm format in current directory:

`ThmTool.exe -pack`
