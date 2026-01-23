using Python.Runtime;
using System;
using UnityEngine;

namespace UnityPython
{
    public static class PythonLifeCycle
    {
        private const string PythonFolder = "python-3.11.3-embed-amd64";
        private const string PythonDll = "python311.dll";
        private const string PythonZip = "python311.zip";
        private const string MyProject = "myproject";
        private const string TestProject = "test_project";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PythonInitialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Application.quitting += PythonShutdown;
#endif
            Initialize();
        }

#if UNITY_EDITOR
        private static bool IsDomainReloadEnabled()
        {
            // Enter Play Mode Options が無効、または Domain Reload が有効な場合は true
            return !UnityEditor.EditorSettings.enterPlayModeOptionsEnabled ||
                   (UnityEditor.EditorSettings.enterPlayModeOptions & UnityEditor.EnterPlayModeOptions.DisableDomainReload) == 0;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                if (!IsDomainReloadEnabled())
                {
                    // Domain Reload が無効の場合のみ明示的に Shutdown
                    Shutdown();
                }
            }
        }
#else
        private static void PythonShutdown()
        {
            Application.quitting -= PythonShutdown;
            Shutdown();
        }
#endif

        public static void Initialize()
        {
            var pythonHome = $"{Application.streamingAssetsPath}/{PythonFolder}";
            var myProject = $"{Application.streamingAssetsPath}/{MyProject}";
            var testProject = $"{Application.streamingAssetsPath}/{TestProject}";
            var pythonPath = string.Join(";",
                $"{myProject}",
#if UNITY_EDITOR
                $"{testProject}",
#endif
                $"{pythonHome}/Lib/site-packages",
                $"{pythonHome}/{PythonZip}",
                $"{pythonHome}"
            );

            var scripts = $"{pythonHome}/Scripts";

            var path = Environment.GetEnvironmentVariable("PATH")?.TrimEnd(';');
            path = string.IsNullOrEmpty(path) ? $"{pythonHome};{scripts}" : $"{pythonHome};{scripts};{path}";
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", $"{pythonHome}/Lib", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", $"{pythonHome}/{PythonDll}", EnvironmentVariableTarget.Process);
#if UNITY_EDITOR
            Environment.SetEnvironmentVariable("PYTHONDONTWRITEBYTECODE", "1", EnvironmentVariableTarget.Process);
#endif

            PythonEngine.PythonHome = pythonHome;
            PythonEngine.PythonPath = pythonPath;

            PythonEngine.Initialize();
        }

        public static void Shutdown()
        {
            PythonEngine.Shutdown();
        }
    }
}
