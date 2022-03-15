# SAAB-Simulation

## Requirements: <br>
* git
* ros noetic
* unity version 2021.2.7f1
* unity-ros integration package (in Unity) <br>

<ins> Setup Guide </ins> : https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/setup.md
<br>
<br>

## Important Note: <br>
* fixedupdate rate : 0.001
* Ocean renderer fps : 60
* IMU model : mti-100 

## Setup: <br>

#### (a.)Download SAAB-Simulation <br>
```git clone https://github.com/zejiekong/SAAB-Simulation.git``` <br>

#### (b.)Download unity-slam_ws  <br>
```git clone https://github.com/zejiekong/unity-slam_ws.git```<br> 
```cd unity-slam_ws/src``` <br>

#### (c.)Built catkin workspace <br>
_May require catkin clean_ <br>
```catkin build``` <br>
```source devel/setup.bash``` <br>

#### (d.)Launch  <br>
(i.) ``` roslaunch unity-slam unity-slam.launch ``` -> launch tcp endpoint, rqt plot for imu and position analysis, rviz for CG position visualisation <br>

##### Update
Other ros packages can be ignored after C# migration from previous imu_noise python script.

#### (e.)Press play in Unity simulation
