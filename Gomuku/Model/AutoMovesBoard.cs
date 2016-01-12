using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gomoku.Model;

namespace Gomuku.Model
{
    public class AutoMovesBoard
    {
        #region Biến

        public CellValues CurrentPlayer;
        EValueBoard EBoard = new EValueBoard();
        int MAX_SQUARE;

        #endregion

        #region Hàm chung

        public AutoMovesBoard()
        {
            CurrentPlayer = CellValues.Player1;

            EBoard = new EValueBoard();

            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;
        }

        public bool CheckWin(int row, int col, int increRow, int increCol, CellValues [,] BoardCell)
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
        
        public bool IsInBoard(int row, int col)
        {
            return row >= 1 && row <= MAX_SQUARE && col >= 1 && col <= MAX_SQUARE;
        }

        #endregion

        #region Sinh nước đi

        public Node GetMoves(CellValues [,] BoardCell)
        {
            Node node = new Node();
            Random rand = new Random();
            int count = rand.Next(4); // Lấy 1 số từ 0 -> 4

            EBoard.ResetBoard();
            
            GetGenResult(BoardCell); // Tìm nước đi

            if (CanWin)
            {
                node = PlayerMoves[1];
            }

            else
            {
                EBoard.ResetBoard();
                EvalueGomokuBoard(CellValues.Machine, BoardCell);
                node = EBoard.GetMaxNode();
                if (!CanLose)
                    for (int i = 0; i < count; i++)
                    {
                        EBoard.Board[node.Row, node.Column] = 0;
                        node = EBoard.GetMaxNode();
                    }
            }

            return node;
        }

        // Tạo bảng lượng giá cho bàn cờ
        private void EvalueGomokuBoard(CellValues player, CellValues[,] BoardCell)
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

        #region Biến sử dụng cho sinh nước đi

        public int Depth = 0;
        static public int MaxDepth = 12;
        static public int MaxStep = 10;
        public int[] TScore = new int[5] { 0, 1, 9, 85, 769 };
        public int[] KScore = new int[5] { 0, 2, 28, 256, 2308 };

        public Node[] PlayerMoves = new Node[MaxDepth + 1];
        public Node[] PCMoves = new Node[MaxStep + 1];
        public Node[] CompetitorMoves = new Node[MaxStep + 1];
        public bool CanWin, CanLose;

        #endregion

        // Đệ quy sinh nước đi
        public void GenerateMoves(CellValues[,] BoardCell)
        {
            if (Depth >= MaxDepth)
                return;

            Depth++;
            CanWin = false;

            Node PCNode = new Node();   // Duong di quan ta.
            Node CompetitorNode = new Node();  // Duong di doi thu.
            int count = 0;

            // Tạo bảng lượng giá cho Machine
            EvalueGomokuBoard(CellValues.Machine, BoardCell);

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
                EvalueGomokuBoard(CellValues.Player1, BoardCell);
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
                    if ((CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 0, BoardCell) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, 0, 1, BoardCell) ||
                        CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 1, BoardCell) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, -1, 1, BoardCell))
                        && CurrentPlayer == CellValues.Machine) // Có thể thắng
                        CanWin = true;

                    if ((CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 0, BoardCell) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, 0, 1, BoardCell) ||
                        CheckWin(CompetitorNode.Row, CompetitorNode.Column, 1, 1, BoardCell) || CheckWin(CompetitorNode.Row, CompetitorNode.Column, -1, 1, BoardCell))
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

                    else GenerateMoves(BoardCell);
                    BoardCell[CompetitorNode.Row, CompetitorNode.Column] = CellValues.None;
                }

                BoardCell[PCNode.Row, PCNode.Column] = CellValues.None;
            }

            #endregion
        }

        // Tìm đường đi
        public void GetGenResult(CellValues [,] BoardCell)
        {
            CanWin = false;
            CanLose = false;

            PlayerMoves = new Node[MaxDepth + 1];

            for (int i = 0; i <= MaxDepth; i++)
                PlayerMoves[i] = new Node();

            for (int i = 0; i < MaxStep; i++)
                PCMoves[i] = new Node();

            Depth = 0;
            GenerateMoves(BoardCell);
        }

        #endregion

    }
}
