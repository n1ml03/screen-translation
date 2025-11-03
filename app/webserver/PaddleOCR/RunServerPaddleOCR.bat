@echo off

REM Allow duplicate MKL libraries (for Paddle)
set KMP_DUPLICATE_LIB_OK=TRUE

REM Activate virtual environment
call venv\Scripts\activate.bat

REM Run the Paddle server
python server_paddle.py

REM Keep the console window open after the script finishes
pause
