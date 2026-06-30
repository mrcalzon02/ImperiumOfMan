using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class ModuleInitializerAttribute : Attribute { }
}

namespace ImperiumReforged.Diagnostics
{
    public static class StationProbe
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object Sync = new object();
        private static readonly string[] StationIds = { "iomTerra", "iomMoom", "iomMoon", "iomPhobos", "iomHavoc", "iomVizg" };
        private static readonly string[] Tokens = { "station", "space", "planet", "solar", "location", "mission", "faction", "barter", "bram" };
        private static Timer _timer;
        private static string _outputDirectory;
        private static string _lastScene = string.Empty;
        private static int _pollCount;

        [ModuleInitializer]
        public static void Initialize()
        {
            try
            {
                string assemblyDirectory = Path.GetDirectoryName(typeof(StationProbe).Assembly.Location);
                _outputDirectory = Path.Combine(assemblyDirectory ?? AppDomain.CurrentDomain.BaseDirectory, "ImperiumReforgedDiagnostics");
                Directory.CreateDirectory(_outputDirectory);
                File.WriteAllText(Path.Combine(_outputDirectory, "HOW_TO_REQUEST_ANOTHER_DUMP.txt"),
                    "Create an empty REQUEST_DUMP.txt file here. A new report will be written within five seconds.\r\n", Encoding.UTF8);
                _timer = new Timer(_ => Poll(), null, 1000, 5000);
            }
            catch (Exception exception)
            {
                SafeError(exception);
            }
        }

        private static void Poll()
        {
            try
            {
                _pollCount++;
                Type dataType = FindType("MGSC.Data");
                if (dataType == null) return;

                string scene = GetSceneName();
                if (_pollCount == 1)
                    WriteReport("data-available");
                if (!string.IsNullOrWhiteSpace(scene) && !string.Equals(scene, _lastScene, StringComparison.Ordinal))
                {
                    _lastScene = scene;
                    WriteReport("scene-" + scene);
                }
                if (_pollCount == 6 || _pollCount == 24)
                    WriteReport("timed-" + (_pollCount * 5) + "s");

                string request = Path.Combine(_outputDirectory, "REQUEST_DUMP.txt");
                if (File.Exists(request))
                {
                    File.Delete(request);
                    WriteReport("manual-request");
                }
            }
            catch (Exception exception)
            {
                SafeError(exception);
            }
        }

        private static void WriteReport(string stage)
        {
            lock (Sync)
            {
                var report = new StringBuilder(65536);
                report.AppendLine("Imperium Reforged station diagnostics");
                report.AppendLine("UTC: " + DateTime.UtcNow.ToString("O"));
                report.AppendLine("Stage: " + stage);
                report.AppendLine("Scene: " + GetSceneName());

                Type dataType = FindType("MGSC.Data");
                report.AppendLine("MGSC.Data found: " + (dataType != null));
                if (dataType == null)
                {
                    Save(stage, report);
                    return;
                }

                object stations = ReadStaticMember(dataType, "Stations");
                object barter = ReadStaticMember(dataType, "StationBarter");
                object donor = GetRecord(stations, "Paragon");

                report.AppendLine();
                report.AppendLine("=== PARAGON DONOR ===");
                DumpObject(donor, report);

                foreach (string stationId in StationIds)
                {
                    report.AppendLine();
                    report.AppendLine("=== STATION " + stationId + " ===");
                    object station = GetRecord(stations, stationId);
                    DumpObject(station, report);
                    report.AppendLine("=== BARTER " + stationId + " ===");
                    DumpObject(GetRecord(barter, stationId), report);
                    if (station != null && donor != null)
                        CompareDefaults(donor, station, report);
                }

                report.AppendLine();
                report.AppendLine("=== RELATED DATA REGISTRIES ===");
                foreach (MemberInfo member in StaticMembers(dataType)
                    .Where(member => Tokens.Any(token => member.Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(member => member.Name))
                {
                    object registry;
                    try { registry = Read(member, null); }
                    catch (Exception exception)
                    {
                        report.AppendLine("Data." + member.Name + ": read error " + exception.GetBaseException().Message);
                        continue;
                    }

                    KeyValuePair<object, object>[] entries = Entries(registry).Take(750).ToArray();
                    report.AppendLine("Data." + member.Name + ": " + (registry == null ? "<null>" : registry.GetType().FullName) + "; entries=" + entries.Length);
                    foreach (KeyValuePair<object, object> entry in entries)
                        report.AppendLine("  " + (entry.Key ?? "<null>") + " -> " + (GetId(entry.Value) ?? (entry.Value == null ? "<null>" : entry.Value.GetType().Name)));
                }

                Save(stage, report);
            }
        }

        private static void Save(string stage, StringBuilder report)
        {
            string safe = new string(stage.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character).ToArray());
            string path = Path.Combine(_outputDirectory, "stations-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + safe + ".txt");
            File.WriteAllText(path, report.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(_outputDirectory, "LATEST_REPORT_PATH.txt"), path, Encoding.UTF8);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }

        private static string GetSceneName()
        {
            try
            {
                Type sceneManager = FindType("UnityEngine.SceneManagement.SceneManager");
                MethodInfo method = sceneManager?.GetMethod("GetActiveScene", StaticFlags, null, Type.EmptyTypes, null);
                object scene = method?.Invoke(null, null);
                return ReadNamedMember(scene, "name")?.ToString() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private static object ReadStaticMember(Type type, string name)
        {
            MemberInfo member = StaticMembers(type).FirstOrDefault(candidate => candidate.Name == name);
            return member == null ? null : Read(member, null);
        }

        private static object ReadNamedMember(object value, string name)
        {
            if (value == null) return null;
            MemberInfo member = ReadableMembers(value.GetType()).FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
            return member == null ? null : Read(member, value);
        }

        private static object GetRecord(object registry, string id)
        {
            if (registry == null) return null;
            try
            {
                MethodInfo method = registry.GetType().GetMethods(InstanceFlags)
                    .FirstOrDefault(candidate => candidate.Name == "GetRecord" && candidate.GetParameters().Length == 1 && candidate.GetParameters()[0].ParameterType == typeof(string));
                if (method != null) return method.Invoke(registry, new object[] { id });
            }
            catch { }
            return Entries(registry).FirstOrDefault(entry => string.Equals(entry.Key?.ToString(), id, StringComparison.Ordinal)).Value;
        }

        private static void DumpObject(object value, StringBuilder report)
        {
            if (value == null) { report.AppendLine("MISSING"); return; }
            report.AppendLine("Type: " + value.GetType().FullName);
            foreach (MemberInfo member in ReadableMembers(value.GetType()))
            {
                try { report.AppendLine(member.Name + " = " + Format(Read(member, value))); }
                catch (Exception exception) { report.AppendLine(member.Name + " = <error: " + exception.GetBaseException().Message + ">"); }
            }
        }

        private static void CompareDefaults(object donor, object target, StringBuilder report)
        {
            report.AppendLine("Fields empty compared with Paragon:");
            Dictionary<string, MemberInfo> donorMembers = ReadableMembers(donor.GetType()).ToDictionary(member => member.Name);
            int found = 0;
            foreach (MemberInfo targetMember in ReadableMembers(target.GetType()))
            {
                if (!donorMembers.TryGetValue(targetMember.Name, out MemberInfo donorMember)) continue;
                object donorValue;
                object targetValue;
                try { donorValue = Read(donorMember, donor); targetValue = Read(targetMember, target); }
                catch { continue; }
                if (IsEmpty(targetValue) && !IsEmpty(donorValue))
                {
                    report.AppendLine("  " + targetMember.Name + ": target=" + Format(targetValue) + " donor=" + Format(donorValue));
                    found++;
                }
            }
            if (found == 0) report.AppendLine("  none detected");
        }

        private static IEnumerable<MemberInfo> StaticMembers(Type type)
        {
            return type.GetFields(StaticFlags).Cast<MemberInfo>()
                .Concat(type.GetProperties(StaticFlags).Where(property => property.GetIndexParameters().Length == 0));
        }

        private static IEnumerable<MemberInfo> ReadableMembers(Type type)
        {
            return type.GetFields(InstanceFlags).Cast<MemberInfo>()
                .Concat(type.GetProperties(InstanceFlags).Where(property => property.CanRead && property.GetIndexParameters().Length == 0))
                .OrderBy(member => member.Name);
        }

        private static object Read(MemberInfo member, object instance)
        {
            FieldInfo field = member as FieldInfo;
            return field != null ? field.GetValue(instance) : ((PropertyInfo)member).GetValue(instance, null);
        }

        private static IEnumerable<KeyValuePair<object, object>> Entries(object registry)
        {
            if (registry == null) yield break;
            IDictionary direct = registry as IDictionary;
            if (direct != null)
            {
                foreach (DictionaryEntry entry in direct)
                    yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
                yield break;
            }
            foreach (FieldInfo field in registry.GetType().GetFields(InstanceFlags))
            {
                IDictionary dictionary;
                try { dictionary = field.GetValue(registry) as IDictionary; }
                catch { continue; }
                if (dictionary == null) continue;
                foreach (DictionaryEntry entry in dictionary)
                    yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
            }
        }

        private static string GetId(object value)
        {
            object id = ReadNamedMember(value, "Id");
            return id?.ToString();
        }

        private static string Format(object value)
        {
            if (value == null) return "<null>";
            string text = value as string;
            if (text != null) return "\"" + text + "\"";
            ICollection collection = value as ICollection;
            if (collection != null) return value.GetType().Name + "(count=" + collection.Count + ")";
            return value.ToString();
        }

        private static bool IsEmpty(object value)
        {
            if (value == null) return true;
            string text = value as string;
            if (text != null) return string.IsNullOrWhiteSpace(text);
            ICollection collection = value as ICollection;
            if (collection != null) return collection.Count == 0;
            Type type = value.GetType();
            return type.IsValueType && value.Equals(Activator.CreateInstance(type));
        }

        private static void SafeError(Exception exception)
        {
            try
            {
                string directory = _outputDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
                Directory.CreateDirectory(directory);
                File.AppendAllText(Path.Combine(directory, "probe-errors.txt"), DateTime.UtcNow.ToString("O") + " " + exception + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }
    }
}
