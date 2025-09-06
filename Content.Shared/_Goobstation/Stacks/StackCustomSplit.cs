// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Stacks
{
    [Serializable, NetSerializable]
    public sealed class StackCustomSplitAmountMessage : BoundUserInterfaceMessage
    {
        public int Amount;

        public StackCustomSplitAmountMessage(int amount)
        {
            Amount = amount;
        }
    }

    [Serializable, NetSerializable]
    public enum StackCustomSplitUiKey
    {
        Key,
    }
}
