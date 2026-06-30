using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    internal static class StationDiagnostics
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly string[] StationIds =
        {
            "iomTerra", "iomMoom", "iomMoon", "iomPhobos", "iomHavoc", "iomVizg"
        };

        private static readonly string[] DataTokens =
        {
            "station", "space", "planet", "solar", "location", "mission", "faction", "barter", "bram"
        };

        private static string _contentPath = string.Empty;
        private static bool _behaviourCreated;

        public static void Initialize(IModContext context)
        {
            _contentPath = context?.ModContentPath ?? string.Empty;
            if (_behaviourCreated)
            {
                return;
            }

            var gameObject = new GameObject("ImperiumReforged.StationDiagnostics");
            Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<StationDiagnosticsBehaviour>();
            _behaviourCreated = true;
        }

        public static void Write(string stage, Exception registrationException = null)
        {
            var report = new StringBuilder();
            report.AppendLine("Imperium Reforged station diagnostics");
            report.AppendLine($"UTC: {DateTime.UtcNow:O}");
            report.AppendLine($"Stage: {stage}");
            report.AppendLine($"ModContentPath: {_contentPath}");
            report.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            report.AppendLine($"Bundle loaded: {Plugin.ModBundle != null}");

            if (registrationException != null)
            {
                report.AppendLine("REGISTRATION EXCEPTION:");
                report.AppendLine(registrationException.ToString());
            }

            object donor = GetRecord(Data.Stations, "Paragon");
            report.AppendLine();
            report.AppendLine("=== PARAGON DONOR ===");
            DumpObject(donor, report);

            foreach (string stationId in StationIds)
            {
                report.AppendLine();
                report.AppendLine($"=== STATION {stationId} ===");
                object station = GetRecord(Data.Stations, stationId);
                DumpObject(station, report);

                report.AppendLine($"=== BARTER {stationId} ===");
                DumpObject(GetRecord(Data.StationBarter, stationId), report);

                if (station != null && donor != null)
                {
                    report.AppendLine("Fields left empty compared with Paragon:");
                    CompareDefaults(donor, station, report);
                }
            }

            report.AppendLine();
            report.AppendLine("=== RELATED DATA REGISTRIES ===");
            foreach (MemberInfo member in GetDataMembers()
                         .Where(member => DataTokens.Any(token => member.Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                         .OrderBy(member => member.Name))
            {
                object registry;
                try
                {
                    registry = Read(member, null);
                }
                catch (Exception exception)
                {
                    report.AppendLine($"Data.{member.Name}: read error {exception.GetBaseException().Message}");
                    continue;
                }

                var entries = Entries(registry).Take(500).ToArray();
                report.AppendLine($"Data.{member.Name}: {registry?.GetType().FullName ?? "<null>"}; entries={entries.Length}");
                foreach (var entry in entries)
                {
                    report.AppendLine($"  {entry.Key ?? "<null>"} -> {GetId(entry.Value) ?? entry.Value?.GetType().Name ?? "<null>"}");
                }
            }

            string directory = Path.Combine(Application.persistentDataPath, "ImperiumReforgedDiagnostics");
            Directory.CreateDirectory(directory);
            string safeStage = string.Concat(stage.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
            string path = Path.Combine(directory, $"stations-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{safeStage}.txt");
            File.WriteAllText(path, report.ToString(), Encoding.UTF8);
            Debug.Log($"[ImperiumReforged] Station diagnostics written to {path}");
        }

        private static object GetRecord(object registry, string id)
        {
            if (registry == null)
            {
                return null;
            }

            try
            {
                MethodInfo method = registry.GetType().GetMethods(InstanceFlags)
                    .FirstOrDefault(candidate => candidate.Name == "GetRecord" &&
                                                 candidate.GetParameters().Length == 1 &&
                                                 candidate.GetParameters()[0].ParameterType == typeof(string));
                if (method != null)
                {
                    return method.Invoke(registry, new object[] { id });
                }
            }
            catch
            {
                // Fall through to dictionary inspection.
            }

            return Entries(registry)
                .FirstOrDefault(entry => string.Equals(entry.Key?.ToString(), id, StringComparison.Ordinal)).Value;
        }

        private static void DumpObject(object value, StringBuilder report)
        {
            if (value == null)
            {
                report.AppendLine("MISSING");
                return;
            }

            report.AppendLine($"Type: {value.GetType().FullName}");
            foreach (MemberInfo member in ReadableMembers(value.GetType()))
            {
                try
                {
                    report.AppendLine($"{member.Name} = {Format(Read(member, value))}");
                }
                catch (Exception exception)
                {
                    report.AppendLine($"{member.Name} = <error: {exception.GetBaseException().Message}>");
                }
            }
        }

        private static void CompareDefaults(object donor, object target, StringBuilder report)
        {
            var donorMembers = ReadableMembers(donor.GetType()).ToDictionary(member => member.Name);
            int count = 0;
            foreach (MemberInfo targetMember in ReadableMembers(target.GetType()))
            {
                if (!donorMembers.TryGetValue(targetMember.Name, out MemberInfo donorMember))
                {
                    continue;
                }

                object donorValue;
                object targetValue;
                try
                {
                    donorValue = Read(donorMember, donor);
                    targetValue = Read(targetMember, target);
                }
                catch
                {
                    continue;
                }

                if (IsEmpty(targetValue) && !IsEmpty(donorValue))
                {
                    report.AppendLine($"  {targetMember.Name}: target={Format(targetValue)} donor={Format(donorValue)}");
                    count++;
                }
            }

            if (count == 0)
            {
                report.AppendLine("  none detected");
            }
        }

        private static IEnumerable<MemberInfo> GetDataMembers()
        {
            return typeof(Data).GetFields(StaticFlags).Cast<MemberInfo>()
                .Concat(typeof(Data).GetProperties(StaticFlags).Where(property => property.GetIndexParameters().Length == 0));
        }

        private static IEnumerable<MemberInfo> ReadableMembers(Type type)
        {
            return type.GetFields(InstanceFlags).Cast<MemberInfo>()
                .Concat(type.GetProperties(InstanceFlags).Where(property => property.CanRead && property.GetIndexParameters().Length == 0))
                .OrderBy(member => member.Name);
        }

        private static object Read(MemberInfo member, object instance)
        {
            if (member is FieldInfo field)
            {
                return field.GetValue(instance);
            }
            return ((PropertyInfo)member).GetValue(instance, null);
        }

        private static IEnumerable<KeyValuePair<object, object>> Entries(object registry)
        {
            if (registry == null)
            {
                yield break;
            }

            if (registry is IDictionary direct)
            {
                foreach (DictionaryEntry entry in direct)
                {
                    yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
                }
                yield break;
            }

            foreach (FieldInfo field in registry.GetType().GetFields(InstanceFlags))
            {
                if (!(field.GetValue(registry) is IDictionary dictionary))
                {
                    continue;
                }
                foreach (DictionaryEntry entry in dictionary)
                {
                    yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
                }
            }
        }

        private static string GetId(object value)
        {
            if (value == null)
            {
                return null;
            }
            MemberInfo member = ReadableMembers(value.GetType()).FirstOrDefault(candidate => candidate.Name == "Id");
            if (member == null)
            {
                return null;
            }
            try
            {
                return Read(member, value)?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static string Format(object value)
        {
            if (value == null)
            {
                return "<null>";
            }
            if (value is string text)
            {
                return $"\"{text}\"";
            }
            if (value is ICollection collection)
            {
                return $"{value.GetType().Name}(count={collection.Count})";
            }
            if (value is Object unityObject)
            {
                return $"{unityObject.GetType().Name}(name='{unityObject.name}')";
            }
            return value.ToString();
        }

        private static bool IsEmpty(object value)
        {
            if (value == null)
            {
                return true;
            }
            if (value is string text)
            {
                return string.IsNullOrWhiteSpace(text);
            }
            if (value is ICollection collection)
            {
                return collection.Count == 0;
            }
            Type type = value.GetType();
            return type.IsValueType && value.Equals(Activator.CreateInstance(type));
        }
    }

    internal sealed class StationDiagnosticsBehaviour : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                StationDiagnostics.Write("manual-F8");
            }
        }
    }

    [HarmonyPatch(typeof(Plugin), nameof(Plugin.AfterBootstrap))]
    internal static class StationDiagnosticsAfterBootstrapPatch
    {
        [HarmonyPostfix]
        private static void Postfix(IModContext context)
        {
            StationDiagnostics.Initialize(context);
            StationDiagnostics.Write("after-bootstrap");
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception, IModContext context)
        {
            if (__exception != null)
            {
                StationDiagnostics.Initialize(context);
                StationDiagnostics.Write("bootstrap-exception", __exception);
            }
            return __exception;
        }
    }
}
