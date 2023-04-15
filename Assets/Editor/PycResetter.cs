using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class PycResetter
    {
        private const string GitCommand = "git.exe";
        private const string MenuItem = "Tools/Restore python files";

        private static bool _checked = true;

        static PycResetter()
        {
            Menu.SetChecked(MenuItem, _checked);
        }

        [MenuItem(MenuItem)]
        private static void RegisterMenu()
        {
            _checked = !_checked;
            Menu.SetChecked(MenuItem, _checked);
        }

        [InitializeOnEnterPlayMode]
        private static void RegisterOnPlayModeStateChanged()
        {
            if (!_checked)
            {
                return;
            }

            if (CanExecuteCommand(GitCommand)) { }
            {
                var cwd = new DirectoryInfo($"{Application.dataPath}").Parent.FullName;
                GitRestore(cwd);
                GitClean(cwd);

                void OnPlayModeStateChanged(PlayModeStateChange change)
                {
                    if (change == PlayModeStateChange.ExitingPlayMode)
                    {
                        GitRestore(cwd);
                        GitClean(cwd);
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                    }
                }

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
        }

        private static void GitRestore(string cwd)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GitCommand,
                    ArgumentList = { "restore", "Assets/StreamingAssets/python-3.11.3-embed-amd64/*" },
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardInput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                    WorkingDirectory = cwd
                }
            };
            process.Start();
            process.Dispose();
        }

        private static void GitClean(string cwd)
        {
            var targetFolders = new[]
            {
                "Assets/StreamingAssets/python-3.11.3-embed-amd64/*",
                "Assets/StreamingAssets/myproject/*",
                "Assets/StreamingAssets/test_package/*"
            };
            foreach (string targetFolder in targetFolders)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GitCommand,
                        ArgumentList = { "clean", "-dfx", targetFolder },
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        RedirectStandardInput = false,
                        RedirectStandardError = false,
                        CreateNoWindow = true,
                        WorkingDirectory = cwd
                    }
                };
                process.Start();
                process.Dispose();
            }
        }

        private static bool CanExecuteCommand(string command)
        {
            if (!command.EndsWith(".exe") && !command.EndsWith(".com"))
            {
                command += ".exe";
            }

            var path = Environment.GetEnvironmentVariable("PATH").Split(";");
            foreach (string p in path)
            {
                var fullPath = Path.Combine(p, command);
                if (File.Exists(fullPath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}