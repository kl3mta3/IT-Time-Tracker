﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IT_Time_Tracker
{
    /// <summary>
    /// It Time Tracker was created by Kenneth Lasyone ©2024
    /// No license needed for use.
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Record> Records { get; set; }
        private readonly string JsonFilePath;
        public ICommand DeleteEntryCommand { get; private set; }
        private bool _isEditing = false;
        private string _originalValue = string.Empty;

        private string _lastSortColumn;
        private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            DeleteEntryCommand = new RelayCommand<Record>(DeleteEntry);
            Records = new ObservableCollection<Record>();
            DataContext = this;

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = System.IO.Path.Combine(appDataPath, "ITTimeTracker");
            Directory.CreateDirectory(appFolder);
            JsonFilePath = System.IO.Path.Combine(appFolder, "records.json");

            LoadRecordsFromJson();
            DataContext = this;
        }

        private void DeleteEntry(Record record)
        {
            if (record != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the entry with Reference: {record.Reference}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Records.Remove(record);
                    SaveChanges();

                    MessageBox.Show("Entry deleted successfully.", "Deletion Confirmed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("No record selected for deletion.", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecordsFromJson()
        {
            if (File.Exists(JsonFilePath))
            {
                string jsonString = File.ReadAllText(JsonFilePath);
                var records = System.Text.Json.JsonSerializer.Deserialize<List<Record>>(jsonString);
                Records = new ObservableCollection<Record>(records);
                UpdateTotals();
            }
        }

        private void SaveRecordsToJson()
        {
            string jsonString = JsonSerializer.Serialize(Records, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonFilePath, jsonString);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txb_ReferenceNumberInput.Text, out int reference))
            {
                MessageBox.Show("Please enter a valid integer for Reference Number.");
                return;
            }

            if (!int.TryParse(txb_TimeInput.Text, out int timeSpent))
            {
                MessageBox.Show("Please enter a valid integer for Time Spent.");
                return;
            }

            if (reference <= 0 || timeSpent <= 0)
            {
                MessageBox.Show("Please enter both a positive integer for Reference Number and Time Spent.");
                return;
            }

            var newRecord = new Record
            {
                Reference = reference,
                TimeSpent = timeSpent,
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            };

            Records.Add(newRecord);
            SaveRecordsToJson();
            UpdateTotals();

            lvw_History.Items.Refresh();

            txb_ReferenceNumberInput.Clear();
            txb_TimeInput.Clear();
        }

        private void SaveChanges()
        {
            SaveRecordsToJson();
            lvw_History.Items.Refresh();
            UpdateTotals();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isEditing)
            {
                _isEditing = true;
                var textBox = sender as TextBox;
                _originalValue = textBox.Text;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                HandleTextBoxChange(textBox);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                textBox.Text = _originalValue;
                _isEditing = false;
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                HandleTextBoxChange(sender as TextBox);
            }
        }

        private void HandleTextBoxChange(TextBox textBox)
        {
            if (textBox != null && textBox.DataContext is Record record)
            {
                string fieldName = textBox.Tag as string;
                string newValue = textBox.Text;

                if (fieldName == "TimeSpent")
                {
                    if (int.TryParse(newValue, out int newIntValue) && int.TryParse(_originalValue, out int originalIntValue))
                    {
                        if (newIntValue != originalIntValue)
                        {
                            MessageBoxResult result = MessageBox.Show(
                                $"Do you want to change the {fieldName} from '{_originalValue}' to '{newValue}'?",
                                "Confirm Edit",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                ApplyChange(record, fieldName, newIntValue);
                                SaveChanges();
                                UpdateTotals();
                            }
                            else
                            {
                                textBox.Text = _originalValue;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid integer for Time Spent.");
                        textBox.Text = record.TimeSpent.ToString();
                    }
                }
                else
                {
                    // Handle other fields (Reference and Date)
                    if (newValue != _originalValue)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"Do you want to change the {fieldName} from '{_originalValue}' to '{newValue}'?",
                            "Confirm Edit",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            ApplyChange(record, fieldName, newValue);
                            SaveChanges();
                        }
                        else
                        {
                            textBox.Text = _originalValue;
                        }
                    }
                }

                _isEditing = false;
                textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private string GetOldValue(Record record, string fieldName)
        {
            switch (fieldName)
            {
                case "Reference":
                    return record.Reference.ToString();
                case "TimeSpent":
                    return record.TimeSpent.ToString();
                case "Date":
                    return record.Date;
                default:
                    return string.Empty;
            }
        }

        private void ApplyChange(Record record, string fieldName, object newValue)
        {
            switch (fieldName)
            {
                case "Reference":
                    record.Reference = (int)newValue;
                    break;
                case "TimeSpent":
                    record.TimeSpent = (int)newValue;
                    break;
                case "Date":
                    record.Date = newValue as string;
                    break;
            }
        }

        private void UpdateTotals()
        {
            var today = DateTime.Now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            var dailyTotal = Records.Where(r => DateTime.Parse(r.Date) == today)
                                    .Sum(r => r.TimeSpent);
            var weeklyTotal = Records.Where(r => DateTime.Parse(r.Date) >= startOfWeek)
                                     .Sum(r => r.TimeSpent);
            var monthlyTotal = Records.Where(r => DateTime.Parse(r.Date) >= startOfMonth)
                                      .Sum(r => r.TimeSpent);
            var ytdTotal = Records.Where(r => DateTime.Parse(r.Date) >= startOfYear)
                                  .Sum(r => r.TimeSpent);

            lbl_Daily_Total.Content = FormatTotalTime(dailyTotal);
            lbl_Weekly_Total.Content = FormatTotalTime(weeklyTotal);
            lbl_Monthly_Total.Content = FormatTotalTime(monthlyTotal);
            lbl_YTD_Total.Content = FormatTotalTime(ytdTotal);
        }

        private double ParseTimeSpent(string timeSpent)
        {
            return double.TryParse(timeSpent, out double result) ? result : 0;
        }

        private string FormatTotalTime(int totalMinutes)
        {
            var hours = Math.Floor(totalMinutes / 60.0);
            var minutes = totalMinutes % 60;
            return $"{hours:F0}h {minutes:F0}m";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to reset all records and totals? This action cannot be undone.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ResetApplication();
            }
        }

        private void ResetApplication()
        {
            Records.Clear();

            lbl_Daily_Total.Content = "0h 0m";
            lbl_Weekly_Total.Content = "0h 0m";
            lbl_Monthly_Total.Content = "0h 0m";
            lbl_YTD_Total.Content = "0h 0m";

            File.WriteAllText(JsonFilePath, "[]");

            if (txb_ReferenceNumberInput != null) txb_ReferenceNumberInput.Text = string.Empty;
            if (txb_TimeInput != null) txb_TimeInput.Text = string.Empty;

            lvw_History.Items.Refresh();

            MessageBox.Show("Application has been reset successfully.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DragRegion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement is TextBox textBox)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = lvw_History.SelectedItem as Record;

                if (selectedItem == null)
                {
                    MessageBox.Show("No item selected. Please select an item to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the entry with Reference: {selectedItem.Reference}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Records.Remove(selectedItem);
                    SaveChanges();
                    MessageBox.Show("Entry deleted successfully.", "Deletion Confirmed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            Reports reportsWindow = new Reports(Records);
            reportsWindow.Owner = this;
            reportsWindow.ShowDialog();
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lvw_History.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                string column = headerClicked.Tag as string;

                if (_lastSortColumn == column)
                {
                    _lastSortDirection = _lastSortDirection == ListSortDirection.Ascending ?
                        ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                else
                {
                    _lastSortDirection = ListSortDirection.Ascending;
                }

                _lastSortColumn = column;

                SortRecords(column, _lastSortDirection);
            }
        }

        private void SortRecords(string column, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(Records);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(column, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextNumeric(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static bool IsTextNumeric(string text)
        {
            Regex regex = new Regex("[^0-9]+"); // Regex that matches disallowed text
            return !regex.IsMatch(text);
        }

    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class Record : INotifyPropertyChanged
    {
        private int _reference;
        public int Reference
        {
            get => _reference;
            set
            {
                if (_reference != value)
                {
                    _reference = value;
                    OnPropertyChanged(nameof(Reference));
                }
            }
        }

        private int _timeSpent;
        public int TimeSpent
        {
            get => _timeSpent;
            set
            {
                if (_timeSpent != value)
                {
                    _timeSpent = value;
                    OnPropertyChanged(nameof(TimeSpent));
                }
            }
        }

        private string _date;
        public string Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Data
    {
        public List<Record> Records { get; set; }
    }
}
