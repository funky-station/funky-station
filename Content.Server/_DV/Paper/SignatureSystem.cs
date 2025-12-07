// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Crayon;
using Content.Shared._DV.Paper;
using Content.Shared._DV.Traits;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._DV.Paper;

public sealed class SignatureSystem : SharedSignatureSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignatureWriterComponent, SignAttemptEvent>(OnSignAttempt);
    }

    private void OnSignAttempt(Entity<SignatureWriterComponent> ent, ref SignAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var paper = args.Paper;
        var signer = args.User;
        var pen = args.Pen;
        var paperComp = args.Paper.Comp;
        var signatureComp = ent.Comp;

        var signatureName = DetermineEntitySignature(signer);
        var signatureColor = signatureComp.Color;
        var signatureFont = "Default"; // Noto Sans as fallback

        if (signatureComp.Font is { } penFont)
            signatureFont = penFont;
        else if (TryComp<SignatureFontComponent>(signer, out var signerComp) && signerComp.Font is { } signerFont)
            signatureFont = signerFont;

        if (TryComp<CrayonComponent>(pen, out var crayon))
            signatureColor = crayon.Color;

        var stampInfo = new StampDisplayInfo()
        {
            StampedName = signatureName,
            StampedColor = signatureColor,
            HasIcon = false,
            StampFont = signatureFont
        };

        // TODO: remove redunant contains check when TryStamp isnt a meme
        if (paperComp.StampedBy.Contains(stampInfo) || !_paper.TryStamp(paper, stampInfo, SignatureStampState))
        {
            // Show an error popup.
            _popup.PopupEntity(Loc.GetString("paper-signed-failure", ("target", paper.Owner)), signer, signer, PopupType.SmallCaution);

            args.Cancelled = true;
            return;
        }

        // Show popups and play a paper writing sound
        var signedOtherMessage = Loc.GetString("paper-signed-other", ("user", signer), ("target", paper.Owner));
        _popup.PopupEntity(signedOtherMessage, signer, Filter.PvsExcept(signer, entityManager: EntityManager), true);

        var signedSelfMessage = Loc.GetString("paper-signed-self", ("target", paper.Owner));
        _popup.PopupEntity(signedSelfMessage, signer, signer);

        _audio.PlayEntity(paperComp.Sound, Filter.Pvs(signer), signer, true);
    }
}
