using Celebrium_WPF.Models;
using Celebrium_WPF.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    public class MainAppViewModel:BaseViewModel
    {
        StoriesViewModel vmStories;

        StoryViewModel vmStory;
        AddCollectableViewModel vmAddCollectable;
        BackUpCollectableViewModel vmBackUpCollectable;

        public MainAppViewModel()
        {
            vmStories = new ViewModels.StoriesViewModel();

            vmStory = new ViewModels.StoryViewModel();
            vmAddCollectable = new ViewModels.AddCollectableViewModel();
            vmBackUpCollectable = new ViewModels.BackUpCollectableViewModel();

            vmStories.ShowStoryRequest += VmStories_ShowStoryRequest;

            vmStory.RequestBackNavigation += vm_RequestBackNavigation;
            vmAddCollectable.RequestBackNavigation += vm_RequestBackNavigation;
            vmBackUpCollectable.RequestBackNavigation += vm_RequestBackNavigation;

            CurrentView = vmStories;
            //TODO jsut set the value of the "Version" property to what ever you want, it will autoupdate in the view
            Version = "ALPHA RELEASE V 2.0";
        }

        private void vm_RequestBackNavigation(object sender, EventArgs e)
        {
            vmStories.SelectedItem = null;
            CurrentView = vmStories;
        }

        private void VmStories_ShowStoryRequest(object sender, EventArgs e)
        {
            vmStory.Story = vmStories.SelectedItem;
            CurrentView = vmStory;
        }

        private string _version;

        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }


        private BaseViewModel _currentView;
        public BaseViewModel CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                _currentView = value;
                this.Title1 = value.Title1;
                this.Title2 = value.Title2;

                if (Title1 == "MEMO")
                {
                    ShowFirst = System.Windows.Visibility.Visible;
                    Title1 = Title2;

                }
                else
                {
                    ShowFirst = System.Windows.Visibility.Collapsed;
                }

                if (Title2 == "MEMO")
                {
                    ShowLast = System.Windows.Visibility.Visible;
                }
                else
                {
                    ShowLast = System.Windows.Visibility.Collapsed;
                }


                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private void mShowStory(object obj)
        {
            VmStories_ShowStoryRequest(null, EventArgs.Empty);
        }

        public ICommand ShowStory
        {
            get { return new ActionCommand(mShowStory); }
        }

        private void mShowAddCollectable(object obj)
        {
            CurrentView = vmAddCollectable;
        }

        public ICommand ShowAddCollectable
        {
            get { return new ActionCommand(mShowAddCollectable); }
        }

        private void mShowBackupCollectable(object obj)
        {
            CurrentView = vmBackUpCollectable;
        }

        public ICommand ShowBackUpCollectable
        {
            get { return new ActionCommand(mShowBackupCollectable); }
        }

        private void mShowActivityHistory(object obj)
        {
            //TODO write code here
            System.Windows.Forms.MessageBox.Show("Write the activity logic here");
        }

        public ICommand ShowActivityHistory
        {
            get { return new ActionCommand(mShowActivityHistory); }
        }

        private void mShowExportCollectable(object obj)
        {
            //TODO write code here
            System.Windows.Forms.MessageBox.Show("Write the export logic here");
        }

        public ICommand ShowExportCollectable
        {
            get { return new ActionCommand(mShowExportCollectable); }
        }

        private void mNews(object obj)
        {
            //TODO write code here
            System.Diagnostics.Process.Start("http://celebrium.com/news");
        }

        public ICommand News
        {
            get { return new ActionCommand(mNews); }
        }

        private void mShop(object obj)
        {
            //TODO write code here
            System.Diagnostics.Process.Start("http://celebrium.com/");
        }

        public ICommand Shop
        {
            get { return new ActionCommand(mShop); }
        }

        private void mCustomerSupport(object obj)
        {
            //TODO write code here
            System.Diagnostics.Process.Start("http://celebrium.com/");
        }

        public ICommand CustomerSupport
        {
            get { return new ActionCommand(mCustomerSupport); }
        }

        private void mSoftwareUpdate(object obj)
        {
            //TODO write code here
            System.Windows.Forms.MessageBox.Show("Write the software update logic here");
        }

        public ICommand SoftwareUpdate
        {
            get { return new ActionCommand(mSoftwareUpdate); }
        }

    }
}
