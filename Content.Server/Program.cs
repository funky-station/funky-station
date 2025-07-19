// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Server;

namespace Content.Server
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            ContentStart.Start(args);
        }
    }
}
