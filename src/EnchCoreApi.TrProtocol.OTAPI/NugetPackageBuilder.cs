﻿using ModFramework;
using Mono.Cecil;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EnchCoreApi.TrProtocol.OTAPI
{

    [MonoMod.MonoModIgnore]
    public class NugetPackageBuilder
    {
        string GetNugetVersionFromAssembly(Assembly assembly)
            => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        string GetNugetVersionFromAssembly<TType>()
            => GetNugetVersionFromAssembly(typeof(TType).Assembly);

        public string PackageName { get; set; }
        public string NuspecPath { get; set; }

        public NugetPackageBuilder(string packageName, string nuspecPath)
        {
            PackageName = packageName;
            NuspecPath = nuspecPath;
        }

        public void Build(ModFwModder modder, string otapiVersion, string outputDir)
        {
            var nuspec_xml = File.ReadAllText(NuspecPath);

            nuspec_xml = nuspec_xml.Replace("[INJECT_OTAPI_VERSION]", otapiVersion);

            var version = GetNugetVersionFromAssembly<Patcher>();
            var gitIndex = version.IndexOf('+');
            if (gitIndex > -1)
            {
                var gitCommitSha = version[(gitIndex + 1)..];
                version = version[..gitIndex];
                nuspec_xml = nuspec_xml.Replace("[INJECT_GIT_HASH]", $" git#{gitCommitSha}");
            }
            else
            {
                nuspec_xml = nuspec_xml.Replace("[INJECT_GIT_HASH]", "");
            }
            nuspec_xml = nuspec_xml.Replace("[INJECT_VERSION]", version);

            var platforms = new[] { "net9.0" }; // relinker only does net6 currently. until there is a reason to implement it...
            var steamworks = modder.Module.AssemblyReferences.First(x => x.Name == "Steamworks.NET");
            var newtonsoft = modder.Module.AssemblyReferences.First(x => x.Name == "Newtonsoft.Json");
            var dependencies = new[]
            {
                (typeof(MessageID).Assembly.GetName().Name, Version: GetNugetVersionFromAssembly<MessageID>()),
                (typeof(ModFwModder).Assembly.GetName().Name, Version: GetNugetVersionFromAssembly<ModFwModder>()),
                (typeof(MonoMod.MonoModder).Assembly.GetName().Name, Version: typeof(MonoMod.MonoModder).Assembly.GetName().Version.ToString()),
                (typeof(MonoMod.RuntimeDetour.Hook).Assembly.GetName().Name, Version: typeof(MonoMod.RuntimeDetour.Hook).Assembly.GetName().Version.ToString()),
                (steamworks.Name, Version: steamworks.Version.ToString()),
                (newtonsoft.Name, Version: GetNugetVersionFromAssembly<Newtonsoft.Json.JsonConverter>().Split('+')[0]  ),
            };

            var xml_dependency = String.Join("", dependencies.Select(dep => $"\n\t    <dependency id=\"{dep.Name}\" version=\"{dep.Version}\" />"));
            var xml_group = String.Join("", platforms.Select(platform => $"\n\t<group targetFramework=\"{platform}\">{xml_dependency}\n\t</group>"));
            var xml_dependencies = $"<dependencies>{xml_group}\n    </dependencies>";

            nuspec_xml = nuspec_xml.Replace("[INJECT_DEPENDENCIES]", xml_dependencies);

            nuspec_xml = nuspec_xml.Replace("[INJECT_YEAR]", DateTime.UtcNow.Year.ToString());

            using (var nuspec = new MemoryStream(Encoding.UTF8.GetBytes(nuspec_xml)))
            {
                var manifest = NuGet.Packaging.Manifest.ReadFrom(nuspec, validateSchema: true);
                var packageBuilder = new NuGet.Packaging.PackageBuilder();
                packageBuilder.Populate(manifest.Metadata);

                packageBuilder.AddFiles("../../../../../", "COPYING.txt", "COPYING.txt");

                foreach (var platform in platforms)
                {
                    var dest = Path.Combine("lib", platform);
                    packageBuilder.AddFiles(outputDir, "OTAPI.dll", dest);
                    packageBuilder.AddFiles(outputDir, "OTAPI.Runtime.dll", dest);
                }

                if (File.Exists(PackageName))
                    File.Delete(PackageName);

                using (var srm = File.OpenWrite(PackageName))
                    packageBuilder.Save(srm);
            }
        }
    }

}
