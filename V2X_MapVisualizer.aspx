<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="V2X_MapVisualizer.aspx.cs" Inherits="Neaera_Website_2018.V2X_MapVisualizer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!-- <meta name="viewport" content="initial-scale=1.0, user-scalable=no"/>   -->
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <title>Work Zone Map Data Visualizer</title>
    
    <script src="https://code.jquery.com/jquery-3.5.0.js"></script>
    <style>
        .wzInfo_text {
            height: 100%;
            width: 234px;
            position: absolute;
            left: 5px;
            top: 0px;
            margin-top: 0px;
            font-size: 13px;
            background-color: lightyellow;
        }

        .overlay_btn {
            height: 25px;
            width: 120px;
            position: absolute;
            left: 55px;
            /*	top:	300px;		*/
            /*	background-color: lightgreen;	*/
            font-size: 13px;
        }

        .wzLegends {
            width: 170px;
            background-color: lightblue;
            position: absolute;
            left: 28px;
            /*	bottom:	30px;					*/
            margin-left: auto;
            margin-right: auto;
        }

        .map_canvas {
            width: calc(100% - 240px);
            height: 100%;
            position: absolute;
            left: 240px;
            top: 0px;
        }
    </style>
</head>

<body onload="startHere()">
    <form id="form1" runat="server">

        <!--	Define area to display WZ Infromation...	-->
        <p class="wzInfo_text" id="wzInfo_text"></p>

        <!--	Button to overlay bounding box...			-->
        <!--	<button class="overlay_btn" onclick="displayMapNodes_drawRectBB();">Overlay WZ Map</button>  -->

        <!--	Show map legends...		-->
        <!--	<img src = "./Icons/Legends-RSZW6.png" class="wzLegends">  -->

        <!--	Canvas for overlaying content on Google Map	-->
        <div class="map_canvas" id="map_canvas"></div>
        <asp:HiddenField runat="server" ID="hiddenMarkers" />

        <div id="mapControlsDiv">
            <button type="button" onclick="add_lane_closure()" style="font-size: 16px">Add Lane Closure</button>
            <button type="button" onclick="add_work_presence()" style="font-size: 16px">Add Worker Presence</button>
        </div>

        <!--
		In the following, insert your API key...
		
		Acquire Google Map API key from: https://developers.google.com/maps/documentation/javascript/get-api-key
	
	-->
        <%--<script type="text/javascript"
            src="https://maps.google.com/maps/api/js?key=api-key-here&libraries=geometry">
        </script>--%>

        <!--	Collected vehicle path and constructed WZ map Data...		-->

        <script type="text/javascript"
            src="./Map Visualizer/RSZW_MAP_Data.js">
        </script>

        <!--	Test vehicle data - created using data collected from RSZW/LC application		-->
        <!--	The following will not be use for map visualization.  ONLY WHEN TESTS are CONDUCTED to see the Results... -->

        <!--		<script	type="text/javascript" 
				src="file:\\C:\CAMP\V2I Projects\V2I - SA Extension 2\Task 14 - WZ Mapping\Test Data\Aug-2017\GM Buick 8-21-17\
					RSZW-jsArray-dv2irszw_20170821_140058.js">
		</script>					-->

        <!--	WZ map overlay on Google Satellite Map  -->

        <script type="text/javascript"
            src="./Map Visualizer/RSZW_MapVisualization_v5.js" defer> 
        </script>
    </form>
</body>
</html>
