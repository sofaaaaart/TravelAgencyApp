
namespace WindowsFormsApp1
{
    partial class LoginForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            this.messageLabel = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.registerButton = new Guna.UI2.WinForms.Guna2Button();
            this.enterButton = new Guna.UI2.WinForms.Guna2Button();
            this.loginField = new Guna.UI2.WinForms.Guna2TextBox();
            this.guna2HtmlLabel2 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2Panel2 = new Guna.UI2.WinForms.Guna2Panel();
            this.guna2PictureBox5 = new Guna.UI2.WinForms.Guna2PictureBox();
            this.closeButton = new Guna.UI2.WinForms.Guna2PictureBox();
            this.hideButton = new Guna.UI2.WinForms.Guna2PictureBox();
            this.guna2PictureBox1 = new Guna.UI2.WinForms.Guna2PictureBox();
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.guna2Panel1.SuspendLayout();
            this.guna2Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.closeButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hideButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // guna2HtmlLabel1
            // 
            this.guna2HtmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel1.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.guna2HtmlLabel1.ForeColor = System.Drawing.Color.White;
            this.guna2HtmlLabel1.Location = new System.Drawing.Point(206, 57);
            this.guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            this.guna2HtmlLabel1.Size = new System.Drawing.Size(129, 32);
            this.guna2HtmlLabel1.TabIndex = 15;
            this.guna2HtmlLabel1.Text = "Авторизация";
            // 
            // guna2Panel1
            // 
            this.guna2Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(73)))));
            this.guna2Panel1.BorderColor = System.Drawing.Color.White;
            this.guna2Panel1.Controls.Add(this.messageLabel);
            this.guna2Panel1.Controls.Add(this.registerButton);
            this.guna2Panel1.Controls.Add(this.enterButton);
            this.guna2Panel1.Controls.Add(this.loginField);
            this.guna2Panel1.Controls.Add(this.guna2HtmlLabel2);
            this.guna2Panel1.Controls.Add(this.guna2Panel2);
            this.guna2Panel1.Controls.Add(this.guna2PictureBox1);
            this.guna2Panel1.Location = new System.Drawing.Point(1, 1);
            this.guna2Panel1.Name = "guna2Panel1";
            this.guna2Panel1.Size = new System.Drawing.Size(454, 204);
            this.guna2Panel1.TabIndex = 6;
            // 
            // messageLabel
            // 
            this.messageLabel.BackColor = System.Drawing.Color.Transparent;
            this.messageLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.messageLabel.ForeColor = System.Drawing.Color.White;
            this.messageLabel.Location = new System.Drawing.Point(206, 174);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(3, 2);
            this.messageLabel.TabIndex = 19;
            this.messageLabel.Text = null;
            // 
            // registerButton
            // 
            this.registerButton.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.registerButton.BorderThickness = 1;
            this.registerButton.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.registerButton.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.registerButton.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.registerButton.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.registerButton.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(137)))), ((int)(((byte)(120)))));
            this.registerButton.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.registerButton.ForeColor = System.Drawing.Color.White;
            this.registerButton.Location = new System.Drawing.Point(302, 137);
            this.registerButton.Name = "registerButton";
            this.registerButton.PressedColor = System.Drawing.Color.Empty;
            this.registerButton.Size = new System.Drawing.Size(128, 32);
            this.registerButton.TabIndex = 18;
            this.registerButton.Text = "Регистрация";
            this.registerButton.Click += new System.EventHandler(this.RegisterButton_Click);
            this.registerButton.MouseEnter += new System.EventHandler(this.RegisterButton_MouseEnter);
            this.registerButton.MouseLeave += new System.EventHandler(this.RegisterButton_MouseLeave);
            // 
            // enterButton
            // 
            this.enterButton.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.enterButton.BorderThickness = 1;
            this.enterButton.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.enterButton.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.enterButton.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.enterButton.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.enterButton.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(137)))), ((int)(((byte)(120)))));
            this.enterButton.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.enterButton.ForeColor = System.Drawing.Color.White;
            this.enterButton.Location = new System.Drawing.Point(196, 137);
            this.enterButton.Margin = new System.Windows.Forms.Padding(3, 12, 12, 3);
            this.enterButton.Name = "enterButton";
            this.enterButton.PressedColor = System.Drawing.Color.Empty;
            this.enterButton.Size = new System.Drawing.Size(96, 32);
            this.enterButton.TabIndex = 17;
            this.enterButton.Text = "Войти";
            this.enterButton.Click += new System.EventHandler(this.EnterButton_Click);
            this.enterButton.MouseEnter += new System.EventHandler(this.EnterButton_MouseEnter);
            this.enterButton.MouseLeave += new System.EventHandler(this.EnterButton_MouseLeave);
            // 
            // loginField
            // 
            this.loginField.BorderColor = System.Drawing.Color.WhiteSmoke;
            this.loginField.BorderThickness = 0;
            this.loginField.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.loginField.DefaultText = "Введите логин";
            this.loginField.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.loginField.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.loginField.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.loginField.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.loginField.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.loginField.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.loginField.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.loginField.Location = new System.Drawing.Point(196, 98);
            this.loginField.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.loginField.Name = "loginField";
            this.loginField.PasswordChar = '\0';
            this.loginField.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(99)))), ((int)(((byte)(99)))));
            this.loginField.PlaceholderText = "";
            this.loginField.SelectedText = "";
            this.loginField.Size = new System.Drawing.Size(234, 32);
            this.loginField.TabIndex = 16;
            this.loginField.Enter += new System.EventHandler(this.LoginField_Enter);
            this.loginField.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LoginField_KeyPress);
            this.loginField.Leave += new System.EventHandler(this.LoginField_Leave);
            // 
            // guna2HtmlLabel2
            // 
            this.guna2HtmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel2.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.guna2HtmlLabel2.ForeColor = System.Drawing.Color.White;
            this.guna2HtmlLabel2.Location = new System.Drawing.Point(196, 59);
            this.guna2HtmlLabel2.Margin = new System.Windows.Forms.Padding(196, 24, 12, 3);
            this.guna2HtmlLabel2.Name = "guna2HtmlLabel2";
            this.guna2HtmlLabel2.Size = new System.Drawing.Size(129, 32);
            this.guna2HtmlLabel2.TabIndex = 15;
            this.guna2HtmlLabel2.Text = "Авторизация";
            // 
            // guna2Panel2
            // 
            this.guna2Panel2.BackColor = System.Drawing.Color.White;
            this.guna2Panel2.Controls.Add(this.guna2PictureBox5);
            this.guna2Panel2.Controls.Add(this.closeButton);
            this.guna2Panel2.Controls.Add(this.hideButton);
            this.guna2Panel2.Location = new System.Drawing.Point(0, 0);
            this.guna2Panel2.Name = "guna2Panel2";
            this.guna2Panel2.Size = new System.Drawing.Size(454, 32);
            this.guna2Panel2.TabIndex = 14;
            // 
            // guna2PictureBox5
            // 
            this.guna2PictureBox5.Image = global::WindowsFormsApp1.Properties.Resources.LogoFull;
            this.guna2PictureBox5.ImageRotate = 0F;
            this.guna2PictureBox5.Location = new System.Drawing.Point(14, 6);
            this.guna2PictureBox5.Name = "guna2PictureBox5";
            this.guna2PictureBox5.Size = new System.Drawing.Size(104, 24);
            this.guna2PictureBox5.TabIndex = 15;
            this.guna2PictureBox5.TabStop = false;
            // 
            // closeButton
            // 
            this.closeButton.ErrorImage = ((System.Drawing.Image)(resources.GetObject("closeButton.ErrorImage")));
            this.closeButton.Image = global::WindowsFormsApp1.Properties.Resources.ButtonClose;
            this.closeButton.ImageRotate = 0F;
            this.closeButton.InitialImage = ((System.Drawing.Image)(resources.GetObject("closeButton.InitialImage")));
            this.closeButton.Location = new System.Drawing.Point(420, 6);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(23, 23);
            this.closeButton.TabIndex = 14;
            this.closeButton.TabStop = false;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            this.closeButton.MouseEnter += new System.EventHandler(this.CloseButton_MouseEnter);
            this.closeButton.MouseLeave += new System.EventHandler(this.CloseButton_MouseLeave);
            // 
            // hideButton
            // 
            this.hideButton.Image = global::WindowsFormsApp1.Properties.Resources.ButtonHide;
            this.hideButton.ImageRotate = 0F;
            this.hideButton.Location = new System.Drawing.Point(387, 6);
            this.hideButton.Name = "hideButton";
            this.hideButton.Size = new System.Drawing.Size(23, 23);
            this.hideButton.TabIndex = 13;
            this.hideButton.TabStop = false;
            this.hideButton.Click += new System.EventHandler(this.HideButton_Click);
            this.hideButton.MouseEnter += new System.EventHandler(this.HideButton_MouseEnter);
            this.hideButton.MouseLeave += new System.EventHandler(this.HideButton_MouseLeave);
            // 
            // guna2PictureBox1
            // 
            this.guna2PictureBox1.ErrorImage = ((System.Drawing.Image)(resources.GetObject("guna2PictureBox1.ErrorImage")));
            this.guna2PictureBox1.Image = global::WindowsFormsApp1.Properties.Resources.Logo;
            this.guna2PictureBox1.ImageRotate = 0F;
            this.guna2PictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("guna2PictureBox1.InitialImage")));
            this.guna2PictureBox1.Location = new System.Drawing.Point(26, 57);
            this.guna2PictureBox1.Name = "guna2PictureBox1";
            this.guna2PictureBox1.Size = new System.Drawing.Size(149, 100);
            this.guna2PictureBox1.TabIndex = 13;
            this.guna2PictureBox1.TabStop = false;
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorColor = System.Drawing.Color.Empty;
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.DragStartTransparencyValue = 1D;
            this.guna2DragControl1.TargetControl = this.guna2Panel2;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(456, 206);
            this.Controls.Add(this.guna2Panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginForm";
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TripFlow - Авторизация";
            this.guna2Panel1.ResumeLayout(false);
            this.guna2Panel1.PerformLayout();
            this.guna2Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.closeButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hideButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel1;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Guna.UI2.WinForms.Guna2Button registerButton;
        private Guna.UI2.WinForms.Guna2Button enterButton;
        private Guna.UI2.WinForms.Guna2TextBox loginField;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel2;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel2;
        private Guna.UI2.WinForms.Guna2PictureBox guna2PictureBox5;
        private Guna.UI2.WinForms.Guna2PictureBox closeButton;
        private Guna.UI2.WinForms.Guna2PictureBox hideButton;
        private Guna.UI2.WinForms.Guna2PictureBox guna2PictureBox1;
        private Guna.UI2.WinForms.Guna2HtmlLabel messageLabel;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}