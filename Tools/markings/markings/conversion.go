// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

package markings

func accessories_to_markings(accessories []SpriteAccessoryPrototype) ([]MarkingPrototype, error) {
	res := make([]MarkingPrototype, 0)

	for _, v := range accessories {
		marking, err := v.toMarking()
		if err != nil {
			return nil, err
		}

		res = append(res, *marking)
	}

	return res, nil
}
