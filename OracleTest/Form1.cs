using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace OracleTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            //写入
            string commandText = "insert into fsk_test_1 (COLUMN2,COLUMN3) values('PASS', 388)";//如果此处是变量，应该这样写26 ,('"+equipmentName+"','"+propertyName+"','"+ruleRequest+"','"+ruleRequestOther+"','"+ruleExplain+"')，别忘记单引号。
            //string Inset_barsaminfo_SQL = "insert into barsaminfo (BARCODE,CDATE,CTIME) values(10,20200112,120203)";
            //string Inset_barsamrec_SQL = "insert into barsamrec (SNUM,CDATE,CTIME) values(10,20200112,120203)";
            //int value = ExecuteNonQuery(commandText);
            Morecheck("388");
            //读取
            string sqlString = "Select * From fsk_test_1";
            DataTable dt = ExecuteDataTable(sqlString);
            dataGridView1.DataSource = dt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connStrFormat = "User ID={0};Password={1};Persist Security Info=True;Data Source=(DESCRIPTION = (ADDRESS_LIST= (ADDRESS = (PROTOCOL = TCP)(HOST ={2})(PORT ={3}))) (CONNECT_DATA = (SERVICE_NAME = {4})))", sqlStrFormat="select * from {0}";
            string strIP = textBox1.Text.Trim(), strPort = textBox2.Text.Trim(), strDb = textBox3.Text.Trim(), strUser = textBox4.Text.Trim(), strPwd = textBox5.Text.Trim(), strTable = textBox6.Text.Trim();
            if (strIP == "" || strPort == "" || strDb == "" || strUser == "" || strTable == "")
            {
                MessageBox.Show("连接信息不能为空，密码除外");
                return;
            }            
            using (OracleConnection oconn = new OracleConnection(string.Format(connStrFormat, strUser, strPwd, strIP, strPort, strDb)))           
            {
                try
                {
                    oconn.Open();
                    DataSet dst = new DataSet();
                    using (OracleDataAdapter oadp = new OracleDataAdapter(string.Format(sqlStrFormat, strTable), oconn))
                    {
                        oadp.Fill(dst);
                    }
                    if (dst.Tables.Count > 0)
                    {
                        dataGridView1.DataSource = dst.Tables[0];
                    }
                    else
                    {
                        MessageBox.Show("查询失败语法错误");
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("查询失败 " + err.Message);
                }
                finally
                {
                    if (oconn.State == ConnectionState.Open)
                    {
                        oconn.Close();
                    }
                }
            }            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private string connstr = "User ID=ZHANG;Password=ZHANG;Persist Security Info=True;Data Source=(DESCRIPTION = (ADDRESS_LIST= (ADDRESS = (PROTOCOL = TCP)(HOST =192.168.101.184)(PORT =1521))) (CONNECT_DATA = (SERVICE_NAME = ORCL)))";
        private void button2_Click(object sender, EventArgs e)
        {
            //string connstr = "User ID=ZHANG;Password=ZHANG;Persist Security Info=True;Data Source=(DESCRIPTION = (ADDRESS_LIST= (ADDRESS = (PROTOCOL = TCP)(HOST =192.168.101.37)(PORT =1521))) (CONNECT_DATA = (SERVICE_NAME = ORCL)))";
            string sqlstr= "Select * From fsk_test_1"; ;
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleDataAdapter adp = new OracleDataAdapter(sqlstr,conn))
                {
                    DataSet dst = new DataSet();
                    try
                    {
                        conn.Open();
                        adp.Fill(dst);
                    }
                    catch{}
                    finally
                    {
                        adp.Dispose();
                        conn.Dispose();
                        if (dst.Tables.Count > 0)
                        {
                            dataGridView1.DataSource = dst.Tables[0];
                        }
                        else
                        {
                            dataGridView1.DataSource = new DataTable();
                        }
                    }
                }
            }
        }
        //string sql_pn = "Select COLUMN2 From fsk_test_1 where COLUMN3 ='" + tiaoma + "'";
        //复判模式上传数据前先检查下数据库的条码，数据库里无条码、或者条码结果为PASS不进行上传；有条码，且结果为FAIL上传
        //FP_Morecheck传入条码，返回true为数据库存在该条码，且结果为FAIL；返回false为数据库不存在该条码或存在该条码，但结果为PASS
        private bool FP_Morecheck(string tiaoma)
        {
            string sql_pn = "select BARCODE from ictdata.TED_FVI_DATA where BARCODE ='" + tiaoma + "'";            
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                using (OracleDataAdapter adp = new OracleDataAdapter(sql_pn, conn))
                {
                    DataSet dst = new DataSet();
                    try
                    {
                        conn.Open();
                        adp.Fill(dst);
                        List<CHECK_RESULT> vs_result = new List<CHECK_RESULT>();
                        if (dst.Tables.Count > 0)
                        {
                            if (dst.Tables[0].Rows[0].ItemArray[0].ToString() == "FAIL")
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }                        
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                    finally
                    {
                        adp.Dispose();
                        conn.Dispose();
                    }
                }
            }
        }
        //复判模式上传数据前先检查下数据库的条码，数据库里无条码、或者条码结果为PASS不进行上传；有条码，且结果为FAIL上传
        public bool Contrastbarcode(string tiaoma)
        {
            //tiaoma = "GQ41213012GQ2C28A";
            if (tiaoma == null)
            {
                return true;
            }
            else
            {
                string sql_pn = "select BARCODE,LINEID from ictdata.TED_FVI_DATA where BARCODE ='" + tiaoma + "'and LINEID='D9-99'";
                try
                {
                    OracleConnection conn = new OracleConnection(connstr);
                    conn.Open();
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql_pn;
                    OracleDataReader odr = cmd.ExecuteReader();
                    if (odr.HasRows)
                    {
                        //有码
                        //判断有几个，有1个PASS的，true，有多个的要求最后2个位PASS
                        while (odr.Read())
                        {
                            //odr.
                        }
                        odr.Close();//关闭DataReader对象
                        return true;
                    }
                    else
                    {
                        //无码
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        #region 执行SQL语句,返回受影响行数
        public int ExecuteNonQuery(string sql, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    int i = cmd.ExecuteNonQuery();
                    conn.Close();
                    return i;
                }
            }
        }
        #endregion
        #region 执行SQL语句,返回DataTable;只用来执行查询结果比较少的情况
        public DataTable ExecuteDataTable(string sql, params OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable datatable = new DataTable();
                    adapter.Fill(datatable);
                    conn.Close();
                    return datatable;
                }
            }
        }
        #endregion
    }

    #region 数据库比对 判断时间和结果
    public class CHECK_RESULT
    {
        public string check_result;
        public string check_datetime;
    }
    #endregion
}
