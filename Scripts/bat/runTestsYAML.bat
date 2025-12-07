REM SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
REM
REM SPDX-License-Identifier: MIT

cd ..\..\

mkdir Scripts\logs

del Scripts\logs\Content.YAMLLinter.log
dotnet run --project Content.YAMLLinter/Content.YAMLLinter.csproj -c DebugOpt -- NUnit.ConsoleOut=0 > Scripts\logs\Content.YAMLLinter.log

pause
