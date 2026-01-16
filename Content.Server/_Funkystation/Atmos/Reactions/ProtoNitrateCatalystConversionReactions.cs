// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Database.Migrations.Postgres;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server._Funkystation.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class ProtoNitrateConversionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initPN = mixture.GetMoles(Gas.ProtoNitrate);
        var initTrit = mixture.GetMoles(Gas.Tritium);
        var initH2 = mixture.GetMoles(Gas.Hydrogen);

        var temperature = mixture.Temperature;

        //Equal amounts of Trit and H2 will slowly gravitate twards certain tempertures. Imbalance in the mix will lean it endo or exo.
        var burnedH2 = initH2 * (.25f * float.Sin(temperature / 250f - 1.2f) + .25f);
        var burnedTrit = initTrit * (-.25f * float.Sin(temperature / 250f - 1.2f) + .25f);
        var producedAmount = burnedH2 + burnedTrit;

        if (initTrit - burnedTrit < 0 && initH2 - burnedH2 < 0 || initPN - producedAmount * 0.01f < 0)
            return ReactionResult.NoReaction;


        mixture.AdjustMoles(Gas.ProtoNitrate, -producedAmount * 0.01f);
        mixture.AdjustMoles(Gas.Plasma, producedAmount * .05f);
        mixture.AdjustMoles(Gas.Tritium, burnedH2 * .95f - burnedTrit);
        mixture.AdjustMoles(Gas.Hydrogen, burnedTrit * .95f - burnedH2);

        var energyReleased = (burnedTrit * .95f - burnedH2 * .95f) * Atmospherics.ProtoNitrateConversionEnergy + producedAmount * 2.5f;

        //This reaction was planned to emit .025 rads per mol of H2 and Trit burned. Unfortunatly it seems that is far above my skillset. Perhaps one day.

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}
