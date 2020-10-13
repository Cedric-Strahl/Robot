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
            byte[] trame = msgPayload;
            //byte[] trame = EncodeWithoutChecksum(msgFunction, msgPayloadLength, msgPayload);

            byte checksum = trame[0];
            for (int i = 1; i < trame.Length; i++)
            {
                checksum ^= trame[i];
            }
            TextBoxInParametre.Text = "CHECKSUM = " + checksum.ToString();
            //System.Diagnostics.Debug.WriteLine("[CHECKSUM] " + trame + " Result : " + checksum);
            return checksum;
        }

        void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            //byte[] trame = EncodeWithoutChecksum(msgFunction, msgPayloadLength, msgPayload);
            byte[] trame = msgPayload;
            byte[] checksum = new byte[] { CalculateChecksum(msgFunction, msgPayloadLength, msgPayload) };
            trame = Combine(trame, checksum);
            if (serialPort1 != null)
                serialPort1.Write(trame, 0, trame.Length);
        }
        /*
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
        }*/
        
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] newTrame = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, newTrame, 0, first.Length);
            Buffer.BlockCopy(second, 0, newTrame, first.Length, second.Length);
            return newTrame;
        }

        public enum StateReception
        {
            Waiting,
            FunctionMSB,
            FunctionLSB,
            PayloadLengthMSB,
            PayloadLengthLSB,
            Payload,
            CheckSum
        }

        StateReception rcvState = StateReception.Waiting;
        //StateReception rcvBefore = StateReception.Waiting;
        int msgDecodedFunction = 0;
        int msgDecodedPayloadLength = 0;
        //byte[] msgDecodedPayload;
        //int msgDecodedPayloadIndex = 0;

        private void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE) rcvState = StateReception.FunctionMSB;
                    break;

                case StateReception.FunctionMSB:
                    msgDecodedFunction = (ushort)(c << 8);
                    rcvState = StateReception.FunctionLSB;
                    break;

                case StateReception.FunctionLSB:
                    msgDecodedFunction += (ushort)(c << 0);
                    rcvState = StateReception.PayloadLengthMSB;
                    break;

                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength = (ushort)(c << 8);
                    rcvState = StateReception.PayloadLengthLSB;
                    break;

                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength += (ushort)(c << 0);
                    if (msgDecodedPayloadLength > 0)
                    {
                        msgDecodedPayload = new byte[msgDecodedPayloadLength];
                        rcvState = StateReception.Payload;
                    }
                    else rcvState = StateReception.CheckSum;
                    break;

                case StateReception.Payload:
                    msgDecodedPayload[msgDecodedPayloadIndex] = c;
                    msgDecodedPayloadIndex++;
                    if (msgDecodedPayloadIndex >= msgDecodedPayloadLength) rcvState = StateReception.CheckSum;
                    break;

                case StateReception.CheckSum:
                    byte calculatedChecksum = CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    byte receivedChecksum = 0;
                    if (calculatedChecksum == receivedChecksum)
                    {
                        //Success, on a un message valide
                        Console.WriteLine("Message is Correct");
                    }

                    rcvState = StateReception.Waiting;
                    msgDecodedFunction = 0;
                    msgDecodedPayloadLength = 0;
                    msgDecodedPayloadIndex = 0;
                    break;

                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }
            //rcvBefore = rcvState;

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
            else TextBoxRéception.Text =TextBoxRéception.Text + "Pas de Port Serie connecte" + "\n";
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
                byte[] array = Encoding.ASCII.GetBytes("Arrietty");
                UartEncodeAndSendMessage(128, array.Length, array);

                /*
                byte[] byteListe = new byte[20];
                int i;
                for (i = 0; i < 20; i++)
                {
                    byteListe[i] = (byte)(2 * i);
                }
                serialPort1.Write(byteListe, 0, byteListe.Count());

                if (TextBoxRéception.Text != "")
                    TextBoxRéception.Text += "\n";
                */
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
            SlideMoteurDroit.Value = 0;
            SlideMoteurGauche.Value = 0;
            //TextBoxInParametre.Text = modeCommande.ToString();
        }

        private void ModeManuel_Checked(object sender, RoutedEventArgs e)
        {
            modeCommande = 2;
            TextBoxVitesseMoteurDroit.Text = SlideMoteurDroit.Value.ToString(); ;
            TextBoxVitesseMoteurGauche.Text = SlideMoteurGauche.Value.ToString(); ;
            //TextBoxInParametre.Text = modeCommande.ToString();
        }

        private void ModeStop_Checked(object sender, RoutedEventArgs e)
        {
            modeCommande = 0;

            TextBoxVitesseMoteurDroit.Text = "0";
            TextBoxVitesseMoteurGauche.Text = "0";
            //TextBoxInParametre.Text = modeCommande.ToString();
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

        private void ModeManuel_KeyUp(object sender, KeyEventArgs e)
        {
            //Commande vitesse moteur droit
            if (e.Key == Key.NumPad9)
            {
                SlideMoteurDroit.Value += 5;
            }
            if (e.Key == Key.NumPad3)
            {
                SlideMoteurDroit.Value -= 5;
            }

            //Commande vitesse moteur gauche
            if (e.Key == Key.NumPad7)
            {
                SlideMoteurGauche.Value += 5;
            }
            if (e.Key == Key.NumPad1)
            {
                SlideMoteurGauche.Value -= 5;
            }

            //Commande tourner 
            if (e.Key == Key.NumPad6)//Tourner droite
            {
                SlideMoteurGauche.Value += 5;
                SlideMoteurDroit.Value -= 5;
            }
            if (e.Key == Key.NumPad4)//Tourner gauche
            {
                SlideMoteurGauche.Value -= 5;
                SlideMoteurDroit.Value += 5;
            }

            //Commande vitesse globale
            if (e.Key == Key.NumPad8)//Avancer
            {
                if(SlideMoteurDroit.Value < SlideMoteurGauche.Value) SlideMoteurDroit.Value += 5;
                else if(SlideMoteurDroit.Value > SlideMoteurGauche.Value) SlideMoteurGauche.Value += 5;
                else
                {
                    SlideMoteurDroit.Value += 5;
                    SlideMoteurGauche.Value += 5;
                }
            }
            if (e.Key == Key.NumPad2)//Reculer
            {
                if (SlideMoteurDroit.Value > SlideMoteurGauche.Value) SlideMoteurDroit.Value -= 5;
                else if (SlideMoteurDroit.Value < SlideMoteurGauche.Value) SlideMoteurGauche.Value -= 5;
                else
                {
                    SlideMoteurDroit.Value -= 5;
                    SlideMoteurGauche.Value -= 5;
                }
            }
            if (e.Key == Key.NumPad5)//Stop
            {
                SlideMoteurGauche.Value = 0;
                SlideMoteurDroit.Value = 0;
            }

        }
    }
}
