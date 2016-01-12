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
    /// 

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

            myMessage.Add(new Message("Server", "Hello! Welcome to Gomoku! Please wait another player."));

            ChatBox.ItemsSource = myMessage;

            #region Khai báo Delegate

            BoardChess.BoardChessOffline.PlayerWin += PlayerWinEvent;

            BoardChess.BoardChessOffline.PlayerDicken += PlayerDickenEvent;

            BoardChess.BoardChessOffline.PaintCellEvent += PaintCellBoardChessOffline;

            BoardChess.BoardChessOffline.HighlightCellWinnerEvent += HighlightCellWinnerEvent;

            BoardChess.Socket.PaintCellEvent += PaintCellBoardChessOnline;

            BoardChess.Socket.ShowMessageEvent += ShowMessageEvent;

            BoardChess.Socket.EndGameEvent += EndGameOnlineEvent;

            BoardChess.Socket.HighlightCellWinnerEvent += HighlightCellWinnerEvent;

            #endregion
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

        public void PaintCellBoardChessOffline(int row, int col)
        {
            CellValues CurrentPlayer = CellValues.None;

            CurrentPlayer = BoardChess.BoardChessOffline.GetCurrentPlayer();

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

        public void HighlightCellWinnerEvent(List<Node> HighlightCellList)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                for (int i = 0; i < HighlightCellList.Count(); i++)
                {
                    LButton[HighlightCellList[i].Row, HighlightCellList[i].Column].Background = Brushes.Orange;
                }
            }));
        }

        public void PaintCellBoardChessOnline(int row, int col, int Id)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                //Console.WriteLine(BoardChess.Socket.player.Name + " " + BoardChess.Socket.player.ID + " -> ID = " + Id);

                if (Id == BoardChess.Socket.player.ID)
                {
                    LButton[row, col].Content = "O";
                    LButton[row, col].Foreground = Brushes.Green;                    
                }
                else
                {
                    LButton[row, col].Content = "X";
                    LButton[row, col].Foreground = Brushes.Red;
                }
            }));
        }

        public void ShowMessageEvent(Message message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                myMessage.Add(message);
            }));
        }

        public void EndGameOnlineEvent(string message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                MessageBox.Show(message);
            }));
        }

        public void PlayerWinEvent(CellValues Player)
        {
            MessageBox.Show(Player + " win!!", "End Game");
            EnableUnenableRDB(true);
        }

        public void PlayerDickenEvent()
        {
            MessageBox.Show("Dicken!...", "End Game");
            EnableUnenableRDB(true);
        }

        public void EnableUnenableRDB(bool status)
        {
            rdbPlayerVsMachine.IsEnabled = status;
            rdbPlayerVsPlayer.IsEnabled = status;
            rdbPlayOnline.IsEnabled = status;
            rdbAutoPlayOnline.IsChecked = false;
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            EnableUnenableRDB(false);

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
            {
                BoardChess.Socket.SendMessage(tbMessage.Text);
            }
        }

        private void rdbPlayerVsPlayer_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 1;
            BoardChess.Socket.AutoMode = false;

            rdbPlayerVsMachine.IsChecked = false;
            rdbPlayOnline.IsChecked = false;
            rdbAutoPlayOnline.IsChecked = false;
            btnStartGame.Visibility = Visibility.Hidden;
        }

        private void rdbPlayerVsMachine_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 2;
            BoardChess.Socket.AutoMode = false;

            rdbPlayerVsPlayer.IsChecked = false;
            rdbPlayOnline.IsChecked = false;
            rdbAutoPlayOnline.IsChecked = false;
            btnStartGame.Visibility = Visibility.Hidden;
        }

        private void rdbPlayOnline_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 3;
            BoardChess.Socket.AutoMode = false;

            rdbPlayerVsPlayer.IsChecked = false;
            rdbPlayerVsMachine.IsChecked = false;
            rdbAutoPlayOnline.IsChecked = false;
            btnStartGame.Visibility = Visibility.Hidden;
        }

        private void rdbAutoPlayOnline_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 4;
            BoardChess.Socket.AutoMode = true;

            rdbPlayerVsPlayer.IsChecked = false;
            rdbPlayerVsMachine.IsChecked = false;
            rdbPlayOnline.IsChecked = false;
            btnStartGame.Visibility = Visibility.Visible;
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            EnableUnenableRDB(true);

            BoardChess.ResetBoardChess();

            for (int i = 0; i < MAX_SQUARE; i++)
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    LButton[i, j].Content = "";

                    if ((i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0))
                        LButton[i, j].Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0E0E0"));
                    else
                        LButton[i, j].Background = Brushes.White;
                }

            BoardChess.Socket.NewGame();
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            BoardChess.Socket.ChangePlayerName(tbPlayerName.Text);
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            BoardChess.Socket.StartGame();
        }
        
    }
}
