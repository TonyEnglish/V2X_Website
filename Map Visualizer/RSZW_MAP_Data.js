//
//        --- CAMP .js file for RSZW/LC Mapping Project --- 
//        --- Data points overlay on Google Map --- 
//        --- File Created: 10/19/2020 - 05:30:49 ---
//
//
//   Work zone description... 
//   Mapped work zone length in meters (approach lane, wz lane)... 
//   Reference Point coordinates... 
//   no of lanes mapped for WZ...
//   Vehicle path data lane...
//   wz lane status...
//   wz lane width, lane padding for approach and WZ lanes...
//
    var wzDesc = "sample work zone";
    var wzLength = [82,584];
    var wzMapDate = "10/19/2020 - 05:30:49";
    var refPoint = [13,40.0609661,-105.2117164,1569.099976];
    var noLanes = 3;
    var mappedLaneNo = 3;
    var laneStat = [[3, 0, 0, 0], [22, 1, 1, 74], [53, 1, 0, 353]];
    var laneWidth = 3.6;
    var lanePadApp = 0;
    var lanePadWZ = 0;
    var wpStat = [[33, 1, 177], [44, 0, 275]];
    var msgSegments = 1;
    var nodesPerSegment = 33;
//
//   List of start and end nodes for approach and wx lane nodes per message segment
//   Approach lane nodes are in segment #1 (always...)
//
    var appNodesMsgSegment = [1, 1, 7];
//
//  Message segmentation list... 
//
    var msgSegList = [
[1, 33],
[1, 1, 7],
[1, 1, 20]];

//
//  Collected vehicle data points for WZ mapping... 
//   Veh speed, lat, lon, alt, heading 
//
    var mapData = [
[8.3759, 40.0610737, -105.2111562, 1569.1, 280.9486],
[8.0296, 40.061079, -105.2112543, 1569.1, 285.862],
[7.9093, 40.0610844, -105.2113467, 1569.1, 290.846],
[6.9968, 40.061092, -105.2114324, 1569.1, 293.8149],
[5.9305, 40.0610959, -105.2115081, 1569.1, 289.9925],
[4.0364, 40.0610934, -105.2115645, 1569.1, 293.7678],
[1.6694, 40.0610888, -105.2115929, 1569.1, 292.4757],
[1.6285, 40.0610812, -105.2115949, 1569.1, 292.6912],
[1.3835, 40.0610749, -105.2116004, 1569.1, 293.9718],
[3.2143, 40.0610711, -105.2116202, 1569.1, 254.0226],
[3.7515, 40.0610541, -105.2116482, 1569.1, 235.6853],
[5.4941, 40.0610221, -105.2116918, 1569.1, 207.1037],
[6.3821, 40.0609661, -105.2117164, 1569.1, 184.0959],
[6.3821, 40.0609661, -105.2117164, 1569.1, 184.0959],
[6.5121, 40.060902, -105.2117208, 1569.1, 184.938],
[7.9241, 40.0608317, -105.2117253, 1569.1, 184.0171],
[8.3034, 40.060754, -105.2117302, 1569.1, 185.9887],
[8.742, 40.0606741, -105.2117381, 1569.1, 187.9919],
[8.7951, 40.0605934, -105.2117457, 1569.1, 184.043],
[9.2138, 40.0605091, -105.2117527, 1569.1, 184.0057],
[9.1054, 40.0604263, -105.2117616, 1569.1, 187.9605],
[9.5577, 40.0603431, -105.2117808, 1569.1, 194.931],
[9.5442, 40.0602574, -105.2118126, 1569.1, 199.9278],
[9.6094, 40.0601743, -105.2118509, 1569.1, 200.9854],
[9.6094, 40.0601743, -105.2118509, 1569.1, 200.9854],
[9.3698, 40.0600893, -105.2118891, 1569.1, 208.85],
[9.1989, 40.0600129, -105.2119377, 1569.1, 213.8634],
[8.9675, 40.0599467, -105.2119894, 1569.1, 216.9142],
[9.0856, 40.059884, -105.2120457, 1569.1, 217],
[9.1774, 40.0598206, -105.2121055, 1569.1, 220.8872],
[9.3549, 40.0597563, -105.2121742, 1569.1, 225.8868],
[9.3498, 40.0596967, -105.2122447, 1569.1, 225.0182],
[9.4043, 40.0596362, -105.2123203, 1569.1, 231.8302],
[9.4043, 40.0596362, -105.2123203, 1569.1, 231.8302],
[9.089, 40.0595796, -105.2124044, 1569.1, 231.0242],
[9.3316, 40.0595243, -105.2124894, 1569.1, 235.8056],
[9.2034, 40.0594781, -105.2125825, 1569.1, 243.7977],
[9.1989, 40.0594368, -105.212677, 1569.1, 244.9707],
[9.1035, 40.0593961, -105.2127742, 1569.1, 244.0217],
[8.9123, 40.0593559, -105.2128703, 1569.1, 247.9328],
[8.9317, 40.0593243, -105.2129639, 1569.1, 252.8969],
[8.8401, 40.0592956, -105.2130601, 1569.1, 255.9486],
[8.4413, 40.0592699, -105.2131559, 1561.3, 258.961],
[8.2824, 40.0592489, -105.2132482, 1561.53, 263.9315],
[8.2824, 40.0592489, -105.2132482, 1561.53, 263.9315],
[8.4541, 40.0592345, -105.2133418, 1562.26, 263.0085],
[8.3422, 40.0592185, -105.213439, 1561.75, 266.9466],
[8.2423, 40.0592116, -105.213533, 1562.01, 272.9197],
[8.5403, 40.0592086, -105.21363, 1561.45, 278.9149],
[8.7648, 40.0592112, -105.2137333, 1560.74, 282.9368],
[8.7721, 40.059217, -105.2138379, 1561.2, 284.9571],
[8.9139, 40.0592212, -105.21394, 1562.16, 282.0491],
[9.6026, 40.0592215, -105.2140469, 1562.42, 278.0734],
[9.6026, 40.0592215, -105.2140469, 1562.42, 278.0734],
[9.0348, 40.0592209, -105.2141623, 1561.9, 273.0712],
[9.1068, 40.0592135, -105.2142705, 1562.05, 270.0428],
[9.4975, 40.0592059, -105.2143809, 1562.17, 272.9581],
[9.6554, 40.0592049, -105.2144987, 1562.37, 272.0134],
[9.651, 40.0592044, -105.2146181, 1562.99, 271.9994],
[9.6125, 40.0592059, -105.2147403, 1563.51, 271.9994],
[9.5328, 40.0592069, -105.2148555, 1564.49, 271.0126],
[9.2689, 40.0592046, -105.2149674, 1564.85, 269.0253],
[9.4444, 40.0592025, -105.2150786, 1565.74, 269.9884],
[9.2657, 40.0592061, -105.2151893, 1564.35, 268.0245],
[9.2657, 40.0592061, -105.2151893, 1564.35, 268.0245]];

//
//   Approach Lanes --- data points... 
//
//   The list has lat,lon,alt,lcloStat for each node for each lane +
//   heading + WP flag + distVec (dist from previous node) 
//
    var appMapData = [
[40.06101013, -105.21117227, 1569.1, 0, 0, 40.06104191, -105.21116423, 1569.1, 0, 0, 40.0610737, -105.2111562, 1569.1, 0, 0, 280.9486, 0, 0],
[40.06102389, -105.21137681, 1569.1, 0, 0, 40.06105414, -105.21136175, 1569.1, 0, 0, 40.0610844, -105.2113467, 1569.1, 0, 0, 290.846, 0, 23.59],
[40.06101573, -105.21163477, 1569.1, 0, 0, 40.06104532, -105.21161759, 1569.1, 0, 0, 40.0610749, -105.2116004, 1569.1, 0, 0, 293.9718, 0, 46.92],
[40.06100885, -105.21159691, 1569.1, 0, 0, 40.06103998, -105.21160856, 1569.1, 0, 0, 40.0610711, -105.2116202, 1569.1, 0, 0, 254.0226, 0, 51.59],
[40.06100062, -105.21160051, 1569.1, 0, 0, 40.06102736, -105.21162435, 1569.1, 0, 0, 40.0610541, -105.2116482, 1569.1, 0, 0, 235.6853, 0, 59.77],
[40.0609926, -105.21161649, 1569.1, 0, 0, 40.06100735, -105.21165414, 1569.1, 0, 0, 40.0610221, -105.2116918, 1569.1, 0, 0, 207.1037, 0, 71.15],
[40.06096148, -105.21163201, 1569.1, 0, 0, 40.06096379, -105.21167421, 1569.1, 0, 0, 40.0609661, -105.2117164, 1569.1, 0, 0, 184.0959, 0, 82.52]];

//
//   Work Zone Lanes --- data points... 
//
//   The list has lat,lon,alt,lcloStat for each node for each lane +
//   heading + WP flag + distVec (dist from prev. node) 
//
    var wzMapData = [
[40.06096148, -105.21163201, 1569.1, 0, 0, 40.06096379, -105.21167421, 1569.1, 0, 0, 40.0609661, -105.2117164, 1569.1, 0, 0, 184.0959, 0, 0],
[40.06050458, -105.21166831, 1569.1, 0, 0, 40.06050684, -105.2117105, 1569.1, 0, 0, 40.0605091, -105.2117527, 1569.1, 0, 0, 184.0057, 0, 60.15],
[40.06032642, -105.21169905, 1569.1, 0, 0, 40.06033476, -105.21173993, 1569.1, 0, 0, 40.0603431, -105.2117808, 1569.1, 0, 0, 194.931, 0, 88.59],
[40.06023533, -105.21173306, 1569.1, 1, 1, 40.06024637, -105.21177283, 1569.1, 0, 0, 40.0602574, -105.2118126, 1569.1, 0, 0, 199.9278, 0, 108.29],
[40.06005806, -105.211815, 1569.1, 1, 1, 40.06007368, -105.21185205, 1569.1, 0, 0, 40.0600893, -105.2118891, 1569.1, 0, 0, 208.85, 0, 137.49],
[40.05997682, -105.21186745, 1569.1, 1, 0, 40.05999486, -105.21190257, 1569.1, 0, 0, 40.0600129, -105.2119377, 1569.1, 0, 0, 213.8634, 0, 155.49],
[40.05977822, -105.21204154, 1569.1, 1, 0, 40.05979941, -105.21207352, 1569.1, 0, 0, 40.0598206, -105.2121055, 1569.1, 0, 0, 220.8872, 0, 190.41],
[40.0596509, -105.2121849, 1569.1, 1, 0, 40.0596738, -105.2122148, 1569.1, 0, 0, 40.0596967, -105.2122447, 1569.1, 0, 0, 225.0182, 0, 217.88],
[40.05958529, -105.21226802, 1569.1, 1, 0, 40.05961075, -105.21229416, 1569.1, 0, 0, 40.0596362, -105.2123203, 1569.1, 0, 0, 231.8302, 1, 236.7],
[40.05947074, -105.21244185, 1569.1, 1, 0, 40.05949752, -105.21246563, 1569.1, 0, 0, 40.0595243, -105.2124894, 1569.1, 0, 0, 235.8056, 1, 265.11],
[40.05937813, -105.21264121, 1569.1, 1, 0, 40.05940746, -105.2126591, 1569.1, 0, 0, 40.0594368, -105.212677, 1569.1, 0, 0, 244.9707, 1, 293.23],
[40.05926241, -105.21293902, 1569.1, 1, 0, 40.05929336, -105.21295146, 1569.1, 0, 0, 40.0593243, -105.2129639, 1569.1, 0, 0, 252.8969, 1, 329.42],
[40.05920635, -105.2131397, 1561.3, 1, 0, 40.05923812, -105.2131478, 1561.3, 0, 0, 40.0592699, -105.2131559, 1561.3, 0, 0, 258.961, 1, 355.03],
[40.05918451, -105.21323926, 1561.53, 1, 0, 40.05921671, -105.21324373, 1561.53, 0, 0, 40.0592489, -105.2132482, 1561.53, 0, 0, 263.9315, 0, 371.33],
[40.05914693, -105.21353731, 1562.01, 1, 0, 40.05917927, -105.21353515, 1562.01, 0, 0, 40.0592116, -105.213533, 1562.01, 0, 0, 272.9197, 0, 404.13],
[40.05914809, -105.21375224, 1560.74, 1, 0, 40.05917965, -105.21374277, 1560.74, 0, 0, 40.0592112, -105.2137333, 1560.74, 0, 0, 282.9368, 0, 430.09],
[40.05915739, -105.21405878, 1562.42, 0, 2, 40.05918945, -105.21405284, 1562.42, 0, 0, 40.0592215, -105.2140469, 1562.42, 0, 0, 278.0734, 0, 466.61],
[40.05914124, -105.21438527, 1562.17, 0, 2, 40.05917357, -105.21438308, 1562.17, 0, 0, 40.0592059, -105.2143809, 1562.17, 0, 0, 272.9581, 0, 505.11],
[40.05913969, -105.21462105, 1562.99, 0, 0, 40.05917204, -105.21461958, 1562.99, 0, 0, 40.0592044, -105.2146181, 1562.99, 0, 0, 271.9994, 0, 535.7],
[40.05914139, -105.21518638, 1564.35, 0, 0, 40.05917374, -105.21518784, 1564.35, 0, 0, 40.0592061, -105.2151893, 1564.35, 0, 0, 268.0245, 0, 584.31]];

