using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;

namespace 档案汇总
{
    class QzTcpClinet
    {
        TcpClient m_tcpClient;
        byte[] m_Buffer = new byte[8192];
        MemoryStream m_memStream = new MemoryStream();

        public QzTcpClinet(TcpClient s)
        {
            m_tcpClient = s;
        }

        public QzTcpClinet()
        {
            m_tcpClient = new TcpClient();
        }

        public TcpClient getTcpClient()
        {
            return m_tcpClient;
        }

        public void SendCmd(byte[] cmdbytes)
        {
            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (BinaryWriter binWriter = new BinaryWriter(memStream))
                    {
                        if (cmdbytes.Length > 300)
                        {
                            using (MemoryStream mstm = new MemoryStream())
                            {
                                using (GZipStream gstm = new GZipStream(mstm, CompressionMode.Compress))
                                {
                                    gstm.Write(cmdbytes, 0, cmdbytes.Length);
                                }
                                byte[] gzipbuf = mstm.ToArray();
                                binWriter.Write(gzipbuf.Length);
                                binWriter.Write((byte)1);
                                binWriter.Write(gzipbuf);
                            }
                        }
                        else
                        {
                            binWriter.Write(cmdbytes.Length);
                            binWriter.Write((byte)0);
                            binWriter.Write(cmdbytes);
                        }

                        NetworkStream ns = m_tcpClient.GetStream();
                        byte[] waitSend = memStream.ToArray();
                        ns.BeginWrite(waitSend, 0, waitSend.Length, new AsyncCallback(OnSendCallBack), waitSend);
                    }
                }
            }
            catch(Exception ex)
            {
                Global.logger.Debug(ex.ToString());
                onDisconnected();
            }
        }

        private void OnSendCallBack(IAsyncResult ar)
        {
            try
            {
                this.m_tcpClient.GetStream().EndWrite(ar);
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
            }
        }

        public void Connect(string Ip, short port)
        {
            m_tcpClient.BeginConnect(Ip, port, new AsyncCallback(ConnectedCallBack), this);
        }

        protected void ConnectedCallBack(IAsyncResult ar)
        {
            try
            {
                m_tcpClient.EndConnect(ar);
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
                onDisconnected();
                return;
            }
            StartRecive();
            onConnencted();
        }

        protected virtual void onConnencted()
        {

        }

        protected virtual void onDisconnected()
        {

        }

        public virtual void Start()
        {
            try
            {
                StartRecive();
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
                onDisconnected();
                Close();
            }
        }

        public void StartRecive()
        {
            NetworkStream ns = m_tcpClient.GetStream();
            ns.BeginRead(m_Buffer, 0, m_Buffer.Length, new AsyncCallback(OnRecived), this);
        }

        protected void OnRecived(IAsyncResult ar)
        {
            int nReadBytes;
            bool readMsgOk = false;
            try
            {
                nReadBytes = m_tcpClient.GetStream().EndRead(ar);
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
                onDisconnected();
                Close();
                return;
            }
            m_memStream.Write(m_Buffer, 0, nReadBytes);
            m_memStream.Seek(0, SeekOrigin.Begin);
            BinaryReader binReader = new BinaryReader(m_memStream);

            if (m_memStream.Length > 5)
            {
                do
                {
                    int msglen;
                    byte btZip;
                    if (m_memStream.Length - m_memStream.Position < 5)
                    {
                        break;
                    }
                    msglen = binReader.ReadInt32();
                    btZip = binReader.ReadByte();
                    if (msglen + 5 < binReader.BaseStream.Length)
                    {
                        break;
                    }
                    readMsgOk = true;
                    if (btZip == 0)
                    {
                        byte[] msg = binReader.ReadBytes(msglen);
                        OnMsg(msg);
                    }
                    else
                    {
                        using (GZipStream gzipStream = new GZipStream(new MemoryStream(binReader.ReadBytes(msglen))
                            , CompressionMode.Decompress))
                        {
                            byte[] gzip_buffer = new byte[8192];
                            int gzip_readbytes = 0;
                            using (MemoryStream mstm = new MemoryStream())
                            {
                                do
                                {
                                    gzip_readbytes = gzipStream.Read(gzip_buffer, 0, gzip_buffer.Length);
                                    if (gzip_readbytes > 0)
                                    {
                                        mstm.Write(gzip_buffer, 0, gzip_readbytes);
                                    }
                                } while (gzip_readbytes > 0);
                                OnMsg(mstm.ToArray());
                            }
                        }
                    }
                } while (true);
            }

            if (readMsgOk)
            {
                if (binReader.BaseStream.Position < binReader.BaseStream.Length)        //剩余字节，放到下一次读取中
                {
                    byte[] bytes = binReader.ReadBytes((int)binReader.BaseStream.Length - (int)binReader.BaseStream.Position);
                    m_memStream.Close();
                    m_memStream = new MemoryStream(bytes);
                }
                else
                {
                    m_memStream.Close();
                    m_memStream = new MemoryStream();
                }
            }

            StartRecive();
        }

        protected virtual void OnMsg(byte[] msgbytes)
        {

        }

        public void Close()
        {
            try
            {
                m_tcpClient.Close();
                m_memStream.Close();
            }
            catch (Exception ex)
            {
                Global.logger.Debug(ex.ToString());
            }
        }
    }
}
