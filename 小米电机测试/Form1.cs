
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
namespace 小米电机测试
{
    public partial class Form1 : Form
    {
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;
        public Form1()
        {
            InitializeComponent();

        }
        private bool motorRunFlag = false;
        private unsafe void Form1_Load(object sender, EventArgs e)
        {
            
            motorRunFlag = false;
            textBox1.Width = this.Width / 2;

            ControlCAN1 CCAN = new ControlCAN1();
            VCI_BOARD_INFO1 pinfo = new VCI_BOARD_INFO1();
            VCI_INIT_CONFIG initCfg = new VCI_INIT_CONFIG();
            baudRate_Enum baudRateCode = baudRate_Enum.B1000K;

            initCfg.AccCode = 0x00000000;
            initCfg.AccMask = 0xFFFFFFFF;
            initCfg.Filter = Filter_Enum.All;
            initCfg.Mode = Mode_Enum.Normal;
            initCfg.Reserved = 0;
            initCfg.Timing0 = ControlCAN1.Timing0(baudRateCode);
            initCfg.Timing1 = ControlCAN1.Timing1(baudRateCode);


            uint count = ControlCAN1.VCI_FindUsbDevice(ref pinfo);
            if (count == 0)
            {
                return;
            }
            else if (count == 1)
            {
                uint returnCode = ControlCAN1.VCI_OpenDevice(DEV_USBCAN2, 0, 0);
                if (returnCode == 0)
                {
                    return;
                }
                else
                {
                    uint returncode = ControlCAN1.VCI_InitCAN(DEV_USBCAN2, 0, 0, ref initCfg);
                    if (returnCode != 1)
                    {
                        return;
                    }
                    else
                    {
                        uint rel = ControlCAN1.VCI_StartCAN(DEV_USBCAN2, 0, 0);
                        timer1.Start();

                    }
                }
            }
        }

        unsafe private void timer1_Tick(object sender, EventArgs e)
        {

            string str = "";
            VCI_CAN_OBJ[] pReceive = new VCI_CAN_OBJ[2500];
            uint rel = ControlCAN1.VCI_Receive(DEV_USBCAN2, 0, 0, ref pReceive[0], 2500, 0);
            if (rel > 0)
            {
                for (int i = 0; i < rel; i++)
                {
                    byte[] data = new byte[8];
                    for (var j = 0; j < pReceive[i].DataLen; j++)
                    {
                        data[j] = pReceive[i].Data[j];
                    }
                    string str2 = "";

                    switch ((pReceive[i].ID & 0xff000000) >> 24)
                    {
                        case 0:
                            str2 = "\tMCU_ID";
                            break;
                        case 2:
                            str2 = "\tMotor_Return" + $"CAN_ID{(pReceive[i].ID & 0x0000FF00) >> 8:X2}" + $"故障信息{(pReceive[i].ID & 0x003F0000) >> 16:X2}" + $"模式状态{(pReceive[i].ID & 0x00C00000) >> 22:X2}";
                            break;
                        case 17:
                            string str3 = "";
                            if (data[0] == 0x05 && data[1] == 0x70)
                            {
                                str3 = data[4].ToString();
                            }
                            else
                            {
                                str3 = BitConverter.ToSingle(data, 4).ToString();
                            }
                            try
                            {

                                str2 = "\tPara_Read\t" + Enum.GetName(typeof(CyberGear.paraList_RW), ((uint)(data[1])) << 8 | data[0]).ToString().PadRight(15) + "\t" + str3;
                            }
                            catch (Exception)
                            {

                                str2 = "\tPara_Read\t";
                                //throw;
                            }
                            break;
                        default:
                            str2 = "\tERR_RETURN";
                            break;
                    }
                    str += pReceive[i].ID.ToString("X8").PadRight(12);
                    str += "\t";
                    for (int j = 0; j < pReceive[i].DataLen; j++)
                    {
                        byte v = pReceive[i].Data[j];
                        str += v.ToString("X2");
                        str += " ";
                    }
                    str += str2;
                    str += "\r\n";
                }
                textBox1.Text += str;
            }
        }
        unsafe private void timer2_Tick(object sender, EventArgs e)
        {

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            textBox1.Width = this.Width / 2;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        unsafe private void button1_Click(object sender, EventArgs e)
        {
            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_获取设备ID(0x7f);
            byte[] data = cyberGear.buildData_获取设备ID();
            pSend.DataLen = 8;
            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            if (motorRunFlag == false)
            {

                pSend.ID = cyberGear.buildID_电机使能运行(0x7f);
                data = cyberGear.buildData_电机使能运行();
                timer2.Start();
                pSend.DataLen = 8;
                for (int i = 0; i < data.Length; i++)
                {
                    pSend.Data[i] = data[i];
                }
            }
            else
            {

                pSend.ID = cyberGear.buildID_电机停止运行(0x7f);
                data = cyberGear.buildData_电机停止运行(true);
                timer2.Stop();
                pSend.DataLen = 8;
                for (int i = 0; i < data.Length; i++)
                {
                    pSend.Data[i] = data[i];
                }
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            motorRunFlag = !motorRunFlag;

        }
        unsafe private void button_n_Click(object sender, EventArgs e)
        {

            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();

            ////////////////////////////////////////////////////////
            pSend.ID = (0x0 << 24) | (0x0 << 8) | 0x7f;
            byte[] data = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            pSend.DataLen = 8;
            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            ////////////////////////////////////////////////////

            pSend.ID = cyberGear.buildID_单个参数读取(0x7f);
            data = cyberGear.buildData_单个参数读取(CyberGear.paraList_RW.run_mode);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);





            ////////////////////////////////////////////////////

            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.run_mode, (byte)CyberGear.para_run_mode.运控模式);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            ////////////////////////////////////////////////////
            pSend.ID = cyberGear.buildID_电机使能运行(0x7f);
            data = cyberGear.buildData_电机使能运行();
            pSend.DataLen = 8;
            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            //////////////////////////////////////////////////////
            pSend.ID = cyberGear.buildID_运控模式电机控制指令(0x7f, 0.01);
            data = cyberGear.buildData_运控模式电机控制指令(-4 * 3.14, 3 * 2 * 3.14 / 60, 0.1, 0.1);
            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            ///////////////////////////////////////////////////////
            pSend.ID = cyberGear.buildID_运控模式电机控制指令(0x7f, 0.01);
            data = cyberGear.buildData_运控模式电机控制指令(4 * 3.14, 3 * 2 * 3.14 / 60, 0.1, 0.1);
            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
            ///////////////////////////////////////////////////////
            pSend.ID = cyberGear.buildID_电机停止运行(0x7f);
            data = cyberGear.buildData_电机停止运行(true);
            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ControlCAN1.VCI_CloseDevice(4, 0);
            timer1.Stop();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        unsafe private void button2_Click(object sender, EventArgs e)
        {

            foreach (CyberGear.paraList_RW v in Enum.GetValues(typeof(CyberGear.paraList_RW)))
            {

                VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
                CyberGear cyberGear = new CyberGear();
                pSend.ID = cyberGear.buildID_单个参数读取(0x7f);
                byte[] data = cyberGear.buildData_单个参数读取(v);

                for (int i = 0; i < data.Length; i++)
                {
                    pSend.Data[i] = data[i];
                }
                pSend.DataLen = 8;
                pSend.SendType = 0;
                pSend.RemoteFlag = 0;
                pSend.ExternFlag = 1;
                ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
                Thread.Sleep(100);
            }

        }

        unsafe private void button3_Click(object sender, EventArgs e)
        {

            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            pSend.DataLen = 8;
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            byte[] data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.run_mode, (byte)CyberGear.para_run_mode.运控模式);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);

        }

        unsafe private void button4_Click(object sender, EventArgs e)
        {

            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            pSend.DataLen = 8;
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            byte[] data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.run_mode, (byte)CyberGear.para_run_mode.位置模式);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
        }

        unsafe private void button5_Click(object sender, EventArgs e)
        {

            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            pSend.DataLen = 8;
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            byte[] data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.run_mode, (byte)CyberGear.para_run_mode.速度模式);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
        }

        unsafe private void button6_Click(object sender, EventArgs e)
        {

            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            pSend.DataLen = 8;
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            byte[] data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.run_mode, (byte)CyberGear.para_run_mode.电流模式);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
        }

        unsafe private void button7_Click(object sender, EventArgs e)
        {

            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            pSend.DataLen = 8;
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            byte[] data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.limit_cur, 2f);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);

            Thread.Sleep(2000);
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.spd_ref, 1f);

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];
            }
            ControlCAN1.VCI_Transmit(4, 0, 0, ref pSend, 1);
        }

        unsafe private void trackBar1_Scroll(object sender, EventArgs e)
        {
            VCI_CAN_OBJ pSend = new VCI_CAN_OBJ();
            CyberGear cyberGear = new CyberGear();
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            pSend.DataLen = 8;
            pSend.SendType = 0;
            pSend.RemoteFlag = 0;
            pSend.ExternFlag = 1;
            pSend.ID = cyberGear.buildID_单个参数写入(0x7f);
            byte[] data = cyberGear.buildData_单个参数写入(CyberGear.paraList_RW.spd_ref, (float)(this.trackBar1.Value / 10.0));

            for (int i = 0; i < data.Length; i++)
            {
                pSend.Data[i] = data[i];                    