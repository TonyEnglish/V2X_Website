using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using System.Web.UI.WebControls;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Neaera_Website_2018
{

    public partial class V2X_Published : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                fillConfigurationFiles();
            }
            //loadPreviousMap();
        }
        protected void listConfigurationFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listConfigurationFiles.SelectedItem != null)
            {
                string id = listConfigurationFiles.SelectedValue;
                string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("publishedworkzones");
                using (FileStream fs = new FileStream(Server.MapPath("~/Unzipped Files/wzdx.geojson"), FileMode.Create))
                {
                    CloudBlockBlob data_file = container.GetBlockBlobReference("wzdx/wzdx--" + id + ".geojson");
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
                    CloudBlockBlob data_file = container.GetBlockBlobReference("config/config--" + id + ".json");
                    using (MemoryStream temp = new MemoryStream())
                    {
                        data_file.DownloadToStream(temp);
                        temp.Position = 0;
                        temp.CopyTo(fs);
                        fs.Flush();
                    }
                }
                loadDescription();
                loadGeoJsonVis();
            }
            return;
        }
        public void loadPreviousMap()
        {
            if (geojsonStringDiv.InnerHtml.Length != 0 && listConfigurationFiles.SelectedItem != null)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "loadGeoJson()", true);
            }
        }
        public void loadGeoJsonVis()
        {
            string[] lines = File.ReadAllLines(Server.MapPath("~/Unzipped Files/wzdx.geojson"));
            geojsonStringDiv.InnerHtml = "";
            foreach (string line in lines)
            {
                geojsonStringDiv.InnerHtml += line;
            }
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "loadGeoJson()", true);
        }
        public void loadDescription()
        {
            var wzConfig = JsonConvert.DeserializeObject<configurationObject>(File.ReadAllText(Server.MapPath("~/Unzipped Files/config.json")));

            string roadName = wzConfig.GeneralInfo.RoadName;

            string wzDesc = wzConfig.GeneralInfo.Description;
            int totalLanes = wzConfig.LaneInfo.NumberOfLanes;

            string wzStartDate = wzConfig.Schedule.StartDate.ToString();
            string wzEndDate = wzConfig.Schedule.EndDate.ToString();
            tableDescriptionCell.Text = wzDesc;
            tableRoadNameCell.Text = roadName;
            tableStartDateCell.Text = wzStartDate;
            tableEndDateCell.Text = wzEndDate;
        }

        public void fillConfigurationFiles()
        {
            //Good link on populating file structure: https://stackoverflow.com/questions/55362899/getting-all-files-in-azure-file-share-cloudfiledirectory
            string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("publishedworkzones");
            var list = container.ListBlobs("wzdx/");
            List<string> blobNames = list.OfType<CloudBlockBlob>().Select(b => b.Name.Replace("wzdx/wzdx--", "").Replace(".geojson", "")).ToList();

            this.listConfigurationFiles.Items.Clear();
            foreach (string listItem in blobNames.Distinct().ToList())
            {
                listConfigurationFiles.Items.Add(new ListItem(listItem));
            }
        }
        protected void DownloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("publishedworkzones");

                string id = listConfigurationFiles.SelectedValue;
                if (id == "")
                {
                    this.hdnParam.Value = "Please Select a Work Zone";
                    this.msgtype.Value = "Error";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
                    return;
                }
                string localName = "Published-WZData--" + id + ".zip";
                List<string[]> files = new List<string[]>();
                if (wzdx_checkbox.Checked) files.Add(new string[] { "wzdx/wzdx--" + id + ".geojson", "Published-WZData--WZDx--" + id + ".geojson" });
                if (rsm_xml_checkbox.Checked)
                {
                    int length = files.Count();
                    var xmlList = container.ListBlobs("rsm-xml/");
                    foreach (var item in xmlList.OfType<CloudBlockBlob>())
                    {
                        if (item.Name.Contains(id))
                        {
                            files.Add(new string[] { item.Name, "Published-WZData--" + item.Name.Split('/').Last() });
                        }
                    }
                    if (length == files.Count())
                    {
                        throw new Exception("No RSM XML Files Found");
                    }
                }
                if (rsm_uper_checkbox.Checked)
                {
                    int length = files.Count();
                    var uperFiles = container.ListBlobs("rsm-uper/");
                    foreach (var item in uperFiles.OfType<CloudBlockBlob>())
                    {
                        if (item.Name.Contains(id))
                        {
                            files.Add(new string[] { item.Name, "Published-WZData--" + item.Name.Split('/').Last() });
                        }
                    }
                    if (length == files.Count())
                    {
                        throw new Exception("No RSM binary Files Found");
                    }
                }

                Download(files, localName);
            }
            catch (System.Exception ex)
            {
                this.hdnParam.Value = "There was an error downloading the requested published files. " + ex.Message.ToString();
                this.msgtype.Value = "Error";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "showContent();", true);
            }
        }

        public void Download(List<string[]> fileNames, string localName)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference("publishedworkzones");
            if (fileNames.Count == 1)
            {
                string[] blobFileName = fileNames[0];
                using (var tempStream = Response.OutputStream)
                {
                    var blob = container.GetBlockBlobReference(blobFileName[0]);
                    blob.DownloadToStream(tempStream);
                }
                Response.BufferOutput = false;
                Response.AddHeader("Content-Disposition", "attachment; filename=" + blobFileName[1]);
                Response.Flush();
                Response.End();
            }
            else
            {
                using (var zipOutputStream = new ZipOutputStream(Response.OutputStream))
                {
                    foreach (string[] blobFileName in fileNames)
                    {
                        zipOutputStream.SetLevel(0);
                        var blob = container.GetBlockBlobReference(blobFileName[0]);
                        var entry = new ZipEntry(blobFileName[1]);
                        zipOutputStream.PutNextEntry(entry);
                        blob.DownloadToStream(zipOutputStream);
                    }
                    zipOutputStream.Finish();
                    zipOutputStream.Close();
                }
                Response.BufferOutput = false;
                Response.AddHeader("Content-Disposition", "attachment; filename=" + localName);
                Response.ContentType = "application/octet-stream";
                Response.Flush();
                Response.End();
            }
        }
    }
}