# SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

#!/usr/bin/env pwsh

Get-ChildItem release/*.zip | Get-FileHash -Algorithm SHA256 | ForEach-Object {
    $_.Hash > "$($_.Path).sha256";
}
