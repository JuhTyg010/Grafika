@echo off
rem =====================================
rem Spustit v adresari s vygenerovanymi snimky
rem out0000.png, out0001.png, ..
rem ffmpeg binarky pro Windows: http://ffmpeg.zeranoe.com/builds/
rem =====================================

set ffmpeg=ffmpeg.exe
%ffmpeg% -i out%%04d.png -f avi -vcodec msmpeg4v2 -q:v 5 -y out.avi

rem =====================================
rem   Doporucovane kodeky (-vcodec xxx):
rem "msmpeg4v2" - MPEG-4 kodek, ktery implicitne umeji Windows
rem "libxvid"   - free-verze kodeku pro DivX prehravac
rem =====================================
