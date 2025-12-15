using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace ConsoleGameEngine.Engine.Audio;

public class AudioEngine : IDisposable
{
    private struct AudioTrack
    {
        public SoundPlayer Player;
        public StreamDataProvider Provider;
    }

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly MiniAudioEngine _engine;
    private readonly AudioPlaybackDevice _playbackDevice;
    private readonly Dictionary<string, AudioTrack> _tracks = new();
    private readonly Lock _lock = new();
    private readonly AudioFormat _format = AudioFormat.DvdHq;

    public AudioEngine()
    {
        try
        {
            _engine = new MiniAudioEngine();
            _playbackDevice = _engine.InitializePlaybackDevice(null, _format);
            _playbackDevice.Start();

            _logger.Info("SoundFlow audio engine initialized");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to initialize audio: {0}", e.Message);
            throw;
        }
    }

    public async Task Play(string file, string id, bool stopIfPlaying = false, bool loop = false)
    {
        if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(id))
        {
            _logger.Warn("Play called with empty file or id");
            return;
        }

        try
        {
            if (stopIfPlaying)
            {
                lock (_lock)
                {
                    if (_tracks.ContainsKey(id))
                        Stop(id);
                }
            }

            var (stream, dataProvider, player) = await Task.Run(() =>
            {
                var fileStream = File.OpenRead(file);
                var provider = new StreamDataProvider(_engine, fileStream);
                var soundPlayer = new SoundPlayer(_engine, _format, provider)
                {
                    IsLooping = loop,
                };
                return (fileStream, provider, soundPlayer);
            });

            lock (_lock)
            {
                _tracks[id] = new AudioTrack
                {
                    Player = player,
                    Provider = dataProvider
                };
                
                _playbackDevice.MasterMixer.AddComponent(player);
                player.Play();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occurred: {0}", e.Message);
        }
    }

    public void Stop(string id)
    {
        try
        {
            lock (_lock)
            {
                if (_tracks.TryGetValue(id, out var track))
                {
                    track.Player.Stop();
                    _playbackDevice.MasterMixer.RemoveComponent(track.Player);

                    track.Player.Dispose();
                    track.Provider.Dispose();
                    _tracks.Remove(id);

                    _logger.Debug("Stopped audio: {0}", id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occurred: {0}", e.Message);
        }
    }

    public void StopAll()
    {
        lock (_lock)
        {
            foreach (var track in _tracks.Values)
            {
                track.Player.Stop();
                _playbackDevice.MasterMixer.RemoveComponent(track.Player);
                track.Player.Dispose();
                track.Provider.Dispose();
            }

            _tracks.Clear();
            _logger.Debug("Stopped all audio");
        }
    }

    public void Pause(string id)
    {
        try
        {
            lock (_lock)
            {
                if (_tracks.TryGetValue(id, out var track))
                {
                    track.Player.Pause();
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occurred: {0}", e.Message);
        }
    }

    public void Resume(string id)
    {
        try
        {
            lock (_lock)
            {
                if (_tracks.TryGetValue(id, out var track))
                {
                    track.Player.Play();
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occurred: {0}", e.Message);
        }
    }

    public void SetVolume(string id, byte volume)
    {
        try
        {
            lock (_lock)
            {
                if (_tracks.TryGetValue(id, out var track))
                {
                    track.Player.Volume = volume / 100.0f;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occurred: {0}", e.Message);
        }
    }

    public void SetAllVolume(byte volume)
    {
        lock (_lock)
        {
            float vol = volume / 100.0f;
            foreach (var track in _tracks.Values)
            {
                track.Player.Volume = vol;
            }
        }
    }

    public void Dispose()
    {
        StopAll();
        _playbackDevice.Stop();
        _playbackDevice.Dispose();
        _engine.Dispose();
        _logger.Info("AudioEngine disposed");
    }
}