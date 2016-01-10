using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Gomoku___Five_in_arrow___multiplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MAX_SQUARE = 12;

        Button[,] LButton = new Button[MAX_SQUARE, MAX_SQUARE];
        
        ObservableCollection<Message> myMessage = new ObservableCollection<Message>();
        
        public MainWindow()
        {
            InitializeComponent();
            khoiTaoBanCo();
            
            myMessage.Add(new Message() { UserName = "Server", Time = DateTime.Now.ToLongTimeString(), MessageText = "Hello! Welcome to Gomoku! Please wait another player." });
            ChatBox.ItemsSource = myMessage;
        }

        public void khoiTaoBanCo()
        {
            for (int i = 0; i < MAX_SQUARE; i++)
            {
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    string btnName = "btnSquare" + i.ToString() + j.ToString();

                    Button btn = new Button()
                    {
                        Name = btnName,
                        Height = 33,
                        Width = 33,
                        Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FAFAFA")),
                        BorderThickness = new Thickness(0),
                        FontSize = 15,
                        FontWeight = FontWeights.Medium
                    };

                    if ((i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0))
                    {
                        btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0E0E0"));
                    }

                    btn.Click += btnSquare_Click;
                    wpCaroGrid.Children.Add(btn);

                    LButton[i, j] = btn;
                }
            }
        }

        private void btnSquare_Click(object sender, RoutedEventArgs e)
        {
            string btnName = ((Button)sender).Name;

            for (int i = 0; i < MAX_SQUARE; i++)
            {
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    if (LButton[i, j].Name == btnName)
                    {
                        ((Button)sender).Content = "O";
                        MessageBox.Show("Positon: " + i.ToString() + " - " + j.ToString());
                    }
                }
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text.CompareTo("") != 0)
                myMessage.Add(new Message() { UserName = tbPlayerName.Text, Time = DateTime.Now.ToLongTimeString(), MessageText = tbMessage.Text });
        }     
    }
}
