using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
string[] koFiles = { "",};
string[] jpFiles = { "", };

try
{
    koFiles = Directory.GetFiles("output_ko", "ko", SearchOption.AllDirectories);
} 
catch (IOException ex)
{ 
    Console.WriteLine(ex.Message);
    return;
}
try
{
    jpFiles = Directory.GetFiles("output", "ja", SearchOption.AllDirectories);
}
catch (IOException ex)
{
    Console.WriteLine(ex.Message);
    return;
}
foreach (string koFile in koFiles)
{
    string koFilePath = koFile;
    string koTargetFilePath = koFilePath.Substring(10);
    koTargetFilePath = koTargetFilePath.Substring(0, koTargetFilePath.Length - 3);
    string jpFilePath = "";
    string outputFilePath = "";

    int loop = 0;
    foreach (string jpFile in jpFiles)
    {
        jpFilePath = jpFile;
        outputFilePath = jpFilePath.Substring(0, jpFilePath.Length - 2) + "ja";
        
        string jpTargetFilePath = jpFilePath.Substring(7);
        jpTargetFilePath = jpTargetFilePath.Substring(0, jpTargetFilePath.Length - 3);
        if(jpTargetFilePath == koTargetFilePath)
        {
            break;
        }
        loop++;
    }
    if (loop >= jpFiles.Length) continue;


    StreamReader jpsr = new StreamReader(jpFilePath);
    string jpData = jpsr.ReadToEnd();
    jpsr.Close();
    List<datamodel> jpDataList = JsonConvert.DeserializeObject<List<datamodel>>(jpData);

    StreamReader kosr = new StreamReader(koFilePath);
    string koData = new StreamReader(koFilePath).ReadToEnd();
    kosr.Close();
    List<datamodel> koDataList = JsonConvert.DeserializeObject<List<datamodel>>(koData);

    StreamReader IgnoreD = new StreamReader("ignore");
    string ignoreData = new StreamReader("ignore").ReadToEnd();
    IgnoreD.Close();
    List<IgnoreCell> ignoreDataList = JsonConvert.DeserializeObject<List<IgnoreCell>>(ignoreData);

    foreach (var jp in jpDataList)
    {
        var target = koDataList.Find(x => jp.Key == x.Key);
        bool skip = false;
        if (target != null)
        {
            foreach (var item in ignoreDataList)
            {
                if (koFile.Contains(item.Name) && jp.Key == item.Key)
                {
                    Console.WriteLine(koFilePath);
                    Console.WriteLine(item.Name);
                    skip = true;
                    break;
                }
            }
            if (target.Fields.Count > 0 && skip == false)
            {
                jp.Size = target.Size;
                jp.Fields = target.Fields;
            }
        }
    }

    // offset fix
    int lastOffset = -1;
    foreach (var jp in jpDataList)
    {
        if (lastOffset < 0) lastOffset = jp.Offset + jp.Size + 6;
        else
        {
            jp.Offset = lastOffset;
            lastOffset = lastOffset + jp.Size + 6;
        }
    }

    string output = JsonConvert.SerializeObject(jpDataList, Formatting.Indented);
    FileStream fs = File.Create(outputFilePath);
    Console.WriteLine(outputFilePath);
    fs.Close();
    if (File.Exists(outputFilePath))
        File.WriteAllText(outputFilePath, output);
}


Console.WriteLine("F");

public class datamodel
    {
        public int Key { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }
        public int CheckDigit { get; set; }
        public List<Field> Fields { get; set; }
    }


public class Field
{
    public int FieldKey { get; set; }
    public List<FieldValue> FieldValue { get; set; }
}

public class FieldValue
{
    public string EntryType { get; set; }
    public object EntryValue { get; set; }
}

public class IgnoreCell
{
    public string Name { get; set; }
    public int Key { get; set; }
    public int column { get; set; }
    public string forceLanguage { get; set; }
}