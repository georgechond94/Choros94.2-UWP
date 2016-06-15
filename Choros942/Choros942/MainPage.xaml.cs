using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Navigation;
using Choros942.Converters;
using Choros942.Models;
using Choros942.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Choros942
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SystemMediaTransportControls systemControls;

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = (Application.Current as App).PVM;


            //Value="{Binding ElementName=BackgroundMediaPlayer.Current, Path=Volume,Mode=TwoWay,Converter={StaticResource DoubleConverter}}"

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundColor = Color.FromArgb(100, 56, 79, 161);
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            }
            else
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.BackgroundColor = Color.FromArgb(100, 56, 79, 161);
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ForegroundColor = Colors.White;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Color.FromArgb(100, 56, 79, 161);
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            }

            Binding volumeB = new Binding
            {
                Source = BackgroundMediaPlayer.Current,
                Path = new PropertyPath("Volume"),
                Mode = BindingMode.TwoWay,
                Converter = new IntToDoubleConverter()
            };
            volumeslider.SetBinding(Slider.ValueProperty, volumeB);

            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            BackgroundMediaPlayer.Current.MediaFailed += Current_MediaFailed;
            // Hook up app to system transport controls.
            systemControls = SystemMediaTransportControls.GetForCurrentView();
            systemControls.ButtonPressed += SystemControls_ButtonPressed;

            // Register to handle the following system transpot control buttons.
            systemControls.IsPlayEnabled = true;
            systemControls.IsPauseEnabled = true;
        }

        private void Current_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            
        }


        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            if (e.Data != null && (e.Data.ContainsKey("state") && (string)e.Data["state"] == "play"))
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    userimage.IsChecked = false;
                });

            }
            else if (e.Data != null && (e.Data.ContainsKey("state") && (string)e.Data["state"] == "pause"))
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    userimage.IsChecked = true;
                });
            }
        }
        private async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        userimage.IsChecked = false;
                        var vs = new ValueSet();
                        vs.Add("state", "play");
                        try
                        {
                            BackgroundMediaPlayer.SendMessageToBackground(vs);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        userimage.IsChecked = true;
                        var vs = new ValueSet();
                        vs.Add("state", "pause");
                        try
                        {
                            BackgroundMediaPlayer.SendMessageToBackground(vs);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                    break;
                 }
        }



        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            if (Helpers.HasInternet())
            {
                userimage.IsChecked = false;
                if (!(Application.Current as App).PVM.ProducersLoaded)
                    await (Application.Current as App).PVM.LoadProducers();
            }
            else
                {
                    userimage.IsChecked = true;

                    (Application.Current as App).PVM.NowPlaying = new Producer
                    {
                        Name = "No Internet Connection.",
                        Time = ""
                    };
                }
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            if (Helpers.HasInternet())
                            {
                                userimage.IsChecked = false;
                                if (!(Application.Current as App).PVM.ProducersLoaded)
                                    await (Application.Current as App).PVM.LoadProducers();
                                (Application.Current as App).PVM.Callback(null);
                                var vs = new ValueSet();
                                vs.Add("state", "play");
                                BackgroundMediaPlayer.SendMessageToBackground(vs);
                            }
                            else
                            {
                                userimage.IsChecked = true;
                                var vs = new ValueSet();
                                vs.Add("state", "pause");
                                BackgroundMediaPlayer.SendMessageToBackground(vs);

                                (Application.Current as App).PVM.NowPlaying = new Producer
                                {
                                    Name = "No Internet Connection.",
                                    Time = ""
                                };

                            }

                        });
            
        }

        private void NormalizeParallax(UIElement target)
        {
            if (ParallaxRoot.ActualWidth < 1)
                return;
            var transform = target.Transform3D as CompositeTransform3D;

            if (transform != null)
            {
                var transformToVisual = ParallaxRoot.TransformToVisual(target);
                var center = new Point(ParallaxRoot.ActualWidth/2.0, RootGrid.ActualHeight/2.0);

                // Center of ParallaxRoot in the coordinates of target.
                var transformedCenter = transformToVisual.TransformPoint(center);

                transform.CenterX = transformedCenter.X;

                // TransformToVisual doesn't account for ScrollViewer offset
                transform.CenterY = transformedCenter.Y - ParallaxRoot.VerticalOffset;

                // This could be done statically in markup but it's easier to show here.
                transform.ScaleX = transform.ScaleY = -transform.TranslateZ/PerspectiveTransform.Depth + 1.0;
            }
        }
        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NormalizeParallax(RainierBackgroundImage);
            //NormalizeParallax(CliffBackgroundImage);
        }

        private void Userimage_OnClick(object sender, RoutedEventArgs e)
        {
            if (userimage.IsChecked == true)
            {
                var vs = new ValueSet();
                vs.Add("state", "pause");
                try
                {
                    BackgroundMediaPlayer.SendMessageToBackground(vs);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            else if (userimage.IsChecked == false)
            {
                var vs = new ValueSet();
                vs.Add("state", "play");
                try
                {
                    BackgroundMediaPlayer.SendMessageToBackground(vs);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/Xoros94.2"));
        }
        private async void UIElement_OnTapped0(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://xoros.samos.aegean.gr/"));
        }
        private async void UIElement_OnTapped1(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/xoros942"));
        }

        private async void UIElement_OnTapped2(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.youtube.com/channel/UCNv71mJUcqTJtgXu8tCrJZQ"));
        }

        private async void Help_OnClick(object sender, RoutedEventArgs e)
        {
            AboutDialog about = new AboutDialog();
            await about.ShowAsync();
        }

        private async void Feedback_OnClick(object sender, RoutedEventArgs e)
        {
            var emailMessage = new EmailMessage();
            emailMessage.Subject = "Χώρος 94.2 - Feedback";
            emailMessage.To.Add(new EmailRecipient("georgechond94@gmail.com", "George Chondrompilas"));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private async void Rate_OnClick(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=17169GeorgiosChondrompila.94.2_9t599v86sk7yt"));
        }

/*        private void MElem_OnCurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (BackgroundMediaPlayer.Current.CurrentState)
            {
                case MediaPlayerState.Playing:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaPlayerState.Closed:
                    BackgroundMediaPlayer.Current.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    break;
            }
        }*/
    }
}
