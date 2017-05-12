using InCollege.Core.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Media;

namespace InCollege.Client.UI.ChatUI
{
    public partial class ChatMessageView : Grid
    {
        public static DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(Message), typeof(ChatMessageView));

        public static readonly RoutedEvent SenderClickEvent = EventManager.RegisterRoutedEvent("OnSave", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ChatMessageView));
        public event RoutedEventHandler SenderClick
        {
            add => AddHandler(SenderClickEvent, value);
            remove => RemoveHandler(SenderClickEvent, value);
        }

        public Message Message
        {
            get => (Message)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }


        public string SenderName => Message?.Sender?.FullName;
        public string MessageText => Message?.MessageText;
        public Account Sender => Message?.Sender;
        public bool IsRead => Message?.IsRead ?? false;

        public ChatMessageView()
        {
            InitializeComponent();
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

    class MessageUnseenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = (Message)value;
            return !message.IsRead && message.Sender.ID == App.Account.ID ? Visibility.Visible : Visibility.Collapsed;
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
            return ((Account)value).ID == App.Account.ID ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
