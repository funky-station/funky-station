// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Input;
using Content.Shared.Movement.Pulling.Components;

namespace Content.IntegrationTests.Tests.Buckle;

public sealed class BuckleDragTest : InteractionTest
{
    // Check that dragging a buckled player unbuckles them.
    [Test]
    public async Task BucklePullTest()
    {
        var urist = await SpawnTarget("MobHuman");
        var sUrist = ToServer(urist);
        await SpawnTarget("Chair");

        var buckle = Comp<BuckleComponent>(urist);
        var strap = Comp<StrapComponent>(Target);
        var puller = Comp<PullerComponent>(Player);
        var pullable = Comp<PullableComponent>(urist);

#pragma warning disable RA0002
        buckle.Delay = TimeSpan.Zero;
#pragma warning restore RA0002

        // Initially not buckled to the chair and not pulling anything
        Assert.That(buckle.Buckled, Is.False);
        Assert.That(buckle.BuckledTo, Is.Null);
        Assert.That(strap.BuckledEntities, Is.Empty);
        Assert.That(puller.Pulling, Is.Null);
        Assert.That(pullable.Puller, Is.Null);
        Assert.That(pullable.BeingPulled, Is.False);

        // Strap the human to the chair
        await Server.WaitAssertion(() =>
        {
            Assert.That(Server.System<SharedBuckleSystem>().TryBuckle(sUrist, SPlayer, STarget.Value));
        });

        await RunTicks(5);
        Assert.That(buckle.Buckled, Is.True);
        Assert.That(buckle.BuckledTo, Is.EqualTo(STarget));
        Assert.That(strap.BuckledEntities, Is.EquivalentTo(new[] { sUrist }));
        Assert.That(puller.Pulling, Is.Null);
        Assert.That(pullable.Puller, Is.Null);
        Assert.That(pullable.BeingPulled, Is.False);

        // Start pulling, and thus unbuckle them
        await PressKey(ContentKeyFunctions.TryPullObject, cursorEntity: urist);
        await RunTicks(5);
        Assert.That(buckle.Buckled, Is.False);
        Assert.That(buckle.BuckledTo, Is.Null);
        Assert.That(strap.BuckledEntities, Is.Empty);
        Assert.That(puller.Pulling, Is.EqualTo(sUrist));
        Assert.That(pullable.Puller, Is.EqualTo(SPlayer));
        Assert.That(pullable.BeingPulled, Is.True);
    }
}
