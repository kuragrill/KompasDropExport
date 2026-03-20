using System.ComponentModel;

namespace KompasDropExport.Domain
{
    public sealed class FileRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FullPath { get; }
        public string FileName { get; }

        private string _marking;
        public string Marking
        {
            get => _marking;
            set { if (_marking == value) return; _marking = value; OnChanged(nameof(Marking)); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnChanged(nameof(Name)); }
        }

        public CellState MarkingState { get; set; } = CellState.Clean;
        public CellState NameState { get; set; } = CellState.Clean;

        public string Status { get; set; } // общая строка статуса для UI

        public FileRecord(string fullPath, string fileName)
        {
            FullPath = fullPath;
            FileName = fileName;
        }

        private void OnChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}