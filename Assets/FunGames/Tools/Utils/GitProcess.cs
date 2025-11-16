using System;
using System.Diagnostics;
using System.IO;

namespace FunGames.Tools.Utils
{
    public class GitProcess
    {
        private ProcessStartInfo _gitProcessInfo;
        private string _command;
        private Action<bool> _ended;

        public event Action<bool> OnThreadEnded
        {
            add => _ended += value;
            remove => _ended -= value;
        }

        public GitProcess(string command, string repositoryPath)
        {
            _command = command;
            _gitProcessInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = _command,
                WorkingDirectory = repositoryPath,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        public string Run()
        {
            Process process = new Process();
            process.StartInfo = _gitProcessInfo;
            try
            {
                process.Start();
                using StreamReader reader = process.StandardOutput;
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log(result);
                // if (!process.HasExited) process.Kill();
                return result;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Source);
                UnityEngine.Debug.LogError(e.Message);
                UnityEngine.Debug.LogError(e.StackTrace);
                throw new System.Exception("Error running git command: " + _command + "\n" + e.Message);
            }
            finally
            {
                // process.Dispose();
                // if (!process.HasExited) process.Kill();
                // process.Close();
                // process.Dispose();
                // _ended?.Invoke(true);
                // _ended = null;
            }
        }

        private void KillProcess(Process process)
        {
            if (!process.HasExited) process.Kill();
        }

        public void Kill()
        {
            // if (!process.HasExited) process.Kill();
        }
    }
}