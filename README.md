# SAAB-Simulation

### (a.)Download unity-slam_ws  <br>
```git clone https://github.com/zejiekong/unity-slam_ws.git```<br> 
```cd unity-slam_ws/src``` <br>

### (b.)Built catkin workspace. **May require catkin clean** <br>
```catkin build``` <br>
```source devel/setup.bash``` <br>

### (c.)Launch  <br>
(i.) ``` roslaunch imu_noise imu_noise_demo.launch ``` -> launch tcp endpoint, noise_adding node, imu_reading rqt plot <br>
(ii.)``` roslaunch position_analysis.launch ``` -> launch position_acquiring nodes, rviz plot, rqt plot <br>

### (d.)Press play in Unity simulation
