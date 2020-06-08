using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text.RegularExpressions;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.InteropServices;
using System.Data;
using System.ComponentModel;
using System.Net.Mail;

namespace Neaera_Website_2018
{

    public partial class V2X_ConfigCreator : System.Web.UI.Page
    {
        int normalspeedmax = 100;
        int atrefpointmax = 100;
        int whenworkerspresentmax = 100;
        int numberoflanes = 8;
        configurationObject jsonConfig;
        string strdownloadfilepath = "";
        string strRequiredfield = "";
        string Typeofconfig = "";
        private ClientScriptManager clientScript;

        protected void Page_Load(object sender, EventArgs e)
        {
            clientScript = this.Page.ClientScript;
            

            if (!IsPostBack)
            {
                fill_dropdowns();
                initializefields();
            }
            initcheckboxes();
        }
        public void initcheckboxes()
        {
            for (int i = 0; i < this.chEventStatus.Items.Count; i++)
            {
                chEventStatus.Items[i].Attributes.Add("onclick", "MutExChkList(this)");
            }
            for (int i = 0; i < this.chDirection.Items.Count; i++)
            {
                chDirection.Items[i].Attributes.Add("onclick", "MutExChkList(this)");
            }
            for (int i = 0; i < this.chkBeginningAccuracy.Items.Count; i++)
            {
                chkBeginningAccuracy.Items[i].Attributes.Add("onclick", "MutExChkList(this)");
            }
            for (int i = 0; i < this.chkEndingAccuracy.Items.Count; i++)
            {
                chkEndingAccuracy.Items[i].Attributes.Add("onclick", "MutExChkList(this)");
            }
            for (int i = 0; i < this.chkStartDateAccuracy.Items.Count; i++)
            {
                chkStartDateAccuracy.Items[i].Attributes.Add("onclick", "MutExChkList(this)");
            }
            for (int i = 0; i < this.chkEndDateAccuracy.Items.Count; i++)
            {
                chkEndDateAccuracy.Items[i].Attributes.Add("onclick", "MutExChkList(this)");
            }
        }

        public void initializefields()
        {
            //Clear all fields
            txt_workzonedescription.Text = "WZ Description";
            dd_numberoflanes.SelectedIndex = -1;

            AvgLaneWidth.Text = "3.6";
            AppLanePadding.Text = "0.0";
            WorkZoneLanePadding.Text = "0.0";

            dd_normalspeed.SelectedValue = "30";
            dd_normalspeed.Items.FindByValue("30").Selected = true;

            dd_AtreferencePoint.SelectedValue = "20";
            dd_WhenWorkersArePresent.SelectedValue = "10";
            txtCauseCode.Text = "3";
            txtsubcausecode.Text = "0";

            chkDaysOfWeek.SelectedIndex = -1;
            this.Calendar_BeginDate.SelectedDate = System.DateTime.Now;
            TimeBegin.Value = "00:00:00";
            this.Calendar_BeginDate.SelectedDate = System.DateTime.Today;

            this.Calendar_enddate.SelectedDate = System.DateTime.Today.AddDays(7);

            TimeEnd.Value = "23:59:00";
            chkDaysOfWeek.SelectedValue = "Mon";

            start_lat_hidden.Text = "0";
            start_lng_hidden.Text = "0";
            end_lat_hidden.Text = "0";
            end_lng_hidden.Text = "0";

            this.txtRoadName.Value = "road name";
            this.btnDownloadFile.Enabled = false;
            this.btnPublishFile.Enabled = false;
            this.txtFilepath_configSave.Text = "config--wz-description--road-name.json";


            /******* New Fields ******/
            this.txtIssuingOrganization.Text = "";
            this.txtBeginningCrossStreet.Text = "";
            this.txtEndingCrossStreet.Text = "";
            this.txtBeginMilePost.Text = "";
            this.txtEndMilePost.Text = "";
           // gridTypesOfWork.DataSource = CreateGridData_TypeOfWork();
           // gridTypesOfWork.DataBind();
        }
        public void clearfields()
        {
            txt_workzonedescription.Text = "";
            dd_numberoflanes.SelectedIndex = -1;

            AvgLaneWidth.Text = "0.0";
            AppLanePadding.Text = "0.0";
            WorkZoneLanePadding.Text = "0.0";


            dd_normalspeed.SelectedValue = "0";
            dd_normalspeed.Items.FindByValue("0").Selected = true;

            dd_AtreferencePoint.SelectedValue = "0";
            dd_WhenWorkersArePresent.SelectedValue = "0";
            txtCauseCode.Text = "0";
            txtsubcausecode.Text = "0";

            chkDaysOfWeek.SelectedIndex = -1;
            this.Calendar_BeginDate.SelectedDate = System.DateTime.Now;
            TimeBegin.Value = "00:00:00";
            //datepikedTimeBegin.Value = "";
            this.Calendar_enddate.SelectedDate = System.DateTime.Now.AddDays(7);
            TimeEnd.Value = "23:59:00";

            chkDaysOfWeek.ClearSelection();
            txtRoadName.Value = "";
            txtRoadNumber.Value = "";

            /******* New Fields ******/
            this.txtIssuingOrganization.Text = "";
            this.txtBeginningCrossStreet.Text = "";
            this.txtEndingCrossStreet.Text = "";
            this.txtBeginMilePost.Text = "";
            this.txtEndMilePost.Text = "";

            this.chEventStatus.ClearSelection();
            this.chDirection.ClearSelection();
            this.chkBeginningAccuracy.ClearSelection();
            this.chkEndingAccuracy.ClearSelection();
            this.chkStartDateAccuracy.ClearSelection();
            this.chkEndDateAccuracy.ClearSelection();

            this.txtIrsType.Text = "";
            this.txtLocationVerifyMethod.Text = "";
            this.txtDataFeedFrequencyMethod.Text = "";

        }
        public void fillConfigurationFiles()
        {

            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // In Progress COnfiguration Files
            CloudBlobContainer container = blobClient.GetContainerReference("inprogressconfigfiles");

            // Convert and sort blobs in conrainer
            var IBlobList = container.ListBlobs(useFlatBlobListing: true);
            List<CloudBlockBlob> blobList = new List<CloudBlockBlob>();
            foreach (IListBlobItem blob in IBlobList)
            {
                blobList.Add((CloudBlockBlob)blob);
            }
            blobList = blobList.OrderByDescending(b => b.Properties.LastModified).ToList();

            // Write blob names to ListBox
            this.listConfigurationFiles.Items.Clear();
            foreach (CloudBlockBlob listItem in blobList.Distinct().ToList())
            {
                listConfigurationFiles.Items.Add(new ListItem(listItem.Name.Split('/')[0]));
            }


            //
            // Published Configuration Files
            //
            CloudBlobContainer container_published = blobClient.GetContainerReference("publishedconfigfiles");

            // Convert and sort blobs in conrainer
            IBlobList = container_published.ListBlobs(useFlatBlobListing: true);
            List<CloudBlockBlob> blobListPublished = new List<CloudBlockBlob>();
            foreach (IListBlobItem blob in IBlobList)
            {
                blobListPublished.Add((CloudBlockBlob)blob);
            }
            blobListPublished = blobListPublished.OrderByDescending(b => b.Properties.LastModified).ToList();

            // Write blob names to ListBox
            this.listPublishedConfigurationFiles.Items.Clear();
            foreach (CloudBlockBlob listItempub in blobListPublished.Distinct().ToList())
            {
                listPublishedConfigurationFiles.Items.Add(new ListItem(listItempub.Name.Split('/')[0]));
            }
        }
        public void fill_dropdowns()
        {
            fill_ddnormalspeeds();
            fill_atreferencepoint();
            fill_whenworkerspresent();
            fill_numberoflanes();
            fillConfigurationFiles();
            fill_typesofwork();
            fill_restrictiontype();
            fill_restrictionunits();
            fill_lanetypes();
        }
        public void fill_lanetypes()
        {
            this.ddLaneTypes.Items.Clear();
            this.ddLaneTypes.Items.Add("None");
            //foreach (LANETYPES r in Enum.GetValues(typeof(LANETYPES)))
            //{
            //    ListItem item = new ListItem(Enum.GetName(typeof(LANETYPES), r), r.ToString());
            //    ddLaneTypes.Items.Add(item);
            //}
            foreach (var r in typeof(LANETYPES).GetFields())
            {
                if (r.Name == "value__") continue;
                var attribute = Attribute.GetCustomAttribute(r, typeof(DescriptionAttribute)) as DescriptionAttribute;
                ListItem item = new ListItem(attribute != null ? attribute.Description : r.Name, r.Name);
                ddLaneTypes.Items.Add(item);
            }
        }
        public void fill_typesofwork()
        {
            this.ddTypeOfWork.Items.Clear();
            this.ddTypeOfWork.Items.Add("None");
            //foreach (WORKTYPE r in Enum.GetValues(typeof(WORKTYPE)))
            //{
            //    ListItem item = new ListItem(Enum.GetName(typeof(WORKTYPE), r), r.ToString());
            //    ddTypeOfWork.Items.Add(item);
            //}
            foreach (var r in typeof(WORKTYPE).GetFields())
            {
                if (r.Name == "value__") continue;
                var attribute = Attribute.GetCustomAttribute(r, typeof(DescriptionAttribute)) as DescriptionAttribute;
                ListItem item = new ListItem(attribute != null ? attribute.Description : r.Name, r.Name);
                ddTypeOfWork.Items.Add(item);
            }
        }
        public void fill_restrictiontype()
        {
            this.ddRestrictionTypes.Items.Clear();
            this.ddRestrictionTypes.Items.Add("None");
            //foreach (RESTRICTIONTYPE r in Enum.GetValues(typeof(RESTRICTIONTYPE)))
            //{
            //    ListItem item = new ListItem(Enum.GetName(typeof(RESTRICTIONTYPE), r), r.ToString());
            //    ddRestrictionTypes.Items.Add(item);
            //}
            foreach (var r in typeof(RESTRICTIONTYPE).GetFields())
            {
                if (r.Name == "value__") continue;
                var attribute = Attribute.GetCustomAttribute(r, typeof(DescriptionAttribute)) as DescriptionAttribute;
                ListItem item = new ListItem(attribute != null ? attribute.Description : r.Name, r.Name);
                ddRestrictionTypes.Items.Add(item);
            }
        }
        public void fill_restrictionunits()
        {
            this.ddRestrictionUnits.Items.Clear();
            this.ddRestrictionUnits.Items.Add("None");
            //foreach (RESTRICTIONUNITS r in Enum.GetValues(typeof(RESTRICTIONUNITS)))
            //{
            //    ListItem item = new ListItem(Enum.GetName(typeof(RESTRICTIONUNITS), r), r.ToString());
            //    ddRestrictionUnits.Items.Add(item);
            //}
            foreach (var r in typeof(RESTRICTIONUNITS).GetFields())
            {
                if (r.Name == "value__") continue;
                var attribute = Attribute.GetCustomAttribute(r, typeof(DescriptionAttribute)) as DescriptionAttribute;
                ListItem item = new ListItem(attribute != null ? attribute.Description : r.Name, r.Name);
                ddRestrictionUnits.Items.Add(item);
            }
        }
        public void fill_ddnormalspeeds()
        {
            //Fill normal speed dropdown 
            this.dd_normalspeed.Items.Clear();
            dd_normalspeed.Items.Add(string.Empty);
            for (int i = 0; i <= normalspeedmax; i+=5)
            {
                dd_normalspeed.Items.Add(i.ToString());
            }
        }
        public void fill_atreferencepoint()
        {
            //Fill at reference point dropdown 
            this.dd_AtreferencePoint.Items.Clear();
            dd_AtreferencePoint.Items.Add(string.Empty);
            for (int i = 0; i <= atrefpointmax; i+=5)
            {
                dd_AtreferencePoint.Items.Add(i.ToString());
            }
        }
        public void fill_whenworkerspresent()
        {
            //Fill when workers are presant dropdown 
            this.dd_WhenWorkersArePresent.Items.Clear();
            dd_WhenWorkersArePresent.Items.Add(string.Empty);
            for (int i = 0; i <= whenworkerspresentmax; i+=5)
            {
                dd_WhenWorkersArePresent.Items.Add(i.ToString());
            }
        }

        public void fill_numberoflanes()
        {
            //Fillnumber of lanes and vehicle path data lane dropdowns 
            this.dd_numberoflanes.Items.Clear();
            this.dd_avgvehicledatapath.Items.Clear();
            ListItem listdditems;
            for (int i = 1; i <= numberoflanes; i++)
            {
                listdditems = new ListItem(i.ToString(), i.ToString());
                dd_avgvehicledatapath.Items.Add(listdditems);
                dd_numberoflanes.Items.Add(listdditems);
            }
            dd_avgvehicledatapath.DataBind();
            dd_numberoflanes.DataBind();
            //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "initLaneType();", true);
        }
        public void fillobject()
        {
            //Load imported config file
            try
            {
                txt_workzonedescription.Text = jsonConfig.GeneralInfo.Description;
                dd_numberoflanes.ClearSelection(); //Clear all values before loading
                dd_numberoflanes.Items.FindByText(jsonConfig.LaneInfo.NumberOfLanes.ToString()).Selected = true;
                dd_avgvehicledatapath.ClearSelection();
                dd_avgvehicledatapath.Items.FindByText(jsonConfig.LaneInfo.VehiclePathDataLane.ToString()).Selected = true;
                AvgLaneWidth.Text = jsonConfig.LaneInfo.AverageLaneWidth.ToString();
                AppLanePadding.Text = jsonConfig.LaneInfo.ApproachLanePadding.ToString();
                WorkZoneLanePadding.Text = jsonConfig.LaneInfo.WorkzoneLanePadding.ToString();

                dd_normalspeed.ClearSelection();
                dd_normalspeed.Items.FindByText(jsonConfig.SpeedLimits.NormalSpeed.ToString()).Selected = true;
                dd_AtreferencePoint.ClearSelection();
                dd_AtreferencePoint.Items.FindByText(jsonConfig.SpeedLimits.ReferencePointSpeed.ToString()).Selected = true;

                dd_WhenWorkersArePresent.ClearSelection();
                dd_WhenWorkersArePresent.Items.FindByText(jsonConfig.SpeedLimits.WorkersPresentSpeed.ToString()).Selected = true;
                txtCauseCode.Text = jsonConfig.CauseCodes.CauseCode.ToString();
                txtsubcausecode.Text = jsonConfig.CauseCodes.SubCauseCode.ToString();

                // CONVERT TO DATETIME
                try { this.Calendar_BeginDate.SelectedDate = System.Convert.ToDateTime(jsonConfig.Schedule.StartDate); }
                catch { this.Calendar_BeginDate.SelectedDate = System.DateTime.Now; }

                try { this.Calendar_enddate.SelectedDate = System.Convert.ToDateTime(jsonConfig.Schedule.EndDate); }
                catch { this.Calendar_enddate.SelectedDate = System.DateTime.Now; }

                List<string> daysofweek;
                Dictionary<string, string> days_week = new Dictionary<string, string> {
                 { "Sun","Sun"},
                 { "Mon","Mon"},
                 { "Tues","Tues"},
                 { "Wed","Wed"},
                 { "Thurs","Thurs"},
                 { "Fri","Fri"},
                 { "Sat","Sat"},
                };
                daysofweek = jsonConfig.Schedule.DaysOfWeek;
                foreach (string day in daysofweek)
                {
                    var item = this.chkDaysOfWeek.Items.FindByText(days_week[day]);
                    if (item != null) item.Selected = true;
                }

                this.txtRoadName.Value = jsonConfig.GeneralInfo.RoadName;
                this.txtRoadNumber.Value = jsonConfig.GeneralInfo.RoadNumber;


                this.start_lat.Value = jsonConfig.Location.BeginningLocation.Lat.ToString();
                start_lat_hidden.Text = jsonConfig.Location.BeginningLocation.Lat.ToString();

                start_lng_hidden.Text = jsonConfig.Location.BeginningLocation.Lon.ToString();
                this.start_lng.Value = jsonConfig.Location.BeginningLocation.Lon.ToString();

                end_lat_hidden.Text = jsonConfig.Location.EndingLocation.Lat.ToString();
                this.end_lat.Value = jsonConfig.Location.EndingLocation.Lat.ToString();

                end_lng_hidden.Text = jsonConfig.Location.EndingLocation.Lon.ToString();
                this.end_lng.Value = jsonConfig.Location.EndingLocation.Lon.ToString();


                /******* New Fields ******/
                txtBeginningCrossStreet.Text = jsonConfig.GeneralInfo.BeginningCrossStreet.ToString();
                txtEndingCrossStreet.Text = jsonConfig.GeneralInfo.EndingCrossStreet.ToString();
                txtBeginMilePost.Text = jsonConfig.GeneralInfo.BeginningMilePost.ToString();
                txtEndMilePost.Text = jsonConfig.GeneralInfo.EndingMilePost.ToString();


                string eventstatus = "";
                eventstatus = convertFirstletter(jsonConfig.GeneralInfo.EventStatus.ToString());
                if (eventstatus != "") this.chEventStatus.Items.FindByText(eventstatus).Selected = true;


                string wzdirection = "";
                wzdirection = convertFirstletter(jsonConfig.GeneralInfo.Direction.ToString());
                if (wzdirection != "") this.chDirection.Items.FindByText(wzdirection).Selected = true;

                string beginingaccuracy = "";
                string endingaccuracy = "";
                string begindateccuracy = "";
                string enddateaccuracy = "";

                beginingaccuracy = convertFirstletter(jsonConfig.Location.BeginningAccuracy.ToString());
                if (beginingaccuracy != "") this.chkBeginningAccuracy.Items.FindByText(beginingaccuracy).Selected = true;

                endingaccuracy = convertFirstletter(jsonConfig.Location.EndingAccuracy.ToString());
                if (endingaccuracy != "") this.chkEndingAccuracy.Items.FindByText(endingaccuracy).Selected = true;

                begindateccuracy = convertFirstletter(jsonConfig.Schedule.StartDateAccuracy.ToString());
                if (begindateccuracy != "") this.chkStartDateAccuracy.Items.FindByText(begindateccuracy).Selected = true;

                enddateaccuracy = convertFirstletter(jsonConfig.Schedule.EndDateAccuracy.ToString());
                if (enddateaccuracy != "") this.chkEndDateAccuracy.Items.FindByText(enddateaccuracy).Selected = true;

                string wz_location_method = "";
                wz_location_method = jsonConfig.metadata.wz_location_method.ToString();
                string lrs_type = "";
                this.txtIrsType.Text = jsonConfig.metadata.lrs_type.ToString();

                string locatation_verify_method = "";
                this.txtLocationVerifyMethod.Text = jsonConfig.metadata.location_verify_method.ToString();

                string datafeed_frequency_update = "";
                this.txtDataFeedFrequencyMethod.Text = jsonConfig.metadata.datafeed_frequency_update.ToString();

                
                string wz_location = "";

                //WZ_LOCATION_METHODS
                wz_location =jsonConfig.metadata.wz_location_method.ToString();
                if (wz_location != "") this.ck_WZLocationmethod.Items.FindByValue(wz_location).Selected = true;

                this.txtContactEmail.Text = jsonConfig.metadata.contact_email.ToString();
                this.txtContactName.Text = jsonConfig.metadata.contact_name.ToString();
                txtIssuingOrganization.Text = jsonConfig.metadata.issuing_organization.ToString();

                updateTableStrings(jsonConfig);

            }
            catch { Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "updateMarkers();", true); }


            //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "updateMarkers();", true);
        }

        public void updateTableStrings(configurationObject jsonConfig)
        {
            string outString = "";
            if (jsonConfig.TypesOfWork != null)
            {
                foreach (TYPEOFWORK type_of_work in jsonConfig.TypesOfWork)
                {
                    outString += type_of_work.WorkType.ToString() + "," + type_of_work.Is_Architectural_Change.ToString() + ";";
                }
            }
            myHiddenoutputlist.Value = outString.TrimEnd(';');

            string outStringRestrictions = "";
            string outStringTypes = "";
            string strresunits;

            foreach (LANE lane in jsonConfig.LaneInfo.Lanes)
            {
                outStringTypes += lane.LaneNumber.ToString() + "," + lane.LaneType.ToString() + ";";

                foreach (LANERESTRICTIONS type_of_lanerestriction in lane.LaneRestrictions)
                {
                    string strresttype = type_of_lanerestriction.RestrictionType.ToString();

                    if (type_of_lanerestriction.RestrictionUnits == null)
                        strresunits = "None";
                    else
                        strresunits = type_of_lanerestriction.RestrictionUnits.ToString();

                    outStringRestrictions += lane.LaneNumber.ToString() + "," + strresttype + "," + strresunits + "," + type_of_lanerestriction.RestrictionValue.ToString() + ";";
                }
            }
            this.Hidden_list_LaneRestriction.Value = outStringRestrictions.TrimEnd(';');
            this.HiddenLaneType.Value = outStringTypes.TrimEnd(';');
        }

        public string convertFirstletter(string firstletterword)
        {
            if (firstletterword != "")
                return firstletterword.First().ToString().ToUpper() + firstletterword.Substring(1);
            else
                return "";
        }
        public void createobject()
        {
            jsonConfig = new configurationObject();
            jsonConfig.DateCreated = System.DateTime.Now.ToShortDateString();

            jsonConfig.GeneralInfo = new GENERALINFO();
            jsonConfig.GeneralInfo.Description = txt_workzonedescription.Text;
            jsonConfig.GeneralInfo.RoadName = txtRoadName.Value;
            jsonConfig.GeneralInfo.RoadNumber = txtRoadNumber.Value;
            foreach (ListItem item in this.chDirection.Items) if ((item.Selected)) jsonConfig.GeneralInfo.Direction = (DIRECTION)System.Enum.Parse(typeof(DIRECTION), item.Value.ToString().ToLower());
            jsonConfig.GeneralInfo.BeginningCrossStreet = txtBeginningCrossStreet.Text;
            jsonConfig.GeneralInfo.EndingCrossStreet = txtEndingCrossStreet.Text;
            try { jsonConfig.GeneralInfo.BeginningMilePost = int.Parse(txtBeginMilePost.Text); } catch { }
            try { jsonConfig.GeneralInfo.EndingMilePost = int.Parse(txtEndMilePost.Text); } catch { }
            foreach (ListItem item in this.chEventStatus.Items) if ((item.Selected)) jsonConfig.GeneralInfo.EventStatus = (EVENTSTATUS)System.Enum.Parse(typeof(EVENTSTATUS), item.Value.ToString().ToLower());


            //TypesOfWork
            string[] strworktypes_items = this.myHiddenoutputlist.Value.Split(';');
            List<TYPEOFWORK> typeofwork = new List<TYPEOFWORK>();
            if (strworktypes_items.Count() != 0)
            {
                foreach (string wrk_item in strworktypes_items)
                {
                    if (wrk_item != "")
                    {
                        wrk_item.Replace("'", "");
                        string[] items_typeofwork = wrk_item.Split(',');

                        bool temp_is_arch_change;
                        WORKTYPE temp_work_type;
                        if (Enum.TryParse(items_typeofwork[0].ToString().ToLower().Replace("'", ""), out temp_work_type) && bool.TryParse(items_typeofwork[1].ToString().Replace("'", ""), out temp_is_arch_change))
                        {
                            TYPEOFWORK item_typeofwork = new TYPEOFWORK();
                            item_typeofwork.Is_Architectural_Change = temp_is_arch_change;
                            item_typeofwork.WorkType = temp_work_type;
                            typeofwork.Add(item_typeofwork);
                        }
                    }

                }
                jsonConfig.TypesOfWork = typeofwork;
            }


            jsonConfig.LaneInfo = new LANEINFO();
            jsonConfig.LaneInfo.ApproachLanePadding = System.Convert.ToDouble(AppLanePadding.Text);
            jsonConfig.LaneInfo.AverageLaneWidth = System.Convert.ToDouble(AvgLaneWidth.Text);
            jsonConfig.LaneInfo.NumberOfLanes = System.Convert.ToInt32(dd_numberoflanes.SelectedItem.Value);
            jsonConfig.LaneInfo.VehiclePathDataLane = System.Convert.ToInt32(dd_avgvehicledatapath.SelectedValue);
            jsonConfig.LaneInfo.WorkzoneLanePadding = System.Convert.ToDouble(WorkZoneLanePadding.Text);
            jsonConfig.LaneInfo.Lanes = new List<LANE>();
            for (int lane = 1; lane <= jsonConfig.LaneInfo.NumberOfLanes; lane++)
            {
                var laneObj = new LANE();
                laneObj.LaneNumber = lane;
                laneObj.LaneRestrictions = new List<LANERESTRICTIONS>();
                jsonConfig.LaneInfo.Lanes.Add(laneObj);
            }

            //Lane restrictions object
            string[] strlanerestrictions = this.Hidden_list_LaneRestriction.Value.Split(';');
            List<LANERESTRICTIONS> lanerestrictions = new List<LANERESTRICTIONS>();
            if (strlanerestrictions.Count() != 0)
            {
                foreach (string lane_restriction in strlanerestrictions)
                {
                    if (lane_restriction != "")
                    {
                        string[] items_lanestrestrictions = lane_restriction.Replace("'", "").Split(',');

                        RESTRICTIONTYPE temp_restriction_type;
                        RESTRICTIONUNITS temp_restriction_unit;
                        float temp_restriction_value;
                        int temp_lane_number;
                        if (Enum.TryParse(items_lanestrestrictions[1].ToString().ToLower().Replace("'", ""), out temp_restriction_type) && int.TryParse(items_lanestrestrictions[0].ToString(), out temp_lane_number))
                        {
                            LANERESTRICTIONS item = new LANERESTRICTIONS();
                            item.RestrictionType = temp_restriction_type;
                            if (Enum.TryParse(items_lanestrestrictions[2].ToString().ToLower().Replace("'", ""), out temp_restriction_unit) && float.TryParse(items_lanestrestrictions[3].ToString(), out temp_restriction_value))
                            {
                                item.RestrictionUnits = temp_restriction_unit;
                                item.RestrictionValue = temp_restriction_value;
                            }
                            else
                            {
                                item.RestrictionUnits = null;
                                item.RestrictionValue = null;
                            }
                            if (jsonConfig.LaneInfo.Lanes.Count() > temp_lane_number - 1)
                            {
                                jsonConfig.LaneInfo.Lanes[temp_lane_number - 1].LaneRestrictions.Add(item);
                            }
                        }
                    }
                }
            }
            //Lane Type
            string[] strlanetypes = this.HiddenLaneType.Value.Split(';');
            if (strlanetypes.Count() != 0)
            {
                foreach (string lane_type in strlanetypes)
                {
                    if (lane_type != "")
                    {
                        string[] items_lanetypes = lane_type.Replace("'", "").Split(',');

                        int temp_lane_number;
                        LANETYPES temp_lane_type;
                        if (Enum.TryParse(items_lanetypes[1].ToString().ToLower().Replace("'", ""), out temp_lane_type) && int.TryParse(items_lanetypes[0].ToString(), out temp_lane_number))
                        {
                            jsonConfig.LaneInfo.Lanes[temp_lane_number - 1].LaneType = temp_lane_type;
                        }
                    }
                }
            }

            jsonConfig.SpeedLimits = new SPEEDLIMITS();
            jsonConfig.SpeedLimits.NormalSpeed = System.Convert.ToInt32(dd_normalspeed.SelectedValue);
            jsonConfig.SpeedLimits.ReferencePointSpeed = System.Convert.ToInt32(dd_AtreferencePoint.SelectedValue);
            jsonConfig.SpeedLimits.WorkersPresentSpeed = System.Convert.ToInt32(dd_WhenWorkersArePresent.SelectedValue);
            

            jsonConfig.CauseCodes = new CAUSE();
            jsonConfig.CauseCodes.CauseCode = System.Convert.ToInt32(txtCauseCode.Text);
            jsonConfig.CauseCodes.SubCauseCode = System.Convert.ToInt32(txtsubcausecode.Text);


            jsonConfig.Schedule = new SCHEDULE();
            DateTime StartDate = this.Calendar_BeginDate.SelectedDate;
            TimeSpan StartTime = TimeSpan.Parse(TimeBegin.Value); //CONVERT TO UTC
            jsonConfig.Schedule.StartDate = StartDate.Add(StartTime).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            foreach (ListItem item in this.chkStartDateAccuracy.Items) if ((item.Selected)) jsonConfig.Schedule.StartDateAccuracy = (STARTDATEACCURACY)System.Enum.Parse(typeof(STARTDATEACCURACY), item.Value.ToString().ToLower());

            DateTime EndDate = this.Calendar_enddate.SelectedDate;
            TimeSpan EndTime = TimeSpan.Parse(TimeEnd.Value); //CONVERT TO UTC
            jsonConfig.Schedule.EndDate = EndDate.Add(EndTime).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            foreach (ListItem item in this.chkEndDateAccuracy.Items) if ((item.Selected)) jsonConfig.Schedule.EndDateAccuracy = (ENDDATEACCURACY)System.Enum.Parse(typeof(ENDDATEACCURACY), item.Value.ToString().ToLower());

            List<string> wzDaysOfWeek = new List<string>();
            foreach (ListItem item in this.chkDaysOfWeek.Items) if ((item.Selected)) wzDaysOfWeek.Add(item.Value.ToString());
            jsonConfig.Schedule.DaysOfWeek = wzDaysOfWeek;


            jsonConfig.Location = new LOCATION();
            jsonConfig.Location.BeginningLocation = new Coordinate();
            jsonConfig.Location.BeginningLocation.Lat = System.Convert.ToDouble(start_lat_hidden.Text);
            jsonConfig.Location.BeginningLocation.Lon = System.Convert.ToDouble(start_lng_hidden.Text);
            jsonConfig.Location.EndingLocation = new Coordinate();
            jsonConfig.Location.EndingLocation.Lat = System.Convert.ToDouble(end_lat_hidden.Text);
            jsonConfig.Location.EndingLocation.Lon = System.Convert.ToDouble(end_lng_hidden.Text);
            foreach (ListItem item in this.chkBeginningAccuracy.Items) if ((item.Selected)) jsonConfig.Location.BeginningAccuracy = (BEGINNINGACCURACY)System.Enum.Parse(typeof(BEGINNINGACCURACY), item.Value.ToString().ToLower());
            foreach (ListItem item in this.chkEndingAccuracy.Items) if ((item.Selected)) jsonConfig.Location.EndingAccuracy = (ENDINGACCURACY)System.Enum.Parse(typeof(ENDINGACCURACY), item.Value.ToString().ToLower());


            jsonConfig.metadata = new METADATA();
            jsonConfig.metadata.lrs_type = this.txtIrsType.Text.ToString();
            jsonConfig.metadata.datafeed_frequency_update = this.txtDataFeedFrequencyMethod.Text.ToString();
            jsonConfig.metadata.location_verify_method = this.txtLocationVerifyMethod.Text.ToString();
            foreach (ListItem item in this.ck_WZLocationmethod.Items) if ((item.Selected)) jsonConfig.metadata.wz_location_method = (WZ_LOCATION_METHODS)System.Enum.Parse(typeof(WZ_LOCATION_METHODS), item.Value.ToString().ToLower());
            jsonConfig.metadata.timestamp_metadata_update = System.DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            jsonConfig.metadata.contact_name = this.txtContactName.Text.ToString();
            jsonConfig.metadata.contact_email = this.txtContactEmail.Text.ToString();
            jsonConfig.metadata.issuing_organization = txtIssuingOrganization.Text;
            updateTableStrings(jsonConfig);

        }

        public ArrayList CreateGridData_TypeOfWork()
        {
            ArrayList ds = new ArrayList();
            ds.Add("Type of work 1");
            ds.Add("Type of work 2");
            return ds;
        }
        public void deleteblob(string from_blobname)
        {
            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(from_blobname);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(this.txtFilepath_configSave.Text);
            blockBlob.DeleteIfExists();
            fillConfigurationFiles();
        }
        public void createconfigfile(string blobcontainername)
        {
            MemoryStream stream;
            using (stream = new MemoryStream())
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(jsonConfig, Formatting.Indented);
                    file.Write(json);
                    file.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    // Retrieve reference to a previously created container.
                    CloudBlobContainer container = blobClient.GetContainerReference(blobcontainername);
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(txtFilepath_configSave.Text);
                    blockBlob.UploadFromStream(stream);
                }
            }

        }
        public bool checkblobfileexsits(string blobcontainername)
        {
            bool blobfileexists = false;
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(blobcontainername);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(txtFilepath_configSave.Text);
                return blockBlob.Exists();

            }
            catch
            {
                return blobfileexists;
            }

        }
        public bool validaterequiredfields()
        {
            bool isvalid = true;
            strRequiredfield = "";
            //check for all required fields are populated
            if (this.txt_workzonedescription.Text.Length == 0)
            {
                strRequiredfield = "Please enter a Work zone description";
                isvalid = false;
            }
            if (this.txtRoadName.Value.Length == 0)
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a Road name ";
                else
                    strRequiredfield = "Please enter a Road name. ";
                isvalid = false;
            }

            if ((this.start_lat_hidden.Text == "0") || (this.end_lat_hidden.Text == "0") || (this.start_lat_hidden.Text == "0") || (this.start_lng_hidden.Text == "0"))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a valid lattitude and longitude ";
                else
                    strRequiredfield = "Please enter a valid lattitude and longitude ";

                isvalid = false;
            }
            if ((this.TimeBegin.Value.Length == 0) || (this.TimeBegin.Value.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a valid begin and end time ";
                else
                    strRequiredfield = "Please enter a valid begin and end time ";

                isvalid = false;
            }
            //make sure 1 day of the week is selected
            if (this.chkDaysOfWeek.SelectedItem == null)
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and  select at least 1 day of the week ";
                else
                    strRequiredfield = "Please select at least 1 day of the week ";

                isvalid = false;
            }
            if (this.Calendar_BeginDate.SelectedDate == null)
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and  select a begin date for the workzone to be in effect ";
                else
                    strRequiredfield = "Please select a begin date for the workzone to be in effect ";

                isvalid = false;
            }
            if (this.Calendar_enddate.SelectedDate == null)
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and  select a end date for the workzone to be in effect ";
                else
                    strRequiredfield = "Please select a end date for the workzone to be in effect ";

                isvalid = false;
            }

            if ((this.txtIssuingOrganization.Text.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a issuing organization ";
                else
                    strRequiredfield = "Please enter an issuing oragnization ";

                isvalid = false;
            }
            if ((this.txtLocationVerifyMethod.Text.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a location verify method in the metadata section ";
                else
                    strRequiredfield = "Please enter a location verify method in metadata the section ";

                isvalid = false;
            }
            if ((this.txtIrsType.Text.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a LRS type in the metadata section ";
                else
                    strRequiredfield = "Please enter a LRS Type in metadata the section ";

                isvalid = false;
            }
            if ((this.txtContactName.Text.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a contact Name in the metadata section ";
                else
                    strRequiredfield = "Please enter a conatct name in metadata the section ";

                isvalid = false;
            }
            if ((this.txtContactEmail.Text.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a contact email in the metadata section ";
                else
                    strRequiredfield = "Please enter a conatct email in metadata the section ";

                isvalid = false;
            }
            if ((this.txtIrsType.Text.Length == 0))
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a LRS type in the metadata section ";
                else
                    strRequiredfield = "Please enter a LRS Type in metadata the section ";

                isvalid = false;
            }
            try
            {
                MailAddress m = new MailAddress(this.txtContactEmail.Text.ToString());
            }
            catch(Exception ex)
            {
                if (strRequiredfield.Length != 0)
                    strRequiredfield = strRequiredfield + " and enter a valid contact email address ";
                else
                    strRequiredfield = "Please enter a valid contact email address ";

                isvalid = false;
            }

            return isvalid;
        }
        protected void btnSave_Click1(object sender, EventArgs e)
        {
            string test = this.myHiddenoutputlist.Value.ToString();

            //bool fieldsvalid = false;

            //fieldsvalid = validaterequiredfields();

            //if (fieldsvalid == false)
            //{
            //    this.hdnParam.Value = strRequiredfield;
            //    this.msgtype.Value = "Error";
            //    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            //    return;
            //}


            //check to see if file exists in azure already...if so prompt user to continue or cancel


            bool fileexists = checkblobfileexsits("inprogressconfigfiles");
            if (fileexists)
            {
                //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "key", "function Confirm(){window.confirm('aaa?');$get('Text1').value='bbb';}", true);
                ScriptManager.RegisterStartupScript(this, typeof(string), "Message", "if(confirm('The File already exists on Azure .Do you want Overwrite?')){location.replace('default.aspx?" + "FileNAME" + "');}else{location.replace('CampaignTrack.aspx');}", true);
                ClientScript.RegisterStartupScript(typeof(Page), "Confirm", "<script type='text/javascript'>callConfirm();</script>");  
                string confirmValue = Request.Form["confirm_value"];  
                return;
            }
            //ScriptManager.RegisterStartupScript(this, typeof(string), "Message", "if(confirm('The File already exists on Azure .Do you want Overwrite?')){location.replace('default.aspx?" + "FileNAME" + "');}else{location.replace('CampaignTrack.aspx');}", true);
            //ClientScript.RegisterStartupScript(typeof(Page), "Confirm", "<script type='text/javascript'>callConfirm();</script>");  
            //string confirmValue = Request.Form["confirm_value"];  

            try
            {
                jsonConfig = new configurationObject();
                createobject();
                createconfigfile("inprogressconfigfiles");
                fillConfigurationFiles();
                this.hdnParam.Value = "Your configuration file has been successfully saved.";
                this.msgtype.Value = "Success";
                //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                this.btnDownloadFile.Enabled = true;
                this.btnPublishFile.Enabled = true;
                this.btnDownloadFile.ForeColor = System.Drawing.Color.Black;
                this.btnPublishFile.ForeColor = System.Drawing.Color.Black;
                this.btnDownloadFile_tab1.Enabled = true;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Black;
                this.txtConfigType.Text = "Config status: The Configuration file has been saved and is saved as IN-PROGRESS.";
                deleteblob("publishedconfigfiles");
            }
            catch (System.Exception ex)
            {
                this.hdnParam.Value = "There was an error saving you configuration file.  Information has NOT been saved." + ex.Message.ToString();
                this.msgtype.Value = "Error";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                this.btnDownloadFile.Enabled = false;
                this.btnPublishFile.Enabled = false;
                this.btnDownloadFile.ForeColor = System.Drawing.Color.Gray;
                this.btnPublishFile.ForeColor = System.Drawing.Color.Gray;
                this.btnDownloadFile_tab1.Enabled = false;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Gray;
            }
            //Code to call if you need user input
            //string confirmValue = Request.Form["confirm_value"];
            //if (confirmValue == "Yes")
            //{
            //    jsonConfig = new configObj_json();
            //    createobject();
            //    createconfigfile();
            //    fillConfigurationFiles();
            //    this.btnDownloadFile.Enabled = true;
            //    this.hdnParam.Value = "Your configuration file has been successfully saved.";
            //    this.msgtype.Value = "Success";
            //    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);

            //}
            //else
            //{
            //    this.hdnParam.Value = "Your configuration file has NOT been saved.";
            //    this.msgtype.Value = "Info";
            //    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            //    this.btnDownloadFile.Enabled = false;
            //}

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //go back to V2x home page
            Response.Redirect("V2x_Home.aspx", false);

        }

        protected void btnDownloadFile_Click(object sender, EventArgs e)
        {
            try
            {
                this.hdnParam.Value = "Your file has been successfully downloaded.";
                this.msgtype.Value = "Success";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);

                //downloadlocal config, not one selected in a list
                string selectedfile = "";
                string selectedfile_type = "";
                if (this.txtFilepath_configSave.Text != "")
                {
                    selectedfile = this.txtFilepath_configSave.Text.ToString();
                    //Check in Inprogress and published
                    selectedfile_type= checkFolder(selectedfile);
                    if (selectedfile_type == "inprogressconfigfiles")
                        downloadlocal(selectedfile, "inprogressconfigfiles");
                    else if (selectedfile_type == "publishedconfigfiles")
                        downloadlocal(selectedfile, "publishedconfigfiles");
                    else
                    {
                        this.hdnParam.Value = "The file, " + this.txtFilepath_configSave.Text.ToString() + " does not exist.  try to save or import your configuration settings.";
                        this.msgtype.Value = "Error";
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                        return;
                    }
                }

                else
                {

                    this.hdnParam.Value = "Current confirgations file does not exist, try saving your configuration settings or importing in a new configuration file.";
                    this.msgtype.Value = "Error";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                    return;
                }


                //if (this.listConfigurationFiles.SelectedIndex != -1)
                //{
                //    selectedfile = this.listConfigurationFiles.SelectedValue.ToString();
                //    downloadlocal(selectedfile, "inprogressconfigfiles");
                //}
                //else if (this.listPublishedConfigurationFiles.SelectedIndex != -1)
                //{
                //    selectedfile = this.listPublishedConfigurationFiles.SelectedValue.ToString();
                //    downloadlocal(selectedfile, "publishedconfigfiles");
                //}
                //else if (this.txtFilepath_configSave.Text != "")
                //{
                //    selectedfile = this.txtFilepath_configSave.Text.ToString();
                //    if (Typeofconfig == "InProgress")
                //        downloadlocal(selectedfile, "inprogressconfigfiles");
                //    else if (Typeofconfig == "Published")
                //        downloadlocal(selectedfile, "publishedconfigfiles");
                //    else
                //    {
                        
                //        this.hdnParam.Value = "Please select a file to download from the list of published or unpublished files.";
                //        this.msgtype.Value = "Error";
                //        Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                //    }
                //}
                //else
                //{
                //    //no file selected to download
                //    this.hdnParam.Value = "Please select a file to download";
                //    this.msgtype.Value = "Error";
                //    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                //}

                ////good link: https://github.com/Azure-Samples/storage-file-dotnet-getting-started/blob/master/FileStorage/GettingStarted.cs
            }
            catch (System.Exception ex)
            {
                string errstr = ex.Message.ToString();
                this.hdnParam.Value = "There was an error downloading this file. Error msg: " + ex.Message.ToString();
                this.msgtype.Value = "Error";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            }
        }
        public void downloadfile(string filename, string blobcontainter)
        {

            string newpath = Server.MapPath("~/");
            strdownloadfilepath = newpath + filename;
            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(blobcontainter);
            using (FileStream fs = new FileStream(Server.MapPath("~/" + filename), FileMode.Create))
            {
                CloudBlockBlob data_file = container.GetBlockBlobReference(filename);
                using (MemoryStream temp = new MemoryStream())
                {
                    data_file.DownloadToStream(temp);
                    temp.Position = 0;
                    temp.CopyTo(fs);
                    fs.Flush();
                }
            }

        }
        public string checkFolder(string filename)
        {
            //User must have saved the file to azure before downloading
            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("inprogressconfigfiles");
            CloudBlockBlob data_file = container.GetBlockBlobReference(filename);
            if (data_file.Exists())
                return "inprogressconfigfiles";
            container = blobClient.GetContainerReference("publishedconfigfiles");
            if (data_file.Exists())
                return "publishedconfigfiles";

            return "";
        }
        public void downloadlocal(string filename, string Bloblname)
        {
            //User must have saved the file to azure before downloading
            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(Bloblname);
            using (FileStream fs = new FileStream(Server.MapPath("~/testfile.json"), FileMode.Create))
            {
                CloudBlockBlob data_file = container.GetBlockBlobReference(filename);
                using (MemoryStream temp = new MemoryStream())
                {
                    data_file.DownloadToStream(temp);
                    temp.Position = 0;
                    temp.CopyTo(fs);
                    fs.Flush();
                }
            }
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename);
            Response.TransmitFile(Server.MapPath("~/" + filename));
            Response.End();
        }
        public void importjson()
        {
            jsonConfig = JsonConvert.DeserializeObject<configurationObject>(File.ReadAllText(strdownloadfilepath));
        }

        protected void btnImportConfig_Click(object sender, EventArgs e)
        {
            //import config file
            string selectedfile = "";
            string strconfigstatus = "";
            string blobname = "";
            if (this.listConfigurationFiles.SelectedIndex != -1)
            {
                selectedfile = this.listConfigurationFiles.SelectedValue.ToString();
                strconfigstatus = "Config status: An IN-PROGRESS configuration file has been loaded.";
                blobname = "inprogressconfigfiles";
                this.btnDownloadFile.Enabled = true;
                this.btnPublishFile.Enabled = true;
                this.btnDownloadFile.ForeColor = System.Drawing.Color.Black;
                this.btnPublishFile.ForeColor = System.Drawing.Color.Black;
                this.btnDownloadFile_tab1.Enabled = true;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Black;
                Typeofconfig = "InProgress";
            }
            else if (this.listPublishedConfigurationFiles.SelectedIndex != -1)
            {
                selectedfile = this.listPublishedConfigurationFiles.SelectedValue.ToString();
                strconfigstatus = "Config status: A PUBLISHED configuration file has been loaded. You wil need to Save this and it will be moved to IN PROGRESS.";
                blobname = "publishedconfigfiles";
                this.btnDownloadFile.Enabled = true;
                this.btnPublishFile.Enabled = false;
                this.btnDownloadFile.ForeColor = System.Drawing.Color.Black;
                this.btnPublishFile.ForeColor = System.Drawing.Color.Gray;
                this.btnDownloadFile_tab1.Enabled = true;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Black;
                Typeofconfig = "Published";
            }
            else
            {
                this.hdnParam.Value = "Please select a file in the unpublished or published lists.";
                this.msgtype.Value = "Error";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                return;
            }
            try
            {
                this.hdnParam.Value = "Your file has been successfully imported.";
                this.msgtype.Value = "Success";
                clearfields();
                //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                downloadfile(selectedfile, blobname);
                importjson();
                //The object returned should be filled with data
                fillobject();
                //populate the save filename field
                this.txtConfigType.Text = strconfigstatus;

                this.listConfigurationFiles.SelectedIndex = -1;
                foreach (ListItem listItem in this.listPublishedConfigurationFiles.Items)
                {
                    listItem.Selected = false;
                }
                this.txtFilepath_configSave.Text = selectedfile;

            }
            catch (System.Exception ex)
            {
                string errstr = ex.Message.ToString();
                this.hdnParam.Value = "There was an error importing this file. Error msg: " + ex.Message.ToString();
                this.msgtype.Value = "Error";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);

                //Clear all fields
                clearfields(); //set all field to empty
                initializefields();
                this.btnDownloadFile.Enabled = false;
                this.btnPublishFile.Enabled = false;
                this.btnDownloadFile.ForeColor = System.Drawing.Color.Gray;
                this.btnPublishFile.ForeColor = System.Drawing.Color.Gray;
                this.btnDownloadFile_tab1.Enabled = false;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Gray;
                //good site for toast notifications
                ////https://codeseven.github.io/toastr/demo.html
            }
        }


        public void OnConfirm(object sender, EventArgs e)
        {
            string confirmValue = Request.Form["confirm_value"];
            if (confirmValue == "Yes")
            {
                this.btnDownloadFile.Enabled = true;
                this.hdnParam.Value = "Your configuration file has been successfully saved.";
                this.msgtype.Value = "Success";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            }
            else
            {
                this.hdnParam.Value = "Your configuration file has NOT been saved.";
                this.msgtype.Value = "Info";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            }

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            this.hdnParam.Value = "This is my message2";
            this.msgtype.Value = "Success";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
        }

        protected void btnPublishFile_Click(object sender, EventArgs e)
        {
            //Take the current config file save it to the published blob storage area

            //Required fields
            bool fieldsvalid = false;
            fieldsvalid = validaterequiredfields();
            if (fieldsvalid == false)
            {
                this.hdnParam.Value = strRequiredfield;
                this.msgtype.Value = "Error";
                //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                return;
            }


            try
            {
                jsonConfig = new configurationObject();
                createobject();
                createconfigfile("publishedconfigfiles");
                fillConfigurationFiles();
                this.hdnParam.Value = "Your configuration file has been successfully saved.";
                this.msgtype.Value = "Success";
                //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                this.btnDownloadFile.Enabled = true;
                this.btnPublishFile.Enabled = true;
                this.btnDownloadFile_tab1.Enabled = true;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Black;
                this.txtConfigType.Text = "Config status: The Configuration file has been saved and is PUBLISHED.";
                deleteblob("inprogressconfigfiles");
            }
            catch (System.Exception ex)
            {
                this.hdnParam.Value = "There was an error saving you configuration file.  Information has NOT been saved.";
                this.msgtype.Value = "Error";
                //Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                this.btnDownloadFile.Enabled = false;
                this.btnPublishFile.Enabled = false;
                this.btnDownloadFile.ForeColor = System.Drawing.Color.Gray;
                this.btnPublishFile.ForeColor = System.Drawing.Color.Gray;
                this.btnDownloadFile_tab1.Enabled = false;
                this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Gray;
            }
        }

        protected void listConfigurationFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.listPublishedConfigurationFiles.SelectedIndex = -1;
            this.listPublishedConfigurationFiles.ClearSelection();
            Typeofconfig = "InProgress";
            btnDownloadFile.Enabled = true;
        }

        protected void listPublishedConfigurationFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.listConfigurationFiles.SelectedIndex = -1;
            this.listConfigurationFiles.ClearSelection();
            Typeofconfig = "Published";
            btnDownloadFile.Enabled = true;
        }

        protected void btnClearFields_Click(object sender, EventArgs e)
        {
            clearfields();
            initializefields();

            this.hdnParam.Value = "All Fields have been cleared.";
            this.msgtype.Value = "Success";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            //disable buttons 
            this.btnDownloadFile.Enabled = false;
            this.btnPublishFile.Enabled = false;
            this.btnDownloadFile.ForeColor = System.Drawing.Color.Gray;
            this.btnPublishFile.ForeColor = System.Drawing.Color.Gray;
            this.btnDownloadFile_tab1.Enabled = false;
            this.btnDownloadFile_tab1.ForeColor = System.Drawing.Color.Gray;
            this.txtConfigType.Text = "Config status: Config status : Empty File (not saved or loaded configuration file).";
        }
        

        protected void btnAddWorkType_Click(object sender, EventArgs e)
        {
            //add item to list
            //this.lsttypesofwork.Items.Add(new ListItem(this.ddTypeOfWork.SelectedValue.ToString(), this.chkArchetururalChange.Checked.ToString()));
            String columns = "{1, -55}{1, -35}";
           // lsttypesofwork.Items.Add(String.Format(columns, "Filename", "Selected DateModified"));
            //lsttypesofwork.Items.Add(String.Format(columns, this.ddTypeOfWork.SelectedItem.Text.ToString(), this.chkArchetururalChange.Checked.ToString()));
        }

        
    }



}