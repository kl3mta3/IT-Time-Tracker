using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IT_Time_Tracker
{
    
    public partial class Reports : Window
    {
        private ObservableCollection<Record> allRecords;
        private ObservableCollection<Record> filteredRecords;

        public Reports(ObservableCollection<Record> records)
        {
            InitializeComponent();
            allRecords = records;
            filteredRecords = new ObservableCollection<Record>();
            DataContext = this;
            lvw_SearchResults.ItemsSource = filteredRecords;
        }


        public Reports()
        {
            InitializeComponent();
        }




        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            filteredRecords.Clear();

            if (rbt_RefSearch.IsChecked == true)
            {
                SearchByReference();
            }
            else if (rbt_DateSearch.IsChecked == true)
            {
                SearchByDateRange();
            }

            lvw_SearchResults.ItemsSource = null;
            lvw_SearchResults.ItemsSource = filteredRecords;
            lvw_SearchResults.Items.Refresh();
        }

        private void DragRegion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SearchByReference()
        {
            string searchRef = txb_SearchRef.Text.Trim().ToLower();
            var results = allRecords.Where(r => r.Reference.ToLower().Contains(searchRef));
            filteredRecords.Clear();  
            foreach (var record in results)
            {
                filteredRecords.Add(record);
            }
        }

        private void SearchByDateRange()
        {
            if (DateTime.TryParse(txb_SearchDateFrom.Text, out DateTime fromDate) &&
                DateTime.TryParse(txb_SearchDateTo.Text, out DateTime toDate))
            {
                var results = allRecords.Where(r => DateTime.Parse(r.Date) >= fromDate &&
                                                    DateTime.Parse(r.Date) <= toDate);

                filteredRecords.Clear();
                foreach (var record in results)
                {
                    filteredRecords.Add(record);
                }
            }
            else
            {
                MessageBox.Show("Please enter valid dates in the format YYYY-MM-DD.", "Invalid Date Format", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string CalculateTotalTimeSpent(ObservableCollection<Record> records)
        {
            double totalMinutes = records.Sum(r => double.Parse(r.TimeSpent));
            int hours = (int)totalMinutes / 60;
            int minutes = (int)totalMinutes % 60;
            return $"{hours}h {minutes}m";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (filteredRecords.Count == 0)
            {
                MessageBox.Show("No results to save. Please perform a search first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                Title = "Save Report",
                FileName = $"IT_Time_Tracker_Report_{currentDateTime}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("IT Time Tracker Report");
                        writer.WriteLine("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        writer.WriteLine("-----------------------------------");
                        writer.WriteLine();

                        foreach (var record in filteredRecords)
                        {
                            writer.WriteLine($"Reference: {record.Reference}");
                            writer.WriteLine($"Time Spent: {record.TimeSpent}");
                            writer.WriteLine($"Date: {record.Date}");
                            writer.WriteLine("-----------------------------------");
                        }

                        // Add summary information
                        writer.WriteLine();
                        writer.WriteLine("Summary:");
                        writer.WriteLine($"Total Records: {filteredRecords.Count}");
                        writer.WriteLine($"Total Time Spent: {CalculateTotalTimeSpent(filteredRecords)}");
                    }

                    MessageBox.Show("Report saved successfully!", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
