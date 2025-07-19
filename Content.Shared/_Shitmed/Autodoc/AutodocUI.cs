// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 amatwiedle <amatwiedle@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Autodoc;

[Serializable, NetSerializable]
public enum AutodocUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AutodocCreateProgramMessage(string title) : BoundUserInterfaceMessage
{
    public readonly string Title = title;
}

[Serializable, NetSerializable]
public sealed class AutodocToggleProgramSafetyMessage(int program) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
}

[Serializable, NetSerializable]
public sealed class AutodocRemoveProgramMessage(int program) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
}

[Serializable, NetSerializable]
public sealed class AutodocAddStepMessage(int program, IAutodocStep step, int index) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
    public readonly IAutodocStep Step = step;
    public readonly int Index = index;
}

[Serializable, NetSerializable]
public sealed class AutodocRemoveStepMessage(int program, int step) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
    public readonly int Step = step;
}

[Serializable, NetSerializable]
public sealed class AutodocStartMessage(int program) : BoundUserInterfaceMessage
{
    public readonly int Program = program;
}

[Serializable, NetSerializable]
public sealed class AutodocStopMessage : BoundUserInterfaceMessage;


[Serializable, NetSerializable]
public sealed class AutodocSanitizeMessage : BoundUserInterfaceMessage;