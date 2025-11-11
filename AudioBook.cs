using System;
using System.ComponentModel;

namespace AudioBookViewer
{
    public class AudioBook : INotifyPropertyChanged
    {
        // CSV Column 1: Title
        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        // CSV Column 2: Title Short
        private string titleShort = string.Empty;
        public string TitleShort
        {
            get => titleShort;
            set
            {
                titleShort = value;
                OnPropertyChanged(nameof(TitleShort));
            }
        }

        // CSV Column 3: Series
        private string series = string.Empty;
        public string Series
        {
            get => series;
            set
            {
                series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        // Helper property for sorting - empty series go to end
        public string SeriesSortKey
        {
            get
            {
                // Use a special high-value string for empty series so they sort to the end
                // Using unicode character \uffff which sorts after all normal text
                return string.IsNullOrWhiteSpace(series) ? "\uffff\uffff\uffff" : series;
            }
        }

        // CSV Column 4: Book Numbers
        private string bookNumber = string.Empty;
        public string BookNumber
        {
            get => bookNumber;
            set
            {
                bookNumber = value;
                OnPropertyChanged(nameof(BookNumber));
                
                // Update the numeric value for sorting
                if (double.TryParse(value, out double numericValue))
                {
                    BookNumberValue = numericValue;
                }
                else
                {
                    BookNumberValue = double.MaxValue; // Put non-numeric values at the end
                }
            }
        }

        // Numeric value for proper sorting
        public double BookNumberValue { get; private set; } = double.MaxValue;

        // CSV Column 5: Blurb
        private string blurb = string.Empty;
        public string Blurb
        {
            get => blurb;
            set
            {
                blurb = value;
                OnPropertyChanged(nameof(Blurb));
            }
        }

        // CSV Column 6: Authors
        private string author = string.Empty;
        public string Author
        {
            get => author;
            set
            {
                author = value;
                OnPropertyChanged(nameof(Author));
            }
        }

        // CSV Column 7: Narrators
        private string narrator = string.Empty;
        public string Narrator
        {
            get => narrator;
            set
            {
                narrator = value;
                OnPropertyChanged(nameof(Narrator));
            }
        }

        // CSV Column 8: Tags
        private string tags = string.Empty;
        public string Tags
        {
            get => tags;
            set
            {
                tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        // CSV Column 9: Categories
        private string categories = string.Empty;
        public string Categories
        {
            get => categories;
            set
            {
                categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        // CSV Column 10: Parent Category
        private string parentCategory = string.Empty;
        public string ParentCategory
        {
            get => parentCategory;
            set
            {
                parentCategory = value;
                OnPropertyChanged(nameof(ParentCategory));
            }
        }

        // CSV Column 11: Child Category
        private string childCategory = string.Empty;
        public string ChildCategory
        {
            get => childCategory;
            set
            {
                childCategory = value;
                OnPropertyChanged(nameof(ChildCategory));
            }
        }

        // CSV Column 12: Length
        private string length = string.Empty;
        public string Length
        {
            get => length;
            set
            {
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        // CSV Column 13: Progress
        private string progress = string.Empty;
        public string Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        // CSV Column 14: Release Date
        private string releaseDate = string.Empty;
        public string ReleaseDate
        {
            get => releaseDate;
            set
            {
                releaseDate = value;
                OnPropertyChanged(nameof(ReleaseDate));
            }
        }

        // CSV Column 15: Publishers
        private string publishers = string.Empty;
        public string Publishers
        {
            get => publishers;
            set
            {
                publishers = value;
                OnPropertyChanged(nameof(Publishers));
            }
        }

        // CSV Column 16: My Rating
        private string myRating = string.Empty;
        public string MyRating
        {
            get => myRating;
            set
            {
                myRating = value;
                OnPropertyChanged(nameof(MyRating));
            }
        }

        // CSV Column 17: Rating
        private string rating = string.Empty;
        public string Rating
        {
            get => rating;
            set
            {
                rating = value;
                OnPropertyChanged(nameof(Rating));
                
                // Update the numeric value for sorting
                if (double.TryParse(value, out double numericValue))
                {
                    RatingSortValue = numericValue;
                }
                else
                {
                    RatingSortValue = double.MaxValue; // Put empty/non-numeric values at the end
                }
            }
        }

        // Numeric value for proper sorting - empty ratings go to end
        public double RatingSortValue { get; private set; } = double.MaxValue;

        // CSV Column 18: Ratings
        private string ratings = string.Empty;
        public string Ratings
        {
            get => ratings;
            set
            {
                ratings = value;
                OnPropertyChanged(nameof(Ratings));
                
                // Update the numeric value for sorting
                if (double.TryParse(value, out double numericValue))
                {
                    RatingsSortValue = numericValue;
                }
                else
                {
                    RatingsSortValue = double.MaxValue; // Put empty/non-numeric values at the end
                }
            }
        }

        // Numeric value for proper sorting - empty ratings count go to end
        public double RatingsSortValue { get; private set; } = double.MaxValue;

        // CSV Column 19: Favorite
        private string favorite = string.Empty;
        public string Favorite
        {
            get => favorite;
            set
            {
                favorite = value;
                OnPropertyChanged(nameof(Favorite));
            }
        }

        // CSV Column 20: Format
        private string format = string.Empty;
        public string Format
        {
            get => format;
            set
            {
                format = value;
                OnPropertyChanged(nameof(Format));
            }
        }

        // CSV Column 21: Language
        private string language = string.Empty;
        public string Language
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        // CSV Column 29: ASIN
        private string asin = string.Empty;
        public string ASIN
        {
            get => asin;
            set
            {
                asin = value;
                OnPropertyChanged(nameof(ASIN));
            }
        }

        // CSV Column 30: ISBN10
        private string isbn10 = string.Empty;
        public string ISBN10
        {
            get => isbn10;
            set
            {
                isbn10 = value;
                OnPropertyChanged(nameof(ISBN10));
            }
        }

        // CSV Column 31: ISBN13
        private string isbn13 = string.Empty;
        public string ISBN13
        {
            get => isbn13;
            set
            {
                isbn13 = value;
                OnPropertyChanged(nameof(ISBN13));
            }
        }

        // CSV Column 32: Summary
        private string summary = string.Empty;
        public string Summary
        {
            get => summary;
            set
            {
                summary = value;
                OnPropertyChanged(nameof(Summary));
            }
        }

        // CSV Column 34: Store Page Url
        private string storePageUrl = string.Empty;
        public string StorePageUrl
        {
            get => storePageUrl;
            set
            {
                storePageUrl = value;
                OnPropertyChanged(nameof(StorePageUrl));
            }
        }

        // CSV Column 37: Cover
        private string cover = string.Empty;
        public string Cover
        {
            get => cover;
            set
            {
                cover = value;
                OnPropertyChanged(nameof(Cover));
            }
        }

        // CSV Column 38: Search In Goodreads
        private string searchInGoodreads = string.Empty;
        public string SearchInGoodreads
        {
            get => searchInGoodreads;
            set
            {
                searchInGoodreads = value;
                OnPropertyChanged(nameof(SearchInGoodreads));
            }
        }

        // CSV Column 39: Subtitle
        private string subtitle = string.Empty;
        public string Subtitle
        {
            get => subtitle;
            set
            {
                subtitle = value;
                OnPropertyChanged(nameof(Subtitle));
            }
        }

        // CSV Column 40: Collection Ids
        private string collectionIds = string.Empty;
        public string CollectionIds
        {
            get => collectionIds;
            set
            {
                collectionIds = value;
                OnPropertyChanged(nameof(CollectionIds));
            }
        }

        // Background color for row grouping by author
        private string rowBackground = "White";
        public string RowBackground
        {
            get => rowBackground;
            set
            {
                rowBackground = value;
                OnPropertyChanged(nameof(RowBackground));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

