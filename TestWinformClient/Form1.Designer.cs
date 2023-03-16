namespace TestWinformClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TestConnectionBtn = new Button();
            portTxtBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            nameTxtBox = new TextBox();
            ServerReplyButton = new Button();
            debugTxtBox = new TextBox();
            label4 = new Label();
            activePortLbl = new Label();
            label5 = new Label();
            IPAddressTxtBox = new TextBox();
            IPTestBtn = new Button();
            tabcontrol = new TabControl();
            tabPage1 = new TabPage();
            DisconnectBtn = new Button();
            label7 = new Label();
            tabPage2 = new TabPage();
            UnixNameTxtBox = new TextBox();
            label6 = new Label();
            UnixGetServerReply = new Button();
            UnixSocketBtn = new Button();
            tabPage3 = new TabPage();
            tabControl1 = new TabControl();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            backgroundTxtBox = new TextBox();
            label3 = new Label();
            ConnectionStatusLbl = new Label();
            tabcontrol.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            SuspendLayout();
            // 
            // TestConnectionBtn
            // 
            TestConnectionBtn.Location = new Point(610, 38);
            TestConnectionBtn.Name = "TestConnectionBtn";
            TestConnectionBtn.Size = new Size(131, 23);
            TestConnectionBtn.TabIndex = 0;
            TestConnectionBtn.Text = "Test Connection";
            TestConnectionBtn.UseVisualStyleBackColor = true;
            TestConnectionBtn.Click += TestConnectionBtn_Click;
            // 
            // portTxtBox
            // 
            portTxtBox.Location = new Point(504, 38);
            portTxtBox.Name = "portTxtBox";
            portTxtBox.Size = new Size(100, 23);
            portTxtBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(453, 41);
            label1.Name = "label1";
            label1.Size = new Size(45, 15);
            label1.TabIndex = 3;
            label1.Text = "IP Port:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(426, 191);
            label2.Name = "label2";
            label2.Size = new Size(72, 15);
            label2.TabIndex = 6;
            label2.Text = "Enter Name:";
            // 
            // nameTxtBox
            // 
            nameTxtBox.Location = new Point(504, 187);
            nameTxtBox.Name = "nameTxtBox";
            nameTxtBox.Size = new Size(100, 23);
            nameTxtBox.TabIndex = 5;
            // 
            // ServerReplyButton
            // 
            ServerReplyButton.Location = new Point(610, 186);
            ServerReplyButton.Name = "ServerReplyButton";
            ServerReplyButton.Size = new Size(131, 23);
            ServerReplyButton.TabIndex = 4;
            ServerReplyButton.Text = "Get Server Reply";
            ServerReplyButton.UseVisualStyleBackColor = true;
            ServerReplyButton.Click += ServerReplyButton_Click;
            // 
            // debugTxtBox
            // 
            debugTxtBox.Dock = DockStyle.Fill;
            debugTxtBox.Location = new Point(3, 3);
            debugTxtBox.Multiline = true;
            debugTxtBox.Name = "debugTxtBox";
            debugTxtBox.ReadOnly = true;
            debugTxtBox.ScrollBars = ScrollBars.Both;
            debugTxtBox.Size = new Size(762, 246);
            debugTxtBox.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(6, 8);
            label4.Name = "label4";
            label4.Size = new Size(164, 30);
            label4.TabIndex = 11;
            label4.Text = "Active Address:";
            // 
            // activePortLbl
            // 
            activePortLbl.AutoSize = true;
            activePortLbl.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            activePortLbl.Location = new Point(170, 9);
            activePortLbl.Name = "activePortLbl";
            activePortLbl.Size = new Size(0, 30);
            activePortLbl.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(433, 12);
            label5.Name = "label5";
            label5.Size = new Size(65, 15);
            label5.TabIndex = 13;
            label5.Text = "IP Address:";
            // 
            // IPAddressTxtBox
            // 
            IPAddressTxtBox.Location = new Point(504, 9);
            IPAddressTxtBox.Name = "IPAddressTxtBox";
            IPAddressTxtBox.Size = new Size(100, 23);
            IPAddressTxtBox.TabIndex = 14;
            // 
            // IPTestBtn
            // 
            IPTestBtn.Location = new Point(610, 8);
            IPTestBtn.Name = "IPTestBtn";
            IPTestBtn.Size = new Size(131, 23);
            IPTestBtn.TabIndex = 15;
            IPTestBtn.Text = "Ping IP Address";
            IPTestBtn.UseVisualStyleBackColor = true;
            IPTestBtn.Click += IPTestBtn_Click;
            // 
            // tabcontrol
            // 
            tabcontrol.Controls.Add(tabPage1);
            tabcontrol.Controls.Add(tabPage2);
            tabcontrol.Controls.Add(tabPage3);
            tabcontrol.Location = new Point(12, 287);
            tabcontrol.Name = "tabcontrol";
            tabcontrol.SelectedIndex = 0;
            tabcontrol.Size = new Size(776, 253);
            tabcontrol.TabIndex = 16;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(ConnectionStatusLbl);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(DisconnectBtn);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(activePortLbl);
            tabPage1.Controls.Add(IPTestBtn);
            tabPage1.Controls.Add(nameTxtBox);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(ServerReplyButton);
            tabPage1.Controls.Add(IPAddressTxtBox);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(portTxtBox);
            tabPage1.Controls.Add(TestConnectionBtn);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(768, 225);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "TCP Connections";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // DisconnectBtn
            // 
            DisconnectBtn.Location = new Point(501, 75);
            DisconnectBtn.Name = "DisconnectBtn";
            DisconnectBtn.Size = new Size(240, 23);
            DisconnectBtn.TabIndex = 17;
            DisconnectBtn.Text = "Disconnect From Server";
            DisconnectBtn.UseVisualStyleBackColor = true;
            DisconnectBtn.Click += DisconnectBtn_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(426, 79);
            label7.Name = "label7";
            label7.Size = new Size(69, 15);
            label7.TabIndex = 16;
            label7.Text = "Disconnect:";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(UnixNameTxtBox);
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(UnixGetServerReply);
            tabPage2.Controls.Add(UnixSocketBtn);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(768, 225);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Unix Domain Sockets";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // UnixNameTxtBox
            // 
            UnixNameTxtBox.Location = new Point(525, 65);
            UnixNameTxtBox.Name = "UnixNameTxtBox";
            UnixNameTxtBox.Size = new Size(100, 23);
            UnixNameTxtBox.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(447, 69);
            label6.Name = "label6";
            label6.Size = new Size(72, 15);
            label6.TabIndex = 9;
            label6.Text = "Enter Name:";
            // 
            // UnixGetServerReply
            // 
            UnixGetServerReply.Location = new Point(631, 64);
            UnixGetServerReply.Name = "UnixGetServerReply";
            UnixGetServerReply.Size = new Size(131, 23);
            UnixGetServerReply.TabIndex = 7;
            UnixGetServerReply.Text = "Get Server Reply";
            UnixGetServerReply.UseVisualStyleBackColor = true;
            UnixGetServerReply.Click += UnixGetServerReply_Click;
            // 
            // UnixSocketBtn
            // 
            UnixSocketBtn.Location = new Point(573, 26);
            UnixSocketBtn.Name = "UnixSocketBtn";
            UnixSocketBtn.Size = new Size(189, 23);
            UnixSocketBtn.TabIndex = 0;
            UnixSocketBtn.Text = "Create Unix Socket Connection";
            UnixSocketBtn.UseVisualStyleBackColor = true;
            UnixSocketBtn.Click += UnixSocketBtn_Click;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(768, 225);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Discovery";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(776, 280);
            tabControl1.TabIndex = 17;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(debugTxtBox);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(768, 252);
            tabPage4.TabIndex = 0;
            tabPage4.Text = "Debug Window";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(backgroundTxtBox);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(768, 252);
            tabPage5.TabIndex = 1;
            tabPage5.Text = "Background Services";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // backgroundTxtBox
            // 
            backgroundTxtBox.Dock = DockStyle.Fill;
            backgroundTxtBox.Location = new Point(3, 3);
            backgroundTxtBox.Multiline = true;
            backgroundTxtBox.Name = "backgroundTxtBox";
            backgroundTxtBox.ScrollBars = ScrollBars.Both;
            backgroundTxtBox.Size = new Size(762, 246);
            backgroundTxtBox.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(3, 64);
            label3.Name = "label3";
            label3.Size = new Size(198, 30);
            label3.TabIndex = 18;
            label3.Text = "Connection Status:";
            // 
            // ConnectionStatusLbl
            // 
            ConnectionStatusLbl.AutoSize = true;
            ConnectionStatusLbl.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            ConnectionStatusLbl.Location = new Point(196, 75);
            ConnectionStatusLbl.Name = "ConnectionStatusLbl";
            ConnectionStatusLbl.Size = new Size(0, 25);
            ConnectionStatusLbl.TabIndex = 19;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 552);
            Controls.Add(tabControl1);
            Controls.Add(tabcontrol);
            Name = "Form1";
            Text = "Form1";
            tabcontrol.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button TestConnectionBtn;
        private TextBox portTxtBox;
        private Label label1;
        private Label label2;
        private TextBox nameTxtBox;
        private Button ServerReplyButton;
        private TextBox debugTxtBox;
        private Label label4;
        private Label activePortLbl;
        private Label label5;
        private TextBox IPAddressTxtBox;
        private Button IPTestBtn;
        private TabControl tabcontrol;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private Button UnixSocketBtn;
        private TextBox UnixNameTxtBox;
        private Label label6;
        private Button UnixGetServerReply;
        private TabControl tabControl1;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TextBox backgroundTxtBox;
        private Button DisconnectBtn;
        private Label label7;
        private Label label3;
        private Label ConnectionStatusLbl;
    }
}