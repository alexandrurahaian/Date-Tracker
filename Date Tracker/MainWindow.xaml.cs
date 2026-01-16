using Date_Tracker.Objects;
using Date_Tracker.Scripts;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Date_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static TrackedDate CurrentDate = new TrackedDate();
        public static bool IsDarkMode = true;
        public ObservableCollection<TrackedDate> DateList { get; set; } = new ObservableCollection<TrackedDate>();
        public ObservableCollection<string> Timezones { get; set; } = new ObservableCollection<string>();
        private Thread update_thread;

        public void Reset()
        {
            CurrentDate = new TrackedDate();
            use_crnt_timezone_btn.IsChecked = true;
            timezone_panel.Visibility = Visibility.Hidden;
            none_radio_btn.IsChecked = true;
            delete_when_reached_checked_box_item.IsChecked = false;
            label_box.Text = string.Empty;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

            Match match = Regex.Match(localTimeZone.DisplayName, "\\d{1,2}:\\d{2}");
            if (match.Success)
            {
                use_crnt_timezone_btn.Content = $"Use current (GMT+{match.Value})";
            }

            foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                Timezones.Add(timeZone.DisplayName);
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(DateList);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("IsPinned", System.ComponentModel.ListSortDirection.Descending));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("IsFavourite", System.ComponentModel.ListSortDirection.Descending));

            foreach (TrackedDate t_date in DatabaseHandler.GetSavedDates())
            {
                DateList.Add(t_date);
            }

            foreach (TrackedDate t_date in DateList)
            {
                t_date.TimeLeft = (t_date.Mode == 1 ? Utility.GetTimeLeftStr(t_date.Date, false) : Utility.GetTimeSinceStr(t_date.Date, false));
            }
        }

        private void use_crnt_timezone_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate.Timezone = TimeZoneInfo.Local.StandardName;
            diff_timezone_panel.Visibility = Visibility.Hidden;
        }

        private void use_diff_timezone_btn_Click(object sender, RoutedEventArgs e)
        {
            diff_timezone_panel.Visibility = Visibility.Visible;
        }

        private void favourite_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate.IsFavourite = true;
            CurrentDate.IsPinned = false;
        }

        private void pinned_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate.IsPinned = true;
            CurrentDate.IsFavourite = false;
        }

        private void none_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate.IsFavourite = false;
            CurrentDate.IsPinned = false;
        }

        private void delete_when_reached_check_Click(object sender, RoutedEventArgs e)
        {
            CurrentDate.DeleteWhenReached = delete_when_reached_checked_box_item.IsChecked == true;
        }

        private void add_btn_Click(object sender, RoutedEventArgs e)
        {
            if (use_diff_timezone_btn.IsChecked == true)
            {
                CurrentDate.Timezone = new_timezone_combo.SelectedItem as string ?? TimeZoneInfo.Local.DisplayName;
            }
            else CurrentDate.Timezone = TimeZoneInfo.Local.DisplayName;

            CurrentDate.Name = label_box.Text;
            CurrentDate.Date = calendar.SelectedDate ?? DateTime.Now;
            CurrentDate.DateDisplay = CurrentDate.Date.ToString("dd/MM/yyyy");
            CurrentDate.UID = Utility.GenerateUID();
            CurrentDate.Mode = (count_down_radio_btn.IsChecked == true ? 1 : 2);

            bool wasSuccess = DatabaseHandler.AddTrackedDate(CurrentDate);
            
            if (wasSuccess)
                DateList.Add(CurrentDate);

            CurrentDate.TimeLeft = (CurrentDate.Mode == 1 ? Utility.GetTimeLeftStr(CurrentDate.Date, false) : Utility.GetTimeSinceStr(CurrentDate.Date, false));
            Reset();
        }

        private void search_btn_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(DateList);
            if (!string.IsNullOrEmpty(search_box.Text))
            {
                view.Filter = item =>
                {
                    TrackedDate? _date = item as TrackedDate;
                    string name = _date?.Name ?? string.Empty;
                    return name.Contains(search_box.Text, StringComparison.OrdinalIgnoreCase);
                };
            }
            else view.Filter = null;
        }

        private void favourite_only_check_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(DateList);
            if (favourite_only_check.IsChecked == true && DateList.Count > 0)
            {
                view.Filter = item =>
                {
                    TrackedDate? t_date = item as TrackedDate;
                    bool isFav = t_date?.IsFavourite ?? false;
                    return isFav;
                };
            }
            else view.Filter = null;
        }

        private void delete_when_reached_check(object sender, RoutedEventArgs e)
        {
            CurrentDate.DeleteWhenReached = delete_when_reached_checked_box_item.IsChecked == true;
        }

        private void label_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(label_box.Text)) add_btn.IsEnabled = true;
            else add_btn.IsEnabled = false;
        }

        private void delete_selected_btn_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = date_list.SelectedIndex;
            Debug.WriteLine(selectedIndex);
            if (selectedIndex >= 0)
            {
                TrackedDate t_date = DateList[selectedIndex];
                DatabaseHandler.DeleteTrackedDate(t_date);
                DateList.RemoveAt(selectedIndex);
            }    
        }

        private void unpin_selected_btn_Click(object sender, RoutedEventArgs e)
        {
            TrackedDate? selected = date_list.SelectedItem as TrackedDate;
            int selectedIndex = date_list.SelectedIndex;
            if (selected != null)
            {
                TrackedDate edited = selected.Clone();
                edited.IsPinned = !selected.IsPinned;
                DatabaseHandler.EditTrackedDate(selected, edited);
                DateList[selectedIndex] = edited;
                if (edited.IsPinned) unpin_selected_btn.Content = "Unpin selected";
                else unpin_selected_btn.Content = "Pin selected";
                date_list.Items.Refresh();
            }
        }

        private void unfavourite_selected_btn_Click(object sender, RoutedEventArgs e)
        {
            TrackedDate? selected = date_list.SelectedItem as TrackedDate;
            int selectedIndex = date_list.SelectedIndex;
            if (selected != null)
            {
                TrackedDate edited = selected.Clone();
                edited.IsFavourite = !selected.IsFavourite;
                DatabaseHandler.EditTrackedDate(selected, edited);
                DateList[selectedIndex] = edited;
                if (edited.IsFavourite) unfavourite_selected_btn.Content = "Unfavourite selected";
                else unfavourite_selected_btn.Content = "Favourite selected";
                date_list.Items.Refresh();
            }
        }

        private void date_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrackedDate? selected = date_list.SelectedItem as TrackedDate;
            delete_selected_btn.IsEnabled = selected != null;
            unpin_selected_btn.IsEnabled = (selected != null && !selected.IsFavourite);
            unfavourite_selected_btn.IsEnabled = (selected != null && !selected.IsPinned);

            if (selected != null)
            {
                if (selected.IsFavourite) unfavourite_selected_btn.Content = "Unfavourite selected";
                else unfavourite_selected_btn.Content = "Favourite selected";

                if (selected.IsPinned) unpin_selected_btn.Content = "Unpin selected";
                else unpin_selected_btn.Content = "Pin selected";
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (date_list.SelectedItem != null) date_list.SelectedItem = null;
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = calendar.SelectedDate;
            if (selectedDate == null) return;

            count_up_radio_btn.IsEnabled = selectedDate < DateTime.Today;
        }
    }
}