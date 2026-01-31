using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Internal;

namespace LabControlsWPF
{
    /// <summary>
    /// Logika interakcji dla klasy IconedButton.xaml
    /// </summary>
    public partial class IconedButton : UserControl
    {
        // Control DependencyPropertys
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(IconedButton), new PropertyMetadata("PlaceHolder"));


        public PackIconKind Kind
        {
            get { return (PackIconKind)GetValue(KindProperty); }
            set { SetValue(KindProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Kind.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register("Kind", typeof(PackIconKind), typeof(IconedButton), new PropertyMetadata(PackIconKind.Abacus));




        public object? CommandParameter
        {
            get { return (object?)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(IconedButton), new PropertyMetadata(null));



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(IconedButton), new PropertyMetadata(null));



        public IconedButton()
        {
            InitializeComponent();
        }
    }
}
