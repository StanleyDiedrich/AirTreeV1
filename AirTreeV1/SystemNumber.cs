using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTreeV1
{
    /*public class SystemNumber
    {
        public string SystemName { get; set; }
        public bool IsSelected { get; set; }

        public SystemNumber(string systemname)
        {
            SystemName = systemname;
        }
    }*/

    public class SystemNumber : INotifyPropertyChanged
    {
        private string _systemName;
        public string SystemName
        {
            get { return _systemName; }
            set
            {
                if (_systemName != value)
                {
                    _systemName = value;
                    OnPropertyChanged(nameof(SystemName));
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public SystemNumber(string systemname)
        {
            SystemName = systemname;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
