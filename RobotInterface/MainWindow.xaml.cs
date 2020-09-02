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


namespace RobotInterface
{
  
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool ColorButtonEnvoyer = true;
        SerialPort serialPort1;

        public MainWindow()
        {
            serialPort1 = new ReliableSerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            serialPort1.Open();
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SendMessage()
        {
            TextBoxRéception.Text = "Reçu : " + TextBoxEmission.Text + "\n" + TextBoxRéception.Text;
            TextBoxEmission.Text = null;
            serialPort1.WriteLine(TextBoxEmission.Text);
        }

        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();

            buttonEnvoyer.Background = ColorButtonEnvoyer == true ? Brushes.RoyalBlue : Brushes.Beige;
            ColorButtonEnvoyer = !ColorButtonEnvoyer;

        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }
    }
}
