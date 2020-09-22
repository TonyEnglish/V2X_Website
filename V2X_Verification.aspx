<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="V2X_Verification.aspx.cs" Inherits="Neaera_Website_2018.V2X_Verification" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <link rel="stylesheet" href="assets_v2x/css/uswds.min.css">
    <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <!-- V2X Styles -->
    <link rel="stylesheet" href="css/styles.css">
    <link rel="stylesheet" href="css/formdesign.css">
    <!-- Redesign Styles -->
    <link rel="stylesheet" href="css/redesign_styles.css">
    <script src="https://code.jquery.com/jquery-1.12.4.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>

    <script src="http://code.jquery.com/jquery-1.11.0.min.js"></script>
    <script src="assets_v2x/js/uswds.min.js"></script>
    <script src="https://code.jquery.com/jquery-1.12.4.min.js"></script>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/css/toastr.min.css" rel="stylesheet" />
    <%--<link href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/2.3.2/css/bootstrap.min.css" rel="stylesheet" />--%>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/js/toastr.min.js"></script>
    <style>
        #map {
            height: 600px;
            width: 100%;
        }

        #info-box {
            background-color: white;
            border: 1px solid black;
            font-size: 15px;
            width: 300px;
        }

        .partialCenter {
            margin-left: 20%;
            margin-top: 10px
        }

        #listConfigurationFiles {
            margin-left: 20%;
            width: 400px;
        }

        #top-info {
            position: relative
        }

        #menu-top {
            position: absolute;
            right: 0px;
            bottom: 0px
        }

        #menu-top a {
            color: #000;
            text-decoration: none;
            font-weight: 500;
            padding: 25px 15px 25px 15px;
            text-transform: uppercase;
            font-size: 16px;
            font-weight: 900;
            color: white
        }

        .menu-top-active {
            background-color: #929292;
            cursor: none;
            pointer-events: none;
        }

        .top-nav {
            padding-left: 0;
            margin-bottom: 0;
            list-style: none;
        }

        .top-nav > li {
            position: relative;
            display: block;
        }

        .top-nav > li > a {
            position: relative;
            display: block;
            padding: 10px 15px;
        }

        .top-nav > li > a:hover,
        .top-nav > li > a:focus {
            text-decoration: none;
            background-color: #929292;
        }

        .top-navbar-nav {
            margin: 0;
        }

        .top-navbar-nav > li {
            float: left;
        }

        .top-navbar-nav > li > a {
            padding-top: 15px;
            padding-bottom: 15px;
        }
    </style>
</head>
<body>
    <form id="form1" method="post" runat="server">
        <div id="top-info">
            <div class="grid-container">
                <div class="hero-intro">
                    <div class="grid-row">
                        <div class="tablet:grid-col-8">
                            <h2 align="left">V2X TMC Data Collection Website </h2>
                        </div>
                    </div>
                    <div class="grid-row">
                        <div class="tablet:grid-col-8">
                            <p align="left">
                                Work Zone Data Exchange - Verify Work Zones
                            </p>
                        </div>
                    </div>
                </div>
            </div>
            <ul id="menu-top" class="top-nav top-navbar-nav top-navbar-right">
                <li><a href="V2x_Home.aspx">HOME</a></li>
                <li><a href="V2X_ConfigCreator.aspx">CONFIGURATION</a></li>
                <li><a href="V2X_Upload.aspx">UPLOAD</a></li>
                <li><a href="V2X_Verification.aspx" class="menu-top-active" onclick="return false;">VERIFICATION</a></li>
                <li><a href="V2X_Published.aspx">PUBLISHED</a></li>
            </ul>
        </div>

        <div class="form-style-2">
            <div class="grid-container">
                <h1 class="text-center">Work Zone Verification</h1>
                <div class="form-style-2-heading">Choose Work Zone to Visualize</div>
                <div>
                    <div>
                        <asp:ListBox ID="listConfigurationFiles" runat="server" Style="margin-left: 20%; width: 400px;"></asp:ListBox>
                    </div>
                    <asp:Button ID="VisualizeButton" class="upload partialCenter" runat="server" Text="Load Visualization" OnClick="VisualizeButton_Click" /><br />
                    <asp:Button ID="VerificationButton" class="partialCenter" runat="server" Text="Verify Work Zone Data and Publish" Style="display: none;" OnClientClick="return confirm('Are you sure you want to publish this work zone? This cannot be undone')" OnClick="VerificationButton_Click" /><br />
                    <input type="hidden" id="hdnParam" runat="server" clientidmode="Static" value="This is my message" />
                    <input type="hidden" id="msgtype" runat="server" clientidmode="Static" />
                </div>
            </div>
        </div>
        <div class="form-style-4">
            <div id="map"></div>
            <div id="info-box"></div>
            <div id="geojsonStringDiv" runat="server" style="display: none;"></div>
        </div>
        <script>
            var map;
            function initMap() {
                var boulder = { lat: 40.93977, lng: -105.185182 };
                var myMapOptions =															//gMap options... 			
                {
                    zoom: 4,															//Works only if fitBounds below is commented out...
                    center: boulder,			//Reference Point...
                    // center: new google.maps.LatLng(mapData[0][1], mapData[0][2]),	//First map data point
                    mapTypeId: google.maps.MapTypeId.HYBRID,							//Satellite + Street names
                    mapTypeControl: true,
                    zoomControl: true,
                    scaleControl: true,
                    mapTypeControl: true,
                    streetViewControl: true,
                    rotateControl: true,
                    fullscreenControl: true
                };	
                map = new google.maps.Map(document.getElementById('map'), myMapOptions);
			    map.setTilt(0);
                center_display = document.getElementById('info-box')
                map.controls[google.maps.ControlPosition.TOP_CENTER].push(center_display);
                //initEvents();
            }
            initMap()

            function loadGeoJsonString(geoString) {
                var geojson = JSON.parse(geoString);
                map.data.addGeoJson(geojson);
                zoom(map);
                loadJsonInfo();
                openVisPopup();
            }

            /**
             * Update a map's viewport to fit each geometry in a dataset
             * @param {google.maps.Map} map The map to adjust
             */
            function zoom(map) {
                var bounds = new google.maps.LatLngBounds();
                map.data.forEach(function (feature) {
                    processPoints(feature.getGeometry(), bounds.extend, bounds);
                });
                map.fitBounds(bounds);
            }

            /**
             * Process each point in a Geometry, regardless of how deep the points may lie.
             * @param {google.maps.Data.Geometry} geometry The structure to process
             * @param {function(google.maps.LatLng)} callback A function to call on each
             *     LatLng point encountered (e.g. Array.push)
             * @param {Object} thisArg The value of 'this' as provided to 'callback' (e.g.
             *     myArray)
             */
            function processPoints(geometry, callback, thisArg) {
                if (geometry instanceof google.maps.LatLng) {
                    callback.call(thisArg, geometry);
                }
                else if (geometry instanceof google.maps.Data.Point) {
                    callback.call(thisArg, geometry.get());
                }
                else {
                    geometry.getArray().forEach(function (g) {
                        processPoints(g, callback, thisArg);
                    });
                }
            }

            function loadGeoJson() {
                var geojson_string = $('#geojsonStringDiv').text()
                loadGeoJsonString(geojson_string);
            }

            function loadJsonInfo() {
                var infowindow_json = new google.maps.InfoWindow({
                    content: "hello"
                });
                map.data.setStyle({
                    strokeColor: '#5ADEBF',
                    strokeWeight: 5
                });
                map.data.addListener('mouseover', function (event) {
                    map.data.revertStyle();
                    map.data.overrideStyle(event.feature, { strokeColor: '#D54AD5', strokeWeight: 8 });
                    var road_name = event.feature.getProperty('road_name');
                    var total_num_lanes = event.feature.getProperty('total_num_lanes');
                    var reduced_speed_limit = event.feature.getProperty('reduced_speed_limit');
                    var workers_present = event.feature.getProperty('workers_present');
                    var vehicle_impact = event.feature.getProperty('vehicle_impact');
                    var lanes = event.feature.getProperty('lanes');
                    var lanes_text = ''
                    for (i = 0; i < lanes.length; i++) {
                        lanes_text += '<tr><td>Lane ' + lanes[i]['lane_number'] + '(' + lanes[i]['lane_type'] + '): </td><td>' + lanes[i]['lane_status'] + '</td></tr>';
                    }
                    var html = '<table style="width:100%">' +
                        '<colgroup>' +
                        '<col span="1" style="width: 160px;">' +
                        '<col span="1" style="width: 138px;">' +
                        '</colgroup>' +
                        '<tbody>' +
                        '<tr><td>Road Name: </td><td>' + road_name + '</td></tr>' +
                        '<tr><td>Reduced Speed Limit:  </td><td>' + reduced_speed_limit + ' mph</td></tr>' +
                        '<tr><td>Workers Present: </td><td>' + workers_present + '</td></tr>' +
                        '<tr><td>Vehicle Impact: </td><td>' + vehicle_impact + '</td></tr>' +
                        lanes_text +
                        '</tbody>' +
                        '</table>';
                    document.getElementById("info-box").innerHTML = html
                });
            }

            var mypopup = false
            function openVisPopup() {
                //var url = 'RSZW_MapVisualizer.html'
                var url = 'V2X_MapVisualizer.aspx'
                var mypopup = window.open(url, 'popup_window', 'width=1000,height=600,left=100,top=100,resizable=yes');
                if (!mypopup) {
                    alert('Visualization popup was blocked. Please enable popups for this site');
                }
                else {
                    $('#VerificationButton').css('display', 'block');
                }
            }
            function showContent() {
                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "progressBar": true,
                    "preventDuplicates": false,
                    "positionClass": "toast-bottom-full-width",
                    "showDuration": "400",
                    "hideDuration": "1000",
                    "timeOut": "7000",
                    "extendedTimeOut": "1000",
                    "showEasing": "swing",
                    "hideEasing": "linear",
                    "showMethod": "fadeIn",
                    "hideMethod": "fadeOut"
                }
                var strmsg = document.getElementById("hdnParam").value;
                var errortype = document.getElementById("msgtype").value;

                if (errortype == "Success") {
                    toastr["success"](strmsg, "SUCCESS");
                }
                else if (errortype == "Error") {
                    toastr["error"](strmsg, "ERROR");
                }
                else if (errortype == "Info") {
                    toastr["info"](strmsg, "INFORMATION");
                }
                else {
                    toastr["error"](strmsg, "ERROR");
                }
            }
        </script>
        <%--<script
            src="https://maps.googleapis.com/maps/api/js?key=api-key-here&callback=initMap">
        </script>--%>
    </form>
</body>
</html>
