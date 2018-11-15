using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace VkAPITutorial
{
    public partial class MainForm : Form
    {
        VkAPI _ApiRequest;
        private string _Token;  //Токен, использующийся при запросах
        private string _UserId;  //ID пользователя
        private Dictionary<string, string> _Response;  //Ответ на запросы

        public MainForm()
        {
            InitializeComponent();
        }

        private void GetUserToken(string s)
        {
            File.WriteAllText("UserInf.txt", s + "\n");
            //File.AppendAllText("UserInf.txt", URL[5]);
            //this.Visible = false;
        }
        public bool VkAuth1(string login, string password)
        {
            try
            {
                var Vk = new HttpClient();
                Vk.DefaultRequestHeaders.Add("Connection", "close");
                //client_id = 3697615
                //client_secret = AlVXZFMUqyrnABp8ncuU
                //https://oauth.vk.com/token?grant_type=password&client_id=1914441&client_secret=***&username=***&password=***&v=5.87&2fa_supported=1
                string url = string.Format("https://oauth.vk.com/token?grant_type=password&client_id=3697615&client_secret=AlVXZFMUqyrnABp8ncuU&" +
                    "&username={0}&password={1}&&v=5.87&2fa_supported=1",
                login, password);


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Host = "oauth.vk.com";
                request.UserAgent = "qwert";
                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = false;

                using (HttpWebResponse responsevk = (HttpWebResponse)request.GetResponse())
                using (var stream = responsevk.GetResponseStream())
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {

                    string sReplay = streamReader.ReadToEnd();
                    Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sReplay);
                    string token;
                    if (Dict.TryGetValue("access_token", out token))
                    {
                        GetUserToken(token);
                        return true;
                    }

                }
            }
            catch(Exception ex)
            {

            }
            return false;
        }

        private void Button_GetToken_Click_1(object sender, EventArgs e)
        {
            AuthorizationForm GetToken = new AuthorizationForm();
            GetToken.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                StreamReader ControlInf = new StreamReader("UserInf.txt");
                _Token = ControlInf.ReadLine();
                _UserId = ControlInf.ReadLine();
                ControlInf.Close();
                if (_Token != null)
                {
                    _ApiRequest = new VkAPI(_Token);
                    string[] Params = { "city", "country", "photo_max" };
                    _Response = _ApiRequest.GetInformation(_UserId, Params);
                    if (_Response != null)
                    {
                        User_ID.Text = _UserId;
                        User_Photo.ImageLocation = _Response["photo_max"];
                        User_Name.Text = _Response["first_name"];
                        User_Surname.Text = _Response["last_name"];
                        User_Country.Text = _ApiRequest.GetCountryById(_Response["country"]);
                        User_City.Text = _ApiRequest.GetCityById(_Response["city"]);
                        Button_GetToken.Visible = false;
                    }
                }
            }
            catch { }
        }

        private void Button_GetInformation_Click_1(object sender, EventArgs e)
        {
            try
            {
                StreamReader ControlInf = new StreamReader("UserInf.txt");
                _Token = ControlInf.ReadLine();
                ControlInf.Close();
                _ApiRequest = new VkAPI(_Token);
                _UserId = User_ID.Text;
                string[] Params = { "city", "country", "photo_max" };
                _Response = _ApiRequest.GetInformation(_UserId, Params);
                if (_Response != null)
                {
                    User_ID.Text = _UserId;
                    User_Photo.ImageLocation = _Response["photo_max"];
                    User_Name.Text = _Response["first_name"];
                    User_Surname.Text = _Response["last_name"];
                    User_Country.Text = _ApiRequest.GetCountryById(_Response["country"]);
                    User_City.Text = _ApiRequest.GetCityById(_Response["city"]);
                    Button_GetToken.Visible = false;
                }
            }
            catch
            {

            }
        }

        List<LoginData> m_auth_vars = new List<LoginData>();

        private void button1_Click(object sender, EventArgs e)
        {

            //auth_vars["89671723975"] = "power321";
            m_auth_vars.Add(new LoginData { login = "89261719444", password = "$Power321"}) ;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void SendMessage2Users()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string contents = File.ReadAllText(@"G:\WORK_SPACES\GitHub1\Left_Front\VkAPITutorial-master\left_front.txt", Encoding.UTF8);
                    StreamReader ControlInf = new StreamReader("UserInf.txt");
                    _Token = ControlInf.ReadLine();
                    ControlInf.Close();
                    _ApiRequest = new VkAPI(_Token);
                    var ds = GetDataSet(conn, "SELECT TOP(1) * from users where can_write_private_message='1' and ISNULL(message_sent, 0) = 0");

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string user_id = dr["user_id"].ToString();
                        _Response = _ApiRequest.SendMessage(contents, user_id);

                        int message_sent = 1;
                        if (_Response != null)
                        {
                            string resp = _Response["response"];
                            if(resp == "error")
                            {
                                message_sent = Convert.ToInt32(_Response["error_code"]);
                            }
                            listBox1.Items.Add(resp);
                        }
                        
                        using (SqlCommand cmd = new SqlCommand(string.Format("UPDATE users SET message_sent = {0} where user_id='{1}'", message_sent, user_id), conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        System.Threading.Thread.Sleep(3000);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public DataSet GetDataSet(SqlConnection conn, string SQL)
        {
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = SQL;
            da.SelectCommand = cmd;
            DataSet ds = new DataSet();

            //conn.Open();
            da.Fill(ds);
            //conn.Close();

            return ds;
        }

        private void AddObjectToDb(Item obj1, SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = @"INSERT INTO users([user_id],[message_sent],[photo_id],[verified]" +
                        ",[sex],[bdate],[city],[country],[home_town],[has_photo],[can_write_private_message],[can_send_friend_request]) " +
                        "VALUES(@user_id,@message_sent,@photo_id,@verified,@sex,@bdate,@city,@country,@home_town,@has_photo,@can_write_private_message,@can_send_friend_request)";
                cmd.Parameters.AddWithValue("@user_id", obj1.id);
                cmd.Parameters.AddWithValue("@message_sent", 0);
                cmd.Parameters.AddWithValue("@photo_id", obj1.photo_id);
                cmd.Parameters.AddWithValue("@verified", obj1.verified);
                cmd.Parameters.AddWithValue("@sex", obj1.sex);
                cmd.Parameters.AddWithValue("@bdate", obj1.bdate);
                if(obj1.city != null)
                {
                    cmd.Parameters.AddWithValue("@city", obj1.city.id);
                }
                if(obj1.country != null)
                {
                    cmd.Parameters.AddWithValue("@country", obj1.country.id);
                }                
                cmd.Parameters.AddWithValue("@home_town", obj1.home_town);
                cmd.Parameters.AddWithValue("@has_photo", obj1.has_photo);
                cmd.Parameters.AddWithValue("@can_write_private_message", obj1.can_write_private_message);
                cmd.Parameters.AddWithValue("@can_send_friend_request", obj1.can_send_friend_request);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ee)
                {
                    if(ee.ErrorCode != -2146232060)
                    {
                        int hhh = 0;
                    }

                }
                catch (Exception ee)
                {
                    int iuu = 0;
                }

            }
        }
        private void buttonUpdateTable_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                progressBar1.Maximum = (2002- 1950)*12;
                progressBar1.Value = 0;
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
                buttonUpdateTable.Enabled = false;
            }
        }

        string ConnectionString = @"Data Source = ASUS\FULL2014; Initial Catalog = vk_db; Integrated Security = True";
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            


            //DataSet db = GetDataSet(ConnectionString, "select * from users");

            //for (int i = 0; i < db.Tables[0].Rows.Count; i++)
            //{
            //    string user_id = db.Tables[0].Rows[i].Field<string>("user_id");
            //    short message_sent = db.Tables[0].Rows[i].Field<short>("message_sent");
            //}

            try
            {
                StreamReader ControlInf = new StreamReader("UserInf.txt");
                _Token = ControlInf.ReadLine();
                ControlInf.Close();
                _ApiRequest = new VkAPI(_Token);
                _UserId = User_ID.Text;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    int counter = 0;
                    conn.Open();
                    
                    int i = 0;
                    for (int y = 2002; y > 1950; y--)
                    {
                        for (int m = 1; m <= 12; m++)
                        {
                            counter++;
                            worker.ReportProgress(counter);
                            var Response = _ApiRequest.users_search(m, y);
                            System.Threading.Thread.Sleep(3000);
                            if (Response.response != null)
                            {
                                if(Response.response.count > 1000)
                                {
                                    int test = 0;
                                }
                                for (int j = 0; j < Response.response.items.Count; j++)
                                {
                                    AddObjectToDb(Response.response.items[j], conn);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            try
            {
                progressBar1.Value = e.ProgressPercentage;
            }
            catch(Exception ex)
            {

            }
            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonUpdateTable.Enabled = true;
        }

        LoginData m_cur_LoginData = null;
        int m_send_counter = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(m_cur_LoginData == null)
            {
                m_send_counter = 0;
                if (m_auth_vars.Count > 0)
                {
                    m_cur_LoginData = m_auth_vars[0];
                    m_auth_vars.RemoveAt(0);
                    if (!VkAuth1(m_cur_LoginData.login, m_cur_LoginData.password))
                    {
                        m_cur_LoginData = null;
                    }
                }
                else
                {
                    timer1.Stop();
                }
                return;
            }
            if(m_send_counter >= 20)
            {
                m_cur_LoginData = null;
                return;
            }
            SendMessage2Users();
        }
    }
}