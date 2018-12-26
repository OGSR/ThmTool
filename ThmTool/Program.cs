using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ThmTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CommandLineApplication.Execute<Program>(args);
        }

        [Option(ShortName = "source")]
        public string SourcePath { get; set; }

        [Option(ShortName = "pack")]
        public bool Pack { get; set; }

        [Option(ShortName = "unpack")]
        public bool Unpack { get; set; }

        [Option(ShortName = "m")]
        public int Mode { get; set; }

        private void OnExecute()
        {
            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                SourcePath = Environment.CurrentDirectory;
            }

            if (!Directory.Exists(SourcePath))
            {
                Console.WriteLine($"Cannot find path: {SourcePath}!");
                return;
            }

            if (!Pack && !Unpack)
            {
                Unpack = true;
            }

            var jsonSerializerSettings = new JsonSerializerSettings();

            if (Mode == 0)
            {
                // bin mode
            }
            else if (Mode == 1)
            {
                // name mode
                jsonSerializerSettings.Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                };
            }

            Console.WriteLine($"Working directory: {SourcePath}.");

            if (Pack)
            {
                Console.WriteLine("Packing mode...");

                var adapter = new ThmAdapter();

                foreach (string jsonFile in Directory.GetFiles(SourcePath, "*.thm.json"))
                {
                    ThmAdapter.ETextureParams thm = JsonConvert.DeserializeObject<ThmAdapter.ETextureParams>(File.ReadAllText(jsonFile), jsonSerializerSettings);

                    string thmFile = Path.GetDirectoryName(jsonFile);

                    thmFile = Path.Combine(thmFile ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(jsonFile).Replace(".thm", "_new.thm"));

                    Console.WriteLine($"Packing {Path.GetFileName(jsonFile)} to {Path.GetFileName(thmFile)}");

                    adapter.Save(thm, thmFile);
                }
            }
            else if (Unpack)
            {
                Console.WriteLine("Unpacking mode...");

                var adapter = new ThmAdapter();

                foreach (string thmFile in Directory.GetFiles(SourcePath, "*.thm"))
                {
                    ThmAdapter.ETextureParams thm = adapter.Load(thmFile);

                    if (thm == null)
                    {
                        Console.WriteLine($"Cannot read {thmFile}, probably empty file! Skip.");
                        continue;
                    }

                    string content = JsonConvert.SerializeObject(thm, Formatting.Indented, jsonSerializerSettings);

                    var jsonFile = thmFile + ".json";
                    if (File.Exists(jsonFile))
                    {
                        File.Delete(jsonFile);
                    }

                    Console.WriteLine($"Unpacking {Path.GetFileName(thmFile)} to {Path.GetFileName(jsonFile)}");

                    File.WriteAllText(jsonFile, content);
                }
            }
            else
            {
                Console.WriteLine($"Unknown work mode!");
            }

            Console.WriteLine("DONE!");
        }
    }
}
