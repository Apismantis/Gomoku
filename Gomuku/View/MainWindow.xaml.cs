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
using Gomoku.Model;
using Gomoku.ViewModel;

namespace Gomoku.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int MAX_SQUARE;
        Button[,] LButton;
        ObservableCollection<Message> myMessage;
        CurrentBoard BoardChess;
        int Mode = 1;

        public MainWindow()
        {
            InitializeComponent();

            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            BoardChess = new CurrentBoard();

            LButton = new Button[MAX_SQUARE, MAX_SQUARE];

            myMessage = new ObservableCollection<Message>();

            DrawBoardChess();

            rdbPlayerVsPlayer.IsChecked = true;

            BoardChess.EndGame += EndGameEvent;

            BoardChess.DickenGame += DickenGameEvent;

            BoardChess.GetBoardChess().PaintCellEvent += PaintCellBoardChess;

            BoardChess.GetPCBoardChess().PaintCellEvent += PaintCellBoardChess;

            myMessage.Add(new Message()
            {
                UserName = "Server",
                Time = DateTime.Now.ToLongTimeString(),
                MessageText = "Hello! Welcome to Gomoku! Please wait another player."
            });
            ChatBox.ItemsSource = myMessage;
        }

        public void DrawBoardChess()
        {
            for (int i = 0; i < MAX_SQUARE; i++)
            {
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    string btnName = "btnSquare" + i.ToString() + j.ToString() + (i * j + j).ToString();

                    Button btn = new Button()
                    {
                        Name = btnName,
                        Height = 33,
                        Width = 33,
                        Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FAFAFA")),
                        BorderThickness = new Thickness(0),
                        FontSize = 18,
                        FontWeight = FontWeights.Medium
                    };

                    if ((i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0))
                    {
                        btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0E0E0"));
                    }

                    btn.Click += btnCell_Click;
                    wpCaroGrid.Children.Add(btn);

                    LButton[i, j] = btn;
                }
            }
        }

        public void PaintCellBoardChess(int row, int col)
        {
            CellValues CurrentPlayer = CellValues.None;

            if (rdbPlayerVsPlayer.IsChecked == true)
                CurrentPlayer = BoardChess.GetBoardChess().GetCurrentPlayer();

            else if (rdbPlayerVsMachine.IsChecked == true)
                CurrentPlayer = BoardChess.GetPCBoardChess().GetCurrentPlayer();

            if (CurrentPlayer == CellValues.Player1)
            {
                LButton[row, col].Content = "O";
                LButton[row, col].Foreground = Brushes.Green;
            }

            else if (CurrentPlayer == CellValues.Player2 || CurrentPlayer == CellValues.Machine)
            {
                LButton[row, col].Content = "X";
                LButton[row, col].Foreground = Brushes.Red;
            }
        }

        public void EndGameEvent()
        {
            CellValues CurrentPlayer = CellValues.None;

            if (rdbPlayerVsPlayer.IsChecked == true)
                CurrentPlayer = BoardChess.GetBoardChess().GetCurrentPlayer();

            else if (rdbPlayerVsMachine.IsChecked == true)
                CurrentPlayer = BoardChess.GetPCBoardChess().GetCurrentPlayer();

            MessageBox.Show(CurrentPlayer + " win!!", "End Game");
        }

        public void DickenGameEvent()
        {
            MessageBox.Show("Dicken!...", "End Game");
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            string btnName = ((Button)sender).Name;

            for (int i = 0; i < MAX_SQUARE; i++)
            {
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    if (LButton[i, j].Name.Equals(btnName))
                    {
                        BoardChess.PlayAt(i, j, Mode);
                        return;
                    }
                }
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text.CompareTo("") != 0)
                myMessage.Add(new Message() { UserName = tbPlayerName.Text, Time = DateTime.Now.ToLongTimeString(), MessageText = tbMessage.Text });
        }

        private void rdbPlayerVsPlayer_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 1;
            rdbPlayerVsMachine.IsChecked = false;
        }

        private void rdbPlayerVsMachine_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 2;
            rdbPlayerVsPlayer.IsChecked = false;
        }
    }
}
