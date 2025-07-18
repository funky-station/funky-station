// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 James Simonson <jamessimo89@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using FancyWindow = Content.Client.UserInterface.Controls.FancyWindow;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;

namespace Content.Client.Kitchen.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class MicrowaveMenu : FancyWindow
    {
        [Dependency] private readonly IGameTiming _timing = default!;

        public event Action<BaseButton.ButtonEventArgs, int>? OnCookTimeSelected;

        public ButtonGroup CookTimeButtonGroup { get; }

        public bool IsBusy;
        public TimeSpan CurrentCooktimeEnd;

        public MicrowaveMenu()
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);
            CookTimeButtonGroup = new ButtonGroup();
            InstantCookButton.Group = CookTimeButtonGroup;
            InstantCookButton.OnPressed += args =>
            {
                OnCookTimeSelected?.Invoke(args, 0);
            };

            for (var i = 1; i <= 6; i++)
            {
                var newButton = new MicrowaveCookTimeButton
                {
                    Text = (i * 5).ToString(),
                    TextAlign = Label.AlignMode.Center,
                    ToggleMode = true,
                    CookTime = (uint) (i * 5),
                    Group = CookTimeButtonGroup,
                    HorizontalExpand = true,
                };
                if (i == 4)
                {
                    newButton.StyleClasses.Add("OpenRight");
                }
                else
                {
                    newButton.StyleClasses.Add("OpenBoth");
                }
                CookTimeButtonVbox.AddChild(newButton);
                newButton.OnPressed += args =>
                {
                    OnCookTimeSelected?.Invoke(args, i);
                };
            }
        }

        public void ToggleBusyDisableOverlayPanel(bool shouldDisable)
        {
            DisableCookingPanelOverlay.Visible = shouldDisable;
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            if (!IsBusy)
                return;

            if (CurrentCooktimeEnd > _timing.CurTime)
            {
                CookTimeInfoLabel.Text = Loc.GetString("microwave-bound-user-interface-cook-time-label",
                ("time", CurrentCooktimeEnd.Subtract(_timing.CurTime).Seconds));
            }
        }

        public sealed class MicrowaveCookTimeButton : Button
        {
            public uint CookTime;
        }
    }
}
