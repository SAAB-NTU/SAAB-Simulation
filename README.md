# SAAB-Simulation

## Requirements: <br>
* git
* ros noetic
* unity version 2021.2.7f1
* unity-ros integration package (in Unity) <br>

<ins> setup guide </ins> : https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/setup.md

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
(i.) ``` roslaunch imu_noise imu_noise_demo.launch ``` -> launch tcp endpoint, noise_adding node, imu_reading rqt plot <br>
(ii.)``` roslaunch position_analysis.launch ``` -> launch position_acquiring nodes, rviz plot, rqt plot <br>

#### (e.)Press play in Unity simulation
