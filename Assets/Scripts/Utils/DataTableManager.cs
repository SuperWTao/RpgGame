using System.Collections;
using System.Collections.Generic;
using cfg;
using SimpleJSON;
using UnityEngine;

public static class DataTableManager
{
    public static Tables Tables { get; private set; }
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized)
            return;
        Tables = new Tables(LoadTable);
        _isInitialized = true;
        
    }
    
    public static JSONNode LoadTable(string tableName)
    {
        string resourcePath = "Data/" + tableName;
        //读取配置的数据
        var textAsset = Resources.Load<TextAsset>(resourcePath);
        if (textAsset == null)
        {
            Debug.Log("配表不存在，请检查配表名是否正确并检查是否导入配表");
            return JSONNode.Parse("{}");
        }
        JSONNode node = null;
        try
        {
            node = JSONNode.Parse(textAsset.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"解析配置表 '{tableName}' 失败: {e.Message}\nJSON内容:\n{textAsset.text}");
            node = JSONNode.Parse("{}");
        }

        Debug.Log($"成功加载配置表: {tableName} ({resourcePath})");
        return node;
    }
}
