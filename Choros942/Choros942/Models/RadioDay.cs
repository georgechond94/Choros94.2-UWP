using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Choros942.Annotations;

namespace Choros942.Models
{
    public class RadioDay : INotifyPropertyChanged
    {
        private DayOfWeek _day;
        private IEnumerable<Producer> _producers;

        public DayOfWeek Day
        {
            get
            {
                return _day;
            }

            set
            {
                _day = value;
                OnPropertyChanged(nameof(Day));
            }
        }

        public IEnumerable<Producer> Producers
        {
            get
            {
                return _producers;
            }

            set
            {
                _producers = value;
                OnPropertyChanged(nameof(Producers));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Day.ToString();
        }
    }
}
