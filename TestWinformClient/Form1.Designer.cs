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
            SuspendLayout();
            // 
            // TestConnectionBtn
            // 
            TestConnectionBtn.Location = new Point(635, 343);
            TestConnectionBtn.Name = "TestConnectionBtn";
            TestConnectionBtn.Size = new Size(131, 23);
            TestConnectionBtn.TabIndex = 0;
            TestConnectionBtn.Text = "Test Connection";
            TestConnectionBtn.UseVisualStyleBackColor = true;
            TestConnectionBtn.Click += TestConnectionBtn_Click;
            // 
            // portTxtBox
            // 
            portTxtBox.Location = new Point(520, 343);
            portTxtBox.Name = "portTxtBox";
            portTxtBox.Size = new Size(100, 23);
            portTxtBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(482, 347);
            label1.Name = "label1";
            label1.Size = new Size(32, 15);
            label1.TabIndex = 3;
            label1.Text = "Port:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(442, 417);
            label2.Name = "label2";
            label2.Size = new Size(72, 15);
            label2.TabIndex = 6;
            label2.Text = "Enter Name:";
            // 
            // nameTxtBox
            // 
            nameTxtBox.Location = new Point(520, 414);
            nameTxtBox.Name = "nameTxtBox";
            nameTxtBox.Size = new Size(100, 23);
            nameTxtBox.TabIndex = 5;
            // 
            // ServerReplyButton
            // 
            ServerReplyButton.Location = new Point(635, 414);
            ServerReplyButton.Name = "ServerReplyButton";
            ServerReplyButton.Size = new Size(131, 23);
            ServerReplyButton.TabIndex = 4;
            ServerReplyButton.Text = "Get Server Reply";
            ServerReplyButton.UseVisualStyleBackColor = true;
            ServerReplyButton.Click += ServerReplyButton_Click;
            // 
            // debugTxtBox
            // 
            debugTxtBox.Location = new Point(12, 12);
            debugTxtBox.Multiline = true;
            debugTxtBox.Name = "debugTxtBox";
            debugTxtBox.ReadOnly = true;
            debugTxtBox.Size = new Size(776, 325);
            debugTxtBox.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(427, 382);
            label3.Name = "label3";
            label3.Size = new Size(87, 15);
            label3.TabIndex = 8;
            label3.Text = "Set Active Port:";
            // 
            // portNumbersList
            // 
            portNumbersList.FormattingEnabled = true;
            portNumbersList.Location = new Point(520, 379);
            portNumbersList.Name = "portNumbersList";
            portNumbersList.Size = new Size(100, 23);
            portNumbersList.TabIndex = 9;
            // 
            // SetActivePortBtn
            // 
            SetActivePortBtn.Location = new Point(635, 379);
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
            label4.Location = new Point(12, 379);
            label4.Name = "label4";
            label4.Size = new Size(128, 30);
            label4.TabIndex = 11;
            label4.Text = "Active Port:";
            // 
            // activePortLbl
            // 
            activePortLbl.AutoSize = true;
            activePortLbl.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            activePortLbl.Location = new Point(140, 382);
            activePortLbl.Name = "activePortLbl";
            activePortLbl.Size = new Size(0, 30);
            activePortLbl.TabIndex = 12;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(activePortLbl);
            Controls.Add(label4);
            Controls.Add(SetActivePortBtn);
            Controls.Add(portNumbersList);
            Controls.Add(label3);
            Controls.Add(debugTxtBox);
            Controls.Add(label2);
            Controls.Add(nameTxtBox);
            Controls.Add(ServerReplyButton);
            Controls.Add(label1);
            Controls.Add(portTxtBox);
            Controls.Add(TestConnectionBtn);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
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
    }
}