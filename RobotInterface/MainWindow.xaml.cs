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
        //----------------------------------------Variables

        private int modeCommande = 0; //0=stop; 1=manuel; 2=auto
        private ReliableSerialPort serialPort1;
        private DispatcherTimer GuiUpdate = new DispatcherTimer();
        private Robot robot = new Robot();
        private string selectedPortCOM = "NULL";

        //----------------------------------------Port série

        public MainWindow()
        {/*
            serialPort1 = new ReliableSerialPort("COM5", 115200, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();
            */
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

            while (robot.byteListReceived.Count > 0)
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

            foreach (byte b in e.Data)
            {
                robot.byteListReceived.Enqueue(b);
            }
        }

        //----------------------------------------UART

        byte CalculateChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte[] trame = EncodeWithoutChecksum(msgFunction, msgPayloadLength, msgPayload);

            byte checksum = trame[0];
            for (int i = 1; i < trame.Length; i++)
            {
                checksum ^= trame[i];
            }
            System.Diagnostics.Debug.WriteLine("[CHECKSUM] " + trame + " Result : " + checksum);
            return checksum;
        }

        void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte[] trame = EncodeWithoutChecksum(msgFunction, msgPayloadLength, msgPayload);
            byte[] checksum = new byte[] { CalculateChecksum(msgFunction, msgPayloadLength, msgPayload) };
            trame = Combine(trame, checksum);
            if (serialPort1 != null)
                serialPort1.Write(trame, 0, trame.Length);
        }

        byte[] EncodeWithoutChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            // Convert Function to byte
            byte LbyteFunction = (byte)(msgFunction >> 0);
            byte HbyteFunction = (byte)(msgFunction >> 8);

            byte LbytePayloadsLength = (byte)(msgPayloadLength >> 0);
            byte HbytePayloadsLength = (byte)(msgPayloadLength >> 8);

            // Append all bytes
            byte[] trame = new byte[] { 0xFE, HbyteFunction, LbyteFunction, HbytePayloadsLength, LbytePayloadsLength };
            trame = Combine(trame, msgPayload);
            return trame;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        //----------------------------------------Boîte de message

        private void SendMessage()
        {
            if (selectedPortCOM != "NULL")
            {
                serialPort1.WriteLine(TextBoxEmission.Text);
                if (TextBoxRéception.Text != "")
                    TextBoxRéception.Text = TextBoxRéception.Text + "\n";
                TextBoxEmission.Text = null;
            }
        }

        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxEmission.Text != "")
                SendMessage();
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxEmission.Text != "")
            {
                if (e.Key == Key.Enter)
                {
                    SendMessage();
                }
            }
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPortCOM != "NULL")
            {
                byte[] byteListe = new byte[20];
                int i;
                for (i = 0; i < 20; i++)
                {
                    byteListe[i] = (byte)(2 * i);
                }
                serialPort1.Write(byteListe, 0, byteListe.Count());

                if (TextBoxRéception.Text != "")
                    TextBoxRéception.Text += "\n";

            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxEmission.Text = "";
            TextBoxRéception.Text = "";
        }

        //----------------------------------------Boutons Interface

        private void ChoixCOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPortCOM = ChoixCOM.SelectedItem.ToString();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();

            for (int i = 0; i < ports.Length; i++)
            {
                if (!ChoixCOM.Items.Contains(ports[i]))
                {
                    ChoixCOM.Items.Add(ports[i]);
                }
            }
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string select = ChoixCOM.Text;
            if (select != "Null" && ChoixCOM.SelectedItem != null)
            {
                if (serialPort1 == null)
                {
                    serialPort1 = new ReliableSerialPort(select, 115200, Parity.None, 8, StopBits.One);
                    serialPort1.DataReceived += SerialPort1_DataReceived;
                    serialPort1.Open();
                }
            }

            if ((ConnectionButton.Background != Brushes.Green) && (serialPort1 != null))
            {
                ConnectionButton.Background = Brushes.Green;
                ConnectionButton.Content = "Connecte";
            }

        }

        private void ModeAuto_Checked(object sender, RoutedEventArgs e)
        {
            modeCommande = 1;
            TextBoxInParametre.Text = modeCommande.ToString();
        }

        private void ModeManuel_Checked(object sender, RoutedEventArgs e)
        {
            modeCommande = 2;
            TextBoxVitesseMoteurDroit.Text = SlideMoteurDroit.Value.ToString(); ;
            TextBoxVitesseMoteurGauche.Text = SlideMoteurGauche.Value.ToString(); ;
            TextBoxInParametre.Text = modeCommande.ToString();
        }

        private void ModeStop_Checked(object sender, RoutedEventArgs e)
        {
            modeCommande = 0;
            SlideMoteurDroit.Value = 0;
            SlideMoteurGauche.Value = 0;
            TextBoxVitesseMoteurDroit.Text = "0";
            TextBoxVitesseMoteurGauche.Text = "0";
            TextBoxInParametre.Text = modeCommande.ToString();
        }

        private void SlideMoteurDroit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (modeCommande == 2)
                TextBoxVitesseMoteurDroit.Text = SlideMoteurDroit.Value.ToString();
        }

        private void SlideMoteurGauche_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (modeCommande == 2)
                TextBoxVitesseMoteurGauche.Text = SlideMoteurGauche.Value.ToString();
        }
    }
}
