import matplotlib.pyplot as plt
import pandas as pd
from glob import glob
import numpy as np
from matplotlib.backends.backend_qt5agg import FigureCanvasQTAgg as FigureCanvas
from PyQt5.Qt import *
from PyQt5.QtCore import QTimer
from matplotlib.figure import Figure
import matplotlib
import time
from PIL import Image
matplotlib.use("Qt5Agg")  # 声明使用QT5

"""
angle       - 环形夹角度数
r0          - 环形内圆半径，r0=0输出扇形
k           - 临近点插值密度（如果图像上出现白噪点，可适当增加k值）
top         - 扇尖（环心）在上部
rotate      - 旋转角度,正数表示逆时针旋转，负数表示顺时针旋转
"""

angle = 70
r0 = 0
k = 100
top = False
rotate = 0


class MyFigure(FigureCanvas):
    def __init__(self, width=5, height=4, dpi=100):
        # 第一步：创建一个创建Figure
        self.fig = Figure(figsize=(width, height), dpi=dpi)
        # 第二步：在父类中激活Figure窗口
        super().__init__(self.fig)
        self.axes = self.fig.add_subplot(111)


class Window(QWidget):
    circle = 0
    directoin = 1

    def __init__(self):
        super().__init__()
        self.setWindowTitle("SONAR Simulation")
        self.resize(1440, 800)
        self.setup_ui()

    def setup_ui(self):
        # self.QGridLay()
        # 开启定时器参数
        self.timer = QTimer(self)
        # 定时器中断函数
        self.timer.timeout.connect(self.QGridLay)
        self.timer.start(800)

    def int0(self):
        print('Timer is working')
        # self.F.axes.imshow(x, cmap='afmhot')  # 显示输出的程序在这

    def QGridLay(self):
        F = MyFigure(width=3, height=2, dpi=100)
        print("Loading"+"--"+str(self.circle)+".csv")
        arrays = []
        if self.circle < 10:
            arrays = glob("010922_021443/00000"+str(self.circle)+"_.csv")
        else:
            arrays = glob("010922_021443/0000"+str(self.circle)+"_.csv")
        x = []
        for arr in arrays:
            a = pd.read_csv(arr, header=None)
            a = a.fillna(0)
            p = np.array(a)
        x = np.array(a)
        x = np.rot90(x, 1)

        

        # 参数计算
        h, w = x.shape
        r = 2 * h - 1
        im_fan = np.ones((r, r), dtype=np.uint8)
        idx = np.arange(h) if top else np.arange(h)[::-1]
        # 坐标转化，将角度转换为弧度，生成扇形角度序列
        alpha = np.radians(np.linspace(-angle/2, angle/2, k*w))
        for i in range(k*w):  # 遍历输入图像的每一列
            rows = np.int32(np.ceil(np.cos(alpha[i]) * idx)) + r // 2
            cols = np.int32(np.ceil(np.sin(alpha[i]) * idx)) + r // 2
            im_fan[(rows, cols)] = x[:, i//k]
        im_fan = im_fan[r//2:, :]  # 裁切输出图像的空白区域
        im_out = np.flip(im_fan, axis=0)

        F.axes.imshow(im_out, 'afmhot')
        F.fig.suptitle("Cartesian Image")


        if self.circle == 0:
            self.gridlayout = QGridLayout(self)  # 继承容器groupBox
            self.gridlayout.addWidget(F, 0, 0)
            self.circle += self.directoin
        else:
            self.gridlayout.removeWidget(F)
            self.gridlayout.addWidget(F, 0, 0)
            self.circle += self.directoin
        if self.circle > 5:
            self.circle = 5


if __name__ == '__main__':
    import sys

    app = QApplication(sys.argv)

    window = Window()
    window.show()

    sys.exit(app.exec_())
