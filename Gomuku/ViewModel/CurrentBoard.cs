using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gomoku.Model;
using Gomuku.Model;

namespace Gomoku.ViewModel
{
    public class CurrentBoard
    {

        public Board BoardChessOffline;

        public SocketIOClient Socket;

        public CurrentBoard()
        {
            BoardChessOffline = new Board();
            Socket = new SocketIOClient();
        }

        public void ResetBoardChess()
        {
            BoardChessOffline.ResetBoard();
        }

        public void PlayAt(int row, int col, int mode)
        {
            if (mode == 1)
            {
                BoardChessOffline.PlayerPlayAt(row, col);
            }
            else if (mode == 2)
            {
                BoardChessOffline.PCVsPlayerPlayAt(row, col);
            }
            else if (mode == 3)
            {
                Socket.PlayAt(row, col);
            }
        }

    }
}
