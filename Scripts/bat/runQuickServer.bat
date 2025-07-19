REM SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
REM
REM SPDX-License-Identifier: MIT

@echo off
cd ../../

call dotnet run --project Content.Server --no-build %*

pause
