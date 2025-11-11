using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AudioBookViewer
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<AudioBook> allBooks;
        private ObservableCollection<AudioBook> filteredBooks;
        private string currentSortColumn = "";
        private bool isUpdatingFilters = false; // Prevent recursive filter updates

        public MainWindow()
        {
            InitializeComponent();
            allBooks = new ObservableCollection<AudioBook>();
            filteredBooks = new ObservableCollection<AudioBook>();
            BookListBox.ItemsSource = filteredBooks;
            LoadAudioBooks();
        }

        private void LoadAudioBooks()
        {
            try
            {
                string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ALE-spreadsheet-library-v1.csv");
                
                if (!File.Exists(csvPath))
                {
                    MessageBox.Show($"CSV file not found at: {csvPath}\n\nPlease ensure the ALE-spreadsheet-library-v1.csv file is in the application directory.",
                                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var lines = File.ReadAllLines(csvPath);
                
                // Skip header row
                for (int i = 1; i < lines.Length; i++)
                {
                    var book = ParseCsvLine(lines[i]);
                    if (book != null)
                    {
                        allBooks.Add(book);
                        filteredBooks.Add(book);
                    }
                }

                Title = $"Audio Book Collection ({allBooks.Count} books)";
                
                // Initialize row backgrounds
                UpdateRowBackgrounds();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audio books: {ex.Message}", 
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private AudioBook? ParseCsvLine(string line)
        {
            try
            {
                var fields = new List<string>();
                bool inQuotes = false;
                string currentField = "";

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '"')
                    {
                        inQuotes = !inQuotes;
                    }
                    else if (c == ',' && !inQuotes)
                    {
                        fields.Add(currentField);
                        currentField = "";
                    }
                    else
                    {
                        currentField += c;
                    }
                }
                fields.Add(currentField); // Add the last field

                // ALE-spreadsheet-library-v1.csv format has 41 columns
                if (fields.Count >= 41)
                {
                    var (seriesName, bookNumber) = ParseSeriesInfo(fields[3].Trim(), fields[4].Trim());
                    
                    return new AudioBook
                    {
                        Title = fields[1].Trim(),
                        TitleShort = fields[2].Trim(),
                        Series = seriesName,
                        BookNumber = bookNumber,
                        Blurb = fields[5].Trim(),
                        Author = fields[6].Trim(),
                        Narrator = fields[7].Trim(),
                        Tags = fields[8].Trim(),
                        Categories = fields[9].Trim(),
                        ParentCategory = fields[10].Trim(),
                        ChildCategory = fields[11].Trim(),
                        Length = fields[12].Trim(),
                        Progress = fields[13].Trim(),
                        ReleaseDate = fields[14].Trim(),
                        Publishers = fields[15].Trim(),
                        MyRating = fields[16].Trim(),
                        Rating = fields[17].Trim(),
                        Ratings = fields[18].Trim(),
                        Favorite = fields[19].Trim(),
                        Format = fields[20].Trim(),
                        Language = fields[21].Trim(),
                        ASIN = fields[29].Trim(),
                        ISBN10 = fields[30].Trim(),
                        ISBN13 = fields[31].Trim(),
                        Summary = fields[32].Trim(),
                        StorePageUrl = fields[34].Trim(),
                        Cover = fields[37].Trim(),
                        SearchInGoodreads = fields[38].Trim(),
                        Subtitle = fields[39].Trim(),
                        CollectionIds = fields[40].Trim()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {ex.Message}");
            }

            return null;
        }

        private (string seriesName, string bookNumber) ParseSeriesInfo(string seriesText, string bookNumberText)
        {
            string seriesName = string.Empty;
            string bookNumber = bookNumberText.Trim();

            if (string.IsNullOrWhiteSpace(seriesText))
            {
                return (string.Empty, bookNumber);
            }

            // Pattern to match "(book X)" where X is a number - new format
            var bookPattern = @"\s*\(book\s+\d+\)";
            var match = Regex.Match(seriesText, bookPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                // Remove the "(book X)" part from the series name
                seriesName = Regex.Replace(seriesText, bookPattern, "", RegexOptions.IgnoreCase).Trim();
            }
            else
            {
                // No book pattern found, use the series text as is
                seriesName = seriesText.Trim();
            }

            return (seriesName, bookNumber);
        }

        private void DisplaySummaryAsHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                SummaryBrowser.NavigateToString("<html><body style='font-family: Segoe UI, Arial, sans-serif; font-size: 16px; margin: 10px;'><p><i>(Not specified)</i></p></body></html>");
            }
            else
            {
                // Wrap the HTML content in a proper HTML document with styling
                string htmlDocument = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            font-size: 16px;
            margin: 10px;
            color: #333;
            line-height: 1.6;
        }}
        p {{
            margin: 0 0 10px 0;
        }}
        br {{
            line-height: 1.8;
        }}
    </style>
</head>
<body>
    {htmlContent}
</body>
</html>";
                SummaryBrowser.NavigateToString(htmlDocument);
            }
        }

        private void LoadCoverImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    CoverImage.Source = null;
                    return;
                }

                // Create BitmapImage from URL
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                
                CoverImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cover image: {ex.Message}");
                CoverImage.Source = null;
            }
        }

        private void BookListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookListBox.SelectedItem is AudioBook selectedBook)
            {
                // Show details panel and hide placeholder
                DetailsPanel.Visibility = Visibility.Visible;
                NoSelectionText.Visibility = Visibility.Collapsed;

                // Helper method to display value or placeholder
                string DisplayValue(string value, string placeholder = "(Not specified)") =>
                    string.IsNullOrWhiteSpace(value) ? placeholder : value;

                // Update all detail fields
                TitleText.Text = DisplayValue(selectedBook.Title);
                TitleShortText.Text = DisplayValue(selectedBook.TitleShort);
                AuthorText.Text = DisplayValue(selectedBook.Author);
                NarratorText.Text = DisplayValue(selectedBook.Narrator);
                SeriesText.Text = DisplayValue(selectedBook.Series, "(Not part of a series)");
                BookNumberText.Text = DisplayValue(selectedBook.BookNumber);
                SubtitleText.Text = DisplayValue(selectedBook.Subtitle);
                BlurbText.Text = DisplayValue(selectedBook.Blurb);
                
                // Display Summary as HTML
                DisplaySummaryAsHtml(selectedBook.Summary);
                
                LengthText.Text = DisplayValue(selectedBook.Length);
                ProgressText.Text = DisplayValue(selectedBook.Progress);
                ReleaseDateText.Text = DisplayValue(selectedBook.ReleaseDate);
                PublishersText.Text = DisplayValue(selectedBook.Publishers);
                RatingText.Text = DisplayValue(selectedBook.Rating);
                RatingsText.Text = DisplayValue(selectedBook.Ratings);
                MyRatingText.Text = DisplayValue(selectedBook.MyRating);
                FavoriteText.Text = DisplayValue(selectedBook.Favorite);
                CategoriesText.Text = DisplayValue(selectedBook.Categories);
                ParentCategoryText.Text = DisplayValue(selectedBook.ParentCategory);
                ChildCategoryText.Text = DisplayValue(selectedBook.ChildCategory);
                TagsText.Text = DisplayValue(selectedBook.Tags);
                FormatText.Text = DisplayValue(selectedBook.Format);
                LanguageText.Text = DisplayValue(selectedBook.Language);
                ASINText.Text = DisplayValue(selectedBook.ASIN);
                ISBN10Text.Text = DisplayValue(selectedBook.ISBN10);
                ISBN13Text.Text = DisplayValue(selectedBook.ISBN13);
                StorePageUrlText.Text = DisplayValue(selectedBook.StorePageUrl);
                SearchInGoodreadsText.Text = DisplayValue(selectedBook.SearchInGoodreads);
                CollectionIdsText.Text = DisplayValue(selectedBook.CollectionIds);
                
                // Load cover image from URL
                LoadCoverImage(selectedBook.Cover);
                
                // Set DetailsPanel background to match the selected row's author color
                var converter = new AuthorToColorConverter();
                var brush = converter.Convert(selectedBook.Author, typeof(SolidColorBrush), null!, CultureInfo.CurrentCulture) as SolidColorBrush;
                DetailsPanel.Background = brush ?? Brushes.White;
                
                // Enable filter toggle buttons
                SameAuthorToggle.IsEnabled = true;
                SameSeriesToggle.IsEnabled = !string.IsNullOrWhiteSpace(selectedBook.Series);
                SameNarratorToggle.IsEnabled = !string.IsNullOrWhiteSpace(selectedBook.Narrator);
                
                // Update search icon color (removes light green when clicking on book)
                UpdateSearchIconColor();
            }
            else
            {
                // Hide details panel and show placeholder
                DetailsPanel.Visibility = Visibility.Collapsed;
                NoSelectionText.Visibility = Visibility.Visible;
                DetailsPanel.Background = Brushes.White;
                
                // Disable filter toggle buttons
                SameAuthorToggle.IsEnabled = false;
                SameSeriesToggle.IsEnabled = false;
                SameNarratorToggle.IsEnabled = false;
                
                // Update search icon color
                UpdateSearchIconColor();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingFilters) return; // Prevent recursive calls
            
            // Update search icon background color
            UpdateSearchIconColor();
            
            // If user types in search box, uncheck all filter toggles and reset progress filter (mutually exclusive)
            if (!string.IsNullOrWhiteSpace(SearchBox?.Text))
            {
                isUpdatingFilters = true;
                if (SameAuthorToggle != null) SameAuthorToggle.IsChecked = false;
                if (SameSeriesToggle != null) SameSeriesToggle.IsChecked = false;
                if (SameNarratorToggle != null) SameNarratorToggle.IsChecked = false;
                if (ProgressFilterComboBox != null) ProgressFilterComboBox.SelectedIndex = 0; // Reset to "All Progress"
                // Clear selection when using search filter
                if (BookListBox != null) BookListBox.SelectedItem = null;
                isUpdatingFilters = false;
            }
            
            ApplyFilters();
        }

        private void UpdateSearchIconColor()
        {
            // Update search icon background color based on text content only
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                // Has text: LimeGreen
                SearchIconBackground.Background = new SolidColorBrush(Color.FromRgb(50, 205, 50)); // LimeGreen
            }
            else
            {
                // No text: LightGray
                SearchIconBackground.Background = new SolidColorBrush(Color.FromRgb(211, 211, 211)); // LightGray
            }
        }

        private void SearchIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Clear the search textbox when the search icon is clicked
            SearchBox.Text = "";
        }

        private void ApplyFilters()
        {
            // Save the current selection
            var currentSelection = BookListBox.SelectedItem as AudioBook;
            
            filteredBooks.Clear();
            
            // Apply filters - mutually exclusive (only one at a time)
            foreach (var book in allBooks)
            {
                bool includeBook = false;

                // Priority 1: "Same Author" filter - filter by author ONLY
                if (SameAuthorToggle.IsChecked == true)
                {
                    if (currentSelection != null)
                    {
                        string bookAuthor = book.Author?.Trim() ?? "";
                        string filterAuthor = currentSelection.Author?.Trim() ?? "";
                        
                        if (string.Equals(bookAuthor, filterAuthor, StringComparison.OrdinalIgnoreCase))
                        {
                            includeBook = true;
                        }
                    }
                }
                // Priority 2: "Same Series" filter - filter by series ONLY
                else if (SameSeriesToggle.IsChecked == true)
                {
                    if (currentSelection != null)
                    {
                        string bookSeries = book.Series?.Trim() ?? "";
                        string filterSeries = currentSelection.Series?.Trim() ?? "";
                        
                        if (!string.IsNullOrWhiteSpace(filterSeries) &&
                            string.Equals(bookSeries, filterSeries, StringComparison.OrdinalIgnoreCase))
                        {
                            includeBook = true;
                        }
                    }
                }
                // Priority 3: "Same Narrator" filter - filter by narrator ONLY
                else if (SameNarratorToggle.IsChecked == true)
                {
                    if (currentSelection != null)
                    {
                        string bookNarrator = book.Narrator?.Trim() ?? "";
                        string filterNarrator = currentSelection.Narrator?.Trim() ?? "";
                        
                        if (!string.IsNullOrWhiteSpace(filterNarrator) &&
                            string.Equals(bookNarrator, filterNarrator, StringComparison.OrdinalIgnoreCase))
                        {
                            includeBook = true;
                        }
                    }
                }
                // Priority 4: Progress filter - filter by progress status
                else if (ProgressFilterComboBox?.SelectedIndex > 0)
                {
                    var selectedProgress = (ProgressFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    if (!string.IsNullOrEmpty(selectedProgress))
                    {
                        if (string.Equals(book.Progress, selectedProgress, StringComparison.OrdinalIgnoreCase))
                        {
                            includeBook = true;
                        }
                    }
                }
                // Priority 5: Search filter - filter by title or author text search
                else if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    string searchText = SearchBox.Text.ToLower();
                    if (book.Title?.ToLower().Contains(searchText) == true || 
                        book.Author?.ToLower().Contains(searchText) == true)
                    {
                        includeBook = true;
                    }
                }
                // No filter active - show all books
                else
                {
                    includeBook = true;
                }

                if (includeBook)
                {
                    filteredBooks.Add(book);
                }
            }
            
            // Restore the selection if the book is still in the filtered list
            if (currentSelection != null && filteredBooks.Contains(currentSelection))
            {
                BookListBox.SelectedItem = currentSelection;
            }
            
            // Update row backgrounds after filtering
            UpdateRowBackgrounds();
        }

        private void SameAuthorToggle_Click(object sender, RoutedEventArgs e)
        {
            if (isUpdatingFilters) return; // Prevent recursive calls
            
            // Check if a book is selected when enabling the filter
            if (SameAuthorToggle.IsChecked == true)
            {
                var selectedBook = BookListBox.SelectedItem as AudioBook;
                if (selectedBook == null)
                {
                    MessageBox.Show("Please select a book first to filter by author.", 
                                    "No Book Selected", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                    SameAuthorToggle.IsChecked = false;
                    return;
                }
            }
            
            isUpdatingFilters = true;
            
            // Make toggles mutually exclusive with each other and with search
            if (SameAuthorToggle.IsChecked == true)
            {
                // Turn off other filters
                SameSeriesToggle.IsChecked = false;
                SameNarratorToggle.IsChecked = false;
                ProgressFilterComboBox.SelectedIndex = 0; // Reset to "All Progress"
                SearchBox.Text = ""; // Clear search box
            }
            
            isUpdatingFilters = false;
            ApplyFilters();
        }

        private void SameSeriesToggle_Click(object sender, RoutedEventArgs e)
        {
            if (isUpdatingFilters) return; // Prevent recursive calls
            
            // Check if a book is selected when enabling the filter
            if (SameSeriesToggle.IsChecked == true)
            {
                var selectedBook = BookListBox.SelectedItem as AudioBook;
                if (selectedBook == null)
                {
                    MessageBox.Show("Please select a book first to filter by series.", 
                                    "No Book Selected", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                    SameSeriesToggle.IsChecked = false;
                    return;
                }
            }
            
            isUpdatingFilters = true;
            
            // Make toggles mutually exclusive with each other and with search
            if (SameSeriesToggle.IsChecked == true)
            {
                // Turn off other filters
                SameAuthorToggle.IsChecked = false;
                SameNarratorToggle.IsChecked = false;
                ProgressFilterComboBox.SelectedIndex = 0; // Reset to "All Progress"
                SearchBox.Text = ""; // Clear search box
            }
            
            isUpdatingFilters = false;
            ApplyFilters();
        }

        private void SameNarratorToggle_Click(object sender, RoutedEventArgs e)
        {
            if (isUpdatingFilters) return; // Prevent recursive calls
            
            // Check if a book is selected when enabling the filter
            if (SameNarratorToggle.IsChecked == true)
            {
                var selectedBook = BookListBox.SelectedItem as AudioBook;
                if (selectedBook == null)
                {
                    MessageBox.Show("Please select a book first to filter by narrator.", 
                                    "No Book Selected", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                    SameNarratorToggle.IsChecked = false;
                    return;
                }
            }
            
            isUpdatingFilters = true;
            
            // Make toggles mutually exclusive with each other and with search
            if (SameNarratorToggle.IsChecked == true)
            {
                // Turn off other filters
                SameAuthorToggle.IsChecked = false;
                SameSeriesToggle.IsChecked = false;
                ProgressFilterComboBox.SelectedIndex = 0; // Reset to "All Progress"
                SearchBox.Text = ""; // Clear search box
            }
            
            isUpdatingFilters = false;
            ApplyFilters();
        }

        private void ProgressFilterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (isUpdatingFilters) return; // Prevent recursive calls
            if (ProgressFilterComboBox == null || BookListBox == null || SearchBox == null) return; // Control not initialized yet
            if (!IsLoaded) return; // Window not fully loaded yet
            
            // If a specific progress filter is selected (not "All Progress"), turn off other filters
            if (ProgressFilterComboBox.SelectedIndex > 0)
            {
                isUpdatingFilters = true;
                SameAuthorToggle.IsChecked = false;
                SameSeriesToggle.IsChecked = false;
                SameNarratorToggle.IsChecked = false;
                SearchBox.Text = ""; // Clear search box
                // Clear selection when using progress filter
                BookListBox.SelectedItem = null;
                isUpdatingFilters = false;
            }
            
            ApplyFilters();
        }

        private void BookListBox_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            var column = e.Column;
            var sortPropertyName = column.SortMemberPath;

            // Get the collection view
            var view = CollectionViewSource.GetDefaultView(BookListBox.ItemsSource);
            
            if (view == null)
                return;

            // Determine the sort direction
            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) 
                ? ListSortDirection.Ascending 
                : ListSortDirection.Descending;

            // Clear existing sort descriptions and custom sort
            view.SortDescriptions.Clear();
            
            // Clear custom sort if it's a ListCollectionView
            if (view is ListCollectionView listView)
            {
                listView.CustomSort = null;
            }

            // Apply multi-level sorting based on the column
            if (sortPropertyName == "Author")
            {
                // Primary: Author, Secondary: Series (with custom handling), Tertiary: BookNumberValue (numeric), Quaternary: Title
                view.SortDescriptions.Add(new SortDescription("Author", direction));
                // Use SeriesSortKey with ascending to push empty series to end
                view.SortDescriptions.Add(new SortDescription("SeriesSortKey", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("BookNumberValue", direction));
                view.SortDescriptions.Add(new SortDescription("Title", direction));
            }
            else if (sortPropertyName == "Series")
            {
                // Apply custom sorting: non-empty series in requested direction, empty series always at end
                // CustomSort handles all comparison logic including secondary sorts
                if (view is ListCollectionView listCollectionView)
                {
                    listCollectionView.CustomSort = new SeriesComparer(direction);
                }
            }
            else if (sortPropertyName == "BookNumber")
            {
                // Primary: BookNumberValue (numeric), Secondary: Series (with custom handling), Tertiary: Author, Quaternary: Title
                view.SortDescriptions.Add(new SortDescription("BookNumberValue", direction));
                view.SortDescriptions.Add(new SortDescription("SeriesSortKey", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("Author", direction));
                view.SortDescriptions.Add(new SortDescription("Title", direction));
            }
            else if (sortPropertyName == "Title")
            {
                // Primary: Title, Secondary: Author, Tertiary: Series (with custom handling)
                view.SortDescriptions.Add(new SortDescription("Title", direction));
                view.SortDescriptions.Add(new SortDescription("Author", direction));
                view.SortDescriptions.Add(new SortDescription("SeriesSortKey", ListSortDirection.Ascending));
            }
            else if (sortPropertyName == "Narrator")
            {
                // Primary: Narrator, Secondary: Author, Tertiary: Title
                view.SortDescriptions.Add(new SortDescription("Narrator", direction));
                view.SortDescriptions.Add(new SortDescription("Author", direction));
                view.SortDescriptions.Add(new SortDescription("Title", direction));
            }
            else if (sortPropertyName == "Rating")
            {
                // Apply custom sorting: non-empty ratings in requested direction, empty ratings always at end
                if (view is ListCollectionView listCollectionView)
                {
                    listCollectionView.CustomSort = new RatingComparer(direction);
                }
            }
            else if (sortPropertyName == "Ratings")
            {
                // Apply custom sorting: non-empty ratings count in requested direction, empty always at end
                if (view is ListCollectionView listCollectionView)
                {
                    listCollectionView.CustomSort = new RatingsComparer(direction);
                }
            }
            else if (sortPropertyName == "Progress")
            {
                // Primary: ProgressValue (numeric 0-3), Secondary: Author, Tertiary: Title
                view.SortDescriptions.Add(new SortDescription("ProgressValue", direction));
                view.SortDescriptions.Add(new SortDescription("Author", direction));
                view.SortDescriptions.Add(new SortDescription("Title", direction));
            }

            // Update the column sort direction indicator
            column.SortDirection = direction;

            // Clear sort direction indicators from other columns
            foreach (var col in BookListBox.Columns)
            {
                if (col != column)
                {
                    col.SortDirection = null;
                }
            }

            // Refresh the view
            view.Refresh();
            
            // Store the current sort column
            currentSortColumn = sortPropertyName ?? "";
            
            // Update row backgrounds based on author grouping if sorted by author
            UpdateRowBackgrounds();
        }

        private void UpdateRowBackgrounds()
        {
            // Apply author-based grouping only when sorted by Author
            if (currentSortColumn == "Author")
            {
                string previousAuthor = "";
                bool useAlternateColor = false;
                string color1 = "White";
                string color2 = "#F5F5F5"; // WhiteSmoke

                foreach (var book in filteredBooks)
                {
                    if (book.Author != previousAuthor)
                    {
                        // Author changed, toggle the color
                        useAlternateColor = !useAlternateColor;
                        previousAuthor = book.Author;
                    }

                    book.RowBackground = useAlternateColor ? color2 : color1;
                }
            }
            else
            {
                // For other sorts, use standard alternating rows
                for (int i = 0; i < filteredBooks.Count; i++)
                {
                    filteredBooks[i].RowBackground = (i % 2 == 0) ? "White" : "#F5F5F5";
                }
            }
        }
    }

    // Custom comparer for Series sorting that keeps empty series at the end
    public class SeriesComparer : System.Collections.IComparer
    {
        private readonly ListSortDirection _direction;

        public SeriesComparer(ListSortDirection direction)
        {
            _direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not AudioBook bookX || y is not AudioBook bookY)
                return 0;

            bool xEmpty = string.IsNullOrWhiteSpace(bookX.Series);
            bool yEmpty = string.IsNullOrWhiteSpace(bookY.Series);

            // Both empty - equal
            if (xEmpty && yEmpty)
                return 0;

            // X empty - X goes after Y (empty always at end)
            if (xEmpty)
                return 1;

            // Y empty - Y goes after X (empty always at end)
            if (yEmpty)
                return -1;

            // Neither empty - sort normally by direction
            int comparison = string.Compare(bookX.Series, bookY.Series, StringComparison.OrdinalIgnoreCase);
            
            // Apply direction
            if (_direction == ListSortDirection.Descending)
                comparison = -comparison;

            // If series are equal, compare by book number
            if (comparison == 0)
            {
                comparison = bookX.BookNumberValue.CompareTo(bookY.BookNumberValue);
                if (_direction == ListSortDirection.Descending)
                    comparison = -comparison;
            }

            // If still equal, compare by author
            if (comparison == 0)
            {
                comparison = string.Compare(bookX.Author, bookY.Author, StringComparison.OrdinalIgnoreCase);
                if (_direction == ListSortDirection.Descending)
                    comparison = -comparison;
            }

            // If still equal, compare by title
            if (comparison == 0)
            {
                comparison = string.Compare(bookX.Title, bookY.Title, StringComparison.OrdinalIgnoreCase);
                if (_direction == ListSortDirection.Descending)
                    comparison = -comparison;
            }

            return comparison;
        }
    }

    // Custom comparer for Rating (Stars) sorting that keeps empty ratings at the end
    public class RatingComparer : System.Collections.IComparer
    {
        private readonly ListSortDirection _direction;

        public RatingComparer(ListSortDirection direction)
        {
            _direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not AudioBook bookX || y is not AudioBook bookY)
                return 0;

            bool xEmpty = string.IsNullOrWhiteSpace(bookX.Rating);
            bool yEmpty = string.IsNullOrWhiteSpace(bookY.Rating);

            // Both empty - equal
            if (xEmpty && yEmpty)
                return 0;

            // X empty - X goes after Y (empty always at end)
            if (xEmpty)
                return 1;

            // Y empty - Y goes after X (empty always at end)
            if (yEmpty)
                return -1;

            // Neither empty - sort by numeric value
            int comparison = bookX.RatingSortValue.CompareTo(bookY.RatingSortValue);
            
            // Apply direction
            if (_direction == ListSortDirection.Descending)
                comparison = -comparison;

            // If ratings are equal, compare by ratings count (always descending - higher counts first)
            if (comparison == 0)
            {
                comparison = bookY.RatingsSortValue.CompareTo(bookX.RatingsSortValue);
            }

            // If still equal, compare by title
            if (comparison == 0)
            {
                comparison = string.Compare(bookX.Title, bookY.Title, StringComparison.OrdinalIgnoreCase);
                if (_direction == ListSortDirection.Descending)
                    comparison = -comparison;
            }

            return comparison;
        }
    }

    // Custom comparer for Ratings (count) sorting that keeps empty ratings at the end
    public class RatingsComparer : System.Collections.IComparer
    {
        private readonly ListSortDirection _direction;

        public RatingsComparer(ListSortDirection direction)
        {
            _direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not AudioBook bookX || y is not AudioBook bookY)
                return 0;

            bool xEmpty = string.IsNullOrWhiteSpace(bookX.Ratings);
            bool yEmpty = string.IsNullOrWhiteSpace(bookY.Ratings);

            // Both empty - equal
            if (xEmpty && yEmpty)
                return 0;

            // X empty - X goes after Y (empty always at end)
            if (xEmpty)
                return 1;

            // Y empty - Y goes after X (empty always at end)
            if (yEmpty)
                return -1;

            // Neither empty - sort by numeric value
            int comparison = bookX.RatingsSortValue.CompareTo(bookY.RatingsSortValue);
            
            // Apply direction
            if (_direction == ListSortDirection.Descending)
                comparison = -comparison;

            // If ratings count are equal, compare by rating value
            if (comparison == 0)
            {
                comparison = bookX.RatingSortValue.CompareTo(bookY.RatingSortValue);
                if (_direction == ListSortDirection.Descending)
                    comparison = -comparison;
            }

            // If still equal, compare by title
            if (comparison == 0)
            {
                comparison = string.Compare(bookX.Title, bookY.Title, StringComparison.OrdinalIgnoreCase);
                if (_direction == ListSortDirection.Descending)
                    comparison = -comparison;
            }

            return comparison;
        }
    }

    // Converter to assign distinct colors to each author
    public class AuthorToColorConverter : IValueConverter
    {
        private static readonly Dictionary<string, SolidColorBrush> AuthorColors = new Dictionary<string, SolidColorBrush>();
        
        private static readonly Color[] ColorPalette = new Color[]
        {
            Color.FromRgb(255, 250, 240), // FloralWhite
            Color.FromRgb(240, 248, 255), // AliceBlue
            Color.FromRgb(245, 255, 250), // MintCream
            Color.FromRgb(255, 250, 250), // Snow
            Color.FromRgb(255, 248, 220), // Cornsilk
            Color.FromRgb(250, 250, 210), // LightGoldenrodYellow
            Color.FromRgb(255, 239, 213), // PapayaWhip
            Color.FromRgb(255, 245, 238), // Seashell
            Color.FromRgb(245, 245, 220), // Beige
            Color.FromRgb(230, 230, 250), // Lavender
            Color.FromRgb(240, 255, 240), // Honeydew
            Color.FromRgb(248, 248, 255), // GhostWhite
            Color.FromRgb(240, 255, 255), // Azure
            Color.FromRgb(255, 240, 245), // LavenderBlush
            Color.FromRgb(245, 245, 245), // WhiteSmoke
            Color.FromRgb(255, 255, 240), // Ivory
            Color.FromRgb(240, 230, 140), // Khaki (lighter)
            Color.FromRgb(255, 228, 225), // MistyRose
            Color.FromRgb(255, 235, 205), // BlanchedAlmond
            Color.FromRgb(250, 235, 215), // AntiqueWhite
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string author && !string.IsNullOrWhiteSpace(author))
            {
                if (!AuthorColors.ContainsKey(author))
                {
                    // Assign a color based on hash of author name for consistency
                    int hash = author.GetHashCode();
                    int colorIndex = Math.Abs(hash) % ColorPalette.Length;
                    AuthorColors[author] = new SolidColorBrush(ColorPalette[colorIndex]);
                }
                
                return AuthorColors[author];
            }
            
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

