using System;
using System.Windows.Forms;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115;

namespace Projeto_TS
{
    public partial class FormLogin: Form
    {
        FormRegistar formRegistar = new FormRegistar();
        public FormLogin()
        {
            InitializeComponent();
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

            //enviar para o server uma mensagem com os dados de login

            //recebe do server uma imagem de perfil em array de bytes , se for null, significa que o login falhou
            
        }

    }
}
