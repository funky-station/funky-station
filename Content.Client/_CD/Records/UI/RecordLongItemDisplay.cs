// SPDX-FileCopyrightText: 2025 Lyndomen <49795619+Lyndomen@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._CD.Records.UI;

/// <summary>
/// Widget that displays the record on one line if it is short enough, and on two lines if it is
/// too long. This should only be used if you know the length may be long enough to break things when
/// using a normal Label.
/// </summary>
public sealed class RecordLongItemDisplay : BoxContainer
{
    private const int MaxShortLength = 64;

    public string? Title
    {
        get => _titleLabel.Text;
        set => _titleLabel.Text = value;
    }

    // Row containing the title and short value
    private readonly BoxContainer _firstRow = new()
    {
        Orientation = LayoutOrientation.Horizontal,
        HorizontalExpand = true
    };
    // Row containing the long value
    private readonly BoxContainer _secondRow = new()
    {
        Orientation = LayoutOrientation.Horizontal,
        HorizontalExpand = true,
        Visible = false,
    };

    private readonly RichTextLabel _titleLabel = new() { Margin = new Thickness(0, 0, 5, 0) };
    private readonly RichTextLabel _shortContents = new() { Visible = true, };
    private readonly RichTextLabel _longContents = new() { HorizontalExpand = true, Margin = new Thickness(10, 0, 0, 0) };

    public RecordLongItemDisplay()
    {
        Orientation = LayoutOrientation.Vertical;
        _firstRow.AddChild(_titleLabel);
        _firstRow.AddChild(_shortContents);
        AddChild(_firstRow);
        _secondRow.AddChild(_longContents);
        AddChild(_secondRow);
    }

    public void SetValue(string s)
    {
        if (s.Length > MaxShortLength)
        {
            _longContents.Text = s;
            _secondRow.Visible = true;
            _shortContents.Visible = false;
        }
        else
        {
            _shortContents.Text = s;
            _shortContents.Visible = true;
            _secondRow.Visible = false;
        }
    }
}
