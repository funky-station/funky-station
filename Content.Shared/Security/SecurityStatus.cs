// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 B_Kirill <cool.bkirill@yandex.ru>
// SPDX-FileCopyrightText: 2025 Ilya Mikheev <me@ilyamikcoder.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Security;

/// <summary>
/// Status used in Criminal Records.
///
/// None - the default value
/// Suspected - the person is suspected of doing something illegal
/// Search - Funkystation: the person must be searched by Security
/// Wanted - the person is being wanted by security
/// Detained - the person is detained by security
/// Paroled - the person is on parole
/// Discharged - the person has been released from prison
/// Incapacitated - Funkystation: rendered unable to act via non-incarceration means
/// </summary>
public enum SecurityStatus : byte
{
    None,
    Suspected,
    Search, // Funkystation
    Wanted,
    Detained,
    Paroled,
    Discharged,
    Incapacitated // Funkystation
}
