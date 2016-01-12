using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Model
{
    public class Message
    {
        public string UserName { get; set; }
        public string Time { get; set; }
        public string MessageText { get; set; }              

        public Message()
        {

        }
        
        public Message(string _UserName, string _MessageText)
        {
            UserName = _UserName;
            Time = DateTime.Now.ToLongTimeString();
            MessageText = _MessageText;
        }
    }
}
