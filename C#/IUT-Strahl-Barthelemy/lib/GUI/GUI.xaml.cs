using System;
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
using EvArgsLibrary;
using Utilities;
using System.Windows.Threading;
using Constants;


namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class interfaceRobot : Window
    {

        Location robotLocation;
        public DispatcherTimer Updater = new DispatcherTimer();
        public float anglSpeed = 0;
        public float linSpeed = 0;
        public ulong robotTimestamp = 0;

        public interfaceRobot()
        {
            Updater.Interval = new TimeSpan(0, 0, 0, 0, 200);
            Updater.Tick += Updater_Tick;

            InitializeComponent();

            ScopeAngSpeed.AddOrUpdateLine(1, 100, "Angular speed");
            ScopeAngSpeed.AddOrUpdateLine(2, 100, "Angular speed Consigne");
            ScopeLinSpeed.AddOrUpdateLine(1, 100, "Linear speed");
            ScopeLinSpeed.AddOrUpdateLine(2, 100, "Linear speed Consigne");


            AsservDisplay.SetAsservissementMode(Constants.AsservissementMode.Polar);
            AsservDisplay.UpdateDisplay();

            Updater.Start();
            
        }

        private void Updater_Tick(object sender, EventArgs e)
        {

            AsservDisplay.UpdateDisplay();

        }

        public void UpdateLocation(object sender, PositionDataArgs e)
        {
            //updating display contol
            AsservDisplay.UpdatePolarSpeedConsigneValues(e.VitesseLineaireConsigneFromRobot, 0, e.VitesseAngulaireConsigneFromRobot);
            AsservDisplay.UpdatePolarOdometrySpeed(e.VitesseLineaireOdo, 0, e.VitesseAngulaireOdo);


            var robotLocation = new Location(e.XPosFromodo, e.YPosFromodo, e.AngleRadOdo, 0, 0, 0);
            WorldMap.UpdateRobotLocation(robotLocation);
            robotTimestamp = e.Timestamp;
            ScopeAngSpeed.AddPointToLine(1, new PointD(robotTimestamp, e.VitesseAngulaireOdo));
            ScopeAngSpeed.AddPointToLine(2, new PointD(robotTimestamp, e.VitesseAngulaireConsigneFromRobot));

            ScopeLinSpeed.AddPointToLine(1, new PointD(robotTimestamp, e.VitesseLineaireOdo));
            ScopeLinSpeed.AddPointToLine(2, new PointD(robotTimestamp, e.VitesseLineaireConsigneFromRobot));
        }


    }
}
