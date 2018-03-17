using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudCoinCE.UserControls
{
	public class ImageButton : Button
	{

		public readonly static DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));
		public readonly static DependencyProperty ImageHoverProperty = DependencyProperty.Register("ImageHover", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));
		public readonly static DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageButton), new PropertyMetadata(Double.NaN));
		public readonly static DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageButton), new PropertyMetadata(Double.NaN));
		public readonly static DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ImageButton), new FrameworkPropertyMetadata(new CornerRadius(0)));
		//public static readonly DependencyProperty /*BorderBrushProperty*/ = DependencyProperty.Register("BorderBrush", typeof(SolidColorBrush), typeof(ImageButton), new FrameworkPropertyMetadata(Brushes.Black, null));
		static ImageButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));

			//ImageHoverProperty = DependencyProperty.Register("ImageHover", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));
			//ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageButton), new PropertyMetadata(Double.NaN));
			//ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageButton), new PropertyMetadata(Double.NaN));
			//CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ImageButton), new FrameworkPropertyMetadata(new CornerRadius(0)));
		}

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public ImageSource ImageHover
		{
			get { return (ImageSource)GetValue(ImageHoverProperty); }
			set { SetValue(ImageHoverProperty, value); }
		}

		public double ImageHeight
		{
			get { return (double)GetValue(ImageHeightProperty); }
			set { SetValue(ImageHeightProperty, value); }
		}
		public double ImageWidth
		{
			get { return (double)GetValue(ImageWidthProperty); }
			set { SetValue(ImageWidthProperty, value); }
		}

		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

	}
}
