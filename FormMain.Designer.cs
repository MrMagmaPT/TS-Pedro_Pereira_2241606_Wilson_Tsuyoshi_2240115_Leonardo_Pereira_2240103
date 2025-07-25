﻿namespace Projeto_TS
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.enviar_bt_TP = new System.Windows.Forms.Button();
            this.tbxMsg = new System.Windows.Forms.TextBox();
            this.txbChat = new System.Windows.Forms.TextBox();
            this.lbNome = new System.Windows.Forms.Label();
            this.lbServerIP = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pbUserImage = new System.Windows.Forms.PictureBox();
            this.lblPalavras = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txbHashVerificada = new System.Windows.Forms.TextBox();
            this.btnVerificar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbUserImage)).BeginInit();
            this.SuspendLayout();
            // 
            // enviar_bt_TP
            // 
            this.enviar_bt_TP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.enviar_bt_TP.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enviar_bt_TP.Location = new System.Drawing.Point(597, 413);
            this.enviar_bt_TP.Name = "enviar_bt_TP";
            this.enviar_bt_TP.Size = new System.Drawing.Size(128, 49);
            this.enviar_bt_TP.TabIndex = 1;
            this.enviar_bt_TP.Text = "Enviar";
            this.enviar_bt_TP.UseVisualStyleBackColor = true;
            this.enviar_bt_TP.Click += new System.EventHandler(this.btnEnviarMsg_Click);
            // 
            // tbxMsg
            // 
            this.tbxMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxMsg.Location = new System.Drawing.Point(27, 395);
            this.tbxMsg.Multiline = true;
            this.tbxMsg.Name = "tbxMsg";
            this.tbxMsg.Size = new System.Drawing.Size(560, 67);
            this.tbxMsg.TabIndex = 0;
            this.tbxMsg.TextChanged += new System.EventHandler(this.tbxMsg_TextChanged);
            // 
            // txbChat
            // 
            this.txbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbChat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txbChat.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.txbChat.Location = new System.Drawing.Point(27, 74);
            this.txbChat.Multiline = true;
            this.txbChat.Name = "txbChat";
            this.txbChat.ReadOnly = true;
            this.txbChat.Size = new System.Drawing.Size(560, 275);
            this.txbChat.TabIndex = 3;
            // 
            // lbNome
            // 
            this.lbNome.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNome.Location = new System.Drawing.Point(12, 9);
            this.lbNome.Name = "lbNome";
            this.lbNome.Size = new System.Drawing.Size(525, 33);
            this.lbNome.TabIndex = 6;
            this.lbNome.Text = "Nome:";
            this.lbNome.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbServerIP
            // 
            this.lbServerIP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbServerIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbServerIP.Location = new System.Drawing.Point(543, 9);
            this.lbServerIP.Name = "lbServerIP";
            this.lbServerIP.Size = new System.Drawing.Size(182, 17);
            this.lbServerIP.TabIndex = 7;
            this.lbServerIP.Text = "Server IP: ";
            this.lbServerIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(229, 29);
            this.label1.TabIndex = 8;
            this.label1.Text = "Mensagens:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(23, 362);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(229, 29);
            this.label2.TabIndex = 9;
            this.label2.Text = "A sua mensagem:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbUserImage
            // 
            this.pbUserImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbUserImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbUserImage.Location = new System.Drawing.Point(597, 74);
            this.pbUserImage.Name = "pbUserImage";
            this.pbUserImage.Size = new System.Drawing.Size(128, 128);
            this.pbUserImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbUserImage.TabIndex = 17;
            this.pbUserImage.TabStop = false;
            // 
            // lblPalavras
            // 
            this.lblPalavras.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPalavras.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPalavras.ForeColor = System.Drawing.Color.Red;
            this.lblPalavras.Location = new System.Drawing.Point(25, 465);
            this.lblPalavras.Name = "lblPalavras";
            this.lblPalavras.Size = new System.Drawing.Size(562, 17);
            this.lblPalavras.TabIndex = 18;
            this.lblPalavras.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(600, 210);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 20);
            this.label3.TabIndex = 19;
            this.label3.Text = "Hash Verificada";
            // 
            // txbHashVerificada
            // 
            this.txbHashVerificada.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txbHashVerificada.Location = new System.Drawing.Point(613, 233);
            this.txbHashVerificada.Name = "txbHashVerificada";
            this.txbHashVerificada.Size = new System.Drawing.Size(100, 26);
            this.txbHashVerificada.TabIndex = 20;
            // 
            // btnVerificar
            // 
            this.btnVerificar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVerificar.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerificar.Location = new System.Drawing.Point(597, 265);
            this.btnVerificar.Name = "btnVerificar";
            this.btnVerificar.Size = new System.Drawing.Size(128, 49);
            this.btnVerificar.TabIndex = 21;
            this.btnVerificar.Text = "Verificar";
            this.btnVerificar.UseVisualStyleBackColor = true;
            this.btnVerificar.Click += new System.EventHandler(this.btnVerificar_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(734, 486);
            this.Controls.Add(this.btnVerificar);
            this.Controls.Add(this.txbHashVerificada);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblPalavras);
            this.Controls.Add(this.pbUserImage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbServerIP);
            this.Controls.Add(this.lbNome);
            this.Controls.Add(this.txbChat);
            this.Controls.Add(this.tbxMsg);
            this.Controls.Add(this.enviar_bt_TP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "FormMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbUserImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button enviar_bt_TP;
        private System.Windows.Forms.TextBox tbxMsg;
        private System.Windows.Forms.TextBox txbChat;
        private System.Windows.Forms.Label lbNome;
        private System.Windows.Forms.Label lbServerIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pbUserImage;
        private System.Windows.Forms.Label lblPalavras;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txbHashVerificada;
        private System.Windows.Forms.Button btnVerificar;
    }
}

