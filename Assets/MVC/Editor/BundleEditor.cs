using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class BundleEditor
{
    [MenuItem("Tools/打包AB包")]
    static void Build()
    {
        BuildAB();
    }

    /// <summary>
    /// ab包放置路径 根据平台放置
    /// </summary>
    static string BuildPath = Application.dataPath + "/../AssetBundles/" + EditorUserBuildSettings.activeBuildTarget.ToString();

    /// <summary>
    /// 所有打包文件字典
    /// key指的是ab包名，value指的是路径
    /// </summary>
    static Dictionary<string, string> allFileDic = new Dictionary<string, string>();

    /// <summary>
    /// 过滤预制体依赖项和allFileDic所有路径中的冗余  如果已经在其他的包里面 那就不需要和预制体一起打包了
    /// 这样这些路径将变成单独打包预制体的依赖项
    /// </summary>
    static List<string> allFilePath = new List<string>();

    /// <summary>
    /// 所有要单独打包的预制体
    ///  key指的是ab包名也是预制体名称，value指的是预制体的所有依赖项
    /// </summary>
    static Dictionary<string, List<string>> allPrefabDic = new Dictionary<string, List<string>>();
    private static void BuildAB()
    {
        allFileDic.Clear();
        allPrefabDic.Clear();
        allFilePath.Clear();
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>("Assets/Scripts/Editor/ABConfig.asset");
        foreach (FileDirName fileDir in abConfig.m_allFileDirData)
        {
            if (allFileDic.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包名重复了，请检查一下:" + fileDir.ABName);
            }
            else
            {
                allFileDic.Add(fileDir.ABName, fileDir.Path);
                allFilePath.Add(fileDir.Path);
            }
        }

        //找到所有要单独打包的预制体的GUID 
        string[] allPrefabGuid = AssetDatabase.FindAssets("t:Prefab", abConfig.m_allPrefabPath.ToArray());
        for (int i = 0; i < allPrefabGuid.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPrefabGuid[i]);
            Debug.Log(path);
            EditorUtility.DisplayProgressBar("查找预制体", "预制体：" + path, i * 1.0f / allPrefabGuid.Length);
            if (!ContainAllFilePath(path))
            {
                allFilePath.Add(path); //假如是Assets/GameAssets/Prefab/3D/Bear.prefab
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path); //加载出来Bear
                string[] prefabDependencies = AssetDatabase.GetDependencies(path); //获取Bear所有的依赖项
                List<string> allDependencies = new List<string>();//创建一个依赖项列表 记录没有被配置进去的依赖项 被遗忘的依赖项
                allDependencies.Add(path); //先将Assets/GameAssets/Prefab/3D/Bear.prefab放进去
                for (int j = 0; j < prefabDependencies.Length; j++)
                {
                    if (!ContainAllFilePath(prefabDependencies[j]) && !prefabDependencies[j].Contains(".cs") && !prefabDependencies[j].Contains(".prefab"))
                    {
                        allFilePath.Add(prefabDependencies[j]);
                        allDependencies.Add(prefabDependencies[j]);
                    }
                }

                if (allPrefabDic.ContainsKey(obj.name))
                {
                    Debug.Log("你的预制体有重名了！！！！！！！！！！！" + obj.name);
                }
                else
                {
                    allPrefabDic[obj.name] = allDependencies;
                }
            }
        }
        EditorUtility.ClearProgressBar();
        //为整体打包的文件打标签
        foreach (string name in allFileDic.Keys)
        {
            SetABName(name, allFileDic[name]);
        }
        //为预制体单独打包的文件打标签
        foreach (string name in allPrefabDic.Keys)
        {
            SetABName(name, allPrefabDic[name]);
        }
        //生成AB包
        BuildAssetBundle();
    }

    /// <summary>
    /// 打包AB包
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private static void BuildAssetBundle()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            //根据ab包标签 拿到所有属于这个标签的文件路径
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                if (allBundlePath[j].EndsWith(".cs"))
                    continue;
                resPathDic.Add(allBundlePath[j], allBundles[i]);
            }
        }
        //判断有没有ab包的输出路径 如果没有 生成这个路径的文件夹
        if (!Directory.Exists(BuildPath))
            Directory.CreateDirectory(BuildPath);
        DeleteAssetBundles();
        //生成ab包配置xml 不使用manifest文件 使用自己生成的配置文件
        WriteData(resPathDic);
        //生成ab包
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(BuildPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            Debug.LogError("打包失败");
        }
        else
        {
            Debug.LogError("打包完毕");
        }
        //删除所有manifest文件
        DeleteAllManifest();
        //对所有ab包进行加密
        EncipherAllAB(true);
    }

    /// <summary>
    /// 对ab包进行加密
    /// </summary>
    private static void EncipherAllAB(bool isEn)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(BuildPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith("meta")&& !files[i].Name.EndsWith("manifest"))
            {
                if (isEn)
                {
                    //加密
                    AES.AESEncrypt(files[i].FullName, "2403A");
                }
                else
                {
                    //解密
                    AES.AESFileByteDecrypt(files[i].FullName, "2403A");
                }
            }
        }
    }

    /// <summary>
    /// 由于我们加载资源使用自己生成的xml配置表 所以不用manifest 直接删除打包路径下所有后缀为.manifest的文件即可
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private static void DeleteAllManifest()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(BuildPath);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".manifest"))
            {
                File.Delete(files[i].FullName);
            }
        }
    }

    /// <summary>
    /// 生成ab包配置表
    /// </summary>
    /// <param name="resPathDic"></param>
    private static void WriteData(Dictionary<string, string> resPathDic)
    {
        //创建配置文件数据接收容器
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<ABBase>();
        //遍历所有要打包的文件路径
        foreach (string path in resPathDic.Keys)
        {
            ABBase abBase = new ABBase();
            abBase.Path = path; //获取资源路径
            abBase.Crc = Crc32.GetCrc32(path); //根据路径获取crc编码
            abBase.ABName = resPathDic[path]; //获取资源路径对应的ab包名
            //将路径最后一个‘/’及以前的所有内容全部删除 例如Assets/GameAssets/Prefab/3D/Bear.Prefab将得到 Bear.Prefab
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
            abBase.ABDependce = new List<string>();//创建依赖项
            string[] resDependce = AssetDatabase.GetDependencies(path); //根据资源路径获取所有依赖项
            //遍历所有依赖项
            for (int i = 0; i < resDependce.Length; i++)
            {
                //获取依赖项路径
                string tempPath = resDependce[i];
                //如果依赖项是自身或者cs脚本不做处理 跳过本次循环
                if (tempPath.Equals(path) || path.EndsWith(".cs"))
                    continue;
                //尝试在获取这个路径对应的ab包名
                if(resPathDic.TryGetValue(tempPath,out string abName))
                {
                    //如果ab包名和路径一致直接跳过
                    if (abName.Equals(resPathDic[path]))
                        continue;
                    //因为依赖项可能存在同一个包里 所以会出现重复添加同一个ab包名的情况 所以做筛查
                    if (!abBase.ABDependce.Contains(abName))
                    {
                        abBase.ABDependce.Add(abName);
                    }
                }
            }
            //添加到配置文件当中
            config.ABList.Add(abBase);
        }
        //获取xml写入路径
        string xmlOutPath = Application.dataPath + "/AssetBundleConfig.xml";
        //如果包含这个文件 就删掉
        if (File.Exists(xmlOutPath))
            File.Delete(xmlOutPath);
        //通过流的方式写入 路径 创建 读写
        using (FileStream fileStream = new FileStream(xmlOutPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            //创建写入流 写入格式UTF-8
            using (StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8))
            {
                XmlSerializer xml = new XmlSerializer(config.GetType());
                xml.Serialize(sw, config);
            }
        }
        //获取二进制写入路径
        string binaryOutPath = Application.dataPath + "/AssetBundleConfig.bytes";
        //通过流的方式写入 路径 创建 读写
        using (FileStream fileStream = new FileStream(binaryOutPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            //从头写入
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.SetLength(0);
            BinaryFormatter binary = new BinaryFormatter();
            binary.Serialize(fileStream, config);
        }
        //刷新资源
        AssetDatabase.Refresh();
        //给配置文件打标签 因为配置文件也要打包到ab包中
        SetABName("assetbundleconfig", "Assets/" + xmlOutPath.Replace(Application.dataPath, ""));
    }

    /// <summary>
    /// 删除之前没用的ab包 还在继续使用的直接覆盖
    /// </summary>
    private static void DeleteAssetBundles()
    {
        //删除输出路径下所有的ab包 并且和现在的ab包名作对比 
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        //根据输出路径获取文件夹的信息
        DirectoryInfo info = new DirectoryInfo(BuildPath);
        //根据文件夹信息获取该文件夹下所有的文件的信息
        FileInfo[] files = info.GetFiles("*", SearchOption.AllDirectories);
        //遍历所有文件信息
        for (int i = 0; i < files.Length; i++)
        {
            //如果当前的标签中有这个ab包名 那说明这个ab包可能要被修改了 所以不用管 
            //生成ab包的时候把这个包体替换了就行了
            if (ContainABName(files[i].Name, allBundleNames) || files[i].Name.EndsWith(".manifest"))
            {
                continue;
            }
            else
            {
                //如果当前标签中没有这个ab包名 说明这个ab包没用了 直接吧ab包和manifest文件都删除即可
                Debug.Log("这个ab包已经被删除 或者重命名" + files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
                if (File.Exists(files[i].FullName + ".manifest"))
                {
                    File.Delete(files[i].FullName + ".manifest");
                }
            }
        }
    }

    /// <summary>
    /// 判断当前所有标签中 是否包含这个文件名
    /// </summary>
    /// <param name="name"></param>
    /// <param name="allBundleNames"></param>
    /// <returns></returns>
    private static bool ContainABName(string name, string[] allBundleNames)
    {
        foreach (var item in allBundleNames)
        {
            if (name.Equals(item))
            {
                return true;
            }
        }
        return false;
    }

    static void SetABName(string abName, string paths)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(paths);
        if (assetImporter == null)
        {
            Debug.LogError("不存在此路径文件：" + paths);
        }
        else
        {
            assetImporter.assetBundleName = abName;
        }
    }

    static void SetABName(string abName, List<string> paths)
    {
        foreach (var item in paths)
        {
            SetABName(abName, item);
        }
    }

    private static bool ContainAllFilePath(string path)
    {
        for (int j = 0; j < allFilePath.Count; j++)
        {
            if (path == allFilePath[j] || (path.Contains(allFilePath[j]) && (path.Replace(allFilePath[j], "")[0] == '/')))
            {
                return true;
            }
        }

        return false;
    }
}

[System.Serializable]
public class AssetBundleConfig
{
    [XmlElement("ABList")]
    public List<ABBase> ABList { get; set; }
}

/// <summary>
/// 每个资源的具体配置
/// </summary>
[System.Serializable]
public class ABBase
{
    /// <summary>
    /// 路径
    /// </summary>
    [XmlAttribute("Path")]
    public string Path { get; set; }
    /// <summary>
    /// crc编码
    /// </summary>
    [XmlAttribute("Crc")]
    public uint Crc { get; set; }
    /// <summary>
    /// 所属的ab包名字
    /// </summary>
    [XmlAttribute("ABName")]
    public string ABName { get; set; }
    /// <summary>
    /// 资源的名称
    /// </summary>
    [XmlAttribute("AssetName")]
    public string AssetName { get; set; }
    /// <summary>
    /// 这个资源的依赖项
    /// </summary>
    [XmlElement("ABDependce")]
    public List<string> ABDependce { get; set; }
}