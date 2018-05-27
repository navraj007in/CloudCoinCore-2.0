﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using CloudCoinClient.CoreClasses;
using Newtonsoft.Json;

namespace Celebrium_WPF.Models
{
    public class StoryModel:BaseModel
    {
        string RootFolder;
        CloudCoinCore.IFileSystem FS;
        public StoryModel(string imageUrl)
        {
            RootFolder = getWorkspace();
            Celebrium = new Other.Celebrium();
            FS = new FileSystem(RootFolder);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(imageUrl);
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(imageUrl, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            ImagePath = imageUrl;

            Image = src;
            string name = FS.BankFolder + fileName + ".celebrium";
            string frackName = FS.FrackedFolder + fileName + ".celebrium";
            string infoname = App.infoFolder + fileName + ".txt";

            if (File.Exists(name))
            {
                cloudCoin = FS.LoadCoin(name);
                CoinPath = name;
            }
            else if (File.Exists(frackName))
            {
                cloudCoin = FS.LoadCoin(frackName);
                CoinPath = frackName;
            }
            else
                cloudCoin = null;
            string text = File.ReadAllText(infoname, Encoding.UTF8);
            Celebrium = JsonConvert.DeserializeObject<Other.Celebrium>(text);

        }

        public string getWorkspace()
        {
            string workspace = "";
            if (Properties.Settings.Default.WorkSpace != null && Properties.Settings.Default.WorkSpace.Length > 0)
                workspace = Properties.Settings.Default.WorkSpace;
            else
                workspace = AppDomain.CurrentDomain.BaseDirectory;
            Properties.Settings.Default.WorkSpace = workspace;
            return workspace;
        }

        private bool _isDefault = false;
        public bool IsDefault
        {
            get
            {
                return _isDefault;
            }
            set
            {
                _isDefault = value;
                OnPropertyChanged(nameof(IsDefault));
            }
        }
        private string _coinPath;

        public string CoinPath
        {
            get
            {
                return _coinPath;
            }
            set
            {
                _coinPath = value;
                OnPropertyChanged(nameof(CoinPath));
            }
        }

        private Celebrium_WPF.Other.Celebrium _celebrium;

        public Celebrium_WPF.Other.Celebrium Celebrium
        {
            get { return _celebrium; }
            set
            {
                _celebrium = value;
                OnPropertyChanged(nameof(Celebrium));
            }
        }

        private CloudCoinCore.CloudCoin _coin;

        public CloudCoinCore.CloudCoin cloudCoin
        {
            get { return _coin; }
            set
            {
                _coin = value;
                OnPropertyChanged(nameof(cloudCoin));
            }
        }
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _series;

        public string Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        private string _celeb;

        public string Celeb
        {
            get { return _celeb; }
            set
            {
                _celeb = value;
                OnPropertyChanged(nameof(Celeb));
            }
        }


        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        private string _memos;

        public string Memos
        {
            get { return _memos; }
            set
            {
                _memos = value;
                OnPropertyChanged(nameof(Memos));
            }
        }

        private string _content;

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        private string _imagePath;

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }
        private BitmapImage _image;

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

    }
}
