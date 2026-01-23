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
            // Domain Reload 無効時は既に初期化済みの可能性がある
            if (PythonEngine.IsInitialized)
            {
                return;
            }
#else
            Application.quitting += PythonShutdown;
#endif
            Initialize();
        }

#if UNITY_EDITOR
        private static bool IsDomainReloadDisabled()
        {
            // Enter Play Mode Options が有効、かつ Domain Reload が無効な場合は true
            return UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
                   (UnityEditor.EditorSettings.enterPlayModeOptions & UnityEditor.EnterPlayModeOptions.DisableDomainReload) != 0;
        }

        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            if (IsDomainReloadDisabled())
            {
                // Domain Reload が無効の場合、Python モジュールをリロード
                ReloadPythonModules();
            }
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                // Domain Reload が有効/無効に関わらず Shutdown は呼ばない
                // - 有効: Unity が Domain をアンロードする際にクリーンアップ
                // - 無効: 次の Play Start で ReloadPythonModules() でリロード
            }
        }

        private static void ReloadPythonModules()
        {
            var myProject = $"{Application.streamingAssetsPath}/{MyProject}".Replace("\\", "/");
            var testProject = $"{Application.streamingAssetsPath}/{TestProject}".Replace("\\", "/");

            using (Py.GIL())
            {
                // myproject, test_project 配下のモジュールを sys.modules から削除
                // 次の import 時に最新のコードが読み込まれる
                PythonEngine.Exec($@"
import sys
paths = ['{myProject}', '{testProject}']
to_delete = []
for name, module in sys.modules.items():
    file_path = getattr(module, '__file__', None)
    if file_path:
        file_path = file_path.replace('\\\\', '/')
        if any(file_path.startswith(p) for p in paths):
            to_delete.append(name)
for name in to_delete:
    del sys.modules[name]
");
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
