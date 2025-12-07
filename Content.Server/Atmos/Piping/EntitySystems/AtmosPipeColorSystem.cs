// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.Piping.Components;
using Content.Shared.Atmos.Piping;
using Robust.Server.GameObjects;

namespace Content.Server.Atmos.Piping.EntitySystems
{
    public sealed class AtmosPipeColorSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AtmosPipeColorComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<AtmosPipeColorComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, AtmosPipeColorComponent component, ComponentStartup args)
        {
            if (!EntityManager.TryGetComponent(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, PipeColorVisuals.Color, component.Color, appearance);
        }

        private void OnShutdown(EntityUid uid, AtmosPipeColorComponent component, ComponentShutdown args)
        {
            if (!EntityManager.TryGetComponent(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, PipeColorVisuals.Color, Color.White, appearance);
        }

        public void SetColor(EntityUid uid, AtmosPipeColorComponent component, Color color)
        {
            component.Color = color;

            if (!EntityManager.TryGetComponent(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, PipeColorVisuals.Color, color, appearance);

            var ev = new AtmosPipeColorChangedEvent(color);
            RaiseLocalEvent(uid, ref ev);
        }
    }
}
