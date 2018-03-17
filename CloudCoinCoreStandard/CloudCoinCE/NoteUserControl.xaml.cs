using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloudCoinCE
{
    /// <summary>
    /// Interaction logic for NoteUserControl.xaml
    /// </summary>
    public partial class NoteUserControl : UserControl
    {
        public NoteUserControl()
        {
            InitializeComponent();
        }
        public ImageSource Note
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(NoteUserControl));

        public ImageSource Symbol
        {
            get { return (ImageSource)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); imgSymbol.Source = value; }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("CurrencySource", typeof(ImageSource), typeof(NoteUserControl));

        public string NoteCount
        {
            get { return (string)GetValue(NoteCountProperty); }
            set { SetValue(NoteCountProperty, value);
                lblNoteCount.Content = value; }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteCountProperty =
            DependencyProperty.Register("NoteCount", typeof(string), typeof(NoteUserControl));

    }
}
