using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtendedSerialPort;
using MessageDecoder;



namespace RobotConsole
{
    class RobotConsole
    {
        static ExtendedSerialPort.ReliableSerialPort port = new ReliableSerialPort("")
        static MessageDecoder.msgDecoder decoder = new msgDecoder();
        static void Main(string[] args)
        { 
        }
    }
}
