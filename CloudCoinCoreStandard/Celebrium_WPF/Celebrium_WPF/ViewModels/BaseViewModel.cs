﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Celebrium_WPF.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged //, IDataErrorInfo
    {
        public BaseViewModel()
        {
            Title1  = Title2= string.Empty;
        }
        private string _title1;

        public string Title1
        {
            get { return _title1; }
            set
            {
                _title1 = value;
                OnPropertyChanged(nameof(Title1));
            }
        }

        private string _title2;

        public string Title2
        {
            get { return _title2; }
            set
            {
                _title2 = value;
                OnPropertyChanged(nameof(Title2));
            }
        }

        private Visibility _showFirst;
        public Visibility ShowFirst
        {
            get
            {
                return _showFirst;
            }
            set
            {
                _showFirst = value;
                OnPropertyChanged(nameof(ShowFirst));
            }
        }

        private Visibility _showLast;
        public Visibility ShowLast
        {
            get
            {
                return _showLast;
            }
            set
            {
                _showLast = value;
                OnPropertyChanged(nameof(ShowLast));
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        //#region IDataErrorInfo members

        //public string Error
        //{
        //    get;
        //    private set;
        //}

        //public string this[string columnName]
        //{
        //    get
        //    {
        //        if (columnName == "Name")
        //        {
        //            if (string.IsNullOrWhiteSpace(Name))
        //            {
        //                Error = "Name can not be null or empty";
        //            }
        //            else
        //            {
        //                Error = null;
        //            }
        //        }
        //        return Error;
        //    }
        //}

        //#endregion

    }
}