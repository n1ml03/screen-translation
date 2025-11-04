@echo off
cd /d "%~dp0..\..\.."
set KMP_DUPLICATE_LIB_OK=TRUE
venv\Scripts\python.exe app\webserver\PaddleOCR\server_paddle.py
pause