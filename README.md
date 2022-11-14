# EZShare

在内网中任意Windows、Linux、Android机器之间共享文件

## 文件传输流程

```
发送端          接收端
  |------公钥---->|
  |<------OK------|
  |---文件概况---->|
  |<------OK------|
  |---传输文件---->|
  |-----CLOSE---->|
```

## 通信协议包头

加密数据包
```
{长度}{数据}
```

实际数据
```
EZSHARE/1.0\r\n
Content-Length: 111\r\n
Content-Type: text\r\n
Session-Id: -1\r\n
\r\n
BODY
```

## 接收端广播数据

接收端每秒会向255.255.255.255、端口11456发送UDP广播。

```
EZSHARE/1.0
Content-Length: 111
Content-Type: json

{
  "Version": "1.0.0",
  "HostName": "LAPTOP-1A1418SN",
  "TransportPort": 26705,
  "PublicKey": "",
  "Signature": ""
}
```

*TransportPort*为接收端随机监听的TCP端口号，*PublicKey*为base64编码的接收端公钥，*Signature*为整个数据包的签名。

## 发送端与接收端初始化TCP连接

### 发送端将自己的公钥传输给接收端

```
EZSHARE/1.0
Content-Length: 111
Content-Type: json

{
  "Version": "1.0.0",
  "HostName": "LAPTOP-1A1418SN",
  "SessionKey": "",
  "Identity": "{"PublicKey": "","Signature": ""}"
}
```

*Identity*为加密后的base64数据，解密后包含发送端的公钥和签名

### 接收端确认生成会话

```
EZSHARE/1.0
Content-Length: 2
Content-Type: Text
Session-Id: 1111ABCD

OK或CLOSE
```

## 发送端传输文件概况

```
EZSHARE/1.0
Content-Length: 111
Content-Type: json
Session-Id: 1111ABCD

{
  "TotalCount": 123,
  "TotalSize": 123,
}
```

## 发送端开始传输文件

压缩分块传输文件
```
EZSHARE/1.0
Content-Length: 111
Content-Type: file
Session-Id: 1111ABCD
Transfer-Encoding: gzip, chunked
Content-Description: pathname=realtive/path/filename

bytes
```

文件
```
EZSHARE/1.0
Content-Length: 111
Content-Type: file
Session-Id: 1111ABCD
Content-Description: pathname=realtive/path/filename

bytes
```

文件夹
```
EZSHARE/1.0
Content-Type: directory
Session-Id: 1111ABCD
Content-Description: pathname=realtive/path/

```

## 取消或完成文件传输

```
EZSHARE/1.0
Content-Length: 2
Content-Type: Text
Session-Id: 1111ABCD

CLOSE
```