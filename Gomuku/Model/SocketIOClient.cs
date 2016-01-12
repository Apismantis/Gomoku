using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using Gomoku.Model;

namespace Gomuku.Model
{
    public class SocketIOClient
    {
        #region Biến

        public Player player;
        private Socket socket;
        List<Node> HighlitghtCell;
        private CellValues[,] BoardCell;
        int MAX_SQUARE;
        public bool AutoMode;
        AutoMovesBoard AMB;
        bool IsStart;

        #endregion

        #region Delegate

        public delegate void ShowMessageHandle(Message message);
        public event ShowMessageHandle ShowMessageEvent;
        public delegate void PaintCellHandle(int row, int col, int Id);
        public event PaintCellHandle PaintCellEvent;
        public delegate void EndGameHandle(string message);
        public event EndGameHandle EndGameEvent;
        public delegate void HighlightCellWinnerHandle(List<Node> HighlightCellList);
        public event HighlightCellWinnerHandle HighlightCellWinnerEvent;

        #endregion

        public SocketIOClient()
        {
            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            BoardCell = new CellValues[MAX_SQUARE + 2, MAX_SQUARE + 2];

            ResetBoardCell();

            AutoMode = false;

            IsStart = false;

            AMB = new AutoMovesBoard();

            player = new Player("Guest");

            socket = IO.Socket("ws://127.0.0.1:8000");

            HighlitghtCell = new List<Node>();

            #region EVENT

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                ShowMessageEvent(new Message("Server", "Đã kết nối đến Server."));
            });            

            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                ShowMessageEvent(new Message("Server", data.ToString()));
            });
            
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                ShowMessageEvent(new Message("Server", "LỖI: Không thể kết nối đến Server"));
            });

            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                ShowMessageEvent(new Message("Server", "LỖI: " + data.ToString()));
            });

            #endregion

            #region ChatMessage

            socket.On("ChatMessage", (data) =>
            {
                string message = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();
                string from = "";

                // Lấy tên người gửi tin nhắn
                try
                {
                    from = ((Newtonsoft.Json.Linq.JObject)data)["from"].ToString();
                }
                catch (Exception) { }

                // Gửi tên người chơi và yêu cầu kết nối tới người chơi còn lại
                if (message == "Welcome!")
                {
                    socket.Emit("MyNameIs", player.Name);
                    socket.Emit("ConnectToOtherPlayer");
                }

                // Xác định là người chơi đầu tiên hay không
                else if (message.Contains("You are the first player"))
                {
                    player.ID = 0;
                }

                // Nếu không phải tin nhắn do người gửi gửi
                if (from.Equals(""))
                    from = "Server";                  
                    
                ShowMessageEvent(new Message(from, message));
            });

            #endregion

            #region EndGame

            socket.On("EndGame", (data) =>
                {
                    string message = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();

                    // Lấy các ô highlight

                    HighlitghtCell.Clear();
                    JObject newReponses = (Newtonsoft.Json.Linq.JObject)data;
                    List<JToken> result = newReponses["highlight"].Children().ToList();

                    foreach (JToken re in result)
                    {
                        int row = (int)re["row"];
                        int col = (int)re["col"];
                        HighlitghtCell.Add(new Node() { Row = row, Column = col });
                    }

                    if (message.Contains("won the game"))
                    {
                        HighlightCellWinnerEvent(HighlitghtCell);
                        EndGameEvent(message);
                    }
                    else
                        ShowMessageEvent(new Message("Server", message));
                                        
                    //Console.WriteLine(newReponses.ToString());
                });

            #endregion

            #region NextStepIs

            socket.On("NextStepIs", (data) =>
            {
                int Id = -1;
                int row = -1;
                int col = -1;
                
                int.TryParse(((Newtonsoft.Json.Linq.JObject)data)["player"].ToString(), out Id);
                int.TryParse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString(), out row);
                int.TryParse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString(), out col);

                if (IsInBoard(row, col))
                {
                    PaintCellEvent(row, col, Id);
                }

                if (AutoMode)
                {                    
                    if (Id == 0) // Máy đang đánh
                    {
                        BoardCell[row + 1, col + 1] = CellValues.Machine;
                        AMB.CurrentPlayer = CellValues.Machine;
                    }
                    else // Người chơi còn lại đánh
                    {
                        BoardCell[row + 1, col + 1] = CellValues.Player1;
                        AMB.CurrentPlayer = CellValues.Player1;
                        Node moves = AMB.GetMoves(BoardCell);
                        PlayAt(moves.Row - 1, moves.Column - 1);
                    }
                }               

            });

            #endregion
        }

        public void Connect()
        {
            //socket = IO.Socket("ws://127.0.0.1:8000");
            //socket.Connect();
            //socket.Close();
            //socket.Open();
        }

        public void StartGame()
        {
            if (AutoMode && player.ID == 0 && !IsStart)
            {
                PlayAt(6, 6);
                IsStart = true;
            }
        }

        public void NewGame()
        {
            ResetBoardCell();
            Connect();
        }

        public void ResetBoardCell()
        {
            for (int i = 0; i < MAX_SQUARE + 2; i++)
                for (int j = 0; j < MAX_SQUARE + 2; j++)
                {
                    if (i != 0 && j != 0 && i != MAX_SQUARE + 1 && j != MAX_SQUARE + 1)
                        BoardCell[i, j] = CellValues.None;
                }
        }

        public void ChangePlayerName(string NewName)
        {
            player.Name = NewName;
            socket.Emit("MyNameIs", NewName);
        }

        public bool IsInBoard(int row, int col)
        {
            int MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            return row >= 0 && row < MAX_SQUARE && col >= 0 && col < MAX_SQUARE;
        }

        public void SendMessage(string message)
        {
            socket.Emit("ChatMessage", message);
        }

        public void PlayAt(int pRow, int pCol)
        {
            socket.Emit("MyStepIs", JObject.FromObject(new { row = pRow, col = pCol }));
        }

    }
}
