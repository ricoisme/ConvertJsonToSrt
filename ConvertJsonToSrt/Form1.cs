using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using System.Text;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ConvertJsonToSrt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckFile();

            var jsonFilePath = textBox1.Text;
            var outputPath = textBox2.Text;
            var json = File.ReadAllText(jsonFilePath, Encoding.UTF8);
          
            try
            {
                var srtContent = Process(json);
                if(string.IsNullOrEmpty(srtContent))
                {
                    MessageBox.Show("Failed");
                }

                File.WriteAllText(outputPath, srtContent, Encoding.UTF8);
                MessageBox.Show("Success");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }         
        }

        private void CheckFile()
        {
            var jsonFilePath = textBox1.Text;
            var srtPath = textBox2.Text;
            if (string.IsNullOrEmpty(jsonFilePath)
                || string.IsNullOrEmpty(srtPath))
            {
                MessageBox.Show("Please input file path");
                return;
            }

            
            if (!File.Exists(jsonFilePath))
            {
                MessageBox.Show("json file is not exists");
                return;
            }
           
        }

        private string Process(string json)
        {          
            var jd = JsonDocument.Parse(json);
            var root = jd.RootElement;
            var mycontents = new List<MyContent>();
            var sb = new StringBuilder();
            var index = 0;

            foreach (JsonElement elem in root.GetProperty("materials").GetProperty("texts").EnumerateArray())
            {
                var id = elem.GetProperty("id").GetString();
                var content = elem.GetProperty("content").GetString();
                var startIndex = content.IndexOf('[');
                var endIndex = content.IndexOf(']');
                var onlyText = content.Substring(startIndex + 1, endIndex - startIndex - 1);
                var finallyText = onlyText;
                if (checkBox1.Checked)
                {
                    finallyText = ChineseConverter.Convert(onlyText, ChineseConversionDirection.SimplifiedToTraditional);
                }
                mycontents.Add(new MyContent { Id = id, Text = finallyText });
            }
            
            foreach (JsonElement track in root.GetProperty("tracks").EnumerateArray())
            {
                foreach (JsonElement segment in track.GetProperty("segments").EnumerateArray())
                {
                    var materialId = segment.GetProperty("material_id").GetString();
                    var targetTimeRange = segment.GetProperty("target_timerange");
                    var renderIndex = segment.GetProperty("render_index").GetInt32();
                    if (renderIndex == 0)
                    {
                        continue;
                    }
                    var duration = targetTimeRange.GetProperty("duration").GetInt64();
                    var start = targetTimeRange.GetProperty("start").GetInt64();
                    var timeRange = GetTimeRangeFormat(start, duration);
                    var myContext = mycontents.Where(x => x.Id.Equals(materialId)).FirstOrDefault();
                    if (myContext != null)
                    {
                        sb.AppendLine((index + 1).ToString());
                        sb.AppendLine(timeRange);

                        sb.AppendLine(mycontents[index].Text);
                        sb.AppendLine();
                    }
                    index++;
                }
            }
            return sb.ToString();
        }

        private string GetTimeRangeFormat(Int64 start, Int64 duration)
        {
            var startSec = start / 1000000.0;
            var endSec=(duration+ start) / 1000000.0;          
            var startTime = TimeSpan.FromSeconds(double.Parse( startSec.ToString("#0.000")));
            var endTime= TimeSpan.FromSeconds(double.Parse(endSec.ToString("#0.000")));         
            return $"{startTime.ToString(@"hh\:mm\:ss\,fff")} --> {endTime.ToString(@"hh\:mm\:ss\,fff")}"; 
        }
    }
}