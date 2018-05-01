using Celebrium_WPF.Models;
using Celebrium_WPF.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Celebrium_WPF.ViewModels
{
    public class StoryViewModel: BaseNavigationViewModel
    {
        public StoryViewModel()
        {
            Title1 = "MEMO";
            Title2 = "STORY";
        }


        private StoryModel _story;
        public StoryModel Story
        {
            get
            {
                return _story;
            }
            set
            {
                _story=value;
                OnPropertyChanged(nameof(Story));
            }
        }

        public string Title
        {
            get
            {
                return ("TITLE: "+Story.Title).ToUpper();
            }
        }

        public string Series
        {
            get
            {
                return ("SERIES: " + Story.Series).ToUpper();
            }
        }

        public string Celeb
        {
            get
            {
                return ("CELEB: " + Story.Celeb).ToUpper();
            }
        }

        public string Value
        {
            get
            {
                return ("VALUE: " + Story.Value).ToUpper();
            }
        }

        public string Memos
        {
            get
            {
                return ("# MEMOS: " + Story.Memos).ToUpper();
            }
        }

        public string Content
        {
            get
            {
                return Story.Content;
            }
        }

    }
}
