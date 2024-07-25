IT Time Tracker
IT Time Tracker is a sleek, user-friendly Windows Presentation Foundation (WPF) application designed to help IT professionals and teams track their time spent on various tasks and projects.
No need for complex time tracking software or external services. IT Time Tracker provides a simple, efficient solution for logging and monitoring work hours.
It was created with the intent to allow IT professionals to easily log their time without the need for complicated setups or internet-dependent services. At its core, the goal is to provide a straightforward, offline-capable time tracking solution.
IT Time Tracker DOES NOT integrate with any external time tracking or billing systems. It is a standalone application for personal or team use.

[!tip]
Always back up your data regularly to prevent loss of time tracking information.

Features

Simple Time Entry: Quickly log time entries with reference IDs and time spent.
Real-time Totals: View daily, weekly, monthly, and year-to-date totals at a glance.
Editable Entries: Easily modify existing entries by clicking on them in the list view.
Persistent Storage: All entries are automatically saved and loaded between sessions.
Sleek Design: Modern, neon-themed interface with a transparent window for a unique look.
Minimalistic Controls: Simple submit and reset buttons for easy operation.

How to Use
Using IT Time Tracker is straightforward and intuitive. This guide assumes you have already installed the application on your Windows machine.

[!Note]
IT Time Tracker is a standalone application and does not require an internet connection to function.

Installation:

Download the latest release from the Releases page.
Extract the zip file to your desired location.
Run the IT_Time_Tracker.exe file to start the application.

Using the Application
Adding a Time Entry

Enter a Reference ID in the "Reference ID" field.
Enter the time spent (in minutes) in the "Time" field.
Click the "Submit" button to add the entry.

Viewing Totals
The application automatically calculates and displays totals for:

Daily
Weekly
Monthly
Year-to-Date (YTD)

Editing Entries

Click on any field in the list view to edit an entry.
Press Enter or click outside the field to save changes.
Press Esc to cancel editing.

Resetting the Application

Click the "Reset" button at the bottom right to clear all entries and totals.

Closing the Application

Click the "X" button in the top right corner to close the application.

Minimizing the Application

Click the minimize button next to the close button to minimize the window.

Development
IT Time Tracker is developed using C# and WPF. The main components are:

MainWindow.xaml: The main user interface definition.
MainWindow.xaml.cs: The code-behind file containing the application logic.

Key Classes

Record: Represents a single time entry with properties for Reference, TimeSpent, and Date.
RelayCommand: Implements the ICommand interface for handling button clicks and other actions.

csharpCopypublic class Record : INotifyPropertyChanged
{
    private string _reference;
    public string Reference
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

    // Other properties (TimeSpent, Date) follow the same pattern

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
Contributing
Contributions to IT Time Tracker are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.
License
IT Time Tracker was created by Kenneth Lasyone ©2024. No license is needed for use.
Acknowledgments

Background images: Neon Mountain and Neon Beach (not included in the repository)
UI Elements: Custom-designed buttons and layouts


Thank you for using IT Time Tracker! We hope this tool helps you manage your time more effectively.
