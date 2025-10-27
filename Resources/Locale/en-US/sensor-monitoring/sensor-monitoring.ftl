# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

sensor-monitoring-window-title = Sensor Monitoring Console

sensor-monitoring-value-display = {$unit ->
    [PressureKpa] { PRESSURE($value) }
    [PowerW] { POWERWATTS($value) }
    [EnergyJ] { POWERJOULES($value) }
    [TemperatureK] { TOSTRING($value, "N3") } K
    [Ratio] { NATURALPERCENT($value) }
    [Moles] { TOSTRING($value, "N3") } mol
    *[Other] { $value }
}

# ({ TOSTRING(SUB($value, 273.15), "N3") } °C)
