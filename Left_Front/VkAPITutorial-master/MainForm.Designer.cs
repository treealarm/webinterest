namespace VkAPITutorial
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.User_Photo = new System.Windows.Forms.PictureBox();
            this.Label_User_Name = new System.Windows.Forms.Label();
            this.Label_User_Surname = new System.Windows.Forms.Label();
            this.Label_User_City = new System.Windows.Forms.Label();
            this.Label_User_Country = new System.Windows.Forms.Label();
            this.Label_User_UserID = new System.Windows.Forms.Label();
            this.User_ID = new System.Windows.Forms.TextBox();
            this.User_Name = new System.Windows.Forms.Label();
            this.User_Country = new System.Windows.Forms.Label();
            this.User_City = new System.Windows.Forms.Label();
            this.User_Surname = new System.Windows.Forms.Label();
            this.Button_GetToken = new System.Windows.Forms.Button();
            this.Button_GetInformation = new System.Windows.Forms.Button();
            this.buttonSendMessage = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonUpdateTable = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.User_Photo)).BeginInit();
            this.SuspendLayout();
            // 
            // User_Photo
            // 
            this.User_Photo.Location = new System.Drawing.Point(17, 16);
            this.User_Photo.Margin = new System.Windows.Forms.Padding(4);
            this.User_Photo.Name = "User_Photo";
            this.User_Photo.Size = new System.Drawing.Size(265, 242);
            this.User_Photo.TabIndex = 0;
            this.User_Photo.TabStop = false;
            // 
            // Label_User_Name
            // 
            this.Label_User_Name.AutoSize = true;
            this.Label_User_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Label_User_Name.Location = new System.Drawing.Point(292, 16);
            this.Label_User_Name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label_User_Name.Name = "Label_User_Name";
            this.Label_User_Name.Size = new System.Drawing.Size(70, 25);
            this.Label_User_Name.TabIndex = 1;
            this.Label_User_Name.Text = "Name:";
            // 
            // Label_User_Surname
            // 
            this.Label_User_Surname.AutoSize = true;
            this.Label_User_Surname.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Label_User_Surname.Location = new System.Drawing.Point(292, 41);
            this.Label_User_Surname.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label_User_Surname.Name = "Label_User_Surname";
            this.Label_User_Surname.Size = new System.Drawing.Size(98, 25);
            this.Label_User_Surname.TabIndex = 2;
            this.Label_User_Surname.Text = "Surname:";
            // 
            // Label_User_City
            // 
            this.Label_User_City.AutoSize = true;
            this.Label_User_City.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Label_User_City.Location = new System.Drawing.Point(292, 65);
            this.Label_User_City.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label_User_City.Name = "Label_User_City";
            this.Label_User_City.Size = new System.Drawing.Size(52, 25);
            this.Label_User_City.TabIndex = 3;
            this.Label_User_City.Text = "City:";
            // 
            // Label_User_Country
            // 
            this.Label_User_Country.AutoSize = true;
            this.Label_User_Country.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Label_User_Country.Location = new System.Drawing.Point(292, 90);
            this.Label_User_Country.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label_User_Country.Name = "Label_User_Country";
            this.Label_User_Country.Size = new System.Drawing.Size(87, 25);
            this.Label_User_Country.TabIndex = 4;
            this.Label_User_Country.Text = "Country:";
            // 
            // Label_User_UserID
            // 
            this.Label_User_UserID.AutoSize = true;
            this.Label_User_UserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Label_User_UserID.Location = new System.Drawing.Point(548, 16);
            this.Label_User_UserID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label_User_UserID.Name = "Label_User_UserID";
            this.Label_User_UserID.Size = new System.Drawing.Size(78, 25);
            this.Label_User_UserID.TabIndex = 5;
            this.Label_User_UserID.Text = "UserID:";
            // 
            // User_ID
            // 
            this.User_ID.Location = new System.Drawing.Point(641, 16);
            this.User_ID.Margin = new System.Windows.Forms.Padding(4);
            this.User_ID.Name = "User_ID";
            this.User_ID.Size = new System.Drawing.Size(132, 22);
            this.User_ID.TabIndex = 7;
            // 
            // User_Name
            // 
            this.User_Name.AutoSize = true;
            this.User_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.User_Name.Location = new System.Drawing.Point(373, 16);
            this.User_Name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.User_Name.Name = "User_Name";
            this.User_Name.Size = new System.Drawing.Size(56, 25);
            this.User_Name.TabIndex = 8;
            this.User_Name.Text = "none";
            // 
            // User_Country
            // 
            this.User_Country.AutoSize = true;
            this.User_Country.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.User_Country.Location = new System.Drawing.Point(391, 90);
            this.User_Country.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.User_Country.Name = "User_Country";
            this.User_Country.Size = new System.Drawing.Size(56, 25);
            this.User_Country.TabIndex = 9;
            this.User_Country.Text = "none";
            // 
            // User_City
            // 
            this.User_City.AutoSize = true;
            this.User_City.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.User_City.Location = new System.Drawing.Point(352, 65);
            this.User_City.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.User_City.Name = "User_City";
            this.User_City.Size = new System.Drawing.Size(56, 25);
            this.User_City.TabIndex = 10;
            this.User_City.Text = "none";
            // 
            // User_Surname
            // 
            this.User_Surname.AutoSize = true;
            this.User_Surname.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.User_Surname.Location = new System.Drawing.Point(404, 41);
            this.User_Surname.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.User_Surname.Name = "User_Surname";
            this.User_Surname.Size = new System.Drawing.Size(56, 25);
            this.User_Surname.TabIndex = 11;
            this.User_Surname.Text = "none";
            // 
            // Button_GetToken
            // 
            this.Button_GetToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Button_GetToken.Location = new System.Drawing.Point(297, 118);
            this.Button_GetToken.Margin = new System.Windows.Forms.Padding(4);
            this.Button_GetToken.Name = "Button_GetToken";
            this.Button_GetToken.Size = new System.Drawing.Size(153, 44);
            this.Button_GetToken.TabIndex = 12;
            this.Button_GetToken.Text = "Get token";
            this.Button_GetToken.UseVisualStyleBackColor = true;
            this.Button_GetToken.Click += new System.EventHandler(this.Button_GetToken_Click_1);
            // 
            // Button_GetInformation
            // 
            this.Button_GetInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Button_GetInformation.Location = new System.Drawing.Point(553, 48);
            this.Button_GetInformation.Margin = new System.Windows.Forms.Padding(4);
            this.Button_GetInformation.Name = "Button_GetInformation";
            this.Button_GetInformation.Size = new System.Drawing.Size(188, 44);
            this.Button_GetInformation.TabIndex = 13;
            this.Button_GetInformation.Text = "Get information";
            this.Button_GetInformation.UseVisualStyleBackColor = true;
            this.Button_GetInformation.Click += new System.EventHandler(this.Button_GetInformation_Click_1);
            // 
            // buttonSendMessage
            // 
            this.buttonSendMessage.Location = new System.Drawing.Point(553, 118);
            this.buttonSendMessage.Name = "buttonSendMessage";
            this.buttonSendMessage.Size = new System.Drawing.Size(220, 44);
            this.buttonSendMessage.TabIndex = 14;
            this.buttonSendMessage.Text = "SendMessage";
            this.buttonSendMessage.UseVisualStyleBackColor = true;
            this.buttonSendMessage.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(321, 189);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(461, 178);
            this.richTextBox1.TabIndex = 15;
            this.richTextBox1.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(804, 192);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 17);
            this.label1.TabIndex = 16;
            this.label1.Text = "label1";
            // 
            // buttonUpdateTable
            // 
            this.buttonUpdateTable.Location = new System.Drawing.Point(17, 284);
            this.buttonUpdateTable.Name = "buttonUpdateTable";
            this.buttonUpdateTable.Size = new System.Drawing.Size(144, 23);
            this.buttonUpdateTable.TabIndex = 17;
            this.buttonUpdateTable.Text = "UpdateTable";
            this.buttonUpdateTable.UseVisualStyleBackColor = true;
            this.buttonUpdateTable.Click += new System.EventHandler(this.buttonUpdateTable_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 368);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(308, 23);
            this.progressBar1.TabIndex = 18;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 391);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonUpdateTable);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttonSendMessage);
            this.Controls.Add(this.Button_GetInformation);
            this.Controls.Add(this.Button_GetToken);
            this.Controls.Add(this.User_Surname);
            this.Controls.Add(this.User_City);
            this.Controls.Add(this.User_Country);
            this.Controls.Add(this.User_Name);
            this.Controls.Add(this.User_ID);
            this.Controls.Add(this.Label_User_UserID);
            this.Controls.Add(this.Label_User_Country);
            this.Controls.Add(this.Label_User_City);
            this.Controls.Add(this.Label_User_Surname);
            this.Controls.Add(this.Label_User_Name);
            this.Controls.Add(this.User_Photo);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.User_Photo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox User_Photo;
        private System.Windows.Forms.Label Label_User_Name;
        private System.Windows.Forms.Label Label_User_Surname;
        private System.Windows.Forms.Label Label_User_City;
        private System.Windows.Forms.Label Label_User_Country;
        private System.Windows.Forms.Label Label_User_UserID;
        private System.Windows.Forms.TextBox User_ID;
        private System.Windows.Forms.Label User_Name;
        private System.Windows.Forms.Label User_Country;
        private System.Windows.Forms.Label User_City;
        private System.Windows.Forms.Label User_Surname;
        private System.Windows.Forms.Button Button_GetToken;
        private System.Windows.Forms.Button Button_GetInformation;
        private System.Windows.Forms.Button buttonSendMessage;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonUpdateTable;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}