using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ConsoleGameEngine.Engine.System;
using NAudio.Wave;

namespace SimpleDoomEngine.Engine;

public static class AudioPlayer
{
    private static Process? _musicProcess;
    private static WaveOutEvent outputDevice;
    private static Mp3FileReader mp3FileReader;

    public static void PlaySound(string filePath)
    {
        try
        {
            if (SystemInfo.OS.IsMacOsX())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = filePath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            else if (SystemInfo.OS.IsWindows())
            {
                Mp3FileReader mp = new Mp3FileReader(filePath);
                WaveOutEvent soundOut = new WaveOutEvent();
                soundOut.Init(mp);
                soundOut.Play();
            }
        }
        catch
        {
        }
    }

    public static void PlayMusic(string filePath)
    {
        StopMusic();
        try
        {
            if (SystemInfo.OS.IsMacOsX())
            {
                _musicProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = filePath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            else if (SystemInfo.OS.IsWindows())
            {
                mp3FileReader = new Mp3FileReader(filePath);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(mp3FileReader);
                outputDevice.Play();
            }
        }
        catch
        { }
    }

    public static void StopMusic()
    {
        try
        {
            if (SystemInfo.OS.IsMacOsX())
            {
                if (_musicProcess != null)
                {
                    try
                    {
                        if (!_musicProcess.HasExited)
                        {
                            _musicProcess.Kill();
                            _musicProcess.Dispose();
                            _musicProcess = null;
                        }
                    }
                    catch
                    {
                    }
                }
            } else if (SystemInfo.OS.IsWindows())
            {
                if (outputDevice != null)
                    outputDevice.Stop();
            }
        }
        catch (Exception e)
        {
            throw new ("Error stopping music: " + e.Message);
        }
    }
}