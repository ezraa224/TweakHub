using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GenerateTweaks
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var outDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "data");
                var total = args.Length > 1 && int.TryParse(args[1], out var t) ? Math.Max(100, t) : 10000;
                Directory.CreateDirectory(outDir);

                var categories = new[] { "Registry", "Network", "UI", "Power", "Input", "Services", "Security", "Game", "Debug", "Telemetry" };
                var tweaks = new List<object>();
                var rnd = new Random(12345);

                int id = 1;
                foreach (var cat in categories)
                {
                    int perCat = total / categories.Length;
                    for (int i = 0; i < perCat; i++)
                    {
                        var tweakId = $"autogen-{id:D6}";
                        var title = $"{cat} tweak #{i + 1}";
                        var risky = cat == "Network" || cat == "Power" || cat == "Services";
                        object op;
                        if (cat == "Registry")
                        {
                            op = new {
                                registry = new {
                                    hive = (i % 2 == 0) ? "HKCU" : "HKLM",
                                    subKey = $@"Software\TweakHub\AutoGen\{cat}\{i+1}",
                                    valueName = $"AutoVal{i+1}",
                                    valueData = (rnd.Next(0,3)).ToString(),
                                    valueKind = "String"
                                }
                            };
                        }
                        else if (cat == "Network")
                        {
                            op = new {
                                commands = new[] {
                                    new { program = "netsh", args = $"int tcp set global autotuninglevel={(i%2==0? "normal":"disabled")}" }
                                }
                            };
                        }
                        else if (cat == "Power")
                        {
                            op = new {
                                commands = new[] {
                                    new { program = "powercfg", args = $"-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMAX {rnd.Next(50,100)}" }
                                }
                            };
                        }
                        else
                        {
                            op = new {
                                registry = new {
                                    hive = "HKCU",
                                    subKey = $@"Control Panel\AutoGen\{cat}",
                                    valueName = $"Setting{i+1}",
                                    valueData = "0",
                                    valueKind = "String"
                                }
                            };
                        }

                        tweaks.Add(new {
                            id = tweakId,
                            title,
                            description = $"Auto-generated {cat} tweak #{i+1}. SIMULATED by default; inspect before enabling.",
                            category = cat,
                            risk = risky ? 4 : 1,
                            simulated = true,
                            requiresReboot = false,
                            operations = op,
                            tags = new[] { cat.ToLower(), "autogen" }
                        });

                        id++;
                    }
                }

                var outFile = Path.Combine(outDir, "tweaks.json");
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(outFile, JsonSerializer.Serialize(tweaks, opts));
                Console.WriteLine($"Wrote {tweaks.Count} tweaks to {outFile}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex);
                return 2;
            }
        }
    }
}
