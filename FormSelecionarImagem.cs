using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{
    public partial class FormSelecionarImagem: Form
    {
        //declaração de variaveis globais
        public byte[] profPic = null;

        public Image ProfPicImage = null;

        public FormSelecionarImagem()
        {
            InitializeComponent();
        }

        private void pbimagem1_Click(object sender, EventArgs e)
        {
            GetSelectedImage(pbimagem1);
                
        }

        private void pbimagem2_Click(object sender, EventArgs e)
        {
            GetSelectedImage(pbimagem2);
        }

        private void pbimagem3_Click(object sender, EventArgs e)
        {
            GetSelectedImage(pbimagem3);
        }

        private void pbimagem4_Click(object sender, EventArgs e)
        {
            GetSelectedImage(pbimagem4);
        }

        private void pbimagem5_Click(object sender, EventArgs e)
        {
            GetSelectedImage(pbimagem5);
        }

        private void pbimagemCustom_Click(object sender, EventArgs e)
        {
            // Abre um diálogo para selecionar uma imagem
            openFileDialogImagem.Filter = "Image Files|*.png;";
            openFileDialogImagem.Title = "Select User Image - Max 256x256p";
            openFileDialogImagem.DefaultExt = "png";
            openFileDialogImagem.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialogImagem.FileName = string.Empty; // Limpa seleção anterior
            openFileDialogImagem.Multiselect = false; // Permite selecionar apenas uma imagem


            if (openFileDialogImagem.ShowDialog() == DialogResult.OK)
            {
                //converter a imagem selecionada para a bdUserData e exibir na PictureBox
                try
                {
                    string imagePath = openFileDialogImagem.FileName;
                    Image userImage = Image.FromFile(imagePath);
                    // Verifica se a imagem é maior que 256x256 pixels
                    if (userImage.Width > 16 || userImage.Height > 16)
                    {
                        MessageBox.Show("A imagem tem de ser 16x16 pixels ou menor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    // Exibe a imagem na PictureBox
                    pbimagemCustom.Image = userImage;
                    ProfPicImage = userImage;
                    profPic = File.ReadAllBytes(openFileDialogImagem.FileName);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            //funcao para cancelar a selecao de imagem
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        public void GetSelectedImage(PictureBox objetoDeOrigem)
        {
            ProfPicImage = objetoDeOrigem.Image;

            using (MemoryStream ms = new MemoryStream())
            {
                objetoDeOrigem.Image.Save(ms, objetoDeOrigem.Image.RawFormat);
                profPic = ms.ToArray();
            }
            this.DialogResult = DialogResult.OK;
            this.Close();

        }
    }
}
