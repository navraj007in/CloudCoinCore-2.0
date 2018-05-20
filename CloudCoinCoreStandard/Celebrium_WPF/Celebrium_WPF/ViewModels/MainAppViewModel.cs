using Celebrium_WPF.Models;
using Celebrium_WPF.Other;
using CloudCoinCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    public class MainAppViewModel:BaseViewModel
    {
        public static StoriesViewModel vmStories;

        StoryViewModel vmStory;
        AddCollectableViewModel vmAddCollectable;
        BackUpCollectableViewModel vmBackUpCollectable;
        ActivityLogModel vmactivityLogModel;

        public MainAppViewModel()
        {
            vmStories = new ViewModels.StoriesViewModel();

            vmStory = new ViewModels.StoryViewModel();
            vmAddCollectable = new ViewModels.AddCollectableViewModel();
            vmBackUpCollectable = new ViewModels.BackUpCollectableViewModel();
            vmactivityLogModel = new ActivityLogModel();

            vmStories.ShowStoryRequest += VmStories_ShowStoryRequest;
            
            vmStory.RequestBackNavigation += vm_RequestBackNavigation;
            vmAddCollectable.RequestBackNavigation += vm_RequestBackNavigation;
            vmBackUpCollectable.RequestBackNavigation += vm_RequestBackNavigation;
            vmactivityLogModel.RequestBackNavigation += vm_RequestBackNavigation;
            CurrentView = vmStories;
            //TODO jsut set the value of the "Version" property to what ever you want, it will autoupdate in the view
            Version = "ALPHA RELEASE V 1.0";
        }

        private void vm_RequestBackNavigation(object sender, EventArgs e)
        {
            vmStories.SelectedItem = null;
            vmStories.Refresh();
            CurrentView = vmStories;
        }

        private void VmStories_ShowStoryRequest(object sender, EventArgs e)
        {
            vmStory.Story = vmStories.SelectedItem;
            if (vmStories.SelectedItem!=null &&  !vmStories.SelectedItem.IsDefault)
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
            //CurrentView = vmBackUpCollectable;
            Backup();
        }

        private void Backup()
        {
            var bankCoins = MainWindow.FS.LoadFolderCoins(MainWindow.FS.BankFolder);
            var frackedCoins = MainWindow.FS.LoadFolderCoins(MainWindow.FS.FrackedFolder);
            //var partialCoins = MainWindow.FS.LoadFolderCoins(FS.PartialFolder);

            // Add them all up in a single list for backup

            bankCoins.AddRange(frackedCoins);
            //bankCoins.AddRange(partialCoins);

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {

                    if((dialog.SelectedPath + Path.DirectorySeparatorChar).Replace("\\\\", "\\") == MainWindow.FS.BankFolder.Replace("\\\\", "\\"))
                    {
                        MessageBox.Show("Cant save");
                        return;
                    }
                    string backupFileName = "celebrium_backup" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower() ;
                    MainWindow.FS.WriteCoinsToFile(bankCoins, dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar +
                                        backupFileName, ".celebrium");
                    MainWindow.printLineDots();
                    MainWindow.updateLog("Backup file " + backupFileName + " saved to " + dialog.SelectedPath + " .");
                    MainWindow.printLineDots();

                    MainWindow.updateActivityLog("Celebriums backed up to " + backupFileName);
                    MessageBox.Show("Memos Backup completed successfully.");
                }
            }

        }

        public ICommand ShowBackUpCollectable
        {
            get { return new ActionCommand(mShowBackupCollectable); }
        }

        private void mShowActivityHistory(object obj)
        {
            //TODO write code here
            CurrentView = vmactivityLogModel;
           // System.Windows.Forms.MessageBox.Show("Write the activity logic here");
        }

        public ICommand ShowActivityHistory
        {
            get { return new ActionCommand(mShowActivityHistory); }
        }

        private void mShowExportCollectable(object obj)
        {
            //TODO write code here
            if(CurrentView == vmStory)
            {
                var exportResult = MessageBox.Show("Are you sure you want to export this memo?", "Memo Export", MessageBoxButton.YesNo);
                if (exportResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        //JpegWrite(vmStory.Story.CoinPath, vmStory.Story.ImagePath, "", "", vmStory.Story);
                        string fileName = System.IO.Path.GetFileName (vmStory.Story.ImagePath );
                        string exportPath = MainWindow.FS.ExportFolder + Path.DirectorySeparatorChar + fileName;

                        File.Copy(vmStory.Story.ImagePath, exportPath);
                        File.Delete(vmStory.Story.ImagePath);
                        Process.Start(MainWindow.FS.ExportFolder);
                       // vmStories.Refresh();
                        vmStories.SelectedItem = null;
                        vmStories.Refresh();
                        CurrentView = vmStories;

                        CloudCoin cloudCoin = MainWindow.FS.loadOneCloudCoinFromJPEGFile(exportPath);
                        MainWindow.updateActivityLog("Celebrium "+ cloudCoin.sn +" Exported to "+ exportPath);

                    }
                    catch(Exception e)
                    {
                        MainWindow.logger.Error(e.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Select a Memo to Export", "Memo Export", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
            //System.Windows.Forms.MessageBox.Show("Write the export logic here");
        }

        private void JpegWrite(String Path, String Tag, String BankFileName, 
            String FrackedFileName, StoryModel storyModel)
        {
            
            IFileSystem fileSystem = MainWindow.FS;
            
            if (File.Exists(Path))//If the file is a bank file, export a good bank coin
            {
                CloudCoin jpgCoin = fileSystem.LoadCoin(Path);
                if (fileSystem.writeJpeg(jpgCoin, Tag,MainWindow.FS.BankFolder + System.IO.Path.GetFileNameWithoutExtension(storyModel.ImagePath)+ ".jpg"))//If the jpeg writes successfully 
                {
                    File.Delete(Path);//Delete the files if they have been written to
                    File.Delete(MainWindow.FS.TemplateFolder + System.IO.Path.GetFileNameWithoutExtension(storyModel.ImagePath) + ".jpg");
                }//end if write was good. 

            }
            else//Export a fracked coin. 
            {
                CloudCoin jpgCoin = fileSystem.LoadCoin(FrackedFileName);
                if (fileSystem.writeJpeg(jpgCoin, Tag))
                {
                    File.Delete(FrackedFileName);//Delete the files if they have been written to
                }//end if
            }//end else

        }//End write one jpeg 

        public void Export(CloudCoin coin)
        {
            try
            {
                var fileName = MainWindow.FS.TemplateFolder + coin.FileName + ".jpg";
                MainWindow.FS.writeJpeg(coin,"","");
                Process.Start(MainWindow.FS.ExportFolder);
            }
            catch (Exception e)
            {
                MainWindow.updateLog(e.Message);
                MessageBox.Show("An error occured while exporting coins. Please check your log files for more information.");
            }
            //MessageBox.Show("Export completed.", "Cloudcoins", MessageBoxButtons.OK);
        }// end export One

        public ICommand ShowExportCollectable
        {
            get { return new ActionCommand(mShowExportCollectable); }
        }

        private void mNews(object obj)
        {
            //TODO write code here
            System.Diagnostics.Process.Start("https://www.celebrium.com/news");
        }

        public ICommand News
        {
            get { return new ActionCommand(mNews); }
        }

        private void mShop(object obj)
        {
            //TODO write code here
            System.Diagnostics.Process.Start("https://www.celebrium.com/");
        }

        public ICommand Shop
        {
            get { return new ActionCommand(mShop); }
        }

        private void mCustomerSupport(object obj)
        {
            //TODO write code here
            System.Diagnostics.Process.Start("https://www.celebrium.com/");
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
