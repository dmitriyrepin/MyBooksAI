# Audio Book Collection Viewer

A C# WPF application to view and browse your audio book collection from the `2021-AudioBooks.csv` file.

## Features

- **Grid View**: Displays all audio books in a sortable grid with columns:
  - Author (150px width)
  - Title (flexible width)
  - Series (180px width) - automatically parsed to extract book number
  - Book # (70px width) - extracted from series information
- **Intelligent Series Parsing**: Automatically extracts book numbers from series information:
  - "The Dresden Files, Book 1" → Series: "The Dresden Files", Book #: "1"
  - "The Lord of the Rings, Book 0.5" → Series: "The Lord of the Rings", Book #: "0.5"
  - Book numbers are sorted **numerically** (1, 2, 3, 10) not alphabetically (1, 10, 2, 3)
- **Multi-Level Sorting**: Intelligent sorting with secondary and tertiary sort orders:
  - When sorted by Author: also sorts by Series, then Book #, then Title
  - When sorted by Series: also sorts by Book #, then Author, then Title
  - When sorted by Book #: also sorts by Series, then Author, then Title
  - When sorted by Title: also sorts by Author, then Series
  - **Books without a series always appear at the end** of the list (both ascending and descending)
- **Author-Based Row Grouping**: When sorted by author, rows are shaded by author groups:
  - All books by the same author have the same background color
  - Alternates between white and light gray for each author
  - Makes it easy to visually distinguish between different authors
- **Search**: Filter books by title or author name
- **Detail View**: Shows complete information when a book is selected:
  - Title
  - Author
  - Series (if applicable)
  - Book Number (if applicable)
  - Narrator
  - **Text Selection**: All detail fields support text selection and copying to clipboard (Ctrl+C)
- **Responsive UI**: Resizable window with adjustable splitter between grid and details

## Requirements

- .NET 9.0 SDK or later
- Windows operating system

## Building and Running

### Using Visual Studio

1. Open `AudioBookViewer.csproj` in Visual Studio 2022 or later
2. Press F5 to build and run the application

### Using Command Line

1. Open PowerShell or Command Prompt
2. Navigate to the project directory
3. Run the following commands:

```bash
dotnet build
dotnet run
```

### Creating an Executable

To create a standalone executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in `bin\Release\net9.0-windows\win-x64\publish\`

## Usage

1. Launch the application
2. Browse the grid of audio books on the left showing author, title, series, and book number
3. Use the search box to filter by title or author
4. Click on any book row to view its complete details on the right
5. **Select and copy text** from the details panel:
   - Click and drag to select any text in the detail fields
   - Press Ctrl+C to copy selected text to clipboard
   - Right-click to access context menu for copy operations
6. Click column headers to sort by that column (with intelligent multi-level sorting):
   - Click **Author** header to sort by Author → Series → Book # → Title
     - Rows are grouped with alternating background colors by author
     - All books by the same author share the same background color
   - Click **Series** header to sort by Series → Book # → Author → Title
   - Click **Book #** header to sort by Book # → Series → Author → Title
   - Click **Title** header to sort by Title → Author → Series
   - Click the same header again to reverse the sort direction

## File Structure

- `AudioBook.cs` - Data model for audio book information
- `MainWindow.xaml` - User interface layout
- `MainWindow.xaml.cs` - Application logic and CSV parsing
- `App.xaml` - Application definition
- `App.xaml.cs` - Application entry point
- `AudioBookViewer.csproj` - Project configuration
- `2021-AudioBooks.csv` - Audio book data (must be in the same directory as the executable)

## Notes

- The CSV file must be present in the application directory for the app to work
- The project file is configured to automatically copy the CSV file to the output directory during build


