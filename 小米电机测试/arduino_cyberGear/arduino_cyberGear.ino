// 定义 CMode 枚举
 typedef enum CMode {
	GetMotorID = 0,
	MovingControl = 1,
	MotorReturnData = 2,
	MotorEnable = 3,
	MotorDisable = 4,
	SetMechRef = 6,
	SetCAN_ID = 7,
	SingleParaRead = 17,
	SingleParaWrite = 18,
	FaultReturn = 21,
	SetBaudrate = 22
} CMode ;

// 定义 paraList_RW 枚举
 typedef enum paraList_RW {
	run_mode = 0x7005,
	iq_ref = 0x7006,
	spd_ref = 0x700A,
	limit_torque = 0x700B,
	cur_kp = 0x7010,
	cur_ki = 0x7011,
	cur_filt_gain = 0x7014,
	loc_ref = 0x7016,
	limit_spd = 0x7017,
	limit_cur = 0x7018
}paraList_RW;

// 定义 para_run_mode 枚举
 typedef enum para_run_mode {
	MovingControlMode = 0,
	PositionMode = 1,
	SpeedMode = 2,
	CurrentMode = 3
}para_run_mode ;

// 构建 ID 函数
uint32_t buildID(CMode cMode, uint32_t middelData, uint32_t motorId) {
	
	return ((uint32_t)cMode << 24 | (uint32_t)middelData << 8 | (motorId & 0x7f));
}

// 构建 ID 获取设备 ID 函数
uint32_t buildID_GetDeviceID(uint32_t MotorID) {
	return ((uint32_t)GetMotorID << 24 | (uint32_t)0x0 << 8 | MotorID & 0x7F);
}

// 构建 ID 运控模式电机控制指令函数
uint32_t buildID_MovingControl(uint32_t motorID, double Tor) {
	double trueTor = Tor < -12 ? 0 : (Tor > 12 ? 24 : Tor + 12);

	return ((uint32_t)MovingControl << 24) | (((uint32_t)(trueTor * 65535 / 24.0)) & 0xFFFF) << 8 | (motorID & 0x7f);
}

// 构建 ID 电机使能运行函数
uint32_t buildID_MotorEnable(uint32_t motorID) {
	return ((uint32_t)MotorEnable << 24 | (uint32_t)0x0 << 8 | motorID & 0x7f);
}

// 构建 ID 电机停止运行函数
uint32_t buildID_MotorDisable(uint32_t MotorID) {
	return ((uint32_t)MotorDisable << 24 | (uint32_t)0x0 << 8 | MotorID & 0x7f);
}

// 构建 ID 设置电机机械零位函数
uint32_t buildID_SetMechRef(uint32_t MotorID) {
	return ((uint32_t)SetMechRef << 24 | (uint32_t)0x0 << 8 | MotorID & 0x7f);
}

// 构建 ID 设置电机 CAN_ID 函数
uint32_t buildID_SetCAN_ID(uint32_t MotorID, uint32_t newMotorID) {
	return ((uint32_t)SetCAN_ID << 24 | (uint32_t)(newMotorID & 0x7f) << 16 | (uint32_t)0x0 << 8 | MotorID & 0x7f);
}

// 构建 ID 单个参数读取函数
uint32_t buildID_SingleParaRead(uint32_t MotorID) {
	return ((uint32_t)SingleParaRead << 24 | (uint32_t)0x0 << 8 | MotorID & 0x7f);
}

// 构建 ID 单个参数写入函数
uint32_t buildID_SingleParaWrite(uint32_t motorID) {
	return ((uint32_t)SingleParaWrite << 24 | (uint32_t)0x0 << 8 | motorID & 0x7f);
}

// 构建数据
uint8_t* buildData(byte* Data) {
	for (int i = 0; i < 8; i++) {
		Data[i] = 0;
	}
}

// 构建数据获取设备 ID 函数
uint8_t* buildData_GetMotorID(byte* Data) {
	return buildData(Data);
}
// 构建运控模式电机控制指令的数据字节数组
void buildData_MotorControl(double targetAngle, double targetAngularSpeed, double kp, double kd, byte* Data) {
	// 字节数组 Data

	// 将目标角度限制在 4π 范围内（8π 为一个周期）
	if (targetAngle > 4 * M_PI) {
		targetAngle -= 8 * M_PI;
	}
	else if (targetAngle < -4 * M_PI) {
		targetAngle += 8 * M_PI;
	}

	// 将目标角速度限制在 -30 到 30 范围内
	if (targetAngularSpeed < -30) {
		targetAngularSpeed = 0;
	}
	else if (targetAngularSpeed > 30) {
		targetAngularSpeed = 60;
	}

	// 将 kp 和 kd 限制在范围内
	if (kp < 0) {
		kp = 0;
	}
	else if (kp > 500) {
		kp = 500;
	}

	if (kd < 0) {
		kd = 0;
	}
	else if (kd > 5) {
		kd = 5;
	}
	// 将角度和角速度转换为 16 位整数
	int16_t byte12 = (int16_t)(targetAngle * 65535 / (8 * M_PI));
	int16_t byte34 = (int16_t)(targetAngularSpeed * 65535 / 60);
	// 将 kp 和 kd 转换为 16 位整数
	int16_t byte56 = (int16_t)(kp * 65535 / 500);
	int16_t byte78 = (int16_t)(kd * 65535 / 5);
	// 填充字节数组 Data
	Data[1] = (uint8_t)(byte12 >> 8);
	Data[0] = (uint8_t)(byte12 & 0xFF);
	Data[3] = (uint8_t)(byte34 >> 8);
	Data[2] = (uint8_t)(byte34 & 0xFF);
	Data[5] = (uint8_t)(byte56 >> 8);
	Data[4] = (uint8_t)(byte56 & 0xFF);
	Data[7] = (uint8_t)(byte78 >> 8);
	Data[6] = (uint8_t)(byte78 & 0xFF);
}
// 构建电机使能运行的数据字节数组
byte* buildData_MotorEnable(byte* Data) {
	// 将 Data 的所有字节设置为 0
	for (int i = 0; i < 8; i++) {
		Data[i] = 0;
	}
}
// 构建电机停止运行的数据字节数组
void buildData_MotorStop(bool ClearFault, byte* Data) {
	// 如果需要清除故障标志
	if (ClearFault) {
		Data[0] = 1;
	}
	else {
		Data[0] = 0;
	}
	// 将其他字节设置为 0
	for (int i = 1; i < 8; i++) {
		Data[i] = 0;
	}
}

// 构建设置电机机械零位的数据字节数组
void buildData_SetMechRef(byte* Data) {
	Data[0] = 1;
	for (int i = 1; i < 8; i++) {
		Data[i] = 0;
	}
}

// 构建设置电机 ID 的数据字节数组
void buildData_SetCAN_ID(byte* Data) {
	buildData(Data);
}

// 构建读取单个参数的命令数据字节数组
void buildData_SingleParaRead(paraList_RW index, byte* Data) {
	Data[1] = (index >> 8) & 0xFF;
	Data[0] = index & 0xFF;
	for (int i = 2; i < 8; i++) {
		Data[i] = 0;
	}
}

// 构建读取单个参数的命令数据字节数组
void buildData_SingleParaRead(u16 index, byte* Data) {

	Data[1] = (index >> 8) & 0xFF;
	Data[0] = index & 0xFF;
	for (int i = 2; i < 8; i++) {
		Data[i] = 0;
	}
}
// 构建写入单个参数的命令数据字节数组
void buildData_SingleParaWrite(paraList_RW index, byte paraValue, byte* Data) {
	Data[1] = (index >> 8) & 0xFF;
	Data[0] = index & 0xFF;
	Data[2] = 0;
	Data[3] = 0;
	Data[4] = paraValue;
  Data[5]=0;
  Data[6]=0;
  Data[7]=0;
}
void buildData_SingleParaWrite(paraList_RW index, float paraValue, byte* Data) {
	Data[1] = (index >> 8) & 0xFF;
	Data[0] = index & 0xFF;
	Data[2] = 0;
	Data[3] = 0;
	memcpy(Data + 4, &paraValue, 4);
}


// The setup() function runs once each time the micro-controller starts
#define baudrate 9600
void setup() {
	Serial.begin(baudrate);
	Serial.println("串口初始化成功");
	
}
// Add the main program code into the continuous loop() function

void loop() {
	/*
	使用方法：
	1、buildID方法生成帧ID，使用buildData方法生成Data数据
	2、使用相应的CAN工具库发送帧数据
	3、对接收到的帧数据进行解析（未实现）
	*/
	u32 id = buildID_SingleParaWrite(0x7f);
	byte data[8];
	buildData_SingleParaWrite(run_mode, (byte)2, data);
	if (Serial.available()) {
		char receivedChar = Serial.read();

		// 打印接收到的数据
		Serial.print("接收到:");
		Serial.println(receivedChar);
	}
}