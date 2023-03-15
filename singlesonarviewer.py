<<<<<<< Updated upstream
import sys
from glob import glob
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import cv2
import json
import time
import numba
from PIL import Image

top = False
angle = 30
angle_v= 30
bins_vert=512
r0 = 0
k = 100
bins_size=200
e=10**-5
h, w = (512,512)
r = 2 * h - 1


def heatmap2d(arr: np.ndarray):
    
    plt.imshow(arr, cmap='jet')
    plt.colorbar()
    plt.show()

def sum_arrays(arrays):
  result = []
  for array in arrays:
    result.append(sum(array)/(len(array)))
  return result

def image(a,name):
    
    a = np.array(a, dtype=np.uint8)

    a = np.transpose(a)
    plt.imsave(name,a, cmap = 'jet')

   # img = Image.fromarray(a, mode='L')
   # img.save(name)
    return (a)


def c(T=25,S=0.01,z=2):
    if((0<T and T<=35) and (0<S and S<=45)and (0<z and z<=1000)):
        c_val=1449.2+4.6*T-0.055*T**2+0.00029*T**3+(1.34-0.01*T)*(S-35)+0.016*z
        return c_val
    else:
        print("Check values")
        return -1

c_calc=c()

def alpha(theta,phi,Lh,f=750,c=c_calc):
    coeff=np.sin(theta)*np.sin(phi)*Lh/(c/f)
    return(np.sin(coeff)/coeff)

def beta(phi,Lv,f=750,c=c_calc):
    coeff=np.sin(phi)*Lv/(c/f)
    return(np.sin(coeff)/coeff)

phi_vals=np.linspace(-angle_v/2,angle_v/2,bins_vert)
theta_vals=np.linspace(-angle/2,angle/2,bins_size)


def alpha_vals_calc(Lh_val):
    alpha_vals_list=[]
    for phi_val in phi_vals:
        for theta_val in theta_vals:
            alpha_vals_list.append(alpha(np.deg2rad(theta_val),np.deg2rad(phi_val),Lh=Lh_val))
    return(alpha_vals_list)
alpha_vals_list_1=alpha_vals_calc(60.16)
beam_pattern_r=np.array_split(np.array(alpha_vals_list_1),512)+beta(np.deg2rad(phi_vals)[:200],Lv=18.8)
alpha_vals_list_2=alpha_vals_calc(0.4275)
beam_pattern_t=np.array_split(np.array(alpha_vals_list_2),512)+beta(np.deg2rad(phi_vals)[:200],Lv=9.4)
beam_pattern=20*np.log10(beam_pattern_r)+20*np.log10(beam_pattern_t)
def a_w(c_val=c_calc,ph=7,S=45,z_max=4,f=750,T=25):
    
    A1=(8.696/c_val)*10**(0.78*ph-5)
    f1=2.8*np.sqrt(S/35)*10**(4-(1245/(T+273)))
    P1=1
    
    A2=21.44*(S/c_val)*(1+0.025*T)
    f2=(8.17*10**(8-1990/(T+273)))/(1+0.0018*(S-35))
    P2=1-1.37*z_max*10**-4+6.2*(z_max**2)*(10**-9)
    
    if(T<=20):
        A3=4.937*(10**-4)-2.59*(10**-5)*T+9.11*(10**-7)*(T**2)-1.5*(10**-8)*(T**3)
    else:
        A3=3.964*(10**-4)-1.146*(10**-5)*T+1.45*(10**-7)*(T**2)-6.5*(10**-10)*(T**3)
    P3=1-3.83*z_max*10**-5+4.9*(z_max**2)*(10**-10)

    a_w_val=(A1*P1*f1*f**2)/(f1**2+f**2)+(A2*P2*f2*f**2)/(f2**2+f**2)+A3*P3*f**2
    return a_w_val

a_w_calc=a_w()



def a_t(d,a_w_val=a_w_calc):
    return ((2*d-1)*a_w_val/1000)


def S_L(d):
    return 40*np.log10(d)


@numba.jit
def S_v(f=750,pref=2):
    if(pref==1):
        S_p=-50
    elif(pref==2):
        S_p=-70
    elif(pref==3):
        S_P=-90
    return (S_p+7*np.log10(f))
Sv_calc=S_v()



def I_R(a1, T_L, BP=0, TS=0):
    return np.where(a1 == 0, 0, np.add(-T_L, BP + TS))



def RL_V(dist_list, T_L, f=750, Sv_val=Sv_calc, BP=0):
    dist_list = np.insert(dist_list, 0, 0)
    V_diff = (4/3) * np.pi * np.diff(dist_list)
    l1 = Sv_val + 10 * np.log10(V_diff)
    return l1 - T_L + BP
_,b1=np.histogram(0,bins_size,range=[e,30])
dists=np.array([(b1[i]+b1[i+1])/2 for i in range (len(b1)-1)])

T_L=a_t(dists)+S_L(dists)

RL_V_val=RL_V(dist_list=dists,T_L=T_L)
x=1
y="c"
e=10**-5
#open(os.path.join(os.getcwd(),sys.argv[1],f"{x:06d}"+".json"))
newlist = []
while(y!="e"):
    #try:
   # print(sys.argv[1]+f"{x:06d}"+".json")
    os.makedirs(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs"),exist_ok=True)
    f = open(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1],f"{x:06d}"+" .json"))
    
    l2=[]
    data = json.load(f)
    values_dict = {}
    for instance, value in zip(data["instances"], data["Values"]):
        values_dict[instance] = value

    l2 = [values_dict.get(x, 0) for x in range(1*512)]

    l2_slices = np.array_split(l2, 512)
    bins = []

    for l2_slice,beam_pattern_instance in zip(l2_slices,beam_pattern):
        hist, edges = np.histogram(l2_slice, bins=bins_size, range=[e, 30])
        
        c1=RL_V_val+I_R(hist,T_L,BP=beam_pattern_instance)+beam_pattern_instance
        c2=c1.reshape(bins_size,1)
        bins.append(c2.reshape(-1, 1))
        
        
        
    polar_img=np.concatenate(bins,axis=1)
    polar_normal=1-(polar_img-np.min(polar_img))/(np.max(polar_img)-np.min(polar_img))
    polar_raised=polar_normal*255
   # plt.imsave(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs",f"{x:06d}"+" .png"), polar_raised, cmap = 'jet')
    

    newlist.append(sum_arrays(polar_raised))
    sdistance=(newlist[x-1].index(max(newlist[x-1][:200]))+1) * (30/200)
    print(sdistance)
   # print(newlist[x-1])
    if x==int( sys.argv[2]):
         g = image(newlist,os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs",f"{x:06d}"+" combine.png"))


 # plt.imshow(g, cmap='viridis')
    
    
    
    
    # polar_raised=np.rot90(polar_raised,2)
    # im_fan = np.ones((r, r), dtype=np.uint8)
    # idx = np.arange(h) if top else np.arange(h)[::-1]
    # # 坐标转化，将角度转换为弧度，生成扇形角度序列
    # alpha = np.radians(np.linspace(-angle/2, angle/2, k*w))
    # for i in range(k*w):  # 遍历输入图像的每一列
    #     rows = np.int32(np.ceil(np.cos(alpha[i]) * idx)) + r // 2
    #     cols = np.int32(np.ceil(np.sin(alpha[i]) * idx)) + r // 2
    #     im_fan[(rows, cols)] = polar_raised[:, i//k]
    # im_fan = im_fan[r//2:, :]  # 裁切输出图像的空白区域
    # im_out = np.flip(im_fan, axis=0)
    # #plt.figure(figsize=(15,15))
    # #plt.imshow(im_out, 'afmhot')
    # cv2.imshow('cartesian', np.uint8(im_out))
    # cv2.imwrite(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs",f"{x:06d}"+" .png"),np.uint8(im_out))
    # cv2.imshow('polar', np.uint8(polar_normal*255))
    # cv2.waitKey(1)
    x+=1
   # plt.imshow(polar_img)
    #plt.show()
   # except:
    #    print("end of folder, waiting")
    #    y="e"
=======
import sys
from glob import glob
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import cv2
import json
import time
import numba
from PIL import Image

top = False
angle = 30
angle_v= 30
bins_vert=512
r0 = 0
k = 100
bins_size=200
e=10**-5
h, w = (512,512)
r = 2 * h - 1


def heatmap2d(arr: np.ndarray):
    
    plt.imshow(arr, cmap='jet')
    plt.colorbar()
    plt.show()

def sum_arrays(arrays):
  result = []
  for array in arrays:
    result.append(sum(array)/(len(array)))
  return result

def image(a,name):
    
    a = np.array(a, dtype=np.uint8)

    a = np.transpose(a)
    plt.imsave(name,a, cmap = 'jet')

   # img = Image.fromarray(a, mode='L')
   # img.save(name)
    return (a)


def c(T=25,S=0.01,z=2):
    if((0<T and T<=35) and (0<S and S<=45)and (0<z and z<=1000)):
        c_val=1449.2+4.6*T-0.055*T**2+0.00029*T**3+(1.34-0.01*T)*(S-35)+0.016*z
        return c_val
    else:
        print("Check values")
        return -1

c_calc=c()

def alpha(theta,phi,Lh,f=750,c=c_calc):
    coeff=np.sin(theta)*np.sin(phi)*Lh/(c/f)
    return(np.sin(coeff)/coeff)

def beta(phi,Lv,f=750,c=c_calc):
    coeff=np.sin(phi)*Lv/(c/f)
    return(np.sin(coeff)/coeff)

phi_vals=np.linspace(-angle_v/2,angle_v/2,bins_vert)
theta_vals=np.linspace(-angle/2,angle/2,bins_size)


def alpha_vals_calc(Lh_val):
    alpha_vals_list=[]
    for phi_val in phi_vals:
        for theta_val in theta_vals:
            alpha_vals_list.append(alpha(np.deg2rad(theta_val),np.deg2rad(phi_val),Lh=Lh_val))
    return(alpha_vals_list)
alpha_vals_list_1=alpha_vals_calc(60.16)
beam_pattern_r=np.array_split(np.array(alpha_vals_list_1),512)+beta(np.deg2rad(phi_vals)[:200],Lv=18.8)
alpha_vals_list_2=alpha_vals_calc(0.4275)
beam_pattern_t=np.array_split(np.array(alpha_vals_list_2),512)+beta(np.deg2rad(phi_vals)[:200],Lv=9.4)
beam_pattern=20*np.log10(beam_pattern_r)+20*np.log10(beam_pattern_t)
def a_w(c_val=c_calc,ph=7,S=45,z_max=4,f=750,T=25):
    
    A1=(8.696/c_val)*10**(0.78*ph-5)
    f1=2.8*np.sqrt(S/35)*10**(4-(1245/(T+273)))
    P1=1
    
    A2=21.44*(S/c_val)*(1+0.025*T)
    f2=(8.17*10**(8-1990/(T+273)))/(1+0.0018*(S-35))
    P2=1-1.37*z_max*10**-4+6.2*(z_max**2)*(10**-9)
    
    if(T<=20):
        A3=4.937*(10**-4)-2.59*(10**-5)*T+9.11*(10**-7)*(T**2)-1.5*(10**-8)*(T**3)
    else:
        A3=3.964*(10**-4)-1.146*(10**-5)*T+1.45*(10**-7)*(T**2)-6.5*(10**-10)*(T**3)
    P3=1-3.83*z_max*10**-5+4.9*(z_max**2)*(10**-10)

    a_w_val=(A1*P1*f1*f**2)/(f1**2+f**2)+(A2*P2*f2*f**2)/(f2**2+f**2)+A3*P3*f**2
    return a_w_val

a_w_calc=a_w()



def a_t(d,a_w_val=a_w_calc):
    return ((2*d-1)*a_w_val/1000)


def S_L(d):
    return 40*np.log10(d)


@numba.jit
def S_v(f=750,pref=2):
    if(pref==1):
        S_p=-50
    elif(pref==2):
        S_p=-70
    elif(pref==3):
        S_P=-90
    return (S_p+7*np.log10(f))
Sv_calc=S_v()



def I_R(a1, T_L, BP=0, TS=0):
    return np.where(a1 == 0, 0, np.add(-T_L, BP + TS))



def RL_V(dist_list, T_L, f=750, Sv_val=Sv_calc, BP=0):
    dist_list = np.insert(dist_list, 0, 0)
    V_diff = (4/3) * np.pi * np.diff(dist_list)
    l1 = Sv_val + 10 * np.log10(V_diff)
    return l1 - T_L + BP
_,b1=np.histogram(0,bins_size,range=[e,30])
dists=np.array([(b1[i]+b1[i+1])/2 for i in range (len(b1)-1)])

T_L=a_t(dists)+S_L(dists)

RL_V_val=RL_V(dist_list=dists,T_L=T_L)
x=1
y="c"
e=10**-5
#open(os.path.join(os.getcwd(),sys.argv[1],f"{x:06d}"+".json"))
newlist = []
while(y!="e"):
    #try:
   # print(sys.argv[1]+f"{x:06d}"+".json")
    os.makedirs(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs"),exist_ok=True)
    f = open(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1],f"{x:06d}"+" .json"))
    
    l2=[]
    data = json.load(f)
    values_dict = {}
    for instance, value in zip(data["instances"], data["Values"]):
        values_dict[instance] = value

    l2 = [values_dict.get(x, 0) for x in range(1*512)]

    l2_slices = np.array_split(l2, 512)
    bins = []

    for l2_slice,beam_pattern_instance in zip(l2_slices,beam_pattern):
        hist, edges = np.histogram(l2_slice, bins=bins_size, range=[e, 30])
        
        c1=RL_V_val+I_R(hist,T_L,BP=beam_pattern_instance)+beam_pattern_instance
        c2=c1.reshape(bins_size,1)
        bins.append(c2.reshape(-1, 1))
        
        
        
    polar_img=np.concatenate(bins,axis=1)
    polar_normal=1-(polar_img-np.min(polar_img))/(np.max(polar_img)-np.min(polar_img))
    polar_raised=polar_normal*255
   # plt.imsave(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs",f"{x:06d}"+" .png"), polar_raised, cmap = 'jet')
    

    newlist.append(sum_arrays(polar_raised))
    sdistance=(newlist[x-1].index(max(newlist[x-1][:200]))+1) * (30/200)
    print(sdistance)
   # print(newlist[x-1])
    if x==int( sys.argv[2]):
         g = image(newlist,os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs",f"{x:06d}"+" combine.png"))


 # plt.imshow(g, cmap='viridis')
    
    
    
    
    # polar_raised=np.rot90(polar_raised,2)
    # im_fan = np.ones((r, r), dtype=np.uint8)
    # idx = np.arange(h) if top else np.arange(h)[::-1]
    # # 坐标转化，将角度转换为弧度，生成扇形角度序列
    # alpha = np.radians(np.linspace(-angle/2, angle/2, k*w))
    # for i in range(k*w):  # 遍历输入图像的每一列
    #     rows = np.int32(np.ceil(np.cos(alpha[i]) * idx)) + r // 2
    #     cols = np.int32(np.ceil(np.sin(alpha[i]) * idx)) + r // 2
    #     im_fan[(rows, cols)] = polar_raised[:, i//k]
    # im_fan = im_fan[r//2:, :]  # 裁切输出图像的空白区域
    # im_out = np.flip(im_fan, axis=0)
    # #plt.figure(figsize=(15,15))
    # #plt.imshow(im_out, 'afmhot')
    # cv2.imshow('cartesian', np.uint8(im_out))
    # cv2.imwrite(os.path.join(os.getcwd(),"SONAR_Outputs",sys.argv[1]+"_imgs",f"{x:06d}"+" .png"),np.uint8(im_out))
    # cv2.imshow('polar', np.uint8(polar_normal*255))
    # cv2.waitKey(1)
    x+=1
   # plt.imshow(polar_img)
    #plt.show()
   # except:
    #    print("end of folder, waiting")
    #    y="e"
>>>>>>> Stashed changes
