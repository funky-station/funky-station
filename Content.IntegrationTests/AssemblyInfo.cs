// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

[assembly: Parallelizable(ParallelScope.Children)]

// I don't know why this parallelism limit was originally put here.
// I *do* know that I tried removing it, and ran into the following .NET runtime problem:
// https://github.com/dotnet/runtime/issues/107197
// So we can't really parallelize integration tests harder either until the runtime fixes that,
// *or* we fix serv3 to not spam expression trees.
[assembly: LevelOfParallelism(3)]
