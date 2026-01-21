using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Date_Tracker.Objects
{
    public class TrackedDate : ICloneable
    {
        public string Name { get; set; }
        public string Timezone { get; set; } = "GMT";
        public int Mode { get; set; } = 0;
        public DateTime Date { get; set; }
        public string DateDisplay { get; set; }
        public bool IsPinned { get; set; } = false;
        public bool IsFavourite { get; set; } = false;
        public bool DeleteWhenReached { get; set; } = false;
        public string TimeLeft { get; set; }
        public int UID { get; set; }
        public Color CellBackgroundBrush
        {
            get
            {
                if (IsPinned)
                    return Colors.DarkOrange;

                if (IsFavourite)
                    return Colors.Gold;

                if (Date <= DateTime.Now && Mode == 1)
                    return Colors.DarkRed;

                return Colors.Transparent;
            }
        }

        public Brush CellForegroundBrush
        {
            get
            {
                Brush defaultBrush =
                            MainWindow.IsDarkMode ? Brushes.White : Brushes.Black;
                if (IsPinned == true || IsFavourite == true)
                    return Brushes.Black;

                return defaultBrush;
            }
        }

        public TrackedDate Clone()
        {
            TrackedDate cloned = new TrackedDate
            {
                Name = this.Name,
                Date = this.Date,
                Mode = this.Mode,
                TimeLeft = this.TimeLeft,
                IsPinned = this.IsPinned,
                IsFavourite = this.IsFavourite,
                DateDisplay = this.DateDisplay,
                DeleteWhenReached = this.DeleteWhenReached,
                Timezone = this.Timezone,
                UID = this.UID
            };
            
            return cloned;
        }
        object ICloneable.Clone() => Clone();
    }
}