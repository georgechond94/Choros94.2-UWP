using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Choros942.Annotations;
using Choros942.Models;

namespace Choros942.ViewModels
{
    public class ProducerViewModel : INotifyPropertyChanged
    {
        private Producer _nowPlaying;
        private int _selectedDay;
        public ObservableCollection<RadioDay> Days { get; set; }
        private Timer timer;
        public bool ProducersLoaded { get; set; }
        public Producer NowPlaying
        {
            get { return _nowPlaying; }
            set
            {
                _nowPlaying = value;
                Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () =>
                        {
                            OnPropertyChanged(nameof(NowPlaying));
                        });
            }
        }

        public int SelectedDay
        {
            get { return _selectedDay; }

            set
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
            }
        }

        public ProducerViewModel()
        {
            ProducersLoaded = false;
            Days = new ObservableCollection<RadioDay>();

            Days.Add(new RadioDay {Day = DayOfWeek.Sunday});
            Days.Add(new RadioDay {Day = DayOfWeek.Monday});
            Days.Add(new RadioDay {Day = DayOfWeek.Tuesday});
            Days.Add(new RadioDay {Day = DayOfWeek.Wednesday});
            Days.Add(new RadioDay {Day = DayOfWeek.Thursday});
            Days.Add(new RadioDay {Day = DayOfWeek.Friday});
            Days.Add(new RadioDay {Day = DayOfWeek.Saturday});


            SelectedDay = (int) DateTime.Now.DayOfWeek;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
        public async Task LoadProducers()
        {
            HttpClient client = new HttpClient();

            var re = await client.GetStringAsync(new Uri("http://xoros.samos.aegean.gr/Program.html"));

            var str = Regex.Matches(re, "id=\"home\"[\\s\\S]+id=\"menu1\"")[0].Value;


            str = ClearString(str, "id=\"home\" class=\"tab-pane fade in active\"", "div id=\"menu1\"");

            var p = GetProducers(str);

            Days[1].Producers = p;

            str = Regex.Matches(re, "id=\"menu1\"[\\s\\S]+id=\"menu2\"")[0].Value;

            str = ClearString(str, "id=\"menu1\" class=\"tab-pane fade\"", "div id=\"menu2\"");

            p = GetProducers(str);

            Days[2].Producers = p;


            str = Regex.Matches(re, "id=\"menu2\"[\\s\\S]+id=\"menu3\"")[0].Value;

            str = ClearString(str, "id=\"menu2\" class=\"tab-pane fade\"", "div id=\"menu3\"");

            p = GetProducers(str);

            Days[3].Producers = p;


            str = Regex.Matches(re, "id=\"menu3\"[\\s\\S]+id=\"menu4\"")[0].Value;

            str = ClearString(str, "id=\"menu3\" class=\"tab-pane fade\"", "div id=\"menu4\"");

            p = GetProducers(str);

            Days[4].Producers = p;


            str = Regex.Matches(re, "id=\"menu4\"[\\s\\S]+id=\"menu5\"")[0].Value;

            str = ClearString(str, "id=\"menu4\" class=\"tab-pane fade\"", "div id=\"menu5\"");

            p = GetProducers(str);

            Days[5].Producers = p;


            str = Regex.Matches(re, "id=\"menu5\"[\\s\\S]+id=\"menu6\"")[0].Value;

            str = ClearString(str, "id=\"menu5\" class=\"tab-pane fade\"", "div id=\"menu6\"");

            p = GetProducers(str);

            Days[6].Producers = p;


            str = Regex.Matches(re, "id=\"menu6\"[\\s\\S]+/h3")[0].Value;

            str = ClearString(str, "id=\"menu6\" class=\"tab-pane fade\"", "/h3").TrimEnd('|');

            p = GetProducers(str);

            Days[0].Producers = p;


            timer = new Timer(Callback, null, TimeSpan.FromMinutes(60 - DateTime.Now.Minute), TimeSpan.FromHours(1));
            Callback(null);

            ProducersLoaded = true;
        }

        public void Callback(object state)
        {
            //Check who's playing right now.

            var day = Days.FirstOrDefault(x => x.Day == DateTime.Now.DayOfWeek);
            if (DateTime.Now.Hour <= 3)
            {
                day = Days.FirstOrDefault(x => x.Day == DateTime.Now.AddDays(-1).DayOfWeek);
            }
            var hour = DateTime.Now.Hour;
            bool found = false;
            foreach (var producer in day.Producers)
            {
                var h = producer.Time.Split('-');
                var d1 = DateTime.Parse(h[0]).Hour;
                var d2 = DateTime.Parse(h[1]).Hour;
                if (d2 == 0)
                    d2 = 24;
                if (d1 <= hour && d2 > hour)
                {
                    found = true;
                    NowPlaying = producer;
                }

            }

            if (!found)
            {
                NowPlaying = null;
            }

        }

        private IEnumerable<Producer> GetProducers(string str)
        {
            return
                str.Split('|')
                    .Select(trm => trm.Split('/'))
                    .Select(sp => new Producer {Name = sp[1].Trim(), Time = sp[0]})
                    .ToList();
        }

        private string ClearString(string str, string _class, string _nextdiv)
        {
            str = str.Replace("strong", "")
                .Replace(_class, "")
                .Replace(_nextdiv, "")
                .Replace("h3", "")
                .Replace("div", "")
                .Replace("\\h3", "")
                .Replace("<br>", "|")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("|/", "");
            str = Regex.Replace(str, @"\r\n?|\n|\t", "");
            str = str.Trim();
            str = str.Trim('/');

            return str;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
