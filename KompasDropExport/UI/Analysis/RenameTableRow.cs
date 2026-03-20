using System;
using System.ComponentModel;

namespace KompasDropExport.UI.Analysis
{
    /// <summary>
    /// Строка таблицы для ручного переименования.
    /// BaseName редактируется, Extension хранится отдельно.
    /// </summary>
    public sealed class RenameTableRow : INotifyPropertyChanged
    {
        private string _baseName;

        public int NodeId { get; set; }

        /// <summary>
        /// Уровень вложенности для визуального отступа.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Исходное базовое имя (без расширения).
        /// </summary>
        public string OriginalBaseName { get; set; }

        /// <summary>
        /// Редактируемое базовое имя (без расширения).
        /// </summary>
        public string BaseName
        {
            get { return _baseName; }
            set
            {
                if (string.Equals(_baseName, value, StringComparison.Ordinal))
                    return;

                _baseName = value;
                OnPropertyChanged(nameof(BaseName));
                OnPropertyChanged(nameof(FileName));
                OnPropertyChanged(nameof(IsModified));
            }
        }

        /// <summary>
        /// Расширение файла, например ".a3d"
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Полное имя файла = BaseName + Extension
        /// </summary>
        public string FileName
        {
            get { return (BaseName ?? "") + (Extension ?? ""); }
        }

        public string Designation { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Сборка / Деталь / Чертёж / Спецификация
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// Полный путь (скрытый столбец / служебное поле)
        /// </summary>
        public string FullPath { get; set; }

        public bool IsModified
        {
            get
            {
                return !string.Equals(
                    OriginalBaseName ?? "",
                    BaseName ?? "",
                    StringComparison.Ordinal);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}