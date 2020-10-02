using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface
{
    public class Robot
    {
        public string receivedTextOnSerialPort;
        public Queue<byte> byteListReceived = new Queue<byte>();
        public Robot()
        {

        }
    }

}
