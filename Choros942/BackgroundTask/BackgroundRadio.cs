using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;

namespace BackgroundTask
{
    public sealed class BackgroundRadio : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;
        private SystemMediaTransportControls controls;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
            controls= BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            controls.IsEnabled = true;
            controls.IsPlayEnabled = true;
            controls.IsPauseEnabled = true;
            controls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.Current.Source = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("http://195.251.162.97:8000/stream.ogg")));
            BackgroundMediaPlayer.Current.Play();
            var vs = new ValueSet();
            vs.Add("state", "play");
            BackgroundMediaPlayer.SendMessageToForeground(vs);
            taskInstance.Task.Completed += Task_Completed;
            taskInstance.Canceled += TaskInstance_Canceled;

        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (BackgroundMediaPlayer.Current.CurrentState)
            {
                case MediaPlayerState.Closed:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaPlayerState.Opening:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlayerState.Playing:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            if (e.Data != null && (e.Data.ContainsKey("state") && (string) e.Data["state"] == "play"))
            {
                BackgroundMediaPlayer.Current.Source = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("http://195.251.162.97:8000/stream.ogg")));

                BackgroundMediaPlayer.Current.Play();
            }
            else if (e.Data != null && (e.Data.ContainsKey("state") && (string) e.Data["state"] == "pause"))
            {
                BackgroundMediaPlayer.Current.Source = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("http://195.251.162.97:8000/stream.ogg")));

                BackgroundMediaPlayer.Current.Pause();

                //BackgroundMediaPlayer.Shutdown();
            }
            else if (e.Data != null && (e.Data.ContainsKey("state") && (string)e.Data["state"] == "shutdown"))
            {
                controls.ButtonPressed -= SystemMediaTransportControls_ButtonPressed;

                BackgroundMediaPlayer.Shutdown();
                deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            controls.ButtonPressed -= SystemMediaTransportControls_ButtonPressed;

            BackgroundMediaPlayer.Shutdown();
            deferral.Complete();
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            deferral.Complete();
        }

        private void SystemMediaTransportControls_ButtonPressed(Windows.Media.SystemMediaTransportControls sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            var vs = new ValueSet();

            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    BackgroundMediaPlayer.Current.Play();
                    vs.Add("state", "play");
                    BackgroundMediaPlayer.SendMessageToForeground(vs);
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    BackgroundMediaPlayer.Current.Pause();
                    vs.Add("state", "pause");
                    BackgroundMediaPlayer.SendMessageToForeground(vs);
                    break;
            }
        }
    }
}
