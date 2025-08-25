// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 hivehum <ketchupfaced@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

package markings

var accessoryLayerMapping map[string]string

const (
	// Prototype type.
	Marking = "marking"

	// Marking categories/humanoid visual layers.
	Hair       = "Hair"
	FacialHair = "FacialHair"

	// SpriteAccessory categories.
	HumanHair       = "HumanHair"
	HumanFacialHair = "HumanFacialHair"
	VoxFacialHair   = "VoxFacialHair"
	VoxHair         = "VoxHair"
	ThavenHair      = "ThavenHair" // DeltaV
)

func init() {
	accessoryLayerMapping = make(map[string]string)
	accessoryLayerMapping[HumanHair] = Hair
	accessoryLayerMapping[HumanFacialHair] = FacialHair
	accessoryLayerMapping[VoxFacialHair] = FacialHair
	accessoryLayerMapping[VoxHair] = Hair
	accessoryLayerMapping[ThavenHair] = Hair // DeltaV
}

type SpriteAccessoryPrototype struct {
	Type       string          `yaml:"type"`
	Categories string          `yaml:"categories"`
	Id         string          `yaml:"id"`
	Sprite     SpriteSpecifier `yaml:"sprite"`
}

func (s *SpriteAccessoryPrototype) toMarking() (*MarkingPrototype, error) {
	sprites := []SpriteSpecifier{s.Sprite}

	var category string
	category = accessoryLayerMapping[s.Categories]
	// isMatching := false
	/*
		for _, v := range s.Categories {
			if len(category) == 0 {
				category = accessoryLayerMapping[v]
			}

			isMatching = accessoryLayerMapping[v] == category

			if !isMatching {
				return nil, errors.New("sprite accessory prototype has differing accessory categories")
			}
		}
	*/

	return &MarkingPrototype{
		Type:            Marking,
		Id:              s.Id,
		BodyPart:        category,
		MarkingCategory: category,
		Sprites:         sprites,
	}, nil
}

type MarkingPrototype struct {
	Type               string            `yaml:"type"`
	Id                 string            `yaml:"id"`
	BodyPart           string            `yaml:"bodyPart"`
	MarkingCategory    string            `yaml:"markingCategory"`
	SpeciesRestriction []string          `yaml:"speciesRestriction,omitempty"`
	Sprites            []SpriteSpecifier `yaml:"sprites"`
	Shader             string?           `yaml:"shader"` // impstation
}

type SpriteSpecifier struct {
	Sprite string `yaml:"sprite"`
	State  string `yaml:"state"`
}
