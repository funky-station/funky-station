// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Threading;
using System.Threading.Tasks;
using Robust.Client.Graphics;
using Robust.Shared.Graphics;

namespace Content.Client.Parallax.Data
{
    [ImplicitDataDefinitionForInheritors]
    public partial interface IParallaxTextureSource
    {
        /// <summary>
        /// Generates or loads the texture.
        /// Note that this should be cached, but not necessarily *here*.
        /// </summary>
        Task<Texture> GenerateTexture(CancellationToken cancel = default);
    }
}

