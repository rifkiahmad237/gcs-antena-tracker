using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using System.IO;
using System.Net;
using System.IO.Ports;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.CacheProviders;
using GMap.NET.Properties;
using GMap.NET.Internals;

namespace Tracker_New
{
    public partial class Form1 : KryptonForm
    {
        string input, input2;
        string rawIN1s = "00";
        string rawIN2s = "00";
        string rawIN3s = "00";

        double azim, azimSrv, elev, elevSrv, lat = 0, lon = 0, alt = 0, x, y, x1, y1, z,

        //home position:
        hALT,
        hLON,
        hLAT,

        //Servo Calibration:
        tiltMin,
        tiltMax,
        panMin,
        panMax,

        //earth information:
        cirLON = 40075017 / 360,
        cirLAT = 40007860 / 180;

        PointLatLng koordinat = new PointLatLng();
        GMapOverlay marker = new GMapOverlay();
        GMapOverlay trace = new GMapOverlay();
        GMapMarker payload;
        List<PointLatLng> track = new List<PointLatLng>();
        GMapRoute route;


        private void ComboBoxPorts_DropDown(object sender, EventArgs e)
        {
            comboBoxPorts.DataSource = null;
            var p = SerialPort.GetPortNames();
            comboBoxPorts.DataSource = p;
        }

        private void Tb_PanMax_TextChange(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void Tb_TiltMax_TextChange(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void Tb_PanMin_TextChange(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void Tb_TiltMin_TextChange(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void Tb_HomeAlt_TextChange(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        private void Tb_HomeLon_TextChange(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        private void Tb_HomeLat_TextChange(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (check_calibrate.Checked)
            {
                tb_srvPan.Text = ((panMin + panMax) / 2).ToString();
                tb_srvTilt.Text = ((tiltMin + tiltMax) / 2).ToString();
            }
            try
            {
                if (serialPort1.IsOpen == true)
                {
                    serialPort1.Write(tb_srvPan.Text + "a");
                    serialPort1.Write(tb_srvTilt.Text + "b");

                }
            }
            catch
            {
                /*
                tmr_MPstream.Stop();
                
                //timer2.Stop();
                //serialPort1.Close();
                
                rtb_MPdata.Text = "Serial Disconnect, communication stopped!\n";
                btn_MPstream.Text = "Start Stream";
                tb_MPstreamInterval.Enabled = true;
                button1.Text = "Connect";
                comboBoxPorts.DataSource = null;*/
            }
        }
        void loadParameters()
        {
            try
            {
                using (StreamReader sr = new StreamReader("params.txt"))
                {
                    String tMin, tMax, pMin, pMax;

                    tMin = sr.ReadLine();
                    tMax = sr.ReadLine();
                    pMin = sr.ReadLine();
                    pMax = sr.ReadLine();

                    tb_TiltMin.Text = tMin;
                    tb_TiltMax.Text = tMax;
                    tb_PanMin.Text = pMin;
                    tb_PanMax.Text = pMax;

                    tiltMin = Convert.ToInt32(tMin);
                    tiltMax = Convert.ToInt32(tMax);
                    panMin = Convert.ToInt32(pMin);
                    panMax = Convert.ToInt32(pMax);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("params not found! Set to default");
                defaultParameters();
            }
        }
        void defaultParameters()
        {
            String tMin, tMax, pMin, pMax;

            tMin = "0";
            tMax = "180";
            pMin = "0";
            pMax = "180";

            tb_TiltMin.Text = tMin;
            tb_TiltMax.Text = tMax;
            tb_PanMin.Text = pMin;
            tb_PanMax.Text = pMax;

            tiltMin = Convert.ToInt32(tMin);
            tiltMax = Convert.ToInt32(tMax);
            panMin = Convert.ToInt32(pMin);
            panMax = Convert.ToInt32(pMax);

            saveParameters();
        }
        void saveParameters()
        {
            string[] names = new string[]
            {
                tb_TiltMin.Text,tb_TiltMax.Text,tb_PanMin.Text,tb_PanMax.Text
            };
            using (StreamWriter sw = new StreamWriter("params.txt"))
            {
                foreach (string s in names)
                {
                    sw.WriteLine(s);
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //Servo Calibration:
            tiltMin = Convert.ToDouble(tb_TiltMin.Text);
            tiltMax = Convert.ToDouble(tb_TiltMax.Text);
            panMin = Convert.ToDouble(tb_PanMin.Text);
            panMax = Convert.ToDouble(tb_PanMax.Text);
            button1.Enabled = false;
            saveParameters();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            tb_HomeAlt.Text = tb_AirAlt.Text;
            tb_HomeLat.Text = tb_AirLat.Text;
            tb_HomeLon.Text = tb_AirLon.Text;
        }

        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == false)
            {
                try
                {
                    serialPort1.PortName = comboBoxPorts.Text;
                    serialPort1.BaudRate = 57600;
                    serialPort1.Open();
                    timer2.Start();
                    btn_Connect.Text = "Disconnect";
                    comboBoxPorts.Enabled = false;
                    //btn_MPstream.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("serial error" + ex);
                    timer2.Stop();
                    comboBoxPorts.Enabled = true;
                    //btn_MPstream.Enabled = false;
                }
            }
            else
            {
                timer2.Stop();
                //btn_MPstream.Enabled = false;
                serialPort1.Close();
                btn_Connect.Text = "Connect";
                comboBoxPorts.DataSource = null;
                comboBoxPorts.Enabled = true;
            }
        }
        private void comboBoxPorts_Click(object sender, EventArgs e)
        {
            comboBoxPorts.DataSource = null;
            var p = SerialPort.GetPortNames();
            comboBoxPorts.DataSource = p;
        }

        private void Btn_MPstream_Click(object sender, EventArgs e)
        {

            if (btn_MPstream.Text == "Start Stream")
            {
                rtb_MPdata.Text = "Connecting to Mission Planner...";
                tmr_MPstream.Start();
                btn_MPstream.Text = "Stop Stream";
                tmr_MPstream.Interval = Convert.ToInt16(tb_MPstreamInterval.Text);
                tb_MPstreamInterval.Enabled = false;
                //btn_Connect.Enabled = false;
                //comboBoxPorts.Enabled = false;
            }
            else
            {
                tmr_MPstream.Stop();
                btn_MPstream.Text = "Start Stream";
                tb_MPstreamInterval.Enabled = true;
                //btn_Connect.Enabled = true;
                //comboBoxPorts.Enabled = true;
            }
        }

        private void Tmr_MPstream_Tick(object sender, EventArgs e)
        {
            try
            {
                update_map();
                rtb_MPdata.Clear();
                string url = "http://127.0.0.1:56781/mavlink/";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                input = sr.ReadToEnd();
                rtb_MPdata.Text = input;
                sr.Close();

                int pos0 = input.IndexOf("lat");

                int pos1 = input.IndexOf("eph");
                input2 = input.Substring(pos0, pos1 - pos0 + 3);

                int pos2 = input2.IndexOf("lat");
                int pos3 = input2.IndexOf("lon");
                rawIN1s = input2.Substring(pos2 + 5, pos3 - pos2 - 7);

                int pos4 = input2.IndexOf("alt");
                rawIN2s = input2.Substring(pos3 + 5, pos4 - pos3 - 7);

                int pos5 = input2.IndexOf("eph");
                rawIN3s = input2.Substring(pos4 + 5, pos5 - pos4 - 7);

                tb_AirLat.Text = (Convert.ToDouble(rawIN1s) / 10000000).ToString();
                tb_AirLon.Text = (Convert.ToDouble(rawIN2s) / 10000000).ToString();
                tb_AirAlt.Text = (((Convert.ToDouble(rawIN3s) / 1000) - 100) / 2).ToString();

            }
            catch
            {
                tmr_MPstream.Stop();
                rtb_MPdata.Text = "Mission Planner / mavlink offline";
                btn_MPstream.Text = "Start Stream";
                tb_MPstreamInterval.Enabled = true;
                serialPort1.Close();
                /*
                btn_Connect.Enabled = true;
                btn_Connect.Text = "Connect";
                comboBoxPorts.Enabled = true;
                comboBoxPorts.DataSource = null;*/
            }


            lon = Convert.ToDouble(tb_AirLon.Text);
            lat = Convert.ToDouble(tb_AirLat.Text);
            alt = Convert.ToDouble(tb_AirAlt.Text);
            y = lon - hLON;
            x = lat - hLAT;
            y1 = y * cirLON; x1 = x * cirLAT;
            z = Math.Sqrt(y1 * y1 + x1 * x1);
            azim = (Math.Atan2(y, x) * -180 / Math.PI);
            elev = (Math.Atan2(alt, z) * 180 / Math.PI);

            if (check_calibrate.Checked)
            {
                tb_srvPan.Text = ((panMin + panMax) / 2).ToString();
                tb_srvTilt.Text = ((tiltMin + tiltMax) / 2).ToString();
            }
            else
            {
                if (azim >= 0)
                {
                    azimSrv = map(azim, 0, 180, panMin, panMax);
                    if (azimSrv.ToString().Contains(".")) tb_srvPan.Text = azimSrv.ToString().Substring(0, azimSrv.ToString().IndexOf("."));
                    else tb_srvPan.Text = azimSrv.ToString();
                    //pan.write(azimSrv);
                    if (elev < 0) tb_srvTilt.Text = tiltMin.ToString();// tilt.write(tiltMin);
                    else
                    {
                        elevSrv = map(elev, 0, 180, tiltMin, tiltMax);
                        if (elevSrv.ToString().Contains(".")) tb_srvTilt.Text = elevSrv.ToString().Substring(0, elevSrv.ToString().IndexOf("."));//tilt.write(elevSrv);
                        else tb_srvTilt.Text = elevSrv.ToString();
                    }
                }
                else
                {
                    azimSrv = map(180 + azim, 0, 180, panMin, panMax);
                    if (azimSrv.ToString().Contains(".")) tb_srvPan.Text = azimSrv.ToString().Substring(0, azimSrv.ToString().IndexOf("."));
                    else tb_srvPan.Text = azimSrv.ToString();
                    if (180 - elev < 0) tb_srvTilt.Text = tiltMax.ToString();// tilt.write(tiltMax);
                    else
                    {
                        elevSrv = map(180 - elev, 0, 180, tiltMin, tiltMax);
                        if (elevSrv.ToString().Contains(".")) tb_srvTilt.Text = elevSrv.ToString().Substring(0, elevSrv.ToString().IndexOf("."));//tilt.write(elevSrv);
                        else tb_srvTilt.Text = elevSrv.ToString();
                    }
                }
            }

        }

        public Form1()
        {
            InitializeComponent();
        }

        double map(double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            double scaleIn, scaleOut;
            scaleIn = fromHigh - fromLow;
            scaleOut = toHigh - toLow;

            value = value - fromLow;
            value = (value / scaleIn) * scaleOut;
            value = value + toLow;

            return value;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            loadParameters();

            //btn_MPstream.Enabled = false;
            button1.Enabled = false;
            button1.Enabled = false;

            gMapControl1.CanDragMap = true;
            gMapControl1.MarkersEnabled = true;
            gMapControl1.PolygonsEnabled = true;
            gMapControl1.RoutesEnabled = true;
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.ShowCenter = false;

            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gMapControl1.Position = new PointLatLng(-7.769, 110.3);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 18;
            button3.Enabled = false;
        }

        private void BunifuButton210_Click(object sender, EventArgs e)
        {
            hALT = Convert.ToDouble(tb_HomeAlt.Text);
            hLON = Convert.ToDouble(tb_HomeLon.Text);
            hLAT = Convert.ToDouble(tb_HomeLat.Text);
            button3.Enabled = false;
        }

        private void update_map()
        {
            koordinat = new PointLatLng(lat, lon);
            marker = new GMapOverlay("overlay_marker");
            trace = new GMapOverlay("overlay_trace");
            //Bitmap custom_marker = (Bitmap)Image.FromFile("Resources/plane.png");
            payload = new GMarkerGoogle(koordinat, GMarkerGoogleType.blue_dot);

            track.Add(koordinat);
            route = new GMapRoute(track, "Route");
            route.Stroke = new Pen(Color.Navy, 3);
            marker.Markers.Add(payload);
            trace.Routes.Add(route);

            gMapControl1.Overlays.Add(trace);
            gMapControl1.Overlays.Add(marker);
            gMapControl1.Position = koordinat;
            trace.Routes.Clear();
            marker.Routes.Clear();
            gMapControl1.Overlays.Remove(marker);
        }

    }
}
