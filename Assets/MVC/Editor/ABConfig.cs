using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABConfig",menuName = "CreateABConfig",order = 0)]
public class ABConfig : ScriptableObject
{
    /// <summary>
    /// ����Ԥ������·�� �ŵ�����б����·�� �����·�������е�Ԥ����ȫ���������
    /// ab���������Ԥ������������ ��������Щ·�������Ԥ���岻�������� �����޷����
    /// ��Ϊab�������ǲ��������ظ������
    /// </summary>
    public List<string> m_allPrefabPath = new List<string>();

    /// <summary>
    /// ����·����� ��·�������е���Ʒ���һ��ab�� ab�����Լ�����
    /// </summary>
    public List<FileDirName> m_allFileDirData = new List<FileDirName>();
}

[System.Serializable]
public class FileDirName
{
    public string ABName;
    public string Path;
}
