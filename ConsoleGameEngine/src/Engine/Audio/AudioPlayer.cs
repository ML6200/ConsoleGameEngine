using System;
using System.Diagnostics;
using ConsoleGameEngine.Engine.System;
using NAudio.Wave;
using NLog;

namespace SimpleDoomEngine.Engine;

public static class AudioPlayer
{
    private static Logger _logger = LogManager.GetCurrentClassLogger();
    private static Process? _musicProcess;
    private static WaveOutEvent outputDevice;
    private static Mp3FileReader mp3FileReader;

    public static void PlaySound(string filePath)
    {
        try
        {
            if (SystemInfo.Os.IsMacOsX())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = filePath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            else if (SystemInfo.Os.IsWindows())
            {
                Mp3FileReader mp = new Mp3FileReader(filePath);
                WaveOutEvent soundOut = new WaveOutEvent();
                soundOut.Init(mp);
                soundOut.Play();
            }
        }
        catch
        {
            _logger.Warn("Cant play sound: " + filePath);
        }
    }

    public static void PlayMusic(string filePath)
    {
        StopMusic();
        try
        {
            if (SystemInfo.Os.IsMacOsX())
            {
                _musicProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = filePath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            else if (SystemInfo.Os.IsWindows())
            {
                mp3FileReader = new Mp3FileReader(filePath);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(mp3FileReader);
                outputDevice.Play();
            }
        }
        catch
        {
            _logger.Warn("Cant play music: " + filePath);
        }
    }

    public static void StopMusic()
    {
        try
        {
            if (SystemInfo.Os.IsMacOsX())
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
            } else if (SystemInfo.Os.IsWindows())
            {
                if (outputDevice != null)
                    outputDevice.Stop();
            }
        }
        catch (Exception e)
        {
            _logger.Error("Error stopping music: " + e.Message);
        }
    }
}