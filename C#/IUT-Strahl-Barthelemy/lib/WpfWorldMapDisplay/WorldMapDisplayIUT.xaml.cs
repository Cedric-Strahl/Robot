﻿    
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Model.DataSeries.Heatmap2DArrayDataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Drawing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Shapes;
using Utilities;

namespace WpfWorldMapDisplay
{

    /// <summary>
    /// Logique d'interaction pour ExtendedHeatMap.xaml
    /// </summary>
    public partial class WorldMapDisplay : UserControl
    {

        Random random = new Random();
        DispatcherTimer timerAffichage;

        public bool IsExtended = false;

        double TerrainLowerX = -11;
        double TerrainUpperX = 11;
        double TerrainLowerY = -7;
        double TerrainUpperY = 7;

        //Liste des robots à afficher
        RobotDisplay RobotDisplay = new RobotDisplay();
        //Dictionary<int, RobotDisplay> OpponentDisplayDictionary = new Dictionary<int, RobotDisplay>();

        List<PolygonExtended> ObjectDisplayList = new List<PolygonExtended>();

        //Liste des balles à afficher
        BallDisplay Balle = new BallDisplay();
        //List<BallDisplay> ListBalles = new List<BallDisplay>();

        public WorldMapDisplay()
        {
            InitializeComponent();

            InitRobot();

            //Timer de simulation
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();
            //InitSoccerField();
        }

        public void InitRobot()
        {
            PolygonExtended robotShape = new PolygonExtended();
            robotShape.polygon.Points.Add(new Point(-0.15, -0.15));
            robotShape.polygon.Points.Add(new Point(0.15,-0.15));
            robotShape.polygon.Points.Add(new Point(0.15, -0.05));
            robotShape.polygon.Points.Add(new Point(0.25, 0));
            robotShape.polygon.Points.Add(new Point(0.15, 0.05));
            robotShape.polygon.Points.Add(new Point(0.15, 0.15));
            robotShape.polygon.Points.Add(new Point(-0.15, 0.15));
            robotShape.polygon.Points.Add(new Point(-0.15, -0.15));
            RobotDisplay = new RobotDisplay(robotShape, System.Drawing.Color.Red, 1);
            RobotDisplay.SetPosition(0, 0, 0);

        }

        private void TimerAffichage_Tick(object sender, EventArgs e)
        {
            DrawRobot();
            //DrawLidar();            
            PolygonSeries.RedrawAll();
            ObjectsPolygonSeries.RedrawAll();
            BallPolygon.RedrawAll();
        }

        public void DrawRobot()
        {
            XyDataSeries<double, double> lidarPts = new XyDataSeries<double, double>();
            ObjectsPolygonSeries.Clear();

            //Affichage du robot
            PolygonSeries.AddOrUpdatePolygonExtended(0, RobotDisplay.GetRobotPolygon());
            PolygonSeries.AddOrUpdatePolygonExtended(1, RobotDisplay.GetRobotSpeedArrow());
            PolygonSeries.AddOrUpdatePolygonExtended(2, RobotDisplay.GetRobotDestinationArrow());
            PolygonSeries.AddOrUpdatePolygonExtended(3, RobotDisplay.GetRobotWaypointArrow());


            //Rendering des points Lidar
            lidarPts.AcceptsUnsortedData = true;
            var lidarData = RobotDisplay.GetRobotLidarPoints();
            lidarPts.Append(lidarData.XValues, lidarData.YValues);

            //Rendering des objets Lidar
            foreach (var polygonObject in RobotDisplay.GetRobotLidarObjects())
                ObjectsPolygonSeries.AddOrUpdatePolygonExtended(ObjectsPolygonSeries.Count(), polygonObject);
                       
            //Affichage des points lidar
            LidarPoints.DataSeries = lidarPts;
        }

        public void DrawLidar()
        {
            ObjectsPolygonSeries = new PolygonRenderableSeries();
            int i = 0;
            foreach (var r in ObjectDisplayList)
            {
                //Affichage des objets détectés par le Lidar
                PolygonSeries.AddOrUpdatePolygonExtended(i++, r);
            }
        }

        public void UpdateRobotLocation(Location location)
        {
            if (location == null)
                return;

            RobotDisplay.SetPosition(location.X, location.Y, location.Theta);
            RobotDisplay.SetSpeed(location.Vx, location.Vy, location.Vtheta);            
        }

        public void UpdateLidarMap(List<PointD> lidarMap)
        {
            if (lidarMap == null)
                return;

            RobotDisplay.SetLidarMap(lidarMap);
        }

        public void UpdateLidarObjects(List<PolarPointListExtended> lidarObjectList)
        {
            if (lidarObjectList == null)
                return;
            RobotDisplay.SetLidarObjectList(lidarObjectList);
        }

        public void UpdateBallLocation(Location ballLocation)
        {
            Balle.SetLocation(ballLocation);
        }

        public void UpdateRobotWaypoint(Location waypointLocation)
        {
            if (waypointLocation == null)
                return;
            RobotDisplay.SetWayPoint(waypointLocation.X, waypointLocation.Y, waypointLocation.Theta);
        }

        public void UpdateRobotDestination(int robotId, Location destinationLocation)
        {
            if (destinationLocation == null)
                return;
            RobotDisplay.SetDestination(destinationLocation.X, destinationLocation.Y, destinationLocation.Theta);
        }

        void InitSoccerField()
        {
            int fieldLineWidth = 2;
            PolygonExtended p = new PolygonExtended();

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.335, 0));
            p.polygon.Points.Add(new Point(-0.335, 1.5));
            p.polygon.Points.Add(new Point(-1.815, 1.5));
            p.polygon.Points.Add(new Point(-1.815, 0));
            p.polygon.Points.Add(new Point(-0.335, 0));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CarreServiceHautGauche, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.335, 0));
            p.polygon.Points.Add(new Point(-0.335, -1.5));
            p.polygon.Points.Add(new Point(-1.815, -1.5));
            p.polygon.Points.Add(new Point(-1.815, 0));
            p.polygon.Points.Add(new Point(-0.335, 0));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CarreServiceBasGauche, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.335, 0));
            p.polygon.Points.Add(new Point(0.335, 1.5));
            p.polygon.Points.Add(new Point(1.815, 1.5));
            p.polygon.Points.Add(new Point(1.815, 0));
            p.polygon.Points.Add(new Point(0.335, 0));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CarreServiceHautDroit, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.335, 0));
            p.polygon.Points.Add(new Point(0.335, -1.5));
            p.polygon.Points.Add(new Point(1.815, -1.5));
            p.polygon.Points.Add(new Point(1.815, 0));
            p.polygon.Points.Add(new Point(0.335, 0));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CarreServiceBasDroit, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.335, -2));
            p.polygon.Points.Add(new Point(0.335, -2));
            p.polygon.Points.Add(new Point(0.335, 2));
            p.polygon.Points.Add(new Point(-0.335, 2));
            p.polygon.Points.Add(new Point(-0.335, -2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.LigneMediane, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.019, -2));
            p.polygon.Points.Add(new Point(0.019, 2));
            p.polygon.Points.Add(new Point(-0.019, 2));
            p.polygon.Points.Add(new Point(-0.019, -2));
            p.polygon.Points.Add(new Point(0.019, -2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Black;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.LigneNoirCentrale, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.335, -1.5));
            p.polygon.Points.Add(new Point(-4, -1.5));
            p.polygon.Points.Add(new Point(-4, -2));
            p.polygon.Points.Add(new Point(-0.335, -2));
            p.polygon.Points.Add(new Point(-0.335, -1.5));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CouloirBasGauche, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.335, 1.5));
            p.polygon.Points.Add(new Point(-4, 1.5));
            p.polygon.Points.Add(new Point(-4, 2));
            p.polygon.Points.Add(new Point(-0.335, 2));
            p.polygon.Points.Add(new Point(-0.335, 1.5));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CouloirHautGauche, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.335, 1.5));
            p.polygon.Points.Add(new Point(4, 1.5));
            p.polygon.Points.Add(new Point(4, 2));
            p.polygon.Points.Add(new Point(0.335, 2));
            p.polygon.Points.Add(new Point(0.335, 1.5));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CouloirHautDroit, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.335, -1.5));
            p.polygon.Points.Add(new Point(4, -1.5));
            p.polygon.Points.Add(new Point(4, -2));
            p.polygon.Points.Add(new Point(0.335, -2));
            p.polygon.Points.Add(new Point(0.335, -1.5));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.CouloirBasDroit, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-1.815, 1.5));
            p.polygon.Points.Add(new Point(-4, 1.5));
            p.polygon.Points.Add(new Point(-4, -1.5));
            p.polygon.Points.Add(new Point(-1.815, -1.5));
            p.polygon.Points.Add(new Point(-1.815, 1.5));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.FontDeCourtGauche, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(1.815, 1.5));
            p.polygon.Points.Add(new Point(4, 1.5));
            p.polygon.Points.Add(new Point(4, -1.5));
            p.polygon.Points.Add(new Point(1.815, -1.5));
            p.polygon.Points.Add(new Point(1.815, 1.5));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.FontDeCourtDroit, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-4, -0.1));
            p.polygon.Points.Add(new Point(-4.2, -0.1));
            p.polygon.Points.Add(new Point(-4.2, 0.1));
            p.polygon.Points.Add(new Point(-4, 0.1));
            p.polygon.Points.Add(new Point(-4, -0.1));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Brown;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheFontDeCourt, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.9, -2));
            p.polygon.Points.Add(new Point(-0.9, -2.2));
            p.polygon.Points.Add(new Point(-1.1, -2.2));
            p.polygon.Points.Add(new Point(-1.1, -2));
            p.polygon.Points.Add(new Point(-0.9, -2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Brown;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheBas, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(-0.9, 2));
            p.polygon.Points.Add(new Point(-0.9, 2.2));
            p.polygon.Points.Add(new Point(-1.1, 2.2));
            p.polygon.Points.Add(new Point(-1.1, 2));
            p.polygon.Points.Add(new Point(-0.9, 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Brown;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheHaut, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(4, -0.1));
            p.polygon.Points.Add(new Point(4.2, -0.1));
            p.polygon.Points.Add(new Point(4.2, 0.1));
            p.polygon.Points.Add(new Point(4, 0.1));
            p.polygon.Points.Add(new Point(4, -0.1));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Brown;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseDroitFontDeCourt, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.9, -2));
            p.polygon.Points.Add(new Point(0.9, -2.2));
            p.polygon.Points.Add(new Point(1.1, -2.2));
            p.polygon.Points.Add(new Point(1.1, -2));
            p.polygon.Points.Add(new Point(0.9, -2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Brown;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseDroitBas, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new Point(0.9, 2));
            p.polygon.Points.Add(new Point(0.9, 2.2));
            p.polygon.Points.Add(new Point(1.1, 2.2));
            p.polygon.Points.Add(new Point(1.1, 2));
            p.polygon.Points.Add(new Point(0.9, 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.Brown;
            PolygonSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseDroitHaut, p);

        }
    }

    public class PolygonRenderableSeries : CustomRenderableSeries
    {
        Dictionary<int, PolygonExtended> polygonList = new Dictionary<int, PolygonExtended>();
        XyDataSeries<double, double> lineData = new XyDataSeries<double, double> { }; //Nécessaire pour l'update d'affichage

        public PolygonRenderableSeries()
        {
        }

        public void AddOrUpdatePolygonExtended(int id, PolygonExtended p)
        {
            if (polygonList.ContainsKey(id))
                polygonList[id] = p;
            else
                polygonList.Add(id, p);
        }

        public void Clear()
        {
            polygonList.Clear();
        }

        public int Count()
        {
            return polygonList.Count();
        }

        public void RedrawAll()
        {
            //Attention : Permet de déclencher l'update : workaround pas classe du tout
            lineData.Clear();
            lineData.Append(1, 1);
            DataSeries = lineData;
        }

        protected override void Draw(IRenderContext2D renderContext, IRenderPassData renderPassData)
        {
            base.Draw(renderContext, renderPassData);

            // Create a line drawing context. Make sure you dispose it!
            // NOTE: You can create mutliple line drawing contexts to draw segments if you want
            //       You can also call renderContext.DrawLine() and renderContext.DrawLines(), but the lineDrawingContext is higher performance
            foreach (var p in polygonList)
            {
                Polygon polygon = p.Value.polygon;

                if (polygon.Points.Count > 0)
                {
                    Point initialPoint = GetRenderingPoint(polygon.Points[0]);

                    System.Windows.Media.Color backgroundColor = System.Windows.Media.Color.FromArgb(p.Value.backgroundColor.A, p.Value.backgroundColor.R, p.Value.backgroundColor.G, p.Value.backgroundColor.B);

                    using (var brush = renderContext.CreateBrush(backgroundColor))
                    {
                        //IEnumerable<Point> points; // define your points
                        renderContext.FillPolygon(brush, GetRenderingPoints(polygon.Points));
                    }

                    //// Create a pen to draw. Make sure you dispose it! 
                    System.Windows.Media.Color borderColor = System.Windows.Media.Color.FromArgb(p.Value.borderColor.A, p.Value.borderColor.R, p.Value.borderColor.G, p.Value.borderColor.B);

                    using (var linePen = renderContext.CreatePen(borderColor, this.AntiAliasing, p.Value.borderWidth, p.Value.borderOpacity, p.Value.borderDashPattern))
                    {
                        using (var lineDrawingContext = renderContext.BeginLine(linePen, initialPoint.X, initialPoint.Y))
                        {
                            for (int i = 1; i < polygon.Points.Count; i++)
                            {
                                lineDrawingContext.MoveTo(GetRenderingPoint(polygon.Points[i]).X, GetRenderingPoint(polygon.Points[i]).Y);
                            }
                            lineDrawingContext.End();
                        }
                    }
                }
            }
        }
        private Point GetRenderingPoint(Point pt)
        {
            // Get the coordinateCalculators. See 'Converting Pixel Coordinates to Data Coordinates' documentation for coordinate transforms
            var xCoord = CurrentRenderPassData.XCoordinateCalculator.GetCoordinate(pt.X);
            var yCoord = CurrentRenderPassData.YCoordinateCalculator.GetCoordinate(pt.Y);

            //if (CurrentRenderPassData.IsVerticalChart)
            //{
            //    Swap(ref xCoord, ref yCoord);
            //}

            return new Point(xCoord, yCoord);
        }
        private PointCollection GetRenderingPoints(PointCollection ptColl)
        {
            PointCollection ptCollRender = new PointCollection();
            foreach (var pt in ptColl)
            {
                // Get the coordinateCalculators. See 'Converting Pixel Coordinates to Data Coordinates' documentation for coordinate transforms
                var xCoord = CurrentRenderPassData.XCoordinateCalculator.GetCoordinate(pt.X);
                var yCoord = CurrentRenderPassData.YCoordinateCalculator.GetCoordinate(pt.Y);
                ptCollRender.Add(new Point(xCoord, yCoord));
            }

            return ptCollRender;
        }
    }



    public class RobotDisplay
    {
        private PolygonExtended shape;
        private Random rand = new Random();

        private Location location;
        private Location destinationLocation;
        private Location waypointLocation;
        public double[,] heatMap;
        List<PointD> lidarMap;
        List<Location> opponentLocationList;
        List<Location> teamLocationList;
        List<PolarPointListExtended> lidarObjectList;

        System.Drawing.Color displayColor;
        int displayTransparency = 0xFF;
        
        public RobotDisplay()
        {

        }
        public RobotDisplay(PolygonExtended pe, System.Drawing.Color color, double transparency)
        {
            location = new Location(0, 0, 0, 0, 0, 0);
            destinationLocation = new Location(0, 0, 0, 0, 0, 0);
            waypointLocation = new Location(0, 0, 0, 0, 0, 0);
            shape = pe;
            lidarMap = new List<PointD>();
            displayTransparency = (int)(transparency * 255);
            displayColor = System.Drawing.Color.FromArgb((byte)displayTransparency, color.R, color.G, color.B);
        }

        public void SetPosition(double x, double y, double theta)
        {
            location.X = x;
            location.Y = y;
            location.Theta = theta;
        }
        public void SetSpeed(double vx, double vy, double vTheta)
        {
            location.Vx = vx;
            location.Vy = vy;
            location.Vtheta = vTheta;
        }
        public void SetDestination(double x, double y, double theta)
        {
            destinationLocation.X = x;
            destinationLocation.Y = y;
            destinationLocation.Theta = theta;
        }
        public void SetWayPoint(double x, double y, double theta)
        {
            waypointLocation.X = x;
            waypointLocation.Y = y;
            waypointLocation.Theta = theta;
        }

        public void SetHeatMap(double[,] heatMap)
        {
            this.heatMap = heatMap;
        }

        public void SetLidarMap(List<PointD> lidarMap)
        {
            this.lidarMap = lidarMap;
        }
        public void SetLidarObjectList(List<PolarPointListExtended> lidarObjectList)
        {
            this.lidarObjectList = lidarObjectList;
        }

        public void SetOpponentLocationList(List<Location> list)
        {
            this.opponentLocationList = list;
        }

        public void SetTeamLocationList(List<Location> list)
        {
            this.teamLocationList = list;
        }
        public void SetPositionAndSpeed(double x, double y, double theta, double vx, double vy, double vTheta)
        {
            location.X = x;
            location.Y = y;
            location.Theta = theta;
            location.Vx = vx;
            location.Vy = vy;
            location.Vtheta = vTheta;
        }

        public PolygonExtended GetRobotPolygon()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            foreach (var pt in shape.polygon.Points)
            {
                Point polyPt = new Point(pt.X * Math.Cos(location.Theta) - pt.Y * Math.Sin(location.Theta), pt.X * Math.Sin(location.Theta) + pt.Y * Math.Cos(location.Theta));
                polyPt.X += location.X;
                polyPt.Y += location.Y;
                polygonToDisplay.polygon.Points.Add(polyPt);
                polygonToDisplay.backgroundColor = displayColor;// shape.backgroundColor;
                polygonToDisplay.borderColor = shape.borderColor;
                polygonToDisplay.borderWidth = shape.borderWidth;
            }
            return polygonToDisplay;
        }
        public PolygonExtended GetRobotSpeedArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            double angleTeteFleche = Math.PI / 6;
            double longueurTeteFleche = 0.30;
            double LongueurFleche = Math.Sqrt(location.Vx * location.Vx + location.Vy * location.Vy);
            double headingAngle = Math.Atan2(location.Vy, location.Vx) + location.Theta;
            double xTete = LongueurFleche * Math.Cos(headingAngle);
            double yTete = LongueurFleche * Math.Sin(headingAngle);

            polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
            double angleTeteFleche1 = headingAngle + angleTeteFleche;
            double angleTeteFleche2 = headingAngle - angleTeteFleche;
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete - longueurTeteFleche * Math.Cos(angleTeteFleche1), location.Y + yTete - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete - longueurTeteFleche * Math.Cos(angleTeteFleche2), location.Y + yTete - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
            polygonToDisplay.borderWidth = 2;
            polygonToDisplay.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
            polygonToDisplay.borderDashPattern = new double[] { 3, 3 };
            polygonToDisplay.borderOpacity = 1;
            polygonToDisplay.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            return polygonToDisplay;
        }
        public PolygonExtended GetRobotDestinationArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            double angleTeteFleche = Math.PI / 6;
            double longueurTeteFleche = 0.30;
            double headingAngle = Math.Atan2(destinationLocation.Y - location.Y, destinationLocation.X - location.X);

            polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X, destinationLocation.Y));
            double angleTeteFleche1 = headingAngle + angleTeteFleche;
            double angleTeteFleche2 = headingAngle - angleTeteFleche;
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche1), destinationLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X, destinationLocation.Y));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche2), destinationLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X, destinationLocation.Y));
            polygonToDisplay.borderWidth = 5;
            polygonToDisplay.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            polygonToDisplay.borderDashPattern = new double[] { 5, 5 };
            polygonToDisplay.borderOpacity = 0.4;
            polygonToDisplay.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            return polygonToDisplay;
        }
        public PolygonExtended GetRobotWaypointArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            double angleTeteFleche = Math.PI / 6;
            double longueurTeteFleche = 0.30;
            double headingAngle = Math.Atan2(waypointLocation.Y - location.Y, waypointLocation.X - location.X);

            polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X, waypointLocation.Y));
            double angleTeteFleche1 = headingAngle + angleTeteFleche;
            double angleTeteFleche2 = headingAngle - angleTeteFleche;
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche1), waypointLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X, waypointLocation.Y));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche2), waypointLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X, waypointLocation.Y));

            return polygonToDisplay;
        }

        public XyDataSeries<double, double> GetRobotLidarPoints()
        {
            var dataSeries = new XyDataSeries<double, double>();
            if (lidarMap == null)
                return dataSeries;

            var listX = lidarMap.Select(e => e.X);
            var listY = lidarMap.Select(e => e.Y);

            //dataSeries.Clear();
            dataSeries.AcceptsUnsortedData = true;
            dataSeries.Append(listX, listY);
            return dataSeries;
        }

        public List<PolygonExtended> GetRobotLidarObjects()
        {
            var polygonExtendedList = new List<PolygonExtended>();
            if (this.lidarObjectList == null)
                return polygonExtendedList;

            foreach (var obj in this.lidarObjectList)
            {
                PolygonExtended polygonToDisplay = new PolygonExtended();

                foreach (var pt in obj.PolarPointList)
                {
                    polygonToDisplay.polygon.Points.Add(new Point(location.X + pt.Distance * Math.Cos(pt.Angle), location.Y + pt.Distance * Math.Sin(pt.Angle)));
                }
                polygonToDisplay.borderColor = obj.DisplayColor;
                polygonToDisplay.borderWidth = (float)obj.DisplayWidth;
                polygonToDisplay.backgroundColor = obj.DisplayColor;
                polygonExtendedList.Add(polygonToDisplay);
            }
            return polygonExtendedList;
        }
    }

    public class BallDisplay
    {
        private Random rand = new Random();
        private Location location = new Location(0, 0, 0, 0, 0, 0);
        private System.Drawing.Color backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xF2, 0x00);
        private System.Drawing.Color borderColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
        private int borderWidth = 2;

        public BallDisplay()
        {
            location = new Location(0, 0, 0, 0, 0, 0);
        }

        public void SetPosition(double x, double y, double theta)
        {
            location.X = x;
            location.Y = y;
            location.Theta = theta;
        }
        public void SetSpeed(double vx, double vy, double vTheta)
        {
            location.Vx = vx;
            location.Vy = vy;
            location.Vtheta = vTheta;
        }

        public void SetLocation(double x, double y, double theta, double vx, double vy, double vTheta)
        {
            location.X = x;
            location.Y = y;
            location.Theta = theta;
            location.Vx = vx;
            location.Vy = vy;
            location.Vtheta = vTheta;
        }
        public void SetLocation(Location l)
        {
            location = l;
        }

        public PolygonExtended GetBallPolygon()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            if (location != null)
            {
                int nbSegments = 10;
                double radius = 0.4;
                for (double theta = 0; theta <= Math.PI * 2; theta += Math.PI * 2 / nbSegments)
                {
                    Point pt = new Point(radius * Math.Cos(theta), radius * Math.Sin(theta));
                    pt.X += location.X;
                    pt.Y += location.Y;
                    polygonToDisplay.polygon.Points.Add(pt);
                    polygonToDisplay.backgroundColor = backgroundColor;
                    polygonToDisplay.borderColor = borderColor;
                    polygonToDisplay.borderWidth = borderWidth;
                }
            }
            return polygonToDisplay;
        }
        public PolygonExtended GetBallSpeedArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            if (location != null)
            {
                double angleTeteFleche = Math.PI / 6;
                double longueurTeteFleche = 0.30;
                double LongueurFleche = Math.Sqrt(location.Vx * location.Vx + location.Vy * location.Vy);
                double headingAngle = Math.Atan2(location.Vy, location.Vx) + location.Theta;
                double xTete = LongueurFleche * Math.Cos(headingAngle);
                double yTete = LongueurFleche * Math.Sin(headingAngle);

                polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
                polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
                double angleTeteFleche1 = headingAngle + angleTeteFleche;
                double angleTeteFleche2 = headingAngle - angleTeteFleche;
                polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete - longueurTeteFleche * Math.Cos(angleTeteFleche1), location.Y + yTete - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
                polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
                polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete - longueurTeteFleche * Math.Cos(angleTeteFleche2), location.Y + yTete - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
                polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
                polygonToDisplay.borderWidth = 2;
                polygonToDisplay.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
                polygonToDisplay.borderDashPattern = new double[] { 3, 3 };
                polygonToDisplay.borderOpacity = 1;
                polygonToDisplay.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            }
            return polygonToDisplay;
        }
    }

    public enum Terrain
    {
        ZoneProtegee = 101,
        CarreServiceHautDroit = 102,
        CarreServiceHautGauche = 103,
        CarreServiceBasDroit = 104,
        CarreServiceBasGauche = 105,
        FontDeCourtDroit = 106,
        FontDeCourtGauche = 107,
        CouloirHautDroit = 108,
        CouloirHautGauche = 109,
        CouloirBasDroit = 110,
        CouloirBasGauche = 111,
        ZoneTechniqueGauche = 112,
        ZoneTechniqueDroite = 113,
        LigneMediane = 114,
        LigneNoirCentrale = 115,
        BaliseGaucheFontDeCourt = 116,
        BaliseGaucheHaut = 117,
        BaliseGaucheBas = 118,
        BaliseDroitFontDeCourt = 119,
        BaliseDroitHaut = 120,
        BaliseDroitBas = 121

    }
}

