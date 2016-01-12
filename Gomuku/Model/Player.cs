using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomuku.Model
{
    public class Player
    {
        private int _Id;

        public int ID
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public Player(string Name)
        {
            _Id = 1;
            _Name = Name;
        }
    }
}
