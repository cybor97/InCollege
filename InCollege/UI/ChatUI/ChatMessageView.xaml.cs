using InCollege.Core.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Drawing;
using System.Windows.Media;

namespace InCollege.Client.UI.ChatUI
{
    public partial class ChatMessageView : Grid
    {
        public static DependencyProperty SenderProperty = DependencyProperty.Register("Sender", typeof(Account), typeof(ChatMessageView));
        public static DependencyProperty MessageTextProperty = DependencyProperty.Register("MessageText", typeof(string), typeof(ChatMessageView));

        public static readonly RoutedEvent SenderClickEvent = EventManager.RegisterRoutedEvent("OnSave", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ChatMessageView));
        public event RoutedEventHandler SenderClick
        {
            add { AddHandler(SenderClickEvent, value); }
            remove { RemoveHandler(SenderClickEvent, value); }
        }

        public string MessageText
        {
            get
            {
                return (string)GetValue(MessageTextProperty);
            }
            set
            {
                SetValue(MessageTextProperty, value);
            }
        }

        public Account Sender
        {
            get
            {
                return (Account)GetValue(SenderProperty);
            }
            set
            {
                SetValue(SenderProperty, value);
            }
        }

        public string SenderName => Sender?.FullName;

        public ChatMessageView()
        {
            InitializeComponent();
        }

        void ChatMessageView_Loaded(object sender, RoutedEventArgs e)
        {
            //    DataContext = this;
        }

        private void SenderButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SenderClickEvent));
        }
    }

    class MessageSenderToAlignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Account)value).ID == App.Account.ID ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class MessageSenderToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Account)value).ID == App.Account.ID ? new SolidColorBrush(Colors.LightGreen) :new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
