// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;
using Content.Server.Entry;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class ConfigPresetTests
{
    [Test]
    public async Task TestLoadAll()
    {
        var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var resources = server.ResolveDependency<IResourceManager>();
        var config = server.ResolveDependency<IConfigurationManager>();

        await server.WaitPost(() =>
        {
            var originalCVars = new List<(string, object)>();
            foreach (var cvar in config.GetRegisteredCVars())
            {
                var value = config.GetCVar<object>(cvar);
                originalCVars.Add((cvar, value));
            }

            var originalCVarsStream = new MemoryStream();
            config.SaveToTomlStream(originalCVarsStream, config.GetRegisteredCVars());
            originalCVarsStream.Position = 0;

            var presets = resources.ContentFindFiles(EntryPoint.ConfigPresetsDir);
            Assert.Multiple(() =>
            {
                foreach (var preset in presets)
                {
                    var stream = resources.ContentFileRead(preset);
                    Assert.DoesNotThrow(() => config.LoadDefaultsFromTomlStream(stream));
                }
            });

            config.LoadDefaultsFromTomlStream(originalCVarsStream);

            foreach (var (cvar, value) in originalCVars)
            {
                config.SetCVar(cvar, value);
            }

            foreach (var originalCVar in originalCVars)
            {
                var (name, originalValue) = originalCVar;
                var newValue = config.GetCVar<object>(name);
                var originalValueType = originalValue.GetType();
                var newValueType = newValue.GetType();
                if (originalValueType.IsEnum || newValueType.IsEnum)
                {
                    originalValue = Enum.ToObject(originalValueType, originalValue);
                    newValue = Enum.ToObject(originalValueType, newValue);
                }

                if (originalValueType == typeof(float) || newValueType == typeof(float))
                {
                    originalValue = Convert.ToSingle(originalValue);
                    newValue = Convert.ToSingle(newValue);
                }

                if (!Equals(newValue, originalValue))
                    Assert.Fail($"CVar {name} was not reset to its original value.");
            }
        });

        await pair.CleanReturnAsync();
    }
}
