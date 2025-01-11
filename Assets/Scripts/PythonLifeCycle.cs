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
            Application.quitting += PythonShutdown;
            Initialize();
        }

        private static void PythonShutdown()
        {
            Application.quitting -= PythonShutdown;
            Shutdown();
        }

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
