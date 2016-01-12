using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Model
{
    public class PCBoard
    {
        private CellValues CurrentPlayer;
        private CellValues NextPlayer;
        private CellValues[,] BoardCell;
        EValueBoard EBoard = new EValueBoard();
        int MAX_SQUARE;
        public int[] TScore = new int[5] { 0, 1, 9, 85, 769 };
        public int[] KScore = new int[5] { 0, 2, 28, 256, 2308 };

        public delegate void PlayerWinHandle();
        public event PlayerWinHandle PlayerWin;
        public delegate void PlayerDickenHandle();
        public event PlayerDickenHandle PlayerDicken; 
        public delegate void PaintCellHandle(int row, int col);
        public event PaintCellHandle PaintCellEvent;        

        public PCBoard()
        {
            CurrentPlayer = CellValues.Player1;

            NextPlayer = CellValues.Player1;

            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            EBoard = new EValueBoard();

            BoardCell = new CellValues[MAX_SQUARE + 2, MAX_SQUARE + 2];

            for (int i = 0; i < MAX_SQUARE + 2; i++)
                for (int j = 0; j < MAX_SQUARE + 2; j++)
                {
                    if (i == 0 || j == 0 || i == MAX_SQUARE + 1 || j == MAX_SQUARE + 1)
                        BoardCell[i, j] = CellValues.Outside;
                    else
                        BoardCell[i, j] = CellValues.None;
                }
        }

        public CellValues GetCurrentPlayer()
        {
            return CurrentPlayer;
        }

        public void PlayAt(int row, int col)
        {
            Random rand = new Random();
            int count = rand.Next(4); // Lấy 1 số từ 0 -> 4
            Node node = new Node();
            row++;
            col++;

            if (!IsInBoard(row, col))
                return;

            if (BoardCell[row, col] == CellValues.None)
            {
                if (NextPlayer == CellValues.Player1)
                {
                    BoardCell[row, col] = NextPlayer;
                    CurrentPlayer = NextPlayer;
                    NextPlayer = CellValues.Machine;

                    // Phát sự kiện tô màu
                    PaintCellEvent(row - 1, col - 1);

                    // Kiểm tra hòa
                    if(CheckDicken(row, col))
                    {
                        PlayerDicken();
                        return;
                    }

                    // Kiểm tra thắng
                    if (CheckWin(row, col, 1, 0) || CheckWin(row, col, 0, 1) || CheckWin(row, col, 1, 1) || CheckWin(row, col, -1, 1))
                    {
                        // Phat su kien co nguoi choi thang
                        PlayerWin();
                        return;
                    }

                    // Tìm nước chiến thắng
                    EBoard.ResetBoard();

                    GetGenResult(); // Tìm nước đi

                    if (CanWin)
                    {
                        node = PlayerMoves[1];
                    }

                    else
                    {
                        EBoard.ResetBoard();
                        EvalueGomokuBoard(CellValues.Machine);
                        node = EBoard.GetMaxNode();
                        if (!CanLose)
                            for (int i = 0; i < count; i++)
                            {
                                EBoard.Board[node.Row, node.Column] = 0;
                                node = EBoard.GetMaxNode();
                            }
                    }

                    // Máy đánh cờ ở ô tìm được
                    row = node.Row;
                    col = node.Column;
                    BoardCell[row, col] = NextPlayer;
                    Console.WriteLine(row + " " + col);
                    CurrentPlayer = NextPlayer;
                    NextPlayer = CellValues.Player1;

                    PaintCellEvent(row - 1, col - 1);

                    // Kiểm tra hòa
                    if (CheckDicken(row, col))
                    {
                        PlayerDicken();
                        return;
                    }

                    // Kiểm tra thắng
                    if (CheckWin(row, col, 1, 0) || CheckWin(row, col, 0, 1) || CheckWin(row, col, 1, 1) || CheckWin(row, col, -1, 1))
                    {
                        // Phat su kien co nguoi choi thang
                        PlayerWin();
                        return;
                    }

                }
            }
        }

        public bool IsInBoard(int row, int col)
        {
            return row >= 1 && row <= MAX_SQUARE && col >= 1 && col <= MAX_SQUARE;
        }

        public bool CheckWin(int row, int col, int increRow, int increCol)
        {
            int CountCell = 0;
            int cRow = row;
            int cCol = col;

            while (IsInBoard(cRow, cCol))
            {
                if (BoardCell[cRow, cCol] == CurrentPlayer)
                {
                    CountCell++;
                    cRow += increRow;
                    cCol += increCol;
                }
                else
                    break;
            }

            CountCell--;
            cRow = row;
            cCol = col;
            while (IsInBoard(cRow, cCol))
            {
                if (BoardCell[cRow, cCol] == CurrentPlayer)
                {
                    CountCell++;
                    cRow -= increRow;
                    cCol -= increCol;
                }
                else
                    break;
            }

            if (CountCell >= 5)
                return true;
            return false;
        }

        public bool CheckDicken(int row, int col)
        {
            for (int i = 1; i <= MAX_SQUARE; i++)
                for (int j = 1; j <= MAX_SQUARE; j++)
                {
                    if (BoardCell[i, j] == CellValues.None)
                        return false;
                }

            return true;
        }

        // Tạo bảng lượng giá cho bàn cờ
        private void EvalueGomokuBoard(CellValues player)
        {
            int row, col, i;
            int cPlayer, cMachine;

            #region Lượng giá cho hàng

            for (row = 1; row <= MAX_SQUARE; row++)
                for (col = 1; col <= MAX_SQUARE - 4; col++)
                {
                    cPlayer = 0; cMachine = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCell[row, col + i] == CellValues.Player1) cPlayer++;
                        if (BoardCell[row, col + i] == CellValues.Machine) cMachine++;
                    }

                    // Tạo bảng lượng giá
                    if (cPlayer * cMachine == 0 && cPlayer != cMachine)
                        for (i = 0; i < 5; i++)
                            if (BoardCell[row, col + i] == CellValues.None)
                            {
                                if (cMachine == 0)
                                {
                                    if (player == CellValues.Machine)
                                        EBoard.Board[row, col + i] += TScore[cPlayer];

                                    else EBoard.Board[row, col + i] += KScore[cPlayer];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row, col - 1] == CellValues.Machine && BoardCell[row, col + 5] == CellValues.Machine)
                                        EBoard.Board[row, col + i] = 0;
                                }

                                if (cPlayer == 0)
                                {
                                    if (player == CellValues.Player1)
                                        EBoard.Board[row, col + i] += TScore[cMachine];

                                    else EBoard.Board[row, col + i] += KScore[cMachine];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row, col - 1] == CellValues.Player1 && BoardCell[row, col + 5] == CellValues.Player1)
                                        EBoard.Board[row, col + i] = 0;
                                }

                                if ((cPlayer == 4 || cMachine == 4)
                                    && (BoardCell[row, col + i - 1] == CellValues.None || BoardCell[row, col + i + 1] == CellValues.None))
                                    EBoard.Board[row, col + i] *= 2;
                            }
                }

            #endregion

            #region Lượng giá cho cột

            for (col = 1; col <= MAX_SQUARE; col++)
                for (row = 1; row <= MAX_SQUARE - 4; row++)
                {
                    cPlayer = 0; cMachine = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCell[row + i, col] == CellValues.Player1) cPlayer++;
                        if (BoardCell[row + i, col] == CellValues.Machine) cMachine++;
                    }

                    // Tạo bảng lượng giá
                    if (cPlayer * cMachine == 0 && cMachine != cPlayer)
                        for (i = 0; i < 5; i++)
                            if (BoardCell[row + i, col] == CellValues.None)
                            {
                                if (cMachine == 0)
                                {
                                    if (player == CellValues.Machine)
                                        EBoard.Board[row + i, col] += TScore[cPlayer];

                                    else EBoard.Board[row + i, col] += KScore[cPlayer];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row - 1, col] == CellValues.Machine && BoardCell[row + 5, col] == CellValues.Machine)
                                        EBoard.Board[row + i, col] = 0;
                                }

                                if (cPlayer == 0)
                                {
                                    if (player == CellValues.Player1)
                                        EBoard.Board[row + i, col] += TScore[cMachine];

                                    else EBoard.Board[row + i, col] += KScore[cMachine];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row - 1, col] == CellValues.Player1 && BoardCell[row + 5, col] == CellValues.Player1)
                                        EBoard.Board[row + i, col] = 0;

                                }

                                if ((cPlayer == 4 || cMachine == 4)
                                    && (BoardCell[row + i - 1, col] == CellValues.None || BoardCell[row + i + 1, col] == CellValues.None))
                                    EBoard.Board[row + i, col] *= 2;
                            }
                }

            #endregion

            #region Lượng giá đường chéo xuống

            for (row = 1; row <= MAX_SQUARE - 4; row++)
                for (col = 1; col <= MAX_SQUARE - 4; col++)
                {
                    cPlayer = 0; cMachine = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCell[row + i, col + i] == CellValues.Player1) cPlayer++;
                        if (BoardCell[row + i, col + i] == CellValues.Machine) cMachine++;
                    }

                    // Tạo bảng lượng giá
                    if (cPlayer * cMachine == 0 && cMachine != cPlayer)
                        for (i = 0; i < 5; i++)
                            if (BoardCell[row + i, col + i] == CellValues.None)
                            {
                                if (cMachine == 0)
                                {
                                    if (player == CellValues.Machine)
                                        EBoard.Board[row + i, col + i] += TScore[cPlayer];

                                    else EBoard.Board[row + i, col + i] += KScore[cPlayer];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row - 1, col - 1] == CellValues.Machine && BoardCell[row + 5, col + 5] == CellValues.Machine)
                                        EBoard.Board[row + i, col + i] = 0;
                                }

                                if (cPlayer == 0)
                                {
                                    if (player == CellValues.Player1)
                                        EBoard.Board[row + i, col + i] += TScore[cMachine];

                                    else EBoard.Board[row + i, col + i] += KScore[cMachine];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row - 1, col - 1] == CellValues.Player1 && BoardCell[row + 5, col + 5] == CellValues.Player1)
                                        EBoard.Board[row + i, col + i] = 0;
                                }

                                if ((cPlayer == 4 || cMachine == 4)
                                    && (BoardCell[row + i - 1, col + i - 1] == CellValues.None || BoardCell[row + i + 1, col + i + 1] == CellValues.None))
                                    EBoard.Board[row + i, col + i] *= 2;
                            }
                }

            #endregion

            #region Lượng giá đường chéo lên

            for (row = 5; row <= MAX_SQUARE - 4; row++)
                for (col = 1; col <= MAX_SQUARE - 4; col++)
                {
                    cMachine = 0; cPlayer = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCell[row - i, col + i] == CellValues.Player1) cPlayer++;
                        if (BoardCell[row - i, col + i] == CellValues.Machine) cMachine++;
                    }

                    // Tạo bảng lượng giá
                    if (cPlayer * cMachine == 0 && cPlayer != cMachine)
                        for (i = 0; i < 5; i++)
                            if (BoardCell[row - i, col + i] == CellValues.None)
                            {
                                if (cMachine == 0)
                                {
                                    if (player == CellValues.Machine)
                                        EBoard.Board[row - i, col + i] += TScore[cPlayer];

                                    else EBoard.Board[row - i, col + i] += KScore[cPlayer];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row + 1, col - 1] == CellValues.Machine && BoardCell[row - 5, col + 5] == CellValues.Machine)
                                        EBoard.Board[row - i, col + i] = 0;
                                }

                                if (cPlayer == 0)
                                {
                                    if (player == CellValues.Player1)
                                        EBoard.Board[row - i, col + i] += TScore[cMachine];

                                    else EBoard.Board[row - i, col + i] += KScore[cMachine];

                                    // Bị chặn 2 đầu
                                    if (BoardCell[row + 1, col - 1] == CellValues.Player1 && BoardCell[row - 5, col + 5] == CellValues.Player1)
                                        EBoard.Board[row - i, col + i] = 0;
                                }

                                if ((cPlayer == 4 || cMachine == 4)
                                    && (BoardCell[row - i + 1, col + i - 1] == CellValues.None || BoardCell[row - i - 1, col + i + 1] == CellValues.None))
                                    EBoard.Board[row - i, col + i] *= 2;
                            }
                }

            #endregion
        }

        #region 

        public int Depth = 0;
        static public int MaxDepth = 12;
        static public int MaxStep = 10;

        public Node[] PlayerMoves = new Node[MaxDepth + 1];
        public Node[] PCMoves = new Node[MaxStep + 1];
        public Node[] CompetitorMoves = new Node[MaxStep + 1];
        public bool CanWin, CanLose;

        #endregion

        // Đệ quy sinh nước đi
        public void GenerateMoves()
        {
            if (Depth >= MaxDepth)
                return;

            Depth++;
            CanWin = false;

            Node PCNode = new Node();   // Duong di quan ta.
            Node CompetitorNode = new Node();  // Duong di doi thu.
            int count = 0;

            // Tạo bảng lượng giá cho Machine
            EvalueGomokuBoard(CellValues.Machine);

            #region Lấy tất cả các bước đi tốt nhất vào danh sách các bước đi

            for (int i = 1; i <= MaxStep; i++)
            {
                PCNode = EBoard.GetMaxNode();
                PCMoves[i] = PCNode;
                EBoard.Board[PCNode.Row, PCNode.Column] = 0;
            }

            #endregion

            #region Lấy các bước đi tốt nhất để thử

            count = 0;
            while (count < MaxStep)
            {
                count++;
                PCNode = PCMoves[count];
                PlayerMoves.SetValue(PCNode, Depth);
                BoardCell[PCNode.Row, PCNode.Column] = CellValues.Machine;

                // Tìm nước đi tốt nhất
                EBoard.ResetBoard();
                EvalueGomokuBoard(CellValues.Player1);
                for (int i = 1; i <= MaxStep; i++)
                {
                    CompetitorNode = EBoard.GetMaxNode();
                    CompetitorMoves[i] = CompetitorNode;
                    EBoard.Board[CompetitorNode.Row, CompetitorNode.Column] = 0;
                }


                for (int i = 1; i <= MaxStep; i++)
                {
                    CompetitorNode = CompetitorMoves[i];
                    BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.Player1;

                    // Đi thử
                    if ((CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 0) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, 0, 1) ||
                        CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 1) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, -1, 1))
                        && CurrentPlayer == CellValues.Machine) // Có thể thắng
                        CanWin = true;

                    if ((CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 0) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, 0, 1) ||
                        CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 1) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, -1, 1))
                        && CurrentPlayer == CellValues.Player1) // Có thể thua
                        CanLose = true;

                    // Loại bỏ nước đi thử
                    if (CanLose || CanWin)
                    {
                        BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.None;
                        BoardCell[PCNode.Row, PCNode.Column] = CellValues.None;

                        if (CanWin)
                            CanLose = false;

                        return;
                    }

                    else GenerateMoves();
                    BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.None;
                }

                BoardCell[PCNode.Row, PCNode.Column] = CellValues.None;
            }

            #endregion
        }

        // Đệ quy sinh nước đi - V2
        public void GenerateMoves1()
        {
            if (Depth >= MaxDepth)
                return;

            Depth++;
            CanWin = false;

            Node PCNode = new Node();
            Node CompetitorNode = new Node();

            // Tạo bảng lượng giá cho Machine
            EvalueGomokuBoard(CellValues.Machine);

            // Lấy bước đi tốt nhất
            PCNode = EBoard.GetMaxNode();
            PCMoves[1] = PCNode;
            EBoard.Board[PCNode.Row, PCNode.Column] = 0;

            // Đánh thử bước đi tốt nhất vừa tìm được
            PlayerMoves.SetValue(PCNode, Depth);
            BoardCell[PCNode.Row, PCNode.Column] = CellValues.Machine;

            // Tìm các nước tối ưu của đối thủ
            EBoard.ResetBoard();
            EvalueGomokuBoard(CellValues.Player1);

            // Lấy nước đi tốt nhất của đối thủ
            CompetitorNode = EBoard.GetMaxNode();
            CompetitorMoves[1] = CompetitorNode;
            EBoard.Board[CompetitorNode.Row, CompetitorNode.Column] = 0;

            // Đánh thử nước đi tốt nhất của đối thủ
            BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.Player1;

            // Kiểm tra thắng hay thua
            if ((CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 0) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, 0, 1) ||
                CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 1) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, -1, 1))
                && CurrentPlayer == CellValues.Machine) // Nếu máy có thể thắng
                CanWin = true;

            if ((CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 0) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, 0, 1) ||
                CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 1) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, -1, 1))
                && CurrentPlayer == CellValues.Player1) // Người chơi thắng
                CanLose = true;


            if (CanLose || CanWin)
            {
                // Bỏ nước đi vừa chơi thử                
                BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.None;
                BoardCell[PCNode.Row, PCNode.Column] = CellValues.None;
                if (CanWin)
                    CanLose = false;

                return;
            }
            else GenerateMoves1(); // Tìm nước khác

            // Bỏ nước đi vừa chơi thử                
            BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.None;
            BoardCell[PCNode.Row, PCNode.Column] = CellValues.None;

        }

        // Tìm đường đi
        public void GetGenResult()
        {
            CanWin = false;
            CanLose = false;

            PlayerMoves = new Node[MaxDepth + 1];

            for (int i = 0; i <= MaxDepth; i++)
                PlayerMoves[i] = new Node();

            for (int i = 0; i < MaxStep; i++)
                PCMoves[i] = new Node();

            Depth = 0;
            GenerateMoves();
        }

    }
}
