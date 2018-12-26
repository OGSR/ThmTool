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

        [Option(ShortName = "p")]
        public string SourcePath { get; set; }

        [Option(ShortName = "m")]
        public string Mode { get; set; }

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

            if (string.IsNullOrWhiteSpace(Mode))
            {
                Mode = "unpack";
            }

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };

            if (Mode == "pack")
            {
                var adapter = new ThmAdapter();

                foreach (string jsonFile in Directory.GetFiles(SourcePath, "*.thm.json"))
                {
                    ThmAdapter.ETextureParams thm = JsonConvert.DeserializeObject<ThmAdapter.ETextureParams>(File.ReadAllText(jsonFile), jsonSerializerSettings);

                    string thmFile = Path.GetDirectoryName(jsonFile);

                    thmFile = Path.Combine(thmFile, Path.GetFileNameWithoutExtension(jsonFile).Replace(".thm", "_new.thm"));

                    adapter.Save(thm, thmFile);
                }
            }
            else if (Mode == "unpack")
            {
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

                    File.WriteAllText(jsonFile, content);
                }
            }
            else
            {
                Console.WriteLine($"Unknown mode {Mode}!");
            }
        }
    }
}
