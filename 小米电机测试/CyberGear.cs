using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 通讯类型
/// </summary>

namespace 小米电机测试
{

    public class CyberGear
    {
        public const double pi= 3.1415926535897932384626433832795;
        public enum CMode : byte
        {
            获取设备ID = 0,
            运控模式电机控制指令 = 1,
            电机反馈数据 = 2,
            电机使能运行 = 3,
            电机停止运行 = 4,
            设置电机机械零位 = 6,
            设置电机CAN_ID = 7,
            单个参数读取 = 17,
            单个参数写入 = 18,
            故障反馈帧 = 21,
            波特率修改 = 22
        }
        public enum paraList_RW:UInt16
        {
            run_mode=0x7005,
            iq_ref=0x7006,
            spd_ref=0x700A,
            limit_torque=0x700B,
            cur_kp=0x7010,
            cur_ki=0x7011,
            cur_filt_gain=0x7014,
            loc_ref=0x7016,
            limit_spd=0x7017,
            limit_cur=0x7018
        }
        public enum para_run_mode
        {
            运控模式=0,
            位置模式=1,
            速度模式=2,
            电流模式=3
        }
        public uint buildID(CMode cMode,uint middelData,uint motorId)
        {
            return (uint)cMode<<24|middelData<<8|(motorId&0x7f);
        }
        public uint buildID_获取设备ID(uint MotorID)
        {
            return (((uint)CMode.获取设备ID)<<24)|(0x0<<8)|(MotorID&0x7F);
        }
        public uint buildID_运控模式电机控制指令(uint motorID,double Tor=0)
        {
            double trueTor = Tor < -12 ? 0 : (Tor > 12 ? 24 : Tor + 12);
            
            return (uint)((uint)CMode.运控模式电机控制指令 << 24 | ((((int)(trueTor * 65535 / 24.0)) & 0xFFFF)) << 8 | (motorID & 0x7f));
        }
        public uint buildID_电机使能运行(uint motorID)
        {
            return (uint)CMode.电机使能运行 << 24 | (0x0 << 8) | (motorID & 0x7f);
        }
        public uint buildID_电机停止运行(uint MotorID)
        {
            return (uint)CMode.电机停止运行 << 24 | (0x0 << 8) | (MotorID & 0x7f);
        }
        public uint buildID_设置电机机械零位(uint MotorID)
        {
            return (uint)CMode.设置电机机械零位 << 24 | (0x0 << 8) | (MotorID & 0x7f);
        }
        public uint buildID_设置电机CAN_ID(uint MotorID,uint newMotorID)
        {
            return (uint)CMode.设置电机CAN_ID << 24 | (newMotorID&0x7f) << 16 | (0x0 << 8) | (MotorID & 0x7f);
        }
        public uint buildID_单个参数读取(uint MotorID)
        {
            return (uint)CMode.单个参数读取<<24|(0x0<<8)|(MotorID&0x7f);
        }
        public uint buildID_单个参数写入(uint motorID)
        {
            return (uint)CMode.单个参数写入 << 24 | (0x00) << 8 | (motorID & 0x7f);
        }
        public byte[] buildData()
        {
            byte[] Data =new byte[8];
            return Data;
        }
        public byte[] buildData_获取设备ID()
        {
            return buildData();
        }
        public byte[] buildData_运控模式电机控制指令(double 目标角度,double 目标角速度,double Kp,double Kd)
        {
            byte[] Data =new byte[8];
            double trueAngle=目标角度  > 4 * pi ? 8 * pi : (目标角度  < 4*pi ? 0 : 目标角度 + 4 * pi);
            double trueAngleSpeed = 目标角速度 < -30 ? 0 : (目标角速度 > 30 ? 60 : 目标角速度+30);
            double trueKp = Kp < 0 ? 0 : (Kp > 500 ? 500 : Kp);
            double trueKd = Kd < 0 ? 0 : (Kd > 5 ? 5 : Kd);
            Int16 byte12 = (Int16)(trueAngle * 65535 / (8 * pi));
            Int16 byte34 = (Int16)(trueAngleSpeed * 65535 / 60);
            Int16 byte56 = (Int16)(trueKp * 65535 / 500);
            Int16 byte78 = (Int16)(trueKd * 65535 / 5);
            Data[1] = (byte)(byte12 >> 8);
            Data[0]= (byte)(byte12 &0xff   );
            Data[3] = (byte)(byte34 >> 8);
            Data[2]= (byte)(byte34 &0xff );
            Data[5]= (byte)(byte56 >> 8);
            Data[4]=(byte)(byte56 &0xff );
            Data[7]=(byte)(byte78 >> 8);
            Data[6]=(byte)(byte78 &0xff );
            return Data;
        }
        public byte[] buildData_电机使能运行()
        {
            return buildData();
        }
        public byte[] buildData_电机停止运行(bool ClearFault)
        {
            byte[] Data = new byte[8];
            if (ClearFault)
            {
                Data[0] = 1;
                for (int i = 1; i < 8; i++)
                {
                    Data[i] = 0;
                }
                return Data;
            }
            else
            {
                return buildData();
            }
        }
        public byte[] buildData_设置电机机械零位()
        {
            byte[] Data = new byte[8];
                Data[0] = 1;
                for (int i = 1; i < 8; i++)
                {
                    Data[i] = 0;
                }
                return Data;
        }

        public byte[] buildData_设置电机ID()
        {
            return buildData();
        }
        public byte[] buildData_单个参数读取(paraList_RW index)
        {
            byte[] Data = new byte[8];
            Data[1] = (byte)(((ushort)index & 0xFF00) >> 8);
            Data[0] = (byte)(((ushort)index & 0xff));
            for (int i = 2; i < 8; i++)
            {
                Data[i] = 0;
            }
            return Data;
        }
        public byte[] buildData_单个参数读取(UInt16 index)
        {
            byte[] Data = new byte[8];
            Data[1] = (byte)(((ushort)index & 0xFF00) >> 8);
            Data[0] = (byte)(((ushort)index & 0xff));
            for (int i = 2; i < 8; i++)
            {
                Data[i] = 0;
            }
            return Data;
        }
        public byte[] buildData_单个参数写入(paraList_RW index, byte paraValue)
        {
            byte[] Data = new byte[8];
            Data[1] = (byte)(((ushort)index & 0xFF00) >> 8);
            Data[0] = (byte)(((ushort)index & 0xff));
            Data[4] = paraValue;
            return Data;
        }
        public byte[] buildData_单个参数写入(paraList_RW index, float paraValue)
        {
            byte[] Data = new byte[8];
            Data[1] = (byte)(((ushort)index & 0xFF00) >> 8);
            Data[0] = (byte)(((ushort)index & 0xff));
            BitConverter.GetBytes(paraValue).CopyTo(Data, 4);
            return Data;
        }

        public byte formatData(double min,double max,int targetmin, int targetmax,byte byteCount,bool unsigned,byte Index,double realdata)
        {
            double num = (targetmax - targetmin) * (realdata - min) / (max - min);
            return 0;
        }
    }
}
