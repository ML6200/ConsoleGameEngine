using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConsoleGameEngine.Engine.Audio;
public class AudioEngine : IDisposable
{
    private readonly Dictionary<string, Process> _processes = new();

    public void Play(string file, string id, bool stopIfPlaying = false, bool loop = false)
    {
        if (stopIfPlaying && _processes.ContainsKey(id))
            Stop(id);

        Process process;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS built-in afplay
            process = Process.Start("afplay", loop ? $"-q 1 --loop {file}" : file);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            process = Process.Start("aplay", file);
        }
        else // Windows
        {
            // Use PowerShell
            process = Process.Start("powershell", $"-c (New-Object Media.SoundPlayer '{file}').PlaySync()");
        }

        _processes[id] = process;
    }

    public void Stop(string id)
    {
        if (_processes.TryGetValue(id, out var process))
        {
            process.Kill();
            process.Dispose();
            _processes.Remove(id);
        }
    }

    public void StopAll()
    {
        foreach (var process in _processes.Values)
        {
            process.Kill();
        }
    }

    public void Dispose()
    {
        foreach (var process in _processes.Values)
        {
            process.Kill();
            process.Dispose();
        }
        _processes.Clear();
    }
}