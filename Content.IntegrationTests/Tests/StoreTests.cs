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
