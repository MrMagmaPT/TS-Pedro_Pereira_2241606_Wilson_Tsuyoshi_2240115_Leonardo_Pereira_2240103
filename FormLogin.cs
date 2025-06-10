using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using Projeto_TS;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115;
using System.Linq.Expressions;

namespace Projeto_TS
{
    public partial class FormLogin: Form
    {
        // Declaração de variáveis globais
        private const int NUMBER_OF_ITERATIONS = 1000; // Número de iterações para o algoritmo de hashing
        private const int SALT_SIZE = 8; // Tamanho do salt em bytes

        //Controllers
        ControllerFormLogin controllerFormLogin = new ControllerFormLogin();

        //private RSACryptoServiceProvider rsaSign;

        //private RSACryptoServiceProvider rsaVerify;

        FormRegistar formRegistar = new FormRegistar();
        public FormLogin()
        {
            InitializeComponent();
            
            //rsaSign = new RSACryptoServiceProvider(2048);
            //string publicKey = rsaSign.ToXmlString(false);

            //rsaVerify = new RSACryptoServiceProvider(2048);
            //rsaVerify.FromXmlString(publicKey);

        }

        private void btnRegistar_Click(object sender, EventArgs e)
        {
            //verifica se o formRegistar ainda está intanciado, se não estiver, cria uma nova 
            if (formRegistar.IsDisposed)
            {
                formRegistar = new FormRegistar();
            }
            formRegistar.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string password = txbPassword.Text;
            string username = txbUsername.Text;

            Image profilePic = controllerFormLogin.verifyLogin(username, password);
            if (profilePic != null)
            {
                MessageBox.Show("Login efetuado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FormMain formMain = new FormMain(username, profilePic);
                formMain.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Nome de usuário ou senha incorretos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
