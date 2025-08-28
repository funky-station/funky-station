# SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
# SPDX-FileCopyrightText: 2025 willowzeta <willowzeta632146@proton.me>
#
# SPDX-License-Identifier: MIT

#!/usr/bin/env sh

# make sure to start from script dir
if [ "$(dirname $0)" != "." ]; then
    cd "$(dirname $0)"
fi

cd ../../

git submodule update --init --recursive
dotnet build -c Debug
