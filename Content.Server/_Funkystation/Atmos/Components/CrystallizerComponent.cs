// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Server._Funkystation.Atmos.EntitySystems;
using System.Linq;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class CrystallizerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public string? SelectedRecipeId { get; set; }

        [ViewVariables(VVAccess.ReadWrite)]
        public float GasInput { get; set; }

        [DataField("inlet")]
        public string InletName = "inlet";

        [DataField("regulator")]
        public string RegulatorName = "regulator";

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("crystallizerGasMixture")]
        public GasMixture CrystallizerGasMixture { get; set; } = new();

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("progressBar")]
        public float ProgressBar { get; set; } = 0f;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("qualityLoss")]
        public float QualityLoss { get; set; } = 0f;

        [ViewVariables]
        [DataField("totalRecipeMoles")]
        public float TotalRecipeMoles { get; set; } = 0f;
    }
}