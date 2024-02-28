using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;

namespace Template4338
{
    /// <summary>
    /// Логика взаимодействия для _4338_Petrova.xaml
    /// </summary>
    public partial class _4338_Petrova : Window
    {
        public _4338_Petrova()
        {
            InitializeComponent();
        }
        private void BnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = "*.xls;*.xlsx",
                Filter = "файл Excel (Spisok.xlsx)|*.xlsx",
                Title = "Выберите файл базы данных"
            };
            if (!(ofd.ShowDialog() == true))
                return;
            string[,] list;
            Excel.Application ObjWorkExcel = new Excel.Application();
            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(ofd.FileName);
            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1];
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            int _columns = (int)lastCell.Column;
            int _rows = (int)lastCell.Row;
            list = new string[_rows, _columns];
            for (int j = 0; j < _columns; j++)
            {
                for (int i = 0; i < _rows; i++)
                {
                    list[i, j] = ObjWorkSheet.Cells[i + 1, j + 1].Text;
                }
            }
            ObjWorkBook.Close(false, Type.Missing, Type.Missing);
            ObjWorkExcel.Quit();
            GC.Collect();
            using (ISRPO3Entities4 iSRPO3Entities = new ISRPO3Entities4())
            {
                for (int i = 1; i < _rows; i++)
                {
                    iSRPO3Entities.ISRPO.Add(new ISRPO()
                    {
                        Наименование_услуги = list[i, 1],
                        Вид_услуги = list[i, 2],
                        Код_услуги = list[i, 3],
                        Стоимость = list[i, 4]

                    });
                }
                iSRPO3Entities.SaveChanges();
            }


        }
        private void BnExport_Click(object sender, RoutedEventArgs e)
        {
            List<ISRPO> isrpo;

            using (ISRPO3Entities4 iSRPO3Entities = new ISRPO3Entities4())
            {
                isrpo = iSRPO3Entities.ISRPO.ToList().OrderBy(s => s.Стоимость).ToList();
            }
            var app = new Excel.Application();
            app.SheetsInNewWorkbook = 3; // Set the number of sheets to 3
            Excel.Workbook workbook = app.Workbooks.Add(Type.Missing);

            double category1Max = 250;
            double category2Max = 800;

            Excel.Worksheet category1Sheet = app.Worksheets.Item[1];
            category1Sheet.Name = "Категория 1";

            Excel.Worksheet category2Sheet = app.Worksheets.Item[2];
            category2Sheet.Name = "Категория 2";

            Excel.Worksheet category3Sheet = app.Worksheets.Item[3];
            category3Sheet.Name = "Категория 3";
            var groupedByCategory = isrpo.GroupBy(usluga =>
            {
                if (Convert.ToDouble(usluga.Стоимость) <= category1Max)
                    return "Category 1";
                else if (Convert.ToDouble(usluga.Стоимость) <= category2Max)
                    return "Category 2";
                else
                    return "Category 3";
            });

            foreach (var group in groupedByCategory)
            {
                Excel.Worksheet worksheet = null;

                if (group.Key == "Category 1")
                    worksheet = category1Sheet;
                else if (group.Key == "Category 2")
                    worksheet = category2Sheet;
                else
                    worksheet = category3Sheet;

                int startRowIndex = 1;
                worksheet.Cells[1][startRowIndex] = "Id";
                worksheet.Cells[2][startRowIndex] = "Название услуги";
                worksheet.Cells[3][startRowIndex] = "Вид услуги";
                worksheet.Cells[4][startRowIndex] = "Стоимость";
                startRowIndex++;
                foreach (ISRPO usluga in group)
                {
                    

                    worksheet.Cells[1][startRowIndex] = usluga.ID;
                    worksheet.Cells[2][startRowIndex] = usluga.Наименование_услуги;
                    worksheet.Cells[3][startRowIndex] = usluga.Вид_услуги;
                    worksheet.Cells[4][startRowIndex] = usluga.Стоимость;
                    startRowIndex++;
                }

                worksheet.Columns.AutoFit();
            }
            

            app.Visible = true;
        }




    }
}
