// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Content.Server.Store.Systems;
using Content.Server.Traitor.Uplink;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class StoreTests
{

    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  name: InventoryPdaDummy
  id: InventoryPdaDummy
  parent: BasePDA
  components:
  - type: Clothing
    QuickEquip: false
    slots:
    - idcard
  - type: Pda
";

    //todo: reimplement before merge

}
