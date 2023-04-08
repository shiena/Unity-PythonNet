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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PythonInitialize()
        {
#if UNITY_EDITOR
            void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange change)
            {
                if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    Shutdown();
                    UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                }
            }

            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Application.quitting += Shutdown;
#endif
            Initialize();
        }

        public static void Initialize(string appendPythonPath = "")
        {
            var pythonHome = $"{Application.streamingAssetsPath}/{PythonFolder}";
            var appendPath = string.IsNullOrWhiteSpace(appendPythonPath) ? string.Empty : $"{Application.streamingAssetsPath}/{appendPythonPath}";
            var pythonPath = string.Join(";",
                $"{appendPath}",
                $"{pythonHome}/Lib/site-packages",
                $"{pythonHome}/{PythonZip}",
                $"{pythonHome}"
            );

            var scripts = $"{pythonHome}/Scripts";

            var path = Environment.GetEnvironmentVariable("PATH")?.TrimEnd(';');
            path = string.IsNullOrEmpty(path) ? $"{pythonHome};{scripts}" : path + ";" + $"{pythonHome};{scripts}";
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
