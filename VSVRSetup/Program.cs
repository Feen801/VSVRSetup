using System;
using System.Collections.Generic;
using AssetsTools.NET.Extra;
using AssetsTools.NET;
using System.IO;

namespace VSVRSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            string thisDir = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Length < 1)
            {
                Console.WriteLine("You need to drag \"Virtual Succubus.exe\" onto this exe!");
            }
            else
            {
                DirectoryInfo parentDir = Directory.GetParent(args[0]);
                string currentDir = parentDir.FullName;
                string dataPath = "\\Virtual Succubus_Data\\data.unity3d";
                Console.WriteLine($"Patching in VR devices in {currentDir}");
                try
                {
                    AssetsManager assetsManager = new AssetsManager();
                    var assetsReplacers = new List<AssetsReplacer>();
                    var bundleReplacers = new List<BundleReplacer>();
                    assetsManager.LoadClassPackage($"{thisDir}/classdata.tpk");
                    var bundleFileInstance = assetsManager.LoadBundleFile($"{currentDir}{dataPath}");
                    var bundleFile = bundleFileInstance.file;
                    var bundleFileAssetsInstance = assetsManager.LoadAssetsFileFromBundle(bundleFileInstance, 0, false);
                    var bundleFileAssets = bundleFileAssetsInstance.file;
                    Console.WriteLine($"Unity Version {bundleFileAssets.Metadata.UnityVersion}");
                    assetsManager.LoadClassDatabaseFromPackage(bundleFileAssets.Metadata.UnityVersion);
                    foreach (var buildSettings in bundleFileAssets.GetAssetsOfType(AssetClassID.BuildSettings))
                    {
                        var buildSettingsField = assetsManager.GetBaseField(bundleFileAssetsInstance, buildSettings);
                        var vrDevices = buildSettingsField["enabledVRDevices.Array"];
                        vrDevices.Children.Clear();
                        var newArrayItem = ValueBuilder.DefaultValueFieldFromArrayTemplate(vrDevices);
                        newArrayItem.AsString = "Oculus";
                        vrDevices.Children.Add(newArrayItem);
                        newArrayItem = ValueBuilder.DefaultValueFieldFromArrayTemplate(vrDevices);
                        newArrayItem.AsString = "OpenVR";
                        vrDevices.Children.Add(newArrayItem);
                        newArrayItem = ValueBuilder.DefaultValueFieldFromArrayTemplate(vrDevices);
                        newArrayItem.AsString = "None";
                        vrDevices.Children.Add(newArrayItem);
                        assetsReplacers.Add(new AssetsReplacerFromMemory(bundleFileAssets, buildSettings, buildSettingsField));
                    }

                    bundleReplacers.Add(new BundleReplacerFromAssets(bundleFileAssetsInstance.name, null, bundleFileAssets, assetsReplacers));

                    using (AssetsFileWriter writer = new AssetsFileWriter(($"{currentDir}{dataPath}.mod")))
                    {
                        bundleFile.Write(writer, bundleReplacers);
                        writer.Close();
                    }
                    bundleFileInstance.BundleStream.Close();

                    File.Move($"{currentDir}{dataPath}", $"{currentDir}{dataPath}.bak");
                    File.Move($"{currentDir}{dataPath}.mod", $"{currentDir}{dataPath}");


                    File.Copy($"{thisDir}/Plugins/AudioPluginOculusSpatializer.dll", $"{currentDir}/Virtual Succubus_Data/Plugins/x86_64/AudioPluginOculusSpatializer.dll");
                    File.Copy($"{thisDir}/Plugins/OculusXRPlugin.dll", $"{currentDir}/Virtual Succubus_Data/Plugins/x86_64/OculusXRPlugin.dll");
                    File.Copy($"{thisDir}/Plugins/openvr_api.dll", $"{currentDir}/Virtual Succubus_Data/Plugins/x86_64/openvr_api.dll");
                    File.Copy($"{thisDir}/Plugins/OVRGamepad.dll", $"{currentDir}/Virtual Succubus_Data/Plugins/x86_64/OVRGamepad.dll");
                    File.Copy($"{thisDir}/Plugins/OVRPlugin.dll", $"{currentDir}/Virtual Succubus_Data/Plugins/x86_64/OVRPlugin.dll");
                    File.Copy($"{thisDir}/Plugins/UnityMockHMD.dll", $"{currentDir}/Virtual Succubus_Data/Plugins/x86_64/UnityMockHMD.dll");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("If you're getting an error, it may be because you already ran VR setup for this copy of Virtual Succubus.");
                }
            }
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
