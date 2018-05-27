using Celebrium_WPF.Models;
using Celebrium_WPF.Other;
using Celebrium_WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    public class StoriesViewModel : BaseViewModel
    {

        public StoriesViewModel()
        {
            Stories = new ObservableCollection<StoryModel>();

            Title1 = "DIGITAL";
            Title2 = "MEMO";

            Stories = LoadStories();
            Stories.CollectionChanged += Stories_CollectionChanged;
        }
        ObservableCollection<StoryModel> stories = new ObservableCollection<StoryModel>();
        private ObservableCollection<StoryModel> LoadStories()
        {
            stories.Clear();
           string[] exts = new[] {  ".jpeg" , ".jpg"};
            var files = Directory
                .GetFiles(App.bankFolder)
                .Where(file => exts.Any(file.ToLower().EndsWith))
                .ToList();   

            foreach(var file in files)
            {
                StoryModel model = new StoryModel(file) { Title = "", Celeb= "", Content="", Memos= "", Series = "", Value = ""};
                stories.Add(model);
                
            }

            var ffiles = Directory
                .GetFiles(App.frackedFolder)
                .Where(file => exts.Any(file.ToLower().EndsWith))
                .ToList();

            foreach (var file in ffiles)
            {
                StoryModel model = new StoryModel(file) { Title = "", Celeb = "", Content = "", Memos = "", Series = "", Value = "" };
                stories.Add(model);

            }


            if (stories.Count < Constants.GALLERY_IMAGE_COUNT)
            {
                for (int i = 0; i < Constants.GALLERY_IMAGE_COUNT - stories.Count; i++)
                {
                    StoryModel blankmodel = new StoryModel("default.jpg") { IsDefault = true, Title = "", Celeb = "", Content = "", Memos = "", Series = "", Value = "" };
                    stories.Add(blankmodel);
                }
            }

            return stories;
        }
        public event EventHandler ShowStoryRequest;

        protected void OnShowStoryRequest()
        {
            if (ShowStoryRequest != null)
            {
                ShowStoryRequest(this, EventArgs.Empty);
            }
        }
        public void Refresh()
        {
            LoadStories();
        }

        public void AddStories(List<CloudCoinCore.CloudCoin> coins)
        {
            foreach(var coin in coins)
            {
                StoryModel model = new StoryModel(MainWindow.FS.BankFolder + coin.FileName + ".jpg") { Title = "", Celeb = "", Content = "", Memos = "", Series = "", Value = "" };

                Application.Current.Dispatcher.Invoke(new Action(() => {
                    Stories.Add(model);
                    /* Your code here */
                }));

                

            }

        }

        private void Stories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Stories));
        }

        public ObservableCollection<StoryModel> Stories { get; set; }

        private StoryModel _selectedItem;

        public StoryModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                OnShowStoryRequest();
            }
        }



    }
}
