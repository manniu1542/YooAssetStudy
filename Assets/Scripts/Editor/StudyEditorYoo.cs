//�Զ�����չ����
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset.Editor;
using BuildReport = UnityEditor.Build.Reporting.BuildReport;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

public class AA
{
    [MenuItem("Tools/GG")]
    public static void GG()
    {
     
        //BuildPipeline.BuildAssetBundles(Application.dataPath, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();


        // ���ù�����ƽ̨��Ŀ��·��
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Bundle/Scene/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/MyGame.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;

        // ʹ���Զ���Ĺ�������
        BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);

        // ���������棬��ȡ������Ϣ
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("Build failed");
        }
        AssetDatabase.Refresh();

    }
}

public class TestPakeage: IPackRule
{
    PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
    {
        //"Assets/Config/test.txt" --> "Assets/Config"
        string bundleName = Path.GetDirectoryName(data.AssetPath);
        PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
        return result;
    }
    //bool IPackRule.IsRawFilePackRule()
    //{
    //    return false;
    //}
}

public class CollectScene2222 : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        return Path.GetExtension(data.AssetPath) == ".unity";
    }
}