using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetCoreAudio;
using NLog;

namespace SimpleDoomEngine.Engine;

public class AudioEngine : IDisposable
{
    private struct AudioTask(Task task, Player player, EventHandler? OnFinished)
    {
        public Task Task = task;
        public Player Player = player;
        public EventHandler OnFinished;
    }
    
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<string, AudioTask> _audioTasks = new();

    public void Play(string file, string id, bool stopIfPlaying = false, bool loop = false)
    {
        if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(id))
        {
            _logger.Warn("Play called with empty file or id");
            return;
        }
        
        try
        {
            if (stopIfPlaying && _audioTasks.ContainsKey(id)) 
                Stop(id);
            
            var player = new Player();
            EventHandler? onFinished = null;
            
            if (loop)
            {
                onFinished = (_, _) =>
                {
                    if (_audioTasks.TryGetValue(id, out var currentTask) 
                        && player == currentTask.Player)
                    {
                        player.Stop();
                        var newTask = player.Play(file);
                        _audioTasks[id] = new AudioTask(newTask, player, onFinished);
                    }
                };
                player.PlaybackFinished += onFinished;
            }
            
            var task = player.Play(file);
            _audioTasks.TryAdd(id, new AudioTask(task, player, onFinished));
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occured: {0}", e.Message);
        }
    }

    public void Stop(string id)
    {
        try
        {
            if (_audioTasks.TryGetValue(id, out var task))
            {
                task.Player.PlaybackFinished -= task.OnFinished;
                task.Player.Stop();
                _audioTasks.TryRemove(id, out task);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occured: {0}", e.Message);
        }
    }
    
    public void StopAll()
    {
        foreach (var player in _audioTasks.Values)
        {
            player.Player.PlaybackFinished -= player.OnFinished;
            player.Player.Stop();
        }
        _audioTasks.Clear();
    }
    
    public void Pause(string id)
    {
        try
        {
            if (_audioTasks.TryGetValue(id, out var task))
            {
                task.Player.Pause();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occured: {0}", e.Message);
        }
    }

    public void SetVolume(string id, byte volume)
    {
        try
        {
            if (_audioTasks.TryGetValue(id, out var task))
            {
                task.Player.SetVolume(volume);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occured: {0}", e.Message);
        }
    }
    
    public void SetAllVolume(byte volume)
    {
        foreach (var player in _audioTasks.Values)
        {
            player.Player.SetVolume(volume);
        }
    }

    public void Dispose()
    {
        StopAll();
    }
}