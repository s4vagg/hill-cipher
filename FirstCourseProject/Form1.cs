using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HillCipher
{
    public partial class Form1 : Form
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Переменная алфавита

        public Form1()
        {
            InitializeComponent();
        }

        // Очистка всех полей
        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            textBox1.Clear();
        }

        // Открытие файла с текстом
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Текстовые документы (*.txt)|*.txt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                DialogResult dialogResult = MessageBox.Show("Вы открываете зашифрованный текст?", "Выберите действие", MessageBoxButtons.YesNo);

                using (StreamReader sr = new StreamReader(ofd.FileName))
                {
                    string text = sr.ReadToEnd();

                    if (dialogResult == DialogResult.Yes)
                    {
                        richTextBox2.Clear();
                        richTextBox2.Text = text;
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        richTextBox1.Clear();
                        richTextBox1.Text = text;
                    }
                }
            }
        }

        // Сохранение зашифрованного текста в файл
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые документы (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    sw.Write(richTextBox2.Text);
                }
            }
        }

        // Сохранение расшифрованного текста в файл
        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые документы (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    sw.Write(richTextBox1.Text);
                }
            }
        }

        // Шифрование текста
        private void button3_Click(object sender, EventArgs e)
        {
            string text = richTextBox1.Text;
            string key = textBox1.Text;

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Введите шифруемый текст", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Введите ключ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string encrypted = EncryptText(text, key);
                richTextBox2.Text = encrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при шифровании: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Расшифрование текста
        private void button4_Click(object sender, EventArgs e)
        {
            string text = richTextBox2.Text;
            string key = textBox1.Text;

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Введите текст для расшифровки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Введите ключ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string decrypted = DecryptText(text, key);
                if (decrypted != null)
                    richTextBox1.Text = decrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при расшифровке: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для шифрования текста
        private string EncryptText(string plainText, string key)
        {
            string encText = Encrypt(plainText.ToUpper(), key.ToUpper());
            return encText;
        }

        // Метод для расшифрования текста
        private string DecryptText(string encryptedText, string key)
        {
            string decText = Decrypt(encryptedText.ToUpper(), key.ToUpper());
            return decText;
        }

        // Метод для шифрования текста с помощью алгоритма Hill Cipher
        private string Encrypt(string textToEncrypt, string key)
        {
            string encryptedText = "";
            int[,] matrix = GetMatrixFromText(key);
            string text = CleanText(textToEncrypt);

            // Убедимся, что длина текста кратна 2 для корректной работы шифрования
            if (text.Length % 2 != 0)
            {
                text += 'X'; // Добавляем дополнительный символ, чтобы сделать длину текста кратной 2
            }

            for (int i = 0; i < text.Length; i += 2)
            {
                int numOne = alphabet.IndexOf(text[i]);
                int numTwo = alphabet.IndexOf(text[i + 1]);
                int firstProduct = matrix[0, 0] * numOne + matrix[0, 1] * numTwo;
                int secondProduct = matrix[1, 0] * numOne + matrix[1, 1] * numTwo;
                encryptedText += alphabet[(firstProduct % 26 + 26) % 26]; // Исправляем вычисление модуля
                encryptedText += alphabet[(secondProduct % 26 + 26) % 26]; // Исправляем вычисление модуля
            }
            return encryptedText;
        }

        // Метод для расшифрования текста с помощью алгоритма Hill Cipher
        private string Decrypt(string textToDecrypt, string key)
        {
            string decryptedText = "";
            int[,] inputMatrix = GetMatrixFromText(key);
            double[,] matrix = InverseMatrix(inputMatrix);
            string text = textToDecrypt;

            for (int i = 0; i < text.Length; i = i + 2)
            {
                int numOne = alphabet.IndexOf(text[i]);
                int numTwo = alphabet.IndexOf(text[i + 1]);
                double firstProduct = matrix[0, 0] * numOne + matrix[0, 1] * numTwo;
                double secondProduct = matrix[1, 0] * numOne + matrix[1, 1] * numTwo;
                int firstRounded = Convert.ToInt32(Math.Floor(firstProduct) % 26);
                int secondRounded = Convert.ToInt32(Math.Floor(secondProduct) % 26);

                decryptedText += alphabet[(firstRounded + 26) % 26]; // Исправляем вычисление модуля
                decryptedText += alphabet[(secondRounded + 26) % 26]; // Исправляем вычисление модуля
            }

            return decryptedText;
        }

        // Метод для вычисления обратной матрицы
        private double[,] InverseMatrix(int[,] matrix)
        {
            double[,] inverse = new double[2, 2];

            int determinant = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            if (determinant < 0)
            {
                determinant = determinant * -1;
            }
            double detFraction = ModInverse(determinant, 26);
            inverse[0, 0] = (matrix[1, 1] * detFraction) % 26;
            inverse[0, 1] = ((-matrix[0, 1] + 26) * detFraction) % 26;
            inverse[1, 0] = ((-matrix[1, 0] + 26) * detFraction) % 26;
            inverse[1, 1] = (matrix[0, 0] * detFraction) % 26;

            return inverse;
        }

        // Метод для вычисления модуля
        private int ModInverse(int a, int n)
        {
            int i = n, v = 0, d = 1;
            while (a > 0)
            {
                int t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        // Метод для получения матрицы из ключа
        private int[,] GetMatrixFromText(string text)
        {
            int[,] matrix = new int[2, 2];
            int textCounter = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    matrix[i, j] = alphabet.IndexOf(text[textCounter]);
                    textCounter++;
                }
            }
            return matrix;
        }

        // Метод для очистки текста от символов, не входящих в алфавит
        private string CleanText(string text)
        {
            string cleanedText = "";
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()))
                {
                    cleanedText += c;
                }
            }
            return cleanedText;
        }
    }
}
