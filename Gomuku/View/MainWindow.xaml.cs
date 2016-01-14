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
        Node CurrentPosition;

        public MainWindow()
        {
            InitializeComponent();

            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            BoardChess = new CurrentBoard();

            LButton = new Button[MAX_SQUARE, MAX_SQUARE];

            CurrentPosition = new Node() { Row = -1, Column = -1 };

            myMessage = new ObservableCollection<Message>();

            DrawBoardChess();

            rdbPlayerVsPlayer.IsChecked = true;

            myMessage.Add(new Message("Server", "Hello! Welcome to Gomoku!"));

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

        #region Bàn cờ

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
                        Height = 37,
                        Width = 37,
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
                    btn.MouseEnter += btnCell_MouseEnter;
                    wpCaroGrid.Children.Add(btn);

                    LButton[i, j] = btn;
                }
            }
        }

        public void PaintCellBoardChessOffline(int row, int col)
        {
            CellValues CurrentPlayer = CellValues.None;

            // Loại bỏ đánh dấu nổi bật ô đã đánh trước đó
            if (CurrentPosition.Row != -1 && CurrentPosition.Column != -1)
            {
                LButton[CurrentPosition.Row, CurrentPosition.Column].BorderThickness = new Thickness(0);
            }

            CurrentPlayer = BoardChess.BoardChessOffline.GetCurrentPlayer();

            if (CurrentPlayer == CellValues.Player1)
            {
                LButton[row, col].Content = "O";
                LButton[row, col].Foreground = Brushes.Green;
                lbCurrentPlayer.Content = "X";
            }

            else if (CurrentPlayer == CellValues.Player2 || CurrentPlayer == CellValues.Machine)
            {
                LButton[row, col].Content = "X";
                LButton[row, col].Foreground = Brushes.Red;
                lbCurrentPlayer.Content = "O";
            }

            CurrentPosition.Row = row;
            CurrentPosition.Column = col;

            // Đánh dấu nổi bật ô vừa đánh
            LButton[CurrentPosition.Row, CurrentPosition.Column].BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#039BE5"));
            LButton[CurrentPosition.Row, CurrentPosition.Column].BorderThickness = new Thickness(1);
        }

        public void HighlightCellWinnerEvent(List<Node> HighlightCellList)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                for (int i = 0; i < HighlightCellList.Count(); i++)
                {
                    LButton[HighlightCellList[i].Row, HighlightCellList[i].Column].Background = Brushes.Orange;
                }

                // Bỏ đánh dấu ô vừa đánh
                if (CurrentPosition.Row != -1 && CurrentPosition.Column != -1)
                {
                    LButton[CurrentPosition.Row, CurrentPosition.Column].BorderThickness = new Thickness(0);
                }
            }));
        }

        public void PaintCellBoardChessOnline(int row, int col, int Id)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                // Bỏ đánh dấu nổi bật ô đánh trước đó
                if (CurrentPosition.Row != -1 && CurrentPosition.Column != -1)
                {
                    LButton[CurrentPosition.Row, CurrentPosition.Column].BorderThickness = new Thickness(0);
                }

                if (Id == BoardChess.Socket.player.ID)
                {
                    LButton[row, col].Content = "O";
                    LButton[row, col].Foreground = Brushes.Green;
                    lbCurrentPlayer.Content = "X";
                }
                else
                {
                    LButton[row, col].Content = "X";
                    LButton[row, col].Foreground = Brushes.Red;
                    lbCurrentPlayer.Content = "O";
                }

                CurrentPosition.Row = row;
                CurrentPosition.Column = col;

                // Đánh dấu nổi bật ô mới đánh
                LButton[CurrentPosition.Row, CurrentPosition.Column].BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#039BE5"));
                LButton[CurrentPosition.Row, CurrentPosition.Column].BorderThickness = new Thickness(1);
            }));
        }

        private void btnCell_MouseEnter(object sender, MouseEventArgs e)
        {
            string btnName = ((Button)sender).Name;

            for (int i = 0; i < MAX_SQUARE; i++)
            {
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    if (LButton[i, j].Name.Equals(btnName))
                    {
                        lbCurrentCell.Content = "(" + (i + 1) + ", " + (j + 1) + ")";
                        return;
                    }
                }
            }
        }

        #endregion

        #region Chatbox

        public void ShowMessageEvent(Message message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                myMessage.Add(message);
                ChatBoxScrollView.UpdateLayout();
                ChatBoxScrollView.ScrollToBottom();
            }));
        }

        private void tbMessage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            tbMessage.Text = "";
        }

        private void tbMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbMessage.Text.CompareTo("") != 0 && tbMessage.Text.CompareTo("Type your message here...") != 0)
                {
                    if (!BoardChess.Socket.IsConnectingServer())
                    {
                        myMessage.Add(new Message(tbPlayerName.Text, tbMessage.Text));
                        ChatBoxScrollView.UpdateLayout();
                        ChatBoxScrollView.ScrollToBottom();
                    }
                    else
                    {
                        BoardChess.Socket.SendMessage(tbMessage.Text);
                    }

                    tbMessage.Text = "Type your message here...";
                }
            }
        }
        
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text.CompareTo("") != 0 && tbMessage.Text.CompareTo("Type your message here...") != 0)
            {
                if (!BoardChess.Socket.IsConnectingServer())
                {
                    myMessage.Add(new Message(tbPlayerName.Text, tbMessage.Text));
                    ChatBoxScrollView.UpdateLayout();
                    ChatBoxScrollView.ScrollToBottom();
                }
                else
                {
                    BoardChess.Socket.SendMessage(tbMessage.Text);
                }

                tbMessage.Text = "Type your message here...";
            }
        }

        #endregion

        #region Sự kiện game

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

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            if (rdbAutoPlayOnline.IsChecked == false)
            {
                EnableUnenableRDB(false);
            }

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

            // Ket noi lai toi server
            if (rdbPlayOnline.IsChecked == true || rdbAutoPlayOnline.IsChecked == true)
            {
                BoardChess.Socket.CloseConnection();
                BoardChess.RecreateSocketClient();

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

                // Khởi tạo lại chuỗi tin nhắn
                myMessage.Clear();
                myMessage.Add(new Message("Server", "Hello! Welcome to Gomoku!"));

                // Bỏ đánh dấu ô vừa đánh
                if (CurrentPosition.Row != -1 && CurrentPosition.Column != -1)
                {
                    LButton[CurrentPosition.Row, CurrentPosition.Column].BorderThickness = new Thickness(0);
                }

                CurrentPosition.Row = -1;
                CurrentPosition.Column = -1;

                // Bật chế độ tự chơi online
                if (rdbAutoPlayOnline.IsChecked == true)
                {
                    BoardChess.Socket.AutoMode = true;
                }
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            BoardChess.Socket.ChangePlayerName(tbPlayerName.Text);
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            if (rdbAutoPlayOnline.IsEnabled == true)
            {
                EnableUnenableRDB(false);

                bool IsStart = BoardChess.Socket.StartGame();

                if (!IsStart)
                {
                    MessageBox.Show("Đối thủ đánh trước", "Start Game");
                }
            }
            else
            {
                MessageBox.Show("Ván chơi chưa kết thúc. Bấm New Game để chơi ván mới.", "Start Game");
            }
        }

        #endregion

        #region Xử lý hiển thị của RadioButton - Các chế độ chơi

        public void EnableUnenableRDB(bool status)
        {
            rdbPlayerVsMachine.IsEnabled = status;
            rdbPlayerVsPlayer.IsEnabled = status;
            rdbPlayOnline.IsEnabled = status;
            rdbAutoPlayOnline.IsEnabled = status;
        }

        private void rdbPlayerVsPlayer_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 1;
            BoardChess.Socket.AutoMode = false;

            rdbPlayerVsMachine.IsChecked = false;
            rdbPlayOnline.IsChecked = false;
            rdbAutoPlayOnline.IsChecked = false;
            btnStartGame.IsHitTestVisible = false;
            btnStartGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9E9E9E"));
        }

        private void rdbPlayerVsMachine_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 2;
            BoardChess.Socket.AutoMode = false;

            rdbPlayerVsPlayer.IsChecked = false;
            rdbPlayOnline.IsChecked = false;
            rdbAutoPlayOnline.IsChecked = false;
            btnStartGame.IsHitTestVisible = false;
            btnStartGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9E9E9E"));
        }

        private void rdbPlayOnline_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 3;
            BoardChess.Socket.AutoMode = false;

            rdbPlayerVsPlayer.IsChecked = false;
            rdbPlayerVsMachine.IsChecked = false;
            rdbAutoPlayOnline.IsChecked = false;
            btnStartGame.IsHitTestVisible = false;
            btnStartGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9E9E9E"));
        }

        private void rdbAutoPlayOnline_Checked(object sender, RoutedEventArgs e)
        {
            Mode = 4;
            BoardChess.Socket.AutoMode = true;

            rdbPlayerVsPlayer.IsChecked = false;
            rdbPlayerVsMachine.IsChecked = false;
            rdbPlayOnline.IsChecked = false;
            btnStartGame.IsHitTestVisible = true;
            btnStartGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#29BF12"));
        }

        #endregion

        #region Hiệu ứng màu button

        private void btnStartGame_MouseEnter(object sender, MouseEventArgs e)
        {
            btnStartGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#60CA50"));
        }

        private void btnStartGame_MouseLeave(object sender, MouseEventArgs e)
        {
            btnStartGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#29BF12"));
        }

        private void btnNewGame_MouseEnter(object sender, MouseEventArgs e)
        {
            btnNewGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#03A9F4"));
        }

        private void btnNewGame_MouseLeave(object sender, MouseEventArgs e)
        {
            btnNewGame.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#039BE5"));
        }

        private void btnChange_MouseEnter(object sender, MouseEventArgs e)
        {
            btnChange.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0E0E0"));
        }

        private void btnChange_MouseLeave(object sender, MouseEventArgs e)
        {
            btnChange.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E5E5E5"));
        }

        private void btnSend_MouseEnter(object sender, MouseEventArgs e)
        {
            btnSend.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFEB3B"));
        }

        private void btnSend_MouseLeave(object sender, MouseEventArgs e)
        {
            btnSend.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FDD835"));
        }

        #endregion

    }
}
