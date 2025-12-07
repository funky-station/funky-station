# SPDX-FileCopyrightText: 2021 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

### Interaction Messages

# Shown when repairing something
comp-repairable-repair = You repair {PROPER($target) ->
  [true] {""}
  *[false] the{" "}
}{$target} with {PROPER($tool) ->
  [true] {""}
  *[false] the{" "}
}{$tool}
