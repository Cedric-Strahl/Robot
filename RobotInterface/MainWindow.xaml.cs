using System;
using System.IO.Ports;
using System.Collections.Generic;
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
using ExtendedSerialPort;
using System.Windows.Threading;

namespace RobotInterface
{
  
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        bool modeCommande = false;
        bool ColorButtonEnvoyer = true;
        private ReliableSerialPort serialPort1;
        private DispatcherTimer GuiUpdate = new DispatcherTimer();
        Robot robot = new Robot();
        string selectedPortCOM;

        public MainWindow()
        {
            serialPort1 = new ReliableSerialPort("COM5", 115200, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();

            GuiUpdate.Interval = new TimeSpan(0, 0, 0, 0, 100);
            GuiUpdate.Tick += GuiUpdate_Tick;
            GuiUpdate.Start();
        }

        private void GuiUpdate_Tick(object sender, EventArgs e)
        {/*
            if (robot.receivedTextOnSerialPort != "")
            {
                TextBoxRéception.Text += robot.receivedTextOnSerialPort;
                robot.receivedTextOnSerialPort = "";
            }*/

            while(robot.byteListReceived.Count>0)
            {
                byte b = robot.byteListReceived.Dequeue();
                TextBoxRéception.Text += "0x" + b.ToString("X2") + " ";
            }

            /*
            foreach(byte b in e.Data)
            {
                e.Data.Dequeue(b);
            }*/
        }

        private void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            //robot.receivedTextOnSerialPort += Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

            foreach(byte b in e.Data)
            {
                robot.byteListReceived.Enqueue(b);
            }
        }

        private void SendMessage()
        {
            serialPort1.WriteLine(TextBoxEmission.Text);
            TextBoxEmission.Text = null;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxEmission.Text = "";
            TextBoxRéception.Text = "";
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteListe = new byte[20] ;
            int i;
            for(i=0;i<20;i++)
            {
                byteListe[i] = (byte)(2 * i);
            }
            serialPort1.Write(byteListe, 0, byteListe.Count());
            TextBoxRéception.Text += "\r";
        }

        private void ChoixCOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPortCOM = ChoixCOM.SelectedItem.ToString();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();

            for(int i=0; i<ports.Length; i++)
            {
                if(!ChoixCOM.Items.Contains(ports[i]))
                {
                    ChoixCOM.Items.Add(ports[i]);
                }

            }
        }
    }
}
