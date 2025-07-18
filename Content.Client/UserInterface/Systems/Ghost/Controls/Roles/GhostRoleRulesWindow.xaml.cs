// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Just-a-Unity-Dev <rnmangunay@addu.edu.ph>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CCVar;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Ghost.Controls.Roles
{
    [GenerateTypedNameReferences]
    public sealed partial class GhostRoleRulesWindow : DefaultWindow
    {
        [Dependency] private readonly IConfigurationManager _cfg = IoCManager.Resolve<IConfigurationManager>();
        private float _timer;

        public GhostRoleRulesWindow(string rules, Action<BaseButton.ButtonEventArgs> requestAction)
        {
            RobustXamlLoader.Load(this);
            var ghostRoleTime = _cfg.GetCVar(CCVars.GhostRoleTime);
            _timer = ghostRoleTime;

            if (ghostRoleTime > 0f)
            {
                RequestButton.Text = Loc.GetString("ghost-roles-window-request-role-button-timer", ("time", $"{_timer:0.0}"));
                TopBanner.SetMessage(FormattedMessage.FromMarkupPermissive(rules + "\n" + Loc.GetString("ghost-roles-window-rules-footer", ("time", ghostRoleTime))));
                RequestButton.Disabled = true;
            }

            RequestButton.OnPressed += requestAction;
        }


        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);
            if (!RequestButton.Disabled) return;
            if (_timer > 0.0)
            {
                _timer -= args.DeltaSeconds;
                RequestButton.Text = Loc.GetString("ghost-roles-window-request-role-button-timer", ("time", $"{_timer:0.0}"));
            }
            else
            {
                RequestButton.Disabled = false;
                RequestButton.Text = Loc.GetString("ghost-roles-window-request-role-button");
            }
        }
    }
}
