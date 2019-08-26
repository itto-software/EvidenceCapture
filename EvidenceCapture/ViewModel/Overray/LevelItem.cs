using System.Collections.ObjectModel;

namespace EvidenceCapture.ViewModel.Overray
{
    public class LevelItem : OverrayBase
    {
        private ObservableCollection<int> _levelValues;
        private int _selectedIndex;

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (value != _selectedIndex)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        public ObservableCollection<int> LevelValues
        {
            get
            {
                return _levelValues;
            }
            set
            {
                if (value != _levelValues)
                {
                    _levelValues = value;
                    RaisePropertyChanged(nameof(LevelValues));
                }
            }
        }

    }
    
}