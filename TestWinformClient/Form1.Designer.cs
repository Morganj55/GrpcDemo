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
            label3 = new Label();
            portNumbersList = new ComboBox();
            SetActivePortBtn = new Button();
            label4 = new Label();
            activePortLbl = new Label();
            label5 = new Label();
            IPAddressTxtBox = new TextBox();
            IPTestBtn = new Button();
            tabcontrol = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            UnixNameTxtBox = new TextBox();
            label6 = new Label();
            UnixGetServerReply = new Button();
            UnixSocketBtn = new Button();
            tabPage3 = new TabPage();
            tabControl1 = new TabControl();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            BackgroundTxtBox = new TextBox();
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
            label2.Location = new Point(426, 100);
            label2.Name = "label2";
            label2.Size = new Size(72, 15);
            label2.TabIndex = 6;
            label2.Text = "Enter Name:";
            // 
            // nameTxtBox
            // 
            nameTxtBox.Location = new Point(504, 96);
            nameTxtBox.Name = "nameTxtBox";
            nameTxtBox.Size = new Size(100, 23);
            nameTxtBox.TabIndex = 5;
            // 
            // ServerReplyButton
            // 
            ServerReplyButton.Location = new Point(610, 95);
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
            debugTxtBox.Size = new Size(762, 246);
            debugTxtBox.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(411, 71);
            label3.Name = "label3";
            label3.Size = new Size(87, 15);
            label3.TabIndex = 8;
            label3.Text = "Set Active Port:";
            // 
            // portNumbersList
            // 
            portNumbersList.FormattingEnabled = true;
            portNumbersList.Location = new Point(504, 67);
            portNumbersList.Name = "portNumbersList";
            portNumbersList.Size = new Size(100, 23);
            portNumbersList.TabIndex = 9;
            // 
            // SetActivePortBtn
            // 
            SetActivePortBtn.Location = new Point(610, 66);
            SetActivePortBtn.Name = "SetActivePortBtn";
            SetActivePortBtn.Size = new Size(131, 23);
            SetActivePortBtn.TabIndex = 10;
            SetActivePortBtn.Text = "Set Active Port ";
            SetActivePortBtn.UseVisualStyleBackColor = true;
            SetActivePortBtn.Click += SetActivePortBtn_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(6, 56);
            label4.Name = "label4";
            label4.Size = new Size(164, 30);
            label4.TabIndex = 11;
            label4.Text = "Active Address:";
            // 
            // activePortLbl
            // 
            activePortLbl.AutoSize = true;
            activePortLbl.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            activePortLbl.Location = new Point(170, 60);
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
            tabcontrol.Size = new Size(776, 151);
            tabcontrol.TabIndex = 16;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(activePortLbl);
            tabPage1.Controls.Add(IPTestBtn);
            tabPage1.Controls.Add(SetActivePortBtn);
            tabPage1.Controls.Add(nameTxtBox);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(portNumbersList);
            tabPage1.Controls.Add(ServerReplyButton);
            tabPage1.Controls.Add(IPAddressTxtBox);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(portTxtBox);
            tabPage1.Controls.Add(TestConnectionBtn);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(768, 123);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "TCP Connections";
            tabPage1.UseVisualStyleBackColor = true;
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
            tabPage2.Size = new Size(768, 123);
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
            tabPage3.Size = new Size(768, 123);
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
            tabPage5.Controls.Add(BackgroundTxtBox);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(768, 252);
            tabPage5.TabIndex = 1;
            tabPage5.Text = "Background Services";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // BackgroundTxtBox
            // 
            BackgroundTxtBox.Dock = DockStyle.Fill;
            BackgroundTxtBox.Location = new Point(3, 3);
            BackgroundTxtBox.Multiline = true;
            BackgroundTxtBox.Name = "BackgroundTxtBox";
            BackgroundTxtBox.Size = new Size(762, 246);
            BackgroundTxtBox.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
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
        private Label label3;
        private ComboBox portNumbersList;
        private Button SetActivePortBtn;
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
        private TextBox BackgroundTxtBox;
    }
}