using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
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

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
