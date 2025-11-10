// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2020 Vince <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Client.Camera;

public sealed class CameraRecoilSystem : SharedCameraRecoilSystem
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;

    private float _intensity;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CameraKickEvent>(OnCameraKick);

        Subs.CVar(_configManager, CCVars.ScreenShakeIntensity, OnCvarChanged, true);
    }

    private void OnCvarChanged(float value)
    {
        _intensity = value;
    }

    private void OnCameraKick(CameraKickEvent ev)
    {
        KickCamera(GetEntity(ev.NetEntity), ev.Recoil);
    }

    public override void KickCamera(EntityUid uid, Vector2 recoil, CameraRecoilComponent? component = null)
    {
        if (_intensity == 0)
            return;

        if (!Resolve(uid, ref component, false))
            return;

        // Validate input recoil vector
        if (!float.IsFinite(recoil.X) || !float.IsFinite(recoil.Y))
            return;

        recoil *= _intensity;

        // Validate recoil after intensity multiplication
        if (!float.IsFinite(recoil.X) || !float.IsFinite(recoil.Y))
            return;

        // Reset CurrentKick if it contains invalid values
        if (!float.IsFinite(component.CurrentKick.X) || !float.IsFinite(component.CurrentKick.Y))
            component.CurrentKick = Vector2.Zero;

        // Use really bad math to "dampen" kicks when we're already kicked.
        var existing = component.CurrentKick.Length();
        if (!float.IsFinite(existing))
            existing = 0f;

        var dampen = existing / KickMagnitudeMax;
        if (!float.IsFinite(dampen))
            dampen = 0f;

        component.CurrentKick += recoil * (1 - dampen);

        // Validate after addition
        if (!float.IsFinite(component.CurrentKick.X) || !float.IsFinite(component.CurrentKick.Y))
        {
            component.CurrentKick = Vector2.Zero;
            return;
        }

        var currentLength = component.CurrentKick.Length();
        if (currentLength > KickMagnitudeMax && float.IsFinite(currentLength))
        {
            var normalized = component.CurrentKick.Normalized();
            // Only use normalized if it's valid
            if (float.IsFinite(normalized.X) && float.IsFinite(normalized.Y))
            {
                component.CurrentKick = normalized * KickMagnitudeMax;
            }
            else
            {
                component.CurrentKick = Vector2.Zero;
            }
        }

        component.LastKickTime = 0;
    }
}
