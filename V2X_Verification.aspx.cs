using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ionic.Zip;
using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Neaera_Website_2018
{

    public partial class V2X_Verification : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //VerificationButton.Visible = false;
                fillConfigurationFiles();
            }

        }
        protected void VisualizeButton_Click(object sender, EventArgs e)
        {
            try
            {
                string id = this.listConfigurationFiles.SelectedValue.ToString();
                string configFile = "config--" + id + ".json"; //config--test-road--2019-11-8--2019-11-10.json
                string dataFile = "path-data--" + id + ".csv";
                string geojsonFile = "wzdx--" + id + ".geojson";
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container."publishedworkzones"
                CloudBlobContainer container = blobClient.GetContainerReference("unapprovedworkzones");

                using (FileStream fs = new FileStream(Server.MapPath("~/Unzipped Files/data.csv"), FileMode.Create))
                {
                    CloudBlockBlob data_file = container.GetBlockBlobReference("pathdatafiles/" + dataFile);
                    using (MemoryStream temp = new MemoryStream())
                    {
                        data_file.DownloadToStream(temp);
                        temp.Position = 0;
                        temp.CopyTo(fs);
                        fs.Flush();
                    }
                }
                using (FileStream fs = new FileStream(Server.MapPath("~/Unzipped Files/config.json"), FileMode.Create))
                {
                    CloudBlockBlob data_file = container.GetBlockBlobReference("configurationfiles/" + configFile);
                    using (MemoryStream temp = new MemoryStream())
                    {
                        data_file.DownloadToStream(temp);
                        temp.Position = 0;
                        temp.CopyTo(fs);
                        fs.Flush();
                    }
                }
                using (FileStream fs = new FileStream(Server.MapPath("~/Unzipped Files/wzdx.geojson"), FileMode.Create))
                {
                    CloudBlockBlob data_file = container.GetBlockBlobReference("wzdxfiles/" + geojsonFile);
                    using (MemoryStream temp = new MemoryStream())
                    {
                        data_file.DownloadToStream(temp);
                        temp.Position = 0;
                        temp.CopyTo(fs);
                        fs.Flush();
                    }
                }
                createVisualizer();

                loadGeoJsonVis();
            }
            catch (System.Exception ex)
            {
                this.hdnParam.Value = "There was an error loading the requested visualization. " + ex.Message.ToString();
                this.msgtype.Value = "Error";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            }
        }

        protected void VerificationButton_Click(object sender, EventArgs e)
        {
            //Code to call if you need user input

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("unapprovedworkzones");

            List<string[]> files = new List<string[]>();
            List<string[]> extraFiles = new List<string[]>();
            string id = this.listConfigurationFiles.SelectedValue.ToString();
            string configFile = "config--" + id + ".json"; //config--test-road--2019-11-8--2019-11-10.json
            files.Add(new string[] { "configurationfiles/" + configFile, "config/" + configFile });
            string dataFile = "path-data--" + id + ".csv";
            extraFiles.Add(new string[] { "pathdatafiles/" + dataFile, "path-data/" + dataFile });
            string geojsonFile = "wzdx--" + id + ".geojson";
            files.Add(new string[] { "wzdxfiles/" + geojsonFile, "wzdx/" + geojsonFile });

            List<string> xmlFiles = new List<string>();
            var xmlList = container.ListBlobs("rsm-xmlfiles/");
            List<string> blobNamesXml = xmlList.OfType<CloudBlockBlob>().Where(b => b.Name.Contains(id)).Select(b => b.Name.Replace("rsm-xmlfiles/", "")).ToList();
            foreach (string name in blobNamesXml) files.Add(new string[] { "rsm-xmlfiles/" + name, "rsm-xml/" + name });

            List<string> uperFiles = new List<string>();
            var uperList = container.ListBlobs("rsm-uperfiles/");
            List<string> blobNamesUper = uperList.OfType<CloudBlockBlob>().Where(b => b.Name.Contains(id)).Select(b => b.Name.Replace("rsm-uperfiles/", "")).ToList();
            foreach (string name in blobNamesUper) files.Add(new string[] { "rsm-uperfiles/" + name, "rsm-uper/" + name });

            CloudBlobContainer publishedContainer = blobClient.GetContainerReference("publishedworkzones");
            foreach (string[] file in files)
            {
                CloudBlockBlob blob_source = container.GetBlockBlobReference(file[0]);
                CloudBlockBlob blob_dest = publishedContainer.GetBlockBlobReference(file[1]);
                blob_dest.StartCopy(blob_source);
                blob_source.DeleteAsync();
            }

            foreach (string[] file in extraFiles)
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(file[0]);
                blob.DeleteAsync();
            }
            VerificationButton.Style["display"] = "none";
            this.hdnParam.Value = "Your work zone has been successfully published";
            this.msgtype.Value = "Success";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
        }
        protected void loadGeoJsonVis()
        {
            string[] lines = File.ReadAllLines(Server.MapPath("~/Unzipped Files/wzdx.geojson"));
            geojsonStringDiv.InnerHtml = "";
            foreach (string line in lines)
            {
                geojsonStringDiv.InnerHtml += line;
            }
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "loadGeoJson()", true);
        }

        public void fillConfigurationFiles()
        {
            //Good link on populating file structure: https://stackoverflow.com/questions/55362899/getting-all-files-in-azure-file-share-cloudfiledirectory
            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("unapprovedworkzones");
            var list = container.ListBlobs("configurationfiles/");
            List<string> blobNames = list.OfType<CloudBlockBlob>().Select(b => b.Name.Replace("configurationfiles/config--", "").Replace(".json", "")).ToList();

            this.listConfigurationFiles.Items.Clear();
            foreach (string listItem in blobNames)
            {
                listConfigurationFiles.Items.Add(new ListItem(listItem));
            }
        }
        public (int, List<double[]>, List<int[]>, List<int[]>, double[], double[]) buildVehPathData_LaneStat(int totalLanes, int sampleFreq)
        {
            double[] refPoint = new double[] { 0, 0, 0 };
            int laneStatIdx = 0;
            int wpStatIdx = 0;
            int refPtIdx = 0;
            double wzLen = 0;
            double appHeading = 0;
            int lc = 0;
            var laneStat = new List<int[]>();
            var wpStat = new List<int[]>();
            var pathPt = new List<double[]>();
            laneStat.Insert(laneStatIdx, new int[] { totalLanes, 0, 0, 0 });
            laneStatIdx += 1;
            //GPS Date &Time,# of Sats,HDOP,Latitude,Longitude,Altitude(m),Speed(m/s),Heading(Deg),Marker,Value
            List<string> GPSDateTime = new List<string>();
            List<int> numSats = new List<int>();
            List<double> HDOP = new List<double>();
            List<double> lat = new List<double>();
            List<double> lon = new List<double>();
            List<double> elev = new List<double>();
            List<double> vel = new List<double>();
            List<double> head = new List<double>();
            List<string> marker = new List<string>();
            List<string> value = new List<string>();
            bool gotRefPt = false;
            using (var reader = new StreamReader(Server.MapPath("~/Unzipped Files/data.csv")))
            {
                bool isFirst = true;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (isFirst)
                    {
                        isFirst = false;
                        continue;
                    }
                    GPSDateTime.Add(values[0]);
                    numSats.Add(int.Parse(values[1]));
                    HDOP.Add(double.Parse(values[2]));
                    lat.Add(double.Parse(values[3]));
                    lon.Add(double.Parse(values[4]));
                    elev.Add(double.Parse(values[5]));
                    vel.Add(double.Parse(values[6]));
                    head.Add(double.Parse(values[7]));
                    marker.Add(values[8]);
                    value.Add(values[9]);
                }
            }
            for (int i = 0; i < GPSDateTime.Count(); i++)
            {
                if (!gotRefPt && (marker[i] == "RP" || marker[i] == "LC+RP" || marker[i] == "WP+RP")) //Reference point marker
                {
                    refPoint[0] = lat[i];
                    refPoint[1] = lon[i];
                    refPoint[2] = elev[i];
                    appHeading = head[i];
                    refPtIdx = i;
                    gotRefPt = true;
                }

                if (marker[i] == "LC" || marker[i] == "LC+RP")  //Lane closed marker
                {
                    lc = int.Parse(value[i]);                            //lane number starts from 0
                    laneStat.Insert(laneStatIdx, new int[] { i, lc, 1, (int)wzLen }); //LC location, lane number, status flag, and offset from ref. pt.
                    laneStatIdx += 1;
                }

                if (marker[i] == "LO")  //Lane open marker
                {
                    lc = int.Parse(value[i]);                            //lane number starts from 0
                    laneStat.Insert(laneStatIdx, new int[] { i, lc, 0, (int)wzLen }); //LC location, lane number, status flag, and offset from ref. pt.
                    laneStatIdx += 1;
                }

                if (marker[i] == "WP" || marker[i] == "WP+RP") //Workers present flag
                {
                    int wp = 0;
                    if (value[i].ToUpper() == "TRUE") wp = 1;
                    wpStat.Insert(wpStatIdx, new int[] { i, wp, (int)wzLen }); //LC location, lane number, status flag, and offset from ref. pt.
                    wpStatIdx += 1;
                    //string stat = "Start";
                    //if (value[i].ToUpper() == "FALSE") stat = "End";
                }

                if (marker[i] != "App Ended")
                {
                    pathPt.Insert(i, new double[] {
                        Math.Round(vel[i], 4),
                        Math.Round(lat[i], 8),
                        Math.Round(lon[i], 8),
                        Math.Round(elev[i], 2),
                        Math.Round(head[i], 4)
                    });

                    if (refPtIdx != 0) wzLen += (vel[i] / sampleFreq);
                }
            }
            double[] atRefPoint = new double[]
            {
                refPtIdx, wzLen, appHeading
            };

            return (totalLanes, pathPt, laneStat, wpStat, refPoint, atRefPoint);
        }
        public double getChordLength(double[] point1, double[] point2)
        {
            double lat1 = radians(point1[1]);
            double lon1 = radians(point1[2]);
            double lat2 = radians(point2[1]);
            double lon2 = radians(point2[2]);
            double radius = 6371.0 * 1000;                    //meters

            double d = radius * Math.Acos(Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2) + Math.Sin(lat1) * Math.Sin(lat2));
            //double dlat = radians(lat2 - lat1);          //in radians
            //double dlon = radians(lon2 - lon1);

            //double a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) + Math.Cos(radians(lat1))
            //    * Math.Cos(radians(lat2)) * Math.Sin(dlon / 2) * Math.Sin(dlon / 2);
            //double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //double d = radius * c;
            return d;
        }
        public double[] insertMapPt(List<double[]> pathPt, int index, int tLanes, double laneWidth, int dL, double[] lcwpStat, double distVec)
        {
            double[] lla_ls_hwp = new double[4 * tLanes + 3];
            //LO/LC (0/1), WP (0/1) status added on 11/13/2017

            double bearingPP = pathPt[index][4];
            double latPP = pathPt[index][1];
            double lonPP = pathPt[index][2];
            double altPP = pathPt[index][3];

            for (int ln = 0; ln < tLanes; ln++)
            {
                double lW = Math.Abs(ln - dL) * laneWidth;

                if (ln == dL)
                {
                    lla_ls_hwp[ln * 4 + 0] = latPP;      //lat, lon, alt, lcloStat for the lane
                    lla_ls_hwp[ln * 4 + 1] = lonPP;
                    lla_ls_hwp[ln * 4 + 2] = altPP;
                    lla_ls_hwp[ln * 4 + 3] = lcwpStat[ln];
                }
                else
                {
                    double bearing = bearingPP - 90;
                    if (ln > dL) bearing = bearing + 180;

                    double[] ll = getEndPoint(latPP, lonPP, bearing, lW);   //get lat/lon for the point which is laneWidth apart from the data collected lane
                    lla_ls_hwp[ln * 4 + 0] = Math.Round(ll[0], 8);          //computed lat of the adjacent lane...
                    lla_ls_hwp[ln * 4 + 1] = Math.Round(ll[1], 8);          //computed lon of the adjacent lane   
                    lla_ls_hwp[ln * 4 + 2] = pathPt[index][3];
                    lla_ls_hwp[ln * 4 + 3] = lcwpStat[ln];
                }

                if (ln == tLanes - 1)
                {
                    lla_ls_hwp[ln * 4 + 4] = bearingPP;
                    lla_ls_hwp[ln * 4 + 5] = lcwpStat[tLanes];
                    lla_ls_hwp[ln * 4 + 6] = (int)distVec;
                }
            }

            return lla_ls_hwp.ToArray();
        }
        public (List<double[]>, double[]) getConcisePathPoints(int laneType, List<double[]> pathPt, List<double[]> mapPt, double laneWidth, double lanePad, int refPtIdx, double mapPtDist,
        List<int[]> laneStat, List<int[]> wpStat, int dataLane, double[] wzMapLen)
        {


            //////
            //   Total number of lanes are in loc [0][0] in laneStat array
            //////

            int tLanes = laneStat[0][0];

            int totDataPt = pathPt.Count();
            double[] lcwpStat = new double[tLanes + 1];

            int dL = dataLane - 1;                              //set lane number starting 0 as the left most lane

            double distVec = 0;
            int stopIndex = 0;
            int startIndex = 0;
            double actualError = 0;

            double ALLOWABLEERROR = .5;
            double SMALLDELTAPHI = 0.01;
            int CHORDLENGTHTHRESHOLD = 500;
            int MAXESTIMATEDRADIUS = 8388607; //7FFFFF

            if (laneType == 1)
            {
                if (refPtIdx < 3)
                {
                    for (int j = 0; j < refPtIdx; j++)
                    {
                        mapPt.Add(insertMapPt(pathPt, j, tLanes, laneWidth, dL, lcwpStat, distVec));
                        distVec += pathPt[j][0] / 10;
                        // Rework to use actualChordLength
                        wzMapLen[0] = distVec;
                        return (mapPt, wzMapLen);
                    }
                }
                else
                {
                    stopIndex = refPtIdx;
                };
            }
            else
            {
                stopIndex = totDataPt;
                startIndex = refPtIdx;
            }

            // Step 1
            int i = startIndex + 2;
            double[] Pstarting = pathPt[i - 2];
            double[] Pprevious = pathPt[i - 1];
            double[] Pnext = pathPt[i];
            double totalDist = 0;
            double incrementDist = 0;
            mapPt.Add(insertMapPt(pathPt, i - 2, tLanes, laneWidth, dL, lcwpStat, distVec));

            bool requiredNode = false;
            while (i < stopIndex)
            {
                // Step A
                requiredNode = false; //Reset gotNodeLoc
                if (laneType == 2) // WZ region (not approach region)
                {
                    for (int lnStat = 1; lnStat < laneStat.Count(); lnStat++) //total number of lc/lo/wp are length of laneStat-1
                    {
                        if (laneStat[lnStat][0] == i - 1) //Lane closure start/end found, save point and add to node list
                        {
                            requiredNode = true;
                            lcwpStat[laneStat[lnStat][1] - 1] = laneStat[lnStat][2];
                        }
                    }
                    for (int wpZone = 0; wpZone < wpStat.Count(); wpZone++)
                    {
                        if (wpStat[wpZone][0] == i - 1) //Workers present start/end found, save point and add to node list
                        {
                            requiredNode = true;
                            lcwpStat[lcwpStat.Count() - 1] = wpStat[wpZone][1];
                        }
                    }
                };
                bool eval = true;
                // Step 2
                double actualChordLength = getChordLength(Pstarting, Pnext);
                if (actualChordLength > CHORDLENGTHTHRESHOLD)
                {
                    actualError = ALLOWABLEERROR + 1;
                    eval = false;
                    // Go to step 7
                }

                // Step 3
                double deltaHeadings = Math.Abs(Pnext[4] - Pstarting[4]);
                if (deltaHeadings > 180) deltaHeadings = 360 - deltaHeadings;
                deltaHeadings = Math.Abs(radians(deltaHeadings));
                //deltaHeadings = deltaHeadings % 360;
                //if (deltaHeadings < 0) deltaHeadings += 360;

                // Step 4
                double estimatedRadius = 0;
                if (deltaHeadings < SMALLDELTAPHI && eval)
                {
                    actualError = 0;
                    estimatedRadius = MAXESTIMATEDRADIUS;
                    eval = false;
                    // Go to step 8
                }
                else if (eval)
                {
                    estimatedRadius = actualChordLength / (2 * Math.Sin(deltaHeadings / 2));
                }

                // Step 5
                double d = estimatedRadius * Math.Cos(deltaHeadings / 2);

                // Step 6
                if (eval) //Allow step 4 to maintain 0 actualError
                {
                    actualError = estimatedRadius - d;
                }

                // Step 7
                if (actualError > ALLOWABLEERROR || requiredNode)
                {
                    incrementDist = actualChordLength;
                    totalDist += incrementDist;
                    mapPt.Add(insertMapPt(pathPt, i - 1, tLanes, laneWidth, dL, lcwpStat, totalDist));

                    Pstarting = pathPt[i - 1];
                    Pprevious = pathPt[i];
                    if (i != stopIndex - 1) Pnext = pathPt[i + 1];
                    i += 1;
                }
                // Step 8
                else
                {
                    if (i != stopIndex - 1) Pnext = pathPt[i + 1];
                    Pprevious = pathPt[i];
                    i += 1;
                }

                if (i == stopIndex)
                {
                    incrementDist = actualChordLength;
                    totalDist += incrementDist;
                    mapPt.Add(insertMapPt(pathPt, i - 1, tLanes, laneWidth, dL, lcwpStat, totalDist));
                }
                // Step 9
                // Integrated into step 7
            }
            if (laneType == 1) wzMapLen[0] = totalDist;
            else wzMapLen[1] = totalDist;
            return (mapPt, wzMapLen);
        }

        public (List<double[]>, double[]) getLanePt(int laneType, List<double[]> pathPt, List<double[]> mapPt, double laneWidth, double lanePad, int refPtIdx, double mapPtDist,
            List<int[]> laneStat, List<int[]> wpStat, int dataLane, double[] wzMapLen)
        {
            double radMult = 3.14159265 / 180.0;                          //to radian    
            int dL = dataLane - 1;                              //set lane number starting 0 as the left most lane                             
            int refPt = refPtIdx;                                  //starting from the ref. point to the first data point for WZ lanes
            double distTh = 512.0;                                     //max dist between waypoints (node points)
            double dist = 0;                                         //cumulative dist bet data points   
            int Kt = 0;
            double distVecTot = 0;

            double[] originLL = new double[] {
                pathPt[refPt][1],
                pathPt[refPt][2]
            };

            int totDataPt = pathPt.Count();
            int incr = 1;
            if (laneType == 1) //Approach region, decrement refPt until it hits 0
            {
                incr = -1;
                if (refPt != 0) refPt += incr;
            }

            int tLanes = laneStat[0][0];

            double[] lla_ls_hwp = new double[4 * tLanes + 3];
            double[] lcwpStat = new double[tLanes + 1];

            double bearingRP = pathPt[refPt][4];
            double latRP = pathPt[refPt][1];
            double lonRP = pathPt[refPt][2];
            double altRP = pathPt[refPt][3];
            double ctrHead = pathPt[refPt][4];

            int lookAhead = 8;
            while (refPt >= 0 && refPt < totDataPt)
            {
                if (refPt != refPtIdx) dist += (pathPt[refPt][0] / 10);
                double headDiff = Math.Abs(ctrHead - pathPt[refPt][4]);

                if (headDiff > 180) headDiff -= 360;

                double ht = dist * Math.Tan(headDiff * radMult);

                double[] destLL = new double[]
                {
                    pathPt[refPt][1],
                    pathPt[refPt][2]
                };
                double distVec = getDist(originLL, destLL);
                double htNew = distVec * Math.Tan(headDiff * radMult);

                bool gotHt = false;
                int htKt = 0;

                if ((htNew > (laneWidth / 2.0) + lanePad) && (refPt < totDataPt - lookAhead) && (refPt > lookAhead))
                {
                    double distVecNew = distVec;
                    for (int jk = 1; jk < lookAhead + 1; jk++) // 
                    {
                        double[] destLLNew = new double[]
                        {
                            pathPt[refPt + (incr * jk)][1],
                            pathPt[refPt + (incr * jk)][2]
                        };
                        distVecNew = getDist(originLL, destLLNew);                              //computed distance vector    


                        headDiff = Math.Abs(ctrHead - pathPt[refPt + (incr * jk)][4]);


                        if (headDiff > 180) headDiff -= 360;

                        double htNext = distVecNew * Math.Tan(headDiff * radMult);  //compute for next data point, "ht" for rt. angle triangle from angle 
                                                                                    //bet. adj. side & hypotenuse and dist = adj. side (base of the triangle)
                        if (htNext > (laneWidth / 2.0) + lanePad) htKt++;


                    }
                    if (htKt == lookAhead) //if all lookAhead points height is > half lane width+lane pad
                    {
                        gotHt = true;                                               //Node is important! (ignoring would cause too much error)
                        refPt -= incr;
                    }
                }
                bool gotNodeLoc = false; //Reset gotNodeLoc
                if (laneType == 2) // WZ region (not approach region)
                {
                    for (int lnStat = 1; lnStat < laneStat.Count(); lnStat++) //total number of lc/lo/wp are length of laneStat-1
                    {
                        if (laneStat[lnStat][0] == refPt) //Lane closure start/end found, save point and add to node list
                        {
                            gotNodeLoc = true;
                            lcwpStat[laneStat[lnStat][1] - 1] = laneStat[lnStat][2];
                        }
                    }
                    for (int wpZone = 0; wpZone < wpStat.Count(); wpZone++)
                    {
                        if (wpStat[wpZone][0] == refPt) //Workers present start/end found, save point and add to node list
                        {
                            gotNodeLoc = true;
                            lcwpStat[lcwpStat.Count() - 1] = wpStat[wpZone][1];
                        }
                    }
                }

                if (refPt == totDataPt - 1 || refPt == 1) gotNodeLoc = true;

                if (gotHt || dist > distTh || Kt == 0 || gotNodeLoc)                //If first point (Kt==0), Important node (gotNodeLoc, LC, LO, WP), 
                                                                                    //distance from previous node is too high (dist > distTh), or Node is important to maintaining path information/minimizing error (gotHt)
                {
                    double bearingPP = pathPt[refPt][4];
                    double latPP = pathPt[refPt][1];
                    double lonPP = pathPt[refPt][2];
                    double altPP = pathPt[refPt][3];

                    for (int ln = 0; ln < tLanes; ln++)
                    {
                        double lW = Math.Abs(ln - dL) * laneWidth;

                        if (ln == dL)
                        {
                            lla_ls_hwp[ln * 4 + 0] = latPP;      //lat, lon, alt, lcloStat for the lane
                            lla_ls_hwp[ln * 4 + 1] = lonPP;
                            lla_ls_hwp[ln * 4 + 2] = altPP;
                            lla_ls_hwp[ln * 4 + 3] = lcwpStat[ln];
                        }
                        else
                        {
                            double bearing = bearingPP - 90;
                            if (ln > dL) bearing = bearingPP + 180;

                            if (laneType == 1 || laneType == 2)
                            {
                                double[] ll = getEndPoint(latPP, lonPP, bearing, lW);   //get lat/lon for the point which is laneWidth apart from the data collected lane
                                lla_ls_hwp[ln * 4 + 0] = Math.Round(ll[0], 8);          //computed lat of the adjacent lane...
                                lla_ls_hwp[ln * 4 + 1] = Math.Round(ll[1], 8);          //computed lon of the adjacent lane   
                                lla_ls_hwp[ln * 4 + 2] = pathPt[refPt][3];
                                lla_ls_hwp[ln * 4 + 3] = lcwpStat[ln];
                            }
                        }

                        if (ln == tLanes - 1)
                        {
                            lla_ls_hwp[ln * 4 + 4] = bearingPP;
                            lla_ls_hwp[ln * 4 + 5] = lcwpStat[tLanes];
                            lla_ls_hwp[ln * 4 + 6] = (int)distVec;
                        }
                    }

                    mapPt.Insert(Kt, lla_ls_hwp.ToArray());

                    distVecTot += distVec;
                    dist = 0;
                    originLL = new double[]
                    {
                            pathPt[refPt][1],
                            pathPt[refPt][2]
                    };
                    ctrHead = pathPt[refPt][4];
                    Kt++;
                }

                refPt += incr;

                if (laneType == 1) wzMapLen[0] = distVecTot;
                else wzMapLen[1] = distVecTot;
            }
            return (mapPt, wzMapLen);
        }

        public double[] getEndPoint(double lat1, double lon1, double bearing, double d)
        {
            double R = 6371.0 * 1000;              //Radius of the Earth in meters
            double brng = radians(bearing); //convert degrees to radians
            double dist = d;                     //convert distance in meters
            lat1 = radians(lat1);    //Current lat point converted to radians
            lon1 = radians(lon1);    //Current long point converted to radians
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / R) + Math.Cos(lat1) * Math.Sin(d / R) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d / R) * Math.Cos(lat1),
                Math.Cos(d / R) - Math.Sin(lat1) * Math.Sin(lat2));
            lat2 = degrees(lat2);
            lon2 = degrees(lon2);
            return new double[] { lat2, lon2 };
        }

        public double getDist(double[] origin, double[] destination)
        {
            double lat1 = origin[0];
            double lon1 = origin[1];
            double lat2 = destination[0];
            double lon2 = destination[1];
            double radius = 6371.0 * 1000;                    //meters

            double dlat = radians(lat2 - lat1);          //in radians
            double dlon = radians(lon2 - lon1);

            double a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) + Math.Cos(radians(lat1))
                * Math.Cos(radians(lat2)) * Math.Sin(dlon / 2) * Math.Sin(dlon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = radius * c;

            return d;
        }

        public double radians(double angle)
        {
            return angle * 3.14159265 / 180.0;
        }

        public double degrees(double angle)
        {
            return angle * 180.0 / 3.14159265;
        }

        public List<int[]> buildMsgSegNodeList(int appNodesPerLane, int wzNodesPerLane, int totLanes)
        {
            List<int[]> nplList = new List<int[]>();
            int msgSizeOct = 1100;
            int nodeSizeOct = 11;
            int maxNodesPerMsg = msgSizeOct / nodeSizeOct;
            int maxNodesPerLane = maxNodesPerMsg / totLanes;

            int totMsgSeg = -1;
            if (appNodesPerLane > maxNodesPerLane)
            {
                nplList.Insert(0, new int[] { totMsgSeg, maxNodesPerLane });
                nplList.Insert(1, new int[] { 1, 1, appNodesPerLane });

                return nplList;
            }

            if (maxNodesPerLane == appNodesPerLane)
            {
                maxNodesPerLane += 2;
                maxNodesPerMsg += totLanes * 2;
            }

            int totNodes = (appNodesPerLane + wzNodesPerLane) * totLanes;
            totMsgSeg = (int)Math.Ceiling((double)totNodes / (double)maxNodesPerMsg);

            nplList.Insert(0, new int[] { totMsgSeg, maxNodesPerLane });
            nplList.Insert(1, new int[] { 1, 1, appNodesPerLane });

            int wzNodesPerMsgSeg = (int)(maxNodesPerLane - appNodesPerLane);
            int wzNodesRemain = wzNodesPerLane - wzNodesPerMsgSeg;

            if (wzNodesRemain <= 0) wzNodesPerMsgSeg = wzNodesPerLane;

            nplList.Insert(2, new int[] { 1, 1, wzNodesPerMsgSeg });

            int wzStartNode = wzNodesPerMsgSeg;

            int wzEndNode;

            if (wzNodesRemain < maxNodesPerLane) wzEndNode = wzStartNode + wzNodesRemain - 1;
            else wzEndNode = wzStartNode + maxNodesPerLane - 1;

            for (int idx = 3; idx <= totMsgSeg + 1; idx++)
            {
                if (wzNodesRemain > maxNodesPerLane)
                {
                    nplList.Insert(idx, new int[] { idx - 1, wzStartNode, wzEndNode });
                    wzStartNode += maxNodesPerLane - 1;
                    wzEndNode = wzStartNode + maxNodesPerLane - 1;
                    wzNodesRemain = wzNodesRemain - maxNodesPerLane + 1;
                }
                else
                {
                    wzEndNode = wzStartNode + wzNodesRemain;
                    nplList.Insert(idx, new int[] { idx - 1, wzStartNode, wzEndNode });
                }
            }
            return nplList;
        }

        public void createVisualizer()
        {
            var wzConfig = JsonConvert.DeserializeObject<configurationObject>(File.ReadAllText(Server.MapPath("~/Unzipped Files/config.json")));

            int sampleFreq = 10;

            string wzDesc = wzConfig.GeneralInfo.Description;

            int totalLanes = wzConfig.LaneInfo.NumberOfLanes;
            double laneWidth = wzConfig.LaneInfo.AverageLaneWidth;
            double lanePadApp = wzConfig.LaneInfo.ApproachLanePadding;
            double lanePadWZ = wzConfig.LaneInfo.WorkzoneLanePadding;
            int dataLane = wzConfig.LaneInfo.VehiclePathDataLane;

            int[] speedList = new int[]
            {
                wzConfig.SpeedLimits.NormalSpeed,
                wzConfig.SpeedLimits.ReferencePointSpeed,
                wzConfig.SpeedLimits.WorkersPresentSpeed
            };

            int[] c_sc_codes = new int[]
            {
                wzConfig.CauseCodes.CauseCode,
                wzConfig.CauseCodes.SubCauseCode
            };

            //string wzStartDate = wzConfig.Schedule.StartDate.ToString();
            //string wzStartTime = wzConfig.Schedule.EndDate.ToString();
            //List<string> wzDaysOfWeek = wzConfig.SCHEDULE.WZDaysOfWeek;

            string cDT = DateTime.Now.ToString("MM/dd/yyyy - hh:mm:ss");
            //string cDT = "04/14/2020 - 15:30:41";

            double[] atRefPoint = new double[3];
            (int totalLanesOut, List<double[]> pathPt, List<int[]> laneStat, List<int[]> wpStat,
                double[] refPoint, double[] atRefPointOut) = buildVehPathData_LaneStat(totalLanes, sampleFreq);
            totalLanes = totalLanesOut;
            atRefPoint = atRefPointOut;

            int refPtIdx = (int)atRefPoint[0];
            int wzLen = (int)atRefPoint[1];
            double appHeading = atRefPoint[2];

            refPoint[0] = Math.Round(refPoint[0], 8);
            refPoint[1] = Math.Round(refPoint[1], 8);
            refPoint[2] = Math.Round(refPoint[2], 8);

            double[] wzMapLen = new double[2];
            int appMapPtDist = 50;
            int wzMapPtDist = 200;
            List<double[]> appMapPt = new List<double[]>();
            int laneType = 1; //Approach Region
            (List<double[]> appMapPtOut, double[] wzMapLenOut) = getConcisePathPoints(laneType, pathPt, appMapPt, laneWidth, lanePadApp, refPtIdx, appMapPtDist, laneStat, wpStat, dataLane, wzMapLen);
            //(List<double[]> appMapPtOut, double[] wzMapLenOut) = getLanePt(laneType, pathPt, appMapPt, laneWidth, lanePadApp, refPtIdx, appMapPtDist, laneStat, wpStat, dataLane, wzMapLen);
            appMapPt = appMapPtOut;
            wzMapLen = wzMapLenOut;

            List<double[]> wzMapPt = new List<double[]>();
            laneType = 2;
            (List<double[]> wzMapPtOut, double[] wzMapLenOut_2) = getConcisePathPoints(laneType, pathPt, wzMapPt, laneWidth, lanePadWZ, refPtIdx, wzMapPtDist, laneStat, wpStat, dataLane, wzMapLen);
            //(List<double[]> wzMapPtOut, double[] wzMapLenOut_2) = getLanePt(laneType, pathPt, wzMapPt, laneWidth, lanePadWZ, refPtIdx, wzMapPtDist, laneStat, wpStat, dataLane, wzMapLen);
            wzMapPt = wzMapPtOut;
            wzMapLen = wzMapLenOut_2;

            List<int[]> msgSegList = buildMsgSegNodeList(appMapPt.Count(), wzMapPt.Count(), totalLanes);

            using (StreamWriter jsfile = new StreamWriter(Server.MapPath("~/Map Visualizer/RSZW_Map_Data.js")))
            {
                build_jsvars(jsfile, "//\n//        --- CAMP .js file for RSZW/LC Mapping Project --- ");
                build_jsvars(jsfile, "//        --- Data points overlay on Google Map --- ");
                build_jsvars(jsfile, "//        --- File Created: " + cDT + " ---\n//");
                build_jsvars(jsfile, "//\n//   Work zone description... ");
                build_jsvars(jsfile, "//   Mapped work zone length in meters (approach lane, wz lane)... ");
                build_jsvars(jsfile, "//   Reference Point coordinates... ");
                build_jsvars(jsfile, "//   no of lanes mapped for WZ...");
                build_jsvars(jsfile, "//   Vehicle path data lane...");
                build_jsvars(jsfile, "//   wz lane status...");
                build_jsvars(jsfile, "//   wz lane width, lane padding for approach and WZ lanes...\n//");

                build_jsvars(jsfile, "    var wzDesc = \"" + wzDesc + "\";");                              //wz description
                build_jsvars(jsfile, "    var wzLength = [" + ((int)wzMapLen[0]).ToString() + "," + ((int)wzMapLen[1]).ToString() + "];"); //WZ length info
                build_jsvars(jsfile, "    var wzMapDate = \"" + cDT + "\";");                              //map created date and time
                //    build_jsvars (jsfile,"    var refPoint = ["str(refPoint[0])+","+str(refPoint[1])+","+str(refPoint[2])+"];")  //reference point
                build_jsvars(jsfile, "    var refPoint = [" + refPtIdx.ToString() + "," + refPoint[0].ToString() + "," + refPoint[1].ToString() + "," + refPoint[2].ToString() + "];");  //reference point
                build_jsvars(jsfile, "    var noLanes = " + laneStat[0][0].ToString() + ";");                    //no of lanes
                build_jsvars(jsfile, "    var mappedLaneNo = " + dataLane.ToString() + ";");                     //data colleted lane
                build_jsvars(jsfile, "    var laneStat = " + list2str(laneStat) + ";");                         //lane status
                build_jsvars(jsfile, "    var laneWidth = " + laneWidth.ToString() + ";");                       //lane width
                build_jsvars(jsfile, "    var lanePadApp = " + lanePadApp.ToString() + ";");                     //lane padding for approach lanes
                build_jsvars(jsfile, "    var lanePadWZ = " + lanePadWZ.ToString() + ";");                       //lane padding for wz lanes
                build_jsvars(jsfile, "    var wpStat = " + list2str(wpStat) + ";");                             //workers present status
                build_jsvars(jsfile, "    var msgSegments = " + msgSegList[0][0].ToString() + ";");              //number of message segments
                build_jsvars(jsfile, "    var nodesPerSegment = " + msgSegList[0][1].ToString() + ";");          //number of nodes per segment per lane

                //////
                ////   Following builds list of nodes in message segments...
                //////

                build_jsvars(jsfile, "//\n//   List of start and end nodes for approach and wx lane nodes per message segment");
                build_jsvars(jsfile, "//   Approach lane nodes are in segment #1 (always...)\n//");
                build_jsvars(jsfile, "    var appNodesMsgSegment = " + "[" + string.Join(", ", msgSegList[1]) + "]" + ";");
                build_jsarray(jsfile, msgSegList, "//\n//  Message segmentation list... \n//\n    var msgSegList = [");
                build_jsarray(jsfile, pathPt, "//\n//  Collected vehicle data points for WZ mapping... \n" +
                "//   Veh speed, lat, lon, alt, heading \n" +
                   "//\n    var mapData = [");

                ////
                //   Write the constructed approach lane data points array...
                ////

                build_jsarray(jsfile, appMapPt, "//\n//   Approach Lanes --- data points... \n//\n" +
                               "//   The list has lat,lon,alt,lcloStat for each node for each lane +\n" +
                               "//   heading + WP flag + distVec (dist from previous node) \n" +
                               "//\n    var appMapData = [");

                ////
                //   Write the constructed work zone lane data points array...
                ////

                build_jsarray(jsfile, wzMapPt, "//\n//   Work Zone Lanes --- data points... \n//\n" +
                               "//   The list has lat,lon,alt,lcloStat for each node for each lane +\n" +
                               "//   heading + WP flag + distVec (dist from prev. node) \n" +
                               "//\n    var wzMapData = [");
            }
        }

        public string list2str(List<int[]> list)
        {
            var strings = new List<string>();
            for (int i = 0; i < list.Count(); i++) strings.Add("[" + string.Join(", ", list[i]) + "]");
            return "[" + string.Join(", ", strings) + "]";
        }

        public void build_jsvars(StreamWriter jsfile, string discStr)
        {
            jsfile.WriteLine(discStr);
        }

        public void build_jsarray(StreamWriter jsfile, List<double[]> array, string discStr)
        {
            string endStr = "";
            string eolStr = ",";
            string eoaStr = "];\n";
            int totDataPt = array.Count();
            endStr = eolStr;
            jsfile.WriteLine(discStr);

            for (int Kt = 0; Kt < totDataPt; Kt++)
            {
                if (Kt == totDataPt - 1) endStr = eoaStr;
                jsfile.WriteLine("[" + string.Join(", ", array[Kt]) + "]" + endStr);
            }
        }

        public void build_jsarray(StreamWriter jsfile, List<int[]> array, string discStr)
        {
            string endStr = "";
            string eolStr = ",";
            string eoaStr = "];\n";
            int totDataPt = array.Count();
            endStr = eolStr;
            jsfile.WriteLine(discStr);

            for (int Kt = 0; Kt < totDataPt; Kt++)
            {
                if (Kt == totDataPt - 1) endStr = eoaStr;
                jsfile.WriteLine("[" + string.Join(", ", array[Kt]) + "]" + endStr);
            }
        }
    }
}