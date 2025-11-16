using System;
using System.ComponentModel;
using FunGames.Tools.Utils;
using UnityEngine;

public class CustomThreadUtils
{
    public static void ExecuteAsync(Action action, params GitProcess[] threads)
    {
        for (int i = 0; i < threads.Length; i++)
        {
            GitProcess current = threads[i];
            GitProcess next = i < threads.Length - 1 ? threads[i + 1] : null;
            if (next != null)
            {
                current.OnThreadEnded += (b) => next.Run();
            }
            else
            {
                current.OnThreadEnded += (b) => action?.Invoke();
            }
        }

        threads[0]?.Run();
    }

    public static void RunBackgroundThread( Action actionWhenFinished, params GitProcess[] threads)
    {
        BackgroundWorker bw = new BackgroundWorker();
        bw.WorkerReportsProgress = true;
        foreach (var process in threads)
        {
            bw.DoWork += (sender, args) => process?.Run();
        }
        bw.RunWorkerCompleted += ((sender, args) =>
        {
            Debug.Log("Thread finished");
            actionWhenFinished?.Invoke();
        });
        bw.RunWorkerAsync();
    }
    
    public static void RunBackgroundThread(Action actionWhenFinished, params Action[] actions)
    {
        BackgroundWorker bw = new BackgroundWorker();
        bw.WorkerReportsProgress = true;
        foreach (var action in actions)
        {
            bw.DoWork += (sender, args) => action?.Invoke();
        }
        bw.RunWorkerCompleted += ((sender, args) =>
        {
            Debug.Log("Thread finished");
            actionWhenFinished?.Invoke();
        });
        bw.RunWorkerAsync();
    }

}