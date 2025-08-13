using System;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.Async;
using LSCore.Attributes;
using LSCore.Extensions.Time;
using UnityEngine;

public class TimeGoal : MonoBehaviour
{
    [TimeSpan(options = TimeAttribute.Options.Minute | TimeAttribute.Options.Second)] 
    public long time;

    public int timeOuting = 15;
    public AnimSequencer timeOutingAnim;
    public LaLa.PlayClip timerSound;
    private long cachedTime;
    
    public LSText timeText;
    private Tween timer;
    private int pauseCount;
    private TimeSpan timeOutingSpan;
    
    protected void Awake()
    {
        cachedTime = time;
        timeOutingSpan = TimeSpan.FromSeconds(timeOuting);
        UpdateTimer(time.ToTimeSpan());
        IUIView.Showing += PauseOnShowingOtherWindow;
        FieldManager.DragStarted += StartTimer;
    }

    private void OnDestroy()
    {
        IUIView.Showing -= PauseOnShowingOtherWindow;
        FieldManager.DragStarted -= StartTimer;
    }

    private void PauseOnShowingOtherWindow(IUIView view)
    {
        if (view.GetType() != typeof(CoreWindow))
        {
            timer.Pause();
            pauseCount++;
            view.Manager.Hiding += OnHide;

            void OnHide()
            {
                view.Manager.Hiding -= OnHide;
                pauseCount--;
                if(pauseCount == 0) timer.Play();
            }
        }
    }

    private void StartTimer()
    {
        if(pauseCount > 0) return;
        timer ??= time.ToTimeSpan().Seconder(UpdateTimer, false);
    }

    private void UpdateTimer(TimeSpan remaining)
    {
        timeText.text = remaining.ToString(@"mm\:ss");
        if (remaining <= timeOutingSpan)
        {
            var seq = timeOutingAnim.sequence;
            if (!seq.IsActive())
            {
                timerSound.Do();
                timeOutingAnim.Animate();
            }
        }
        else
        {
            StopTimingOut();
        }

        if (remaining <= TimeSpan.Zero)
        {
            StopTimingOut();
            LoseWindow.Show(OnRevive);
        }

        void StopTimingOut()
        {
            timerSound.Stop();
            var seq = timeOutingAnim.sequence;
            if (seq.IsActive())
            {
                seq.KillOnEverySecondLoop();
            }
        }
    }

    private void OnRevive()
    {
        UpdateTimer(cachedTime.ToTimeSpan());
        var lastPauseCount = pauseCount;
        pauseCount = 0;
        timer.Kill();
        timer = null;
        StartTimer();
        pauseCount = lastPauseCount;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(World.IsPlaying || !timeText) return;
        timeText.text = time.ToTimeSpan().ToString(@"mm\:ss");
    }
#endif
}